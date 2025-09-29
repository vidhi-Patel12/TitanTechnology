function showSection(sectionId) {
    // Hide all
    document.getElementById("serviceListSection").style.display = "none";
    document.getElementById("addServiceSection").style.display = "none";
    document.getElementById("updateServiceSection").style.display = "none";

    // Show the requested one
    document.getElementById(sectionId).style.display = "block";
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

const userRoleId = getCookie("UserRoleId");

const apiBase = 'https://localhost:44368';

document.addEventListener("DOMContentLoaded", async () => {
    const tableBody = document.getElementById("serviceTableBody");

    try {
        const response = await fetch(`${apiBase}/api/Service`);

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const services = await response.json();

        // Clear old rows
        tableBody.innerHTML = "";

        // Populate table
        services.forEach(service => {
            const shortDesc = service.description.length > 100 ?
                service.description.substring(0, 100) + "..." :
                service.description;

            const row = document.createElement("tr");
            row.innerHTML = `
        <td>${service.serviceName}</td>
        <td title="${service.description.replace(/"/g, '&quot;')}">${shortDesc}</td>
        <td class="text-center">
        
            <button type="button" class="btn-icon border-0 text-secondary me-2"
              onclick="getbyidService(${service.serviceId})" title="View">
              <i class="fas fa-eye fa-lg"></i>
            </button>

            <button type="button" class="btn-icon border-0 me-2" style="color: #1b3f6f;"
                    onclick="loadService(${service.serviceId})" title="Edit">
              <i class="fas fa-edit fa-lg"></i>
            </button>

            <button type="button" class="btn-icon border-0 text-danger"
                    onclick="openDeleteModal(${service.serviceId})" title="Delete">
              <i class="fas fa-trash-alt fa-lg"></i>
            </button>


        </td>
      `;
            tableBody.appendChild(row);
        });

        //  Initialize DataTable (Bootstrap 5 styling)
        if ($.fn.DataTable.isDataTable("#serviceTable")) {
            $("#serviceTable").DataTable().destroy();
        }

        $("#serviceTable").DataTable({
            pageLength: 5,
            lengthChange: true,
            searching: true,
            ordering: true,
            info: true,
            autoWidth: false,
            language: {
                search: " Search:",
                lengthMenu: "Show _MENU_ entries"
            }
        });

    } catch (error) {
        tableBody.innerHTML = `
      <tr><td colspan="3" class="text-danger text-center">
        Failed to load services: ${error.message}
      </td></tr>
    `;
        console.error("Error fetching services:", error);
    }
});



// Cookie reader function
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(";").shift();
}

// Preview image
document.getElementById("imageFile").addEventListener("change", function () {
    const file = this.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = e => {
            const preview = document.getElementById("previewImage");
            preview.src = e.target.result;
            preview.style.display = "block";
        };
        reader.readAsDataURL(file);
    }
});

// Format datetime → `2025-09-03T16:10:26.415Z`
function formatDateTimeISO(date) {
    return date.toISOString();
}

const fileInput = document.getElementById("imageFile");
const filePlaceholder = document.getElementById("filePlaceholder");
const fileMessage = document.getElementById("fileMessage");
const previewImage = document.getElementById("previewImage");

fileInput.addEventListener("change", function () {
    const file = this.files[0];
    previewImage.src = "default-image.png";
    filePlaceholder.style.display = "block";
    fileMessage.style.display = "none";

    if (!file) {
        fileMessage.style.display = "block";
        return;
    }

    // Hide placeholder if file exists
    filePlaceholder.style.display = "none";

    // Validate file type
    if (file.type !== "image/png") {
        fileMessage.textContent = "Only PNG images are allowed.";
        fileInput.value = "";
        fileMessage.style.display = "block";
        filePlaceholder.style.display = "block";
        return;
    }

    // Validate dimensions
    const img = new Image();
    img.src = URL.createObjectURL(file);
    img.onload = function () {
        if (img.width > 1000 || img.height > 1000) {
            fileMessage.textContent = "Image must be 1000×1000 px or smaller and png.";
            fileInput.value = "";
            filePlaceholder.style.display = "block";
            fileMessage.style.display = "block";
            previewImage.src = "default-image.png";
        } else {
            previewImage.src = img.src; // valid preview
            fileMessage.style.display = "none";
        }
    };
});


// Submit form
document.getElementById("serviceForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    const userRoleId = getCookie("UserRoleId"); // userId from cookie
    console.log("Creating service by user with role:", userRoleId);

    const now = formatDateTimeISO(new Date());

    const formData = new FormData();
    formData.append("ServiceId", 0);
    formData.append("ServiceName", document.getElementById("serviceName").value);
    formData.append("Description", document.getElementById("description").value);

    // Send only filename for Image (API will save actual file separately)
    const fileInput = document.getElementById("imageFile");
    if (fileInput.files[0]) {
        formData.append("Image", fileInput.files[0].name);
        formData.append("imageFile", fileInput.files[0]); // binary
    } else {
        formData.append("Image", "");
    }

    formData.append("IsActive", true);
    formData.append("CreatedBy", userRoleId);
    formData.append("CreatedDate", now);
    //formData.append("UpdatedBy", null);
    //formData.append("UpdatedDate", null);

    try {
        const response = await fetch(`${apiBase}/api/Service/Post`, {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            const data = await response.json();
            document.getElementById("result").innerHTML =
                `<div class=""> </div>`;
            this.reset();
            location.reload();
            showSection("serviceListSection");

        } else {
            const errorText = await response.text();
            document.getElementById("result").innerHTML =
                `<div class="alert alert-danger"> Error: ${errorText}</div>`;
        }
    } catch (err) {
        document.getElementById("result").innerHTML =
            `<div class="alert alert-danger"> API Error: ${err.message}</div>`;
    }
});



// Delete function

let serviceIdToDelete = null; // store id temporarily

// Open modal instead of confirm()
function openDeleteModal(serviceId) {
    serviceIdToDelete = serviceId;
    const modal = new bootstrap.Modal(document.getElementById("deleteConfirmModal"));
    modal.show();
}

// On confirm button click
document.getElementById("confirmDeleteBtn").addEventListener("click", async function () {
    if (!serviceIdToDelete) return;
    const userRoleId = getCookie("UserRoleId");
    // Make sure you have userRoleId defined somewhere
    const updatedBy = userRoleId ? parseInt(userRoleId) : 0;

    if (updatedBy <= 0) {
        alert("Invalid user role.");
        return;
    }

    try {
        const response = await fetch(`${apiBase}/api/Service/${serviceIdToDelete}?updatedBy=${updatedBy}`, {
            method: "DELETE"
        });

        if (response.ok) {
            // Close modal
            bootstrap.Modal.getInstance(document.getElementById("deleteConfirmModal")).hide();
            alert("Service deleted successfully.");
            location.reload();
        } else {
            const data = await response.json();
            alert("Failed to delete service: " + data.message);
        }
    } catch (error) {
        alert("Error deleting service: " + error.message);
    }

    serviceIdToDelete = null;
});





async function deleteService(id) {
    if (!confirm("Are you sure you want to delete this service?")) return;

    try {
        const response = await fetch(`${apiBase}/api/Service/${id}`, {
            method: "DELETE"
        });

        if (response.ok) {
            alert("Service deleted successfully.");
            location.reload(); // refresh table
        } else {
            alert("Failed to delete service.");
        }
    } catch (error) {
        alert("Error deleting service: " + error.message);
    }
}



// Cookie reader
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(";").shift();
}

// Format datetime
function formatDateTimeISO(date) {
    return date.toISOString();
}

// Load existing service by ID
async function loadService(serviceId) {
    try {
        const response = await fetch(`${apiBase}/api/Service/${serviceId}`);
        if (response.ok) {
            const service = await response.json();

            // Fill form fields
            document.getElementById("serviceId").value = service.serviceId;
            document.getElementById("updateServiceName").value = service.serviceName;
            document.getElementById("updateDescription").value = service.description;

            // Handle image preview
            if (service.image) {
                // Extract filename from path (works for Windows and Linux paths)
                const fileName = service.image.split("\\").pop().split("/").pop();

                // Construct public URL (served from wwwroot/uploads/)
                const imageUrl = `${apiBase}/uploads/${fileName}`;

                // Show preview
                const previewImg = document.getElementById("updatePreviewImage");
                previewImg.src = imageUrl;
                previewImg.style.display = "block";

                // Store relative path (so backend can keep it if unchanged)
                document.getElementById("updateImage").value = `uploads/${fileName}`;
                document.getElementById("currentImageName").textContent = fileName;
            } else {
                document.getElementById("updatePreviewImage").style.display = "none";
                document.getElementById("currentImageName").textContent = "";
                document.getElementById("updateImage").value = "";
            }

            // Show update form
            showSection("updateServiceSection");
        } else {
            document.getElementById("updateResult").innerHTML =
                `<div class="alert alert-danger">Failed to load service</div>`;
        }
    } catch (err) {
        document.getElementById("updateResult").innerHTML =
            `<div class="alert alert-danger"> API Error: ${err.message}</div>`;
    }
}

// Handle new image preview
document.getElementById("updateImageFile").addEventListener("change", function () {
    const file = this.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = e => {
            document.getElementById("updatePreviewImage").src = e.target.result;
            document.getElementById("updatePreviewImage").style.display = "block";
            document.getElementById("currentImageName").textContent = file.name;

            // Replace hidden value with new file path
            document.getElementById("updateImage").value = "uploads/" + file.name;
        };
        reader.readAsDataURL(file);
    }
});

const updateFileInput = document.getElementById("updateImageFile");
const updateFilePlaceholder = document.getElementById("updateFilePlaceholder");
const updateFileMessage = document.getElementById("updateFileMessage");
const updatePreviewImage = document.getElementById("updatePreviewImage");
const currentImageName = document.getElementById("currentImageName");

updateFileInput.addEventListener("change", function () {
    const file = this.files[0];

    // Reset preview and messages
    updatePreviewImage.style.display = "none";
    updateFileMessage.style.display = "none";
    updateFilePlaceholder.style.display = "block";

    if (!file) return;

    // Hide placeholder when file is selected
    updateFilePlaceholder.style.display = "none";

    // Validate file type
    if (file.type !== "image/png") {
        updateFileMessage.textContent = "Only PNG images are allowed.";
        updateFileMessage.style.display = "block";
        this.value = ""; // clear invalid file
        updateFilePlaceholder.style.display = "block";
        return;
    }

    // Validate dimensions
    const img = new Image();
    img.src = URL.createObjectURL(file);
    img.onload = function () {
        if (img.width > 1000 || img.height > 1000) {
            updateFileMessage.textContent = "Image must be 1000×1000 px or smaller and PNG.";
            updateFileMessage.style.display = "block";
            updateFilePlaceholder.style.display = "block";
            updateFileInput.value = "";
            updatePreviewImage.src = "";
            updatePreviewImage.style.display = "none";
        } else {
            // Valid file: show preview and current file name
            updatePreviewImage.src = img.src;
            updatePreviewImage.style.display = "block";
            currentImageName.textContent = file.name;
            updateFileMessage.style.display = "none";
        }
    };
});


// Submit update form
document.getElementById("updateForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    const userRoleId = getCookie("UserRoleId");
    const now = formatDateTimeISO(new Date());

    const formData = new FormData();
    formData.append("ServiceId", document.getElementById("serviceId").value);
    formData.append("ServiceName", document.getElementById("updateServiceName").value);
    formData.append("Description", document.getElementById("updateDescription").value);

    const newFile = document.getElementById("updateImageFile").files[0];
    if (newFile) {
        // If user picked a new image
        formData.append("Image", "uploads/" + newFile.name);
        formData.append("imageFile", newFile);
    } else {
        // No new image → keep old path
        const oldImagePath = document.getElementById("updateImage").value;
        formData.append("Image", oldImagePath);

        //  Trick: send dummy empty file so API validation passes
        const dummy = new Blob([], {
            type: "application/octet-stream"
        });
        formData.append("imageFile", dummy, "empty.txt");
    }

    formData.append("IsActive", true);
    formData.append("UpdatedBy", userRoleId);
    formData.append("UpdatedDate", now);

    try {
        const response = await fetch(`${apiBase}/api/Service/Update`, {
            method: "PUT",
            body: formData
        });

        if (response.ok) {
            const data = await response.json();
            document.getElementById("updateResult").innerHTML =
                `<div></div>`;
            this.reset();
            location.reload();
            await loadServices();
            showSection("serviceListSection");
        } else {
            const errorText = await response.text();
            document.getElementById("updateResult").innerHTML =
                `<div class="alert alert-danger"> Error: ${errorText}</div>`;
        }
    } catch (err) {
        document.getElementById("updateResult").innerHTML =
            `<div class="alert alert-danger"> API Error: ${err.message}</div>`;
    }
});

// Reusable function
async function loadServices() {
    const tableBody = document.getElementById("serviceTableBody");

    try {
        const response = await fetch(`${apiBase}/api/Service`);
        if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

        const services = await response.json();
        tableBody.innerHTML = "";

        services.forEach(service => {
            const shortDesc = service.description.length > 100 ?
                service.description.substring(0, 100) + "..." :
                service.description;

            const row = document.createElement("tr");
            row.innerHTML = `
        <td>${service.serviceName}</td>
        <td>
          <span title="${service.description.replace(/"/g, '&quot;')}">
            ${shortDesc}
          </span>
        </td>
        <td>
           <button class="btn btn-link btn-sm text-warning me-2 p-0"
            onclick="loadService(${service.serviceId})" title="Edit">
      <i class="fas fa-edit"></i>
    </button>
    <button class="btn btn-link btn-sm text-danger p-0"
            onclick="deleteService(${service.serviceId})" title="Delete">
      <i class="fas fa-trash-alt"></i>
    </button>
        </td>
      `;
            tableBody.appendChild(row);
        });

        // Initialize DataTable (destroy first if reloading)
        if ($.fn.DataTable.isDataTable('#serviceTable')) {
            $('#serviceTable').DataTable().destroy();
        }
        $('#serviceTable').DataTable({
            pageLength: 10,
            lengthMenu: [5, 10, 25, 50],
            ordering: true,
            searching: true,
            paging: true
        });

    } catch (error) {
        tableBody.innerHTML = `
      <tr><td colspan="3" class="text-danger text-center">
        Failed to load services: ${error.message}
      </td></tr>
    `;
        console.error("Error fetching services:", error);
    }
}


// Load services on first page load
document.addEventListener("DOMContentLoaded", loadServices);

async function getbyidService(serviceId) {
    try {
        const response = await fetch(`${apiBase}/api/Service/${serviceId}`);
        if (!response.ok) throw new Error("Failed to fetch service");

        const service = await response.json();

        // Fill modal content
        document.getElementById("viewServiceName").textContent = service.serviceName;
        document.getElementById("viewServiceDescription").textContent = service.description;

        // Handle image
        if (service.image) {
            const fileName = service.image.split("\\").pop().split("/").pop();
            document.getElementById("viewServiceImage").src = `${apiBase}/uploads/${fileName}`;
        } else {
            document.getElementById("viewServiceImage").src = "https://via.placeholder.com/250?text=No+Image";
        }

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById("viewServiceModal"));
        modal.show();

    } catch (err) {
        console.error("Error fetching service details:", err);
        alert("Could not load service details.");
    }
}

const btn = document.getElementById("quoteBtn");
const popup = document.getElementById("popup");

btn.addEventListener("click", () => {
    popup.classList.toggle("show");
});

// Optional: close popup if you click outside
document.addEventListener("click", (e) => {
    if (!btn.contains(e.target) && !popup.contains(e.target)) {
        popup.classList.remove("show");
    }
});