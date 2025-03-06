import Alpine from "alpinejs";

window.Alpine = Alpine;

Alpine.data("quizDropdownTemplate", function () {
    return {
        init() {
            const selectBreedElements = document.querySelectorAll(".select-breed");
            for (const selectBreedElement of selectBreedElements) {
                const clone = this.$el.content.cloneNode(true);
                selectBreedElement.appendChild(clone);
            }
        }
    };
});

Alpine.start();
