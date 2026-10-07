(function () {
    "use strict";

    function initializePlayer(player) {
        if (!player) {
            return;
        }

        var qrCodeId = player.dataset.qrCodeId;

        if (!qrCodeId) {
            return;
        }

        var mediaType = player.dataset.mediaType;

        if (mediaType !== "Audio" && mediaType !== "Video") {
            return;
        }

        var sentMilestones = {
            25: false,
            50: false,
            75: false,
            100: false
        };

        var lastProgressSent = -1;
        var lastProgressSentAt = 0;

        var hasCompleted = false;

        function getDuration() {
            if (!Number.isFinite(player.duration) ||
                player.duration <= 0) {
                return null;
            }

            return player.duration;
        }

        function getCurrentTime() {
            if (!Number.isFinite(player.currentTime) ||
                player.currentTime < 0) {
                return null;
            }

            return player.currentTime;
        }

        function getProgress() {
            var duration = getDuration();
            var currentTime = getCurrentTime();

            if (duration === null ||
                currentTime === null) {
                return null;
            }

            var progress = (currentTime / duration) * 100;

            if (!Number.isFinite(progress)) {
                return null;
            }

            return Math.max(0, Math.min(100, progress));
        }

        function buildPayload(
            eventType,
            durationSeconds,
            progressPercent) {

            var payload = {
                qrCodeId: Number(qrCodeId),
                eventType: eventType
            };

            if (durationSeconds !== undefined &&
                durationSeconds !== null &&
                Number.isFinite(durationSeconds) &&
                durationSeconds >= 0) {

                payload.durationSeconds = durationSeconds;
            }

            if (progressPercent !== undefined &&
                progressPercent !== null &&
                Number.isFinite(progressPercent) &&
                progressPercent >= 0 &&
                progressPercent <= 100) {

                payload.progressPercent = progressPercent;
            }

            return payload;
        }

        function sendEvent(
            eventType,
            durationSeconds,
            progressPercent) {

            var payload = buildPayload(
                eventType,
                durationSeconds,
                progressPercent);

            fetch("/api/analytics/track", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload),
                keepalive: true,
                credentials: "same-origin"
            }).catch(function () {
                // Analytics نباید باعث اختلال در Player شود.
            });
        }

        function sendEventWithBeacon(
            eventType,
            durationSeconds,
            progressPercent) {

            var payload = buildPayload(
                eventType,
                durationSeconds,
                progressPercent);

            try {
                var blob = new Blob(
                    [JSON.stringify(payload)],
                    {
                        type: "application/json"
                    });

                var sent = navigator.sendBeacon(
                    "/api/analytics/track",
                    blob);

                if (sent) {
                    return true;
                }
            }
            catch (error) {
                // در صورت عدم پشتیبانی، fetch استفاده می‌شود.
            }

            sendEvent(
                eventType,
                durationSeconds,
                progressPercent);

            return false;
        }

        function markMilestone(percent, eventType) {
            if (sentMilestones[percent]) {
                return;
            }

            sentMilestones[percent] = true;

            var currentTime = getCurrentTime();

            sendEvent(
                eventType,
                currentTime,
                percent);
        }

        player.addEventListener("play", function () {

            var currentTime = getCurrentTime();
            var progress = getProgress();

            sendEvent(
                2,
                currentTime,
                progress);
        });

        player.addEventListener("pause", function () {

            if (hasCompleted) {
                return;
            }

            var currentTime = getCurrentTime();
            var progress = getProgress();

            sendEvent(
                3,
                currentTime,
                progress);
        });

        player.addEventListener("timeupdate", function () {

            var progress = getProgress();

            if (progress === null) {
                return;
            }

            /*
             * timeupdate در بعضی مرورگرها تعداد زیادی
             * بار در ثانیه اجرا می‌شود.
             *
             * بنابراین فقط زمانی milestone را بررسی می‌کنیم
             * که progress به محدوده جدیدی وارد شده باشد.
             */

            if (progress >= 25) {
                markMilestone(25, 4);
            }

            if (progress >= 50) {
                markMilestone(50, 5);
            }

            if (progress >= 75) {
                markMilestone(75, 6);
            }

            if (progress >= 100) {
                markMilestone(100, 7);
            }

            /*
             * فقط برای جلوگیری از پردازش بیش از حد
             * وضعیت progress را نگه می‌داریم.
             */

            var now = Date.now();

            if (lastProgressSent === -1 ||
                Math.abs(progress - lastProgressSent) >= 1 ||
                now - lastProgressSentAt >= 5000) {

                lastProgressSent = progress;
                lastProgressSentAt = now;
            }
        });

        player.addEventListener("ended", function () {

            if (hasCompleted) {
                return;
            }

            hasCompleted = true;

            /*
             * در ended، مرورگر باید currentTime را
             * تقریباً برابر duration قرار داده باشد.
             */

            var duration = getDuration();

            if (duration !== null) {

                markMilestone(100, 7);

                sendEventWithBeacon(
                    8,
                    duration,
                    100);

            }
            else {

                markMilestone(100, 7);

                sendEventWithBeacon(
                    8,
                    null,
                    100);
            }
        });

        /*
         * اگر کاربر صفحه را ترک کند، eventهای مهمی که
         * قبل از unload در حال ارسال هستند تا حد ممکن
         * با keepalive / Beacon ارسال می‌شوند.
         *
         * فعلاً event جدیدی هنگام خروج ایجاد نمی‌کنیم؛
         * چون نمی‌خواهیم خروج از صفحه را به اشتباه
         * به عنوان Pause ثبت کنیم.
         */

        window.addEventListener("pagehide", function () {

            if (hasCompleted) {
                return;
            }

            var currentTime = getCurrentTime();
            var progress = getProgress();

            if (currentTime === null) {
                return;
            }

            if (progress === null) {
                sendEventWithBeacon(
                    3,
                    currentTime,
                    null);

                return;
            }

            sendEventWithBeacon(
                3,
                currentTime,
                progress);
        });
    }

    function initialize() {

        var players = document.querySelectorAll(
            "audio[data-qr-code-id], video[data-qr-code-id]"
        );

        players.forEach(function (player) {
            initializePlayer(player);
        });
    }

    if (document.readyState === "loading") {

        document.addEventListener(
            "DOMContentLoaded",
            initialize);

    }
    else {

        initialize();
    }

})();