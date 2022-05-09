if (performance.getEntriesByType("navigation")[0].type == "reload" && location.href.includes("#")) {
    location.replace("");
}
else if (performance.getEntriesByType("navigation")[0].type == "back_forward" && location.href.includes("#")) {
    location.replace("");
}

$('[data-toggle="tooltip"]').tooltip();

const modals = [...document.getElementsByClassName('modal')];
modals.forEach((modal) => {
    let modalId = `#${modal.id}`;

    if (location.href.includes(modalId)) {
        $(modalId).modal('show');
    }
});


const autoResize = area => {
    area.style.height = '18px';
    area.style.height = `${area.scrollHeight}px`;
}

const textareas = [...document.getElementsByClassName('textarea')];
textareas.forEach((textarea) => {
    textarea.addEventListener('input', () => { autoResize(textarea); }, false);
    textarea.addEventListener('click', () => { autoResize(textarea); }, false);


});

let positionOfExtension;

$('select > option').text((i, text) => {
    if (text.length > 38) {

        positionOfExtension = text.lastIndexOf(".");
        return `${text.substr(0, 30)}...${text.substr(positionOfExtension - 1)}`;
    }
});


