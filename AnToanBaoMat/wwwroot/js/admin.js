const button = document.getElementById("menuButton");

const sidebar = document.querySelector(".sidebar");

const main = document.querySelector(".main");

button.addEventListener("click", function () {

    sidebar.classList.toggle("collapsed");

    main.classList.toggle("expand");

});