// Client-side image store — images never travel to the server until analysis time
window.imageHelper = {

    _images: [],      // full-res compressed base64 data URLs
    _lastFiles: null, // captured from the most recent file input change event

    // Called from JS event listener (set up by initFileInputs) when a file input changes.
    // Stores files for processFilesFromEvent to pick up.
    _onInputChange: function (event, dotNetRef) {
        const files = event.target.files;
        if (!files || files.length === 0) return;
        window.imageHelper.processFiles(files, dotNetRef);
        // Reset input so same file can be selected again
        event.target.value = '';
    },

    // Attach JS change listeners to all file inputs on the page.
    // Called from Blazor after render, passing the DotNetObjectReference.
    initFileInputs: function (dotNetRef) {
        const inputs = document.querySelectorAll('input[type="file"]');
        inputs.forEach(input => {
            // Remove any existing listener to avoid duplicates
            if (input._imageHelperListener) {
                input.removeEventListener('change', input._imageHelperListener);
            }
            input._imageHelperListener = (e) => window.imageHelper._onInputChange(e, dotNetRef);
            input.addEventListener('change', input._imageHelperListener);
        });
    },

    // Process an array of File objects: read, compress, store full-res, notify Blazor with thumbnail
    processFiles: async function (files, dotNetRef) {
        for (const file of files) {
            try {
                const dataUrl = await new Promise((resolve, reject) => {
                    const reader = new FileReader();
                    reader.onload = e => resolve(e.target.result);
                    reader.onerror = () => reject(new Error('FileReader failed'));
                    reader.readAsDataURL(file);
                });

                // Full-res compressed copy for analysis (1600px, high quality)
                const fullRes = await window.imageHelper.compressImage(dataUrl, 1600, 0.82);
                window.imageHelper._images.push(fullRes);

                // Small thumbnail for display only (200px) — this is the only thing sent to Blazor
                const thumb = await window.imageHelper.compressImage(dataUrl, 200, 0.7);
                const index = window.imageHelper._images.length - 1;
                await dotNetRef.invokeMethodAsync('OnImageAdded', index, thumb);

            } catch (e) {
                try {
                    await dotNetRef.invokeMethodAsync('OnImageError', file.name, e.message || 'Unknown error');
                } catch (_) { }
            }
        }
    },

    // Called only at analysis time — returns full-res images to server
    getImages: function () {
        return window.imageHelper._images;
    },

    removeImage: function (index) {
        window.imageHelper._images.splice(index, 1);
    },

    clearImages: function () {
        window.imageHelper._images = [];
    },

    compressImage: async function (dataUrl, maxDimension, quality) {
        return new Promise((resolve) => {
            const img = new Image();
            img.onload = function () {
                let width = img.width;
                let height = img.height;
                if (width > maxDimension || height > maxDimension) {
                    if (width > height) {
                        height = Math.round((height * maxDimension) / width);
                        width = maxDimension;
                    } else {
                        width = Math.round((width * maxDimension) / height);
                        height = maxDimension;
                    }
                }
                const canvas = document.createElement('canvas');
                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0, width, height);
                resolve(canvas.toDataURL('image/jpeg', quality));
            };
            img.onerror = () => resolve(dataUrl); // fallback
            img.src = dataUrl;
        });
    }
};
