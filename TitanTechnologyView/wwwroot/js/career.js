function showSection(sectionId) {
    // Hide all known sections if they exist
    const sections = ["careerListSection", "addCareerSection", "updateCareerSection"];
    sections.forEach(id => {
        const el = document.getElementById(id);
        if (el) el.style.display = "none";
    });

    // Show requested section
    const target = document.getElementById(sectionId);
    if (target) target.style.display = "block";
}


const apiBase = 'https://localhost:44368';

async function loadCareerMaster() {
    const response = await fetch(`${apiBase}/api/Career`);
    if (!response.ok) throw new Error("HTTP " + response.status);
    return await response.json();
}

$(document).ready(async function () {
    // ----- TABLE -----
    const tableBody = $("#careerTableBody");
    try {
        const careers = await loadCareerMaster();
        tableBody.empty();

        careers.forEach(p => {
            const shortDesc = p.jobDescription.length > 100 ?
                p.jobDescription.substring(0, 100) + "..." :
                p.jobDescription;

            tableBody.append(`
                <tr>
                  <td>${p.employementtype}</td>
                  <td>${p.location}</td>
                  <td>${p.jobTitle}</td>
                  <td title="${p.jobDescription.replace(/"/g, '&quot;')}">${shortDesc}</td>

                  <td class="text-center">
                      <button class="btn-icon border-0 me-2" style="color:#1b3f6f;"
                        onclick="getbyidCareer('${p.careerId}')">
                        <i class="fas fa-eye fa-lg"></i>
                      </button>

                        <button class="btn-icon border-0 me-2" style="color:#1b3f6f;"
                               onclick="editCareer('${p.careerId}', '${p.employementtype}','${p.location}', '${p.jobTitle}','${p.jobDescription}')">
                           <i class="fas fa-edit fa-lg"></i>
                       </button>
                       <button type="button" class="btn-icon border-0 me-2 text-danger"
                                onclick="openDeleteModal(${p.careerId})" title="Delete">
                          <i class="fas fa-trash-alt fa-lg"></i>
                        </button>
                       
                  </td>
                </tr>
            `);
        });

        if ($.fn.dataTable.isDataTable("#careerTable")) {
            $("#careerTable").DataTable().clear().destroy();
        }
        $("#careerTable").DataTable({
            pageLength: 10,
            language: {
                search: " Search:",
                lengthMenu: "Show _MENU_ entries"
            }
        });

    } catch (err) {
        console.error("Error:", err);
        tableBody.html(`<tr><td colspan="3" class="text-danger text-center">Failed to load: ${err.message}</td></tr>`);
    }



    // ----- DROPDOWN -----
    const careerMenu = $("#dynamicCareer");
    try {
        let items = await loadCareerMaster();

        // distinct + sorted
        const seen = new Set();
        items = items
            .filter(p => {
                if (seen.has(p.employementtype)) return false;
                seen.add(p.employementtype);
                return true;
            })
            .sort((a, b) => a.employementtype.localeCompare(b.employementtype));

        careerMenu.empty();

        if (items.length === 0) {
            careerMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                careerMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.employementtype}">${p.employementtype}</a></li>
                `);
            });
        }

        // Fill input on select
        $(document).on("click", "#dynamicCareer .dropdown-item", function (e) {
            e.preventDefault();
            $("#careerInput").val($(this).text());
        });

    } catch (err) {
        console.error("Error:", err);
        careerMenu.html(`<li><span class="dropdown-item-text text-danger">Failed: ${err.message}</span></li>`);
    }

    const updatecareerMenu = $("#updatedynamicCareer");
    try {
        let items = await loadCareerMaster();

        // distinct + sorted
        const seen = new Set();
        items = items
            .filter(p => {
                if (seen.has(p.employementtype)) return false;
                seen.add(p.employementtype);
                return true;
            })
            .sort((a, b) => a.employementtype.localeCompare(b.employementtype));

        updatecareerMenu.empty();

        if (items.length === 0) {
            updatecareerMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                updatecareerMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.employementtype}">${p.employementtype}</a></li>
                `);
            });
        }

        // Fill input on select
        $(document).on("click", "#updateDynamicCareer .dropdown-item", function (e) {
            e.preventDefault();
            $("#updateCareerInput").val($(this).text());
        });

    } catch (err) {
        console.error("Error:", err);
        updatecareerMenu.html(`<li><span class="dropdown-item-text text-danger">Failed: ${err.message}</span></li>`);
    }
});

async function refreshCareer() {
    const careerMenu = $("#dynamicCareer");
    try {
        let items = await loadCareerMaster();

        // distinct + sorted
        const seen = new Set();
        items = items
            .filter(p => {
                if (seen.has(p.employementtype)) return false;
                seen.add(p.employementtype);
                return true;
            })
            .sort((a, b) => a.employementtype.localeCompare(b.employementtype));

        careerMenu.empty();

        if (items.length === 0) {
            careerMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                careerMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.employementtype}">${p.employementtype}</a></li>
                `);
            });
        }

        // Fill input on select
        $(document).on("click", "#dynamicCareer .dropdown-item", function (e) {
            e.preventDefault();
            $("#careerInput").val($(this).text());
        });

    } catch (err) {
        console.error("Error:", err);
        careerMenu.html(`<li><span class="dropdown-item-text text-danger">Failed: ${err.message}</span></li>`);
    }
}

async function refreshUpdateCareer() {
    const careerMenu = $("#updateDynamicCareer");
    try {
        let items = await loadCareerMaster();

        // distinct + sorted
        const seen = new Set();
        items = items
            .filter(p => {
                if (seen.has(p.employementtype)) return false;
                seen.add(p.employementtype);
                return true;
            })
            .sort((a, b) => a.employementtype.localeCompare(b.employementtype));

        careerMenu.empty();

        if (items.length === 0) {
            careerMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                careerMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.employementtype}">${p.employementtype}</a></li>
                `);
            });
        }

        // Fill input on select
        $(document).on("click", "#updateDynamicCareer .dropdown-item", function (e) {
            e.preventDefault();
            $("#updateCareerInput").val($(this).text());
        });

    } catch (err) {
        console.error("Error:", err);
        careerMenu.html(`<li><span class="dropdown-item-text text-danger">Failed: ${err.message}</span></li>`);
    }
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}
console.log("UserRoleId from cookie:", getCookie("UserRoleId"));


async function saveCareer(employementtype, location, jobTitle, jobDescription) {
    const now = new Date().toISOString();
    const userRoleId = getCookie("UserRoleId");

    const formData = new FormData();
    formData.append("CareerId", 0);
    formData.append("Employementtype", employementtype);
    formData.append("Location", location);
    formData.append("JobTitle", jobTitle);
    formData.append("JobDescription", jobDescription);
    formData.append("IsActive", true);
    formData.append("CreatedBy", userRoleId ? parseInt(userRoleId) : 0);
    formData.append("CreatedDate", now);
    formData.append("UpdatedBy", "");
    formData.append("UpdatedDate", "");

    const response = await fetch(`${apiBase}/api/Career/Post`, {
        method: "POST",
        body: formData //  No headers, browser sets boundary automatically
    });

    if (!response.ok) throw new Error("Save failed: " + response.status);
    return await response.json();
}




$(document).ready(function () {
    const form = document.getElementById("careerForm");

    // 🔹 Reusable function to validate one field
    function validateField(field) {
        const errorSpan = $(field).siblings(".field-validation");

        if (!field.value.trim()) {
            field.classList.add("is-invalid");
            if (errorSpan.length) {
                let msg = field.placeholder || "This field";
                errorSpan.text(msg + " is required");
            }
            return false;
        } else {
            field.classList.remove("is-invalid");
            if (errorSpan.length) errorSpan.text("");
            return true;
        }
    }

    // 🔹 Validate all required fields on Save
    $("#btnSave").on("click", async function (e) {
        let isValid = true;

        $(form).find("input[required], textarea[required]").each(function () {
            if (!validateField(this)) isValid = false;
        });

        if (!isValid) {
            e.preventDefault();
            return;
        }

        try {
            await saveCareer(
                $("#careerInput").val().trim(),
                $("#location").val().trim(),
                $("#jobTitle").val().trim(),
                $("#jobDescription").val().trim()
            );

            //alert("Saved successfully!");
            window.location.reload();

            if (typeof showSection === "function") showSection("careerListSection");

            form.reset();
            $(form).find(".field-validation").text("");
            $(form).find(".is-invalid").removeClass("is-invalid");

            await refreshCareer();
        } catch (err) {
            console.error("Error saving:", err);
            alert("Failed to save: " + err.message);
        }
    });

    // 🔹 Live validation on blur (when leaving a field)
    $(form).find("input[required], textarea[required]").on("blur", function () {
        validateField(this);
    });

    // Cancel button handler
    $("#btnCancel").on("click", function () {
        form.reset();
        $(form).find(".field-validation").text("");
        $(form).find(".is-invalid").removeClass("is-invalid");
    });
});


function editCareer(careerId, employementtype, location, jobTitle, jobDescription) {
    $("#updateCareerId").val(careerId);
    $("#updateCareerInput").val(employementtype);
    $("#updateLocation").val(location);
    $("#updateJobTitle").val(jobTitle);
    $("#updateJobDescription").val(jobDescription);

    showSection('updateCareerSection');
    refreshUpdateCareer();
}


async function updateCareer(careerId, employementtype, location, jobTitle, jobDescription) {
    const now = new Date().toISOString();
    const userRoleId = getCookie("UserRoleId");

    const formData = new FormData();
    formData.append("CareerId", careerId);  //  must match C# property
    formData.append("Employementtype", employementtype);
    formData.append("Location", location);
    formData.append("JobTitle", jobTitle);
    formData.append("JobDescription", jobDescription);
    formData.append("IsActive", true);
    formData.append("UpdatedBy", userRoleId ? parseInt(userRoleId) : 0);
    formData.append("UpdatedDate", now);

    const response = await fetch(`${apiBase}/api/Career/Update`, {
        method: "PUT",
        body: formData
    });

    if (!response.ok) throw new Error("Update failed: " + response.status);
    return await response.json();
}


$("#btnUpdate").on("click", async function () {
    const careerId = $("#updateCareerId").val();  // hidden field storing id
    const employementType = $("#updateCareerInput").val().trim();
    const location = $("#updateLocation").val().trim();
    const jobTitle = $("#updateJobTitle").val().trim();
    const jobDescription = $("#updateJobDescription").val().trim();

    if (!employementType || !location || !jobTitle || !jobDescription) {
        alert("All fields are required!");
        return;
    }

    try {
        await updateCareer(careerId, employementType, location, jobTitle, jobDescription);
        alert("Updated successfully!");
        window.location.reload();
        if (typeof showSection === 'function') showSection('careerListSection');
        await refreshCareer();
    } catch (err) {
        console.error("Error updating:", err);
        alert("Failed to update: " + err.message);
    }
});

$("#btnCancel").on("click", function () {
    $("#careerInput").val("");
    $("#location").val("");
    $("#jobTitle").val("");
    $("#jobDescription").val("");
    showSection('careerListSection'); // go back to list
});

$("#btnUpdateCancel").on("click", function () {
    $("#updateCareerInput").val("");
    $("#updateLocation").val("");
    $("#updateJobTitle").val("");
    $("#updateJobDescription").val("");
    showSection('careerListSection'); // go back to list
});

// Delete function
let careerIdToDelete = null; // store id temporarily

// Open modal instead of confirm()
function openDeleteModal(careerId) {
    careerIdToDelete = careerId;
    const modal = new bootstrap.Modal(document.getElementById("deleteConfirmModal"));
    modal.show();
}

// On confirm button click
document.getElementById("confirmDeleteBtn").addEventListener("click", async function () {
    if (!careerIdToDelete) return;
    const userRoleId = getCookie("UserRoleId");
    // Make sure you have userRoleId defined somewhere
    const updatedBy = userRoleId ? parseInt(userRoleId) : 0;

    if (updatedBy <= 0) {
        alert("Invalid user role.");
        return;
    }

    try {
        const response = await fetch(`${apiBase}/api/Career/${careerIdToDelete}?updatedBy=${updatedBy}`, {
            method: "DELETE"
        });

        if (response.ok) {
            // Close modal
            bootstrap.Modal.getInstance(document.getElementById("deleteConfirmModal")).hide();
            //alert("Career deleted successfully.");
            location.reload();
        } else {
            const data = await response.json();
            alert("Failed to delete career: " + data.message);
        }
    } catch (error) {
        alert("Error deleting career: " + error.message);
    }

    solutionIdToDelete = null;
});


async function deleteSolution(id) {
    if (!confirm("Are you sure you want to delete this career?")) return;

    try {
        const response = await fetch(`${apiBase}/api/Career/${id}`, {
            method: "DELETE"
        });

        if (response.ok) {
            alert("Career deleted successfully.");
            location.reload(); // refresh table
        } else {
            alert("Failed to delete career.");
        }
    } catch (error) {
        alert("Error deleting career: " + error.message);
    }
}


async function getbyidCareer(careerId) {
    try {
        const response = await fetch(`${apiBase}/api/Career/${careerId}`);
        console.log(careerId);
        if (!response.ok) throw new Error("Failed to fetch career");

        const career = await response.json();

        // Fill modal content
        document.getElementById("viewCareerEmployementType").textContent = career.employementtype;
        document.getElementById("viewCareerLocation").textContent = career.location;
        document.getElementById("viewCareerJobTitle").textContent = career.jobTitle;
        document.getElementById("viewCareerJobDescription").textContent = career.jobDescription;

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById("viewCareerModal"));
        modal.show();

    } catch (err) {
        console.error("Error fetching career details:", err);
        alert("Could not load career details.");
    }
}

const btn = document.getElementById("quoteBtn");
const popup = document.getElementById("popup");

btn.addEventListener("click", () => {
    popup.classList.toggle("show");
});

