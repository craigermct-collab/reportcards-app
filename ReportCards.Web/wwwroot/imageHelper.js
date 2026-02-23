// Image compression and file reading via Canvas API
window.imageHelper = {

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
            img.src = dataUrl;
        });
    },

    readAndCompressFiles: async function (inputId, maxDimension, quality) {
        const input = document.getElementById(inputId);
        if (!input || !input.files || input.files.length === 0) return [];
        const results = [];
        for (const file of input.files) {
            const dataUrl = await new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = e => resolve(e.target.result);
                reader.onerror = reject;
                reader.readAsDataURL(file);
            });
            const compressed = await window.imageHelper.compressImage(dataUrl, maxDimension, quality);
            results.push(compressed);
        }
        // Clear the input so same file can be added again if needed
        input.value = '';
        return results;
    },

    clickElement: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) el.click();
    }
};
