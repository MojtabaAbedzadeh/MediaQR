document.addEventListener("DOMContentLoaded", function () {

    const menuButton =
        document.getElementById("menuButton");

    const closeMenuButton =
        document.getElementById("closeMenuButton");

    const sideMenu =
        document.getElementById("sideMenu");

    const menuOverlay =
        document.getElementById("menuOverlay");


    if (
        !menuButton ||
        !closeMenuButton ||
        !sideMenu ||
        !menuOverlay
    ) {
        return;
    }


    function openMenu() {

        sideMenu.classList.add("active");

        menuOverlay.classList.add("active");

        document.body.classList.add("menu-open");

        menuButton.setAttribute(
            "aria-expanded",
            "true"
        );

        sideMenu.setAttribute(
            "aria-hidden",
            "false"
        );

    }


    function closeMenu() {

        sideMenu.classList.remove("active");

        menuOverlay.classList.remove("active");

        document.body.classList.remove("menu-open");

        menuButton.setAttribute(
            "aria-expanded",
            "false"
        );

        sideMenu.setAttribute(
            "aria-hidden",
            "true"
        );

    }


    /* OPEN */

    menuButton.addEventListener(
        "click",
        openMenu
    );


    /* CLOSE */

    closeMenuButton.addEventListener(
        "click",
        closeMenu
    );


    menuOverlay.addEventListener(
        "click",
        closeMenu
    );


    /* ESC */

    document.addEventListener(
        "keydown",
        function (event) {

            if (
                event.key === "Escape" &&
                sideMenu.classList.contains("active")
            ) {

                closeMenu();

            }

        }
    );


    /* CLOSE AFTER NAVIGATION */

    const menuLinks =
        sideMenu.querySelectorAll("a");


    menuLinks.forEach(function (link) {

        link.addEventListener(
            "click",
            closeMenu
        );

    });

});