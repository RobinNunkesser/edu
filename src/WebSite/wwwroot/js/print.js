window.eduPrint = (() => {
    const clearPrintMode = () => {
        document.body.classList.remove("exercise-printing");
    };

    window.addEventListener("afterprint", clearPrintMode);

    return {
        printExerciseSheet: () => {
            document.body.classList.add("exercise-printing");

            window.requestAnimationFrame(() => {
                window.requestAnimationFrame(() => {
                    window.print();
                    window.setTimeout(clearPrintMode, 750);
                });
            });
        }
    };
})();
