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

        function sendEvent(eventType, durationSeconds, progressPercent) {
            var payload = {
                qrCodeId: Number(qrCodeId),
                eventType: eventType
            };

            if (durationSeconds !== undefined &&
                durationSeconds !== null &&
                Number.isFinite(durationSeconds)) {
                payload.durationSeconds = durationSeconds;
            }

            if (progressPercent !== undefined &&
                progressPercent !== null &&
                Number.isFinite(progressPercent)) {
                payload.progressPercent = progressPercent;
            }

            fetch("/api/analytics/track", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload),
                keepalive: true
            }).catch(function () {
                // Analytics نباید باعث اختلال در Player شود.
            });
        }

        function getDuration() {
            if (!Number.isFinite(player.duration) ||
                player.duration <= 0) {
                return null;
            }

            return player.duration;
        }

        function getProgress() {
            var duration = getDuration();

            if (duration === null ||
                !Number.isFinite(player.currentTime)) {
                return null;
            }

            return (player.currentTime / duration) * 100;
        }

        player.addEventListener("play", function () {
            var progress = getProgress();

            sendEvent(
                2,
                player.currentTime,
                progress
            );
        });

        player.addEventListener("pause", function () {
            var progress = getProgress();

            sendEvent(
                3,
                player.currentTime,
                progress
            );
        });

        player.addEventListener("timeupdate", function () {
            var progress = getProgress();

            if (progress === null) {
                return;
            }

            var milestones = [
                {
                    percent: 25,
                    eventType: 4
                },
                {
                    percent: 50,
                    eventType: 5
                },
                {
                    percent: 75,
                    eventType: 6
                },
                {
                    percent: 100,
                    eventType: 7
                }
            ];

            milestones.forEach(function (milestone) {
                if (progress >= milestone.percent &&
                    !sentMilestones[milestone.percent]) {

                    sentMilestones[milestone.percent] = true;

                    sendEvent(
                        milestone.eventType,
                        player.currentTime,
                        milestone.percent
                    );
                }
            });
        });

        player.addEventListener("ended", function () {
            var duration = getDuration();

            if (duration !== null) {
                sendEvent(
                    8,
                    duration,
                    100
                );
            } else {
                sendEvent(
                    8,
                    null,
                    100
                );
            }
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
            initialize
        );
    } else {
        initialize();
    }
})();