//===============================
// Upload JS
//===============================

const dropZone = document.getElementById("dropZone");

const fileInput = document.getElementById("cvFile");

const chooseButton = document.getElementById("btnChoose");

const fileInfo = document.getElementById("fileInfo");

const fileName = document.getElementById("fileName");

const fileSize = document.getElementById("fileSize");

const maxSize = 5 * 1024 * 1024;

const allowExtension = [
    ".pdf",
    ".doc",
    ".docx"
];

//===============================
// Chọn file
//===============================

chooseButton.addEventListener("click", function () {

    fileInput.click();

});

//===============================
// Đổi file
//===============================

fileInput.addEventListener("change", function () {

    if (this.files.length > 0) {

        validateFile(this.files[0]);

    }

});

//===============================
// Drag Enter
//===============================

dropZone.addEventListener("dragenter", function (e) {

    e.preventDefault();

    dropZone.classList.add("dragover");

});

//===============================
// Drag Over
//===============================

dropZone.addEventListener("dragover", function (e) {

    e.preventDefault();

});

//===============================
// Drag Leave
//===============================

dropZone.addEventListener("dragleave", function () {

    dropZone.classList.remove("dragover");

});

//===============================
// Drop
//===============================

dropZone.addEventListener("drop", function (e) {

    e.preventDefault();

    dropZone.classList.remove("dragover");

    if (e.dataTransfer.files.length > 0) {

        fileInput.files = e.dataTransfer.files;

        validateFile(e.dataTransfer.files[0]);

    }

});

//===============================
// Validate
//===============================

function validateFile(file) {

    let extension =
        "." +
        file.name.split(".").pop().toLowerCase();

    if (!allowExtension.includes(extension)) {

        Swal.fire({

            icon: "error",

            title: "Không hợp lệ",

            text: "Chỉ hỗ trợ PDF DOC DOCX"

        });

        fileInput.value = "";

        return;

    }

    if (file.size > maxSize) {

        Swal.fire({

            icon: "warning",

            title: "Quá lớn",

            text: "CV tối đa 5MB"

        });

        fileInput.value = "";

        return;

    }

    showFile(file);

}

//===============================
// Hiển thị file
//===============================

function showFile(file) {

    fileInfo.style.display = "block";

    fileName.innerHTML = file.name;

    fileSize.innerHTML = formatSize(file.size);

}

//===============================
// Format Size
//===============================

function formatSize(bytes) {

    if (bytes < 1024)
        return bytes + " B";

    if (bytes < 1024 * 1024)
        return (bytes / 1024).toFixed(2) + " KB";

    return (bytes / (1024 * 1024)).toFixed(2) + " MB";

}
//===============================
// Progress giả lập
//===============================

const form = document.querySelector("form");

form.addEventListener("submit", function () {

    let progress = 0;

    let bar =
        document.getElementById("progressBar");

    let timer = setInterval(function () {

        progress += 10;

        bar.style.width = progress + "%";

        bar.innerHTML = progress + "%";

        if (progress >= 100) {

            clearInterval(timer);

        }

    }, 100);

});