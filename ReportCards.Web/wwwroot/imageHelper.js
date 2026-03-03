// File download helper
window.downloadFileFromBase64 = function(base64, fileName, mimeType) {
    const bytes = Uint8Array.from(atob(base64), c => c.charCodeAt(0));
    const blob  = new Blob([bytes], { type: mimeType });
    const url   = URL.createObjectURL(blob);
    const a     = document.createElement('a');
    a.href      = url;
    a.download  = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

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
