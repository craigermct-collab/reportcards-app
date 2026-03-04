// ── Text-to-Speech ────────────────────────────────────────────────────────────

window.assistantTts = {
    speak: function (text) {
        if (!window.speechSynthesis) return;
        window.speechSynthesis.cancel();
        const utt = new SpeechSynthesisUtterance(text);
        utt.rate  = 1.05;
        utt.pitch = 1.0;
        const voices = window.speechSynthesis.getVoices();
        const preferred = voices.find(v =>
            v.name.includes("Google") || v.name.includes("Samantha") || v.name.includes("Karen")
        );
        if (preferred) utt.voice = preferred;
        window.speechSynthesis.speak(utt);
    },
    stop: function () {
        if (window.speechSynthesis) window.speechSynthesis.cancel();
    }
};

// ── Speech-to-Text ────────────────────────────────────────────────────────────

let _recognition = null;
let _dotNetRef   = null;
let _isListening = false;

window.assistantStt = {

    isSupported: function () {
        return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
    },

    start: function (dotNetHelper) {
        if (_isListening) return;

        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            dotNetHelper.invokeMethodAsync("OnSttError", "Speech recognition not supported in this browser.");
            return;
        }

        _dotNetRef   = dotNetHelper;
        _recognition = new SpeechRecognition();

        _recognition.lang            = "en-US";
        _recognition.interimResults  = true;
        _recognition.continuous      = false;
        _recognition.maxAlternatives = 1;

        _recognition.onstart = function () {
            _isListening = true;
            _dotNetRef.invokeMethodAsync("OnSttStarted");
        };

        _recognition.onresult = function (event) {
            let interim = "";
            let final   = "";
            for (let i = event.resultIndex; i < event.results.length; i++) {
                const transcript = event.results[i][0].transcript;
                if (event.results[i].isFinal) final   += transcript;
                else                          interim += transcript;
            }
            if (final)        _dotNetRef.invokeMethodAsync("OnSttResult", final.trim(),   true);
            else if (interim) _dotNetRef.invokeMethodAsync("OnSttResult", interim.trim(), false);
        };

        _recognition.onerror = function (event) {
            _isListening = false;
            _dotNetRef.invokeMethodAsync("OnSttError", event.error);
        };

        _recognition.onend = function () {
            _isListening = false;
            _dotNetRef.invokeMethodAsync("OnSttEnded");
        };

        _recognition.start();
    },

    stop: function () {
        if (_recognition && _isListening) _recognition.stop();
    },

    isListening: function () {
        return _isListening;
    }
};

// ── Scroll helper ─────────────────────────────────────────────────────────────

window.assistantUi = {
    scrollToBottom: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) el.scrollTop = el.scrollHeight;
    }
};
