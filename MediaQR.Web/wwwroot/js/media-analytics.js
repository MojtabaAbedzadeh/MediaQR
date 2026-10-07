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

        if (mediaType !== "Audio" &&
            mediaType !== "Video") {
            return;
        }

        var sentMilestones = {
            25: false,
            50: false,
            75: false,
            100: false
        };

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

            var progress =
                (currentTime / duration) * 100;

            if (!Number.isFinite(progress)) {
                return null;
            }

            return Math.max(
                0,
                Math.min(100, progress)
            );
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

                payload.durationSeconds =
                    durationSeconds;
            }


            if (progressPercent !== undefined &&
                progressPercent !== null &&
                Number.isFinite(progressPercent) &&
                progressPercent >= 0 &&
                progressPercent <= 100) {

                payload.progressPercent =
                    progressPercent;
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
                progressPercent
            );


            fetch(
                "/api/analytics/track",
                {
                    method: "POST",

                    headers: {
                        "Content-Type":
                            "application/json"
                    },

                    body:
                        JSON.stringify(payload),

                    keepalive: true,

                    credentials:
                        "same-origin"
                }
            ).catch(function () {

                /*
                 * Analytics نباید باعث اختلال
                 * در پخش Media شود.
                 */

            });
        }


        function sendEventWithBeacon(
            eventType,
            durationSeconds,
            progressPercent) {

            var payload = buildPayload(
                eventType,
                durationSeconds,
                progressPercent
            );


            try {

                var blob =
                    new Blob(
                        [JSON.stringify(payload)],
                        {
                            type:
                                "application/json"
                        }
                    );


                var sent =
                    navigator.sendBeacon(
                        "/api/analytics/track",
                        blob
                    );


                if (sent) {
                    return;
                }

            }
            catch (error) {

                /*
                 * در صورت عدم پشتیبانی
                 * از Beacon، از fetch استفاده می‌کنیم.
                 */

            }


            sendEvent(
                eventType,
                durationSeconds,
                progressPercent
            );
        }


        function markMilestone(
            percent,
            eventType) {

            if (sentMilestones[percent]) {
                return;
            }

            sentMilestones[percent] = true;

            var currentTime =
                getCurrentTime();


            sendEvent(
                eventType,
                currentTime,
                percent
            );
        }


        /*
         * PLAY
         */
        player.addEventListener(
            "play",
            function () {

                var currentTime =
                    getCurrentTime();

                var progress =
                    getProgress();


                sendEvent(
                    2,
                    currentTime,
                    progress
                );
            }
        );


        /*
         * PAUSE
         */
        player.addEventListener(
            "pause",
            function () {

                if (hasCompleted) {
                    return;
                }

                var currentTime =
                    getCurrentTime();

                var progress =
                    getProgress();


                sendEvent(
                    3,
                    currentTime,
                    progress
                );
            }
        );


        /*
         * PROGRESS
         */
        player.addEventListener(
            "timeupdate",
            function () {

                var progress =
                    getProgress();


                if (progress === null) {
                    return;
                }


                if (progress >= 25) {

                    markMilestone(
                        25,
                        4
                    );
                }


                if (progress >= 50) {

                    markMilestone(
                        50,
                        5
                    );
                }


                if (progress >= 75) {

                    markMilestone(
                        75,
                        6
                    );
                }


                if (progress >= 100) {

                    markMilestone(
                        100,
                        7
                    );
                }
            }
        );


        /*
         * COMPLETE
         */
        player.addEventListener(
            "ended",
            function () {

                if (hasCompleted) {
                    return;
                }

                hasCompleted = true;


                var duration =
                    getDuration();


                /*
                 * اطمینان از ثبت 100%
                 */
                markMilestone(
                    100,
                    7
                );


                /*
                 * ثبت Complete
                 */
                sendEventWithBeacon(
                    8,
                    duration,
                    100
                );
            }
        );


        /*
         * PAGE HIDE
         *
         * اگر کاربر صفحه را ترک کند،
         * آن را به عنوان Pause ثبت می‌کنیم.
         */
        window.addEventListener(
            "pagehide",
            function () {

                if (hasCompleted) {
                    return;
                }


                var currentTime =
                    getCurrentTime();


                if (currentTime === null) {
                    return;
                }


                var progress =
                    getProgress();


                sendEventWithBeacon(
                    3,
                    currentTime,
                    progress
                );
            }
        );
    }


    function initialize() {

        var players =
            document.querySelectorAll(
                "audio[data-qr-code-id], " +
                "video[data-qr-code-id]"
            );


        players.forEach(
            function (player) {

                initializePlayer(player);
            }
        );
    }


    if (document.readyState === "loading") {

        document.addEventListener(
            "DOMContentLoaded",
            initialize
        );

    }
    else {

        initialize();
    }

})();