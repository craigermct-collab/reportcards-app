window.imageHelper = {
    compressImage: async function (dataUrl, maxDimension, quality) {
        return new Promise((resolve) => {
            const img = new Image();
            img.onload = function () {
                let w = img.width, h = img.height;
                if (w > maxDimension || h > maxDimension) {
                    if (w > h) { h = Math.round(h * maxDimension / w); w = maxDimension; }
                    else       { w = Math.round(w * maxDimension / h); h = maxDimension; }
                }
                const canvas = document.createElement('canvas');
                canvas.width = w; canvas.height = h;
                canvas.getContext('2d').drawImage(img, 0, 0, w, h);
                resolve(canvas.toDataURL('image/jpeg', quality));
            };
            img.onerror = () => resolve(dataUrl);
            img.src = dataUrl;
        });
    }
};
