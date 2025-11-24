function showSection(sectionId) {
    // Hide all known sections if they exist
    const sections = ["dropdownListSection", "addDropdownSection", "updateDropdownSection"];
    sections.forEach(id => {
        const el = document.getElementById(id);
        if (el) el.style.display = "none";
    });

    // Show requested section
    const target = document.getElementById(sectionId);
    if (target) target.style.display = "block";
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}
console.log("UserRoleId from cookie:", getCookie("UserRoleId"));

const token = getCookie("AuthToken");
console.log("Token:", token);

const apiBase = 'https://api.titentechnology.com';

async function loadDropdownMaster() {
    //const response = await fetch(`${apiBase}/api/UserRole`);

    const response = await fetch(`/Admin/GetUserRoles`, {
        method: 'GET',
        credentials: 'include',
        headers: {
            "Accept": "application/json"
        }
    });

    if (!response.ok) throw new Error("HTTP " + response.status);
    return await response.json();
}

$(document).ready(async function () {
    // ----- TABLE -----
    const tableBody = $("#dropdownTableBody");
    try {
        const projects = await loadDropdownMaster();
        tableBody.empty();

        projects.forEach(p => {
            tableBody.append(`
                <tr>
                  <td>${p.roleName}</td>
                  <td class="text-center">
                       <button class="btn-icon border-0 me-2" style="color:#1b3f6f;"
                               onclick="editDropdown('${p.id}', '${p.roleName}')">
                           <i class="fas fa-edit fa-lg"></i>
                       </button>
                       <button type="button" class="btn-icon border-0 me-2 text-danger"
                                onclick="openDeleteModal(${p.id})" title="Delete">
                          <i class="fas fa-trash-alt fa-lg"></i>
                        </button>
                       
                  </td>
                </tr>
            `);
        });

        if ($.fn.dataTable.isDataTable("#dropdownTable")) {
            $("#dropdownTable").DataTable().clear().destroy();
        }
        $("#dropdownTable").DataTable({
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
    const dropdownMenu = $("#dynamicDropdown");
    try {
        let items = await loadDropdownMaster();

        // distinct + sorted
        const seen = new Set();
        items = items
            .filter(p => {
                if (seen.has(p.roleName)) return false;
                seen.add(p.roleName);
                return true;
            })
            .sort((a, b) => a.roleName.localeCompare(b.roleName));

        dropdownMenu.empty();

        if (items.length === 0) {
            dropdownMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                dropdownMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.roleName}">${p.roleName}</a></li>
                `);
            });
        }

        // Fill input on select
        $(document).on("click", "#dynamicDropdown .dropdown-item", function (e) {
            e.preventDefault();
            $("#dropdownInput").val($(this).text());
        });

    } catch (err) {
        console.error("Error:", err);
        dropdownMenu.html(`<li><span class="dropdown-item-text text-danger">Failed: ${err.message}</span></li>`);
    }

    const updatedropdownMenu = $("#updatedynamicDropdown");
    try {
        let items = await loadDropdownMaster();

        // distinct + sorted
        const seen = new Set();
        items = items
            .filter(p => {
                if (seen.has(p.roleName)) return false;
                seen.add(p.roleName);
                return true;
            })
            .sort((a, b) => a.roleName.localeCompare(b.roleName));

        updatedropdownMenu.empty();

        if (items.length === 0) {
            updatedropdownMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                updatedropdownMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.roleName}">${p.roleName}</a></li>
                `);
            });
        }

        // Fill input on select
        $(document).on("click", "#updateDynamicDropdown .dropdown-item", function (e) {
            e.preventDefault();
            $("#updateDropdownInput").val($(this).text());
        });

    } catch (err) {
        console.error("Error:", err);
        updatedropdownMenu.html(`<li><span class="dropdown-item-text text-danger">Failed: ${err.message}</span></li>`);
    }
});

async function refreshDropdown() {
    const dropdownMenu = $("#dynamicDropdown");
    try {
        let items = await loadDropdownMaster();

        // distinct + sorted
        const seen = new Set();
        items = items
            .filter(p => {
                if (seen.has(p.roleName)) return false;
                seen.add(p.roleName);
                return true;
            })
            .sort((a, b) => a.roleName.localeCompare(b.roleName));

        dropdownMenu.empty();

        if (items.length === 0) {
            dropdownMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                dropdownMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.roleName}">${p.roleName}</a></li>
                `);
            });
        }

        // Fill input on select
        $(document).on("click", "#dynamicDropdown .dropdown-item", function (e) {
            e.preventDefault();
            $("#dropdownInput").val($(this).text());
        });

    } catch (err) {
        console.error("Error:", err);
        dropdownMenu.html(`<li><span class="dropdown-item-text text-danger">Failed: ${err.message}</span></li>`);
    }
}

async function refreshUpdateDropdown() {
    const dropdownMenu = $("#updateDynamicDropdown");
    try {
        let items = await loadDropdownMaster();

        // distinct + sorted
        const seen = new Set();
        items = items
            .filter(p => {
                if (seen.has(p.roleName)) return false;
                seen.add(p.roleName);
                return true;
            })
            .sort((a, b) => a.roleName.localeCompare(b.roleName));

        dropdownMenu.empty();

        if (items.length === 0) {
            dropdownMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                dropdownMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.roleName}">${p.roleName}</a></li>
                `);
            });
        }

        // Fill input on select
        $(document).on("click", "#updateDynamicDropdown .dropdown-item", function (e) {
            e.preventDefault();
            $("#updateDropdownInput").val($(this).text());
        });

    } catch (err) {
        console.error("Error:", err);
        dropdownMenu.html(`<li><span class="dropdown-item-text text-danger">Failed: ${err.message}</span></li>`);
    }
}



async function saveDropdown(roleName) {
    const now = new Date().toISOString();

    // get userroleid from cookie
    const userRoleId = getCookie("UserRoleId");

    const model = {
        id: 0,                       // new item
        roleName: roleName,                  // display roleName
    };

    console.log(model);

    const response = await fetch(`/Admin/SaveUserRole`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        credentials: "include", // sends cookies like AuthToken
        body: JSON.stringify(model)
    });

    if (!response.ok) throw new Error("Save failed: " + response.status);
    return await response.json();
}



$(document).ready(function () {

    // Save button handler
    $("#btnSave").on("click", async function () {
        const roleName = $("#dropdownInput").val().trim();

        if (!roleName) {
            alert("Please enter or select a roleName");
            return;
        }
       
        try {
            await saveDropdown(roleName);
            //alert("Saved successfully!");

            window.location.reload();
            if (typeof showSection === 'function') showSection('dropdownListSection');
            // Optional: clear fields
            $("#dropdownInput").val("");
            
            // Reload dropdown after save
            await refreshDropdown();

        } catch (err) {
            console.error("Error saving:", err);
            alert("Failed to save: " + err.message);
        }
    });

    // Cancel button handler
    $("#btnCancel").on("click", function () {
        $("#dropdownInput").val("");
    });
});

$("#btnCancel").on("click", function () {
    $("#dropdownInput").val("");
    showSection('dropdownListSection'); // go back to list
});


// When clicking edit button in table
function editDropdown(id, roleName, value) {
    $("#updateId").val(id);
    $("#updateDropdownInput").val(roleName);

    showSection('updateDropdownSection');
    refreshUpdateDropdown();
}

// Save updated record
async function updateDropdown(id, roleName) {
    const now = new Date().toISOString();
    const userRoleId = getCookie("UserRoleId");

    const model = {
        id: id,
        roleName: roleName,
    };

    const response = await fetch(`/Admin/SaveUserRole`, {
        method: "POST", // same endpoint handles insert/update
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        credentials: "include",
        body: JSON.stringify(model)
    });

    if (!response.ok) throw new Error("Update failed: " + response.status);
    return await response.json();
}

$(document).ready(function () {
    // Update button handler
    $("#btnUpdate").on("click", async function () {
        const id = $("#updateId").val();
        const roleName = $("#updateDropdownInput").val().trim();

        if (!roleName) {
            alert("Please fill all fields");
            return;
        }

        try {
            await updateDropdown(id, roleName);
            //alert("Updated successfully!");

            showSection('dropdownListSection');
            await refreshDropdown();
            window.location.reload();

        } catch (err) {
            console.error("Error updating:", err);
            //alert("Failed to update: " + err.message);
        }
    });

    // Cancel update
    $("#btnUpdateCancel").on("click", function () {
        $("#updateId").val("");
        $("#updateDropdownInput").val("");
        showSection('dropdownListSection');
    });
});


let userroleIdToDelete = null; // store id temporarily

// Open modal instead of confirm()
function openDeleteModal(id) {
    userroleIdToDelete = id;
    const modal = new bootstrap.Modal(document.getElementById("deleteConfirmModal"));
    modal.show();
}

// On confirm button click
document.getElementById("confirmDeleteBtn").addEventListener("click", async function () {
    if (!userroleIdToDelete) return;

    const userRoleId = getCookie("UserRoleId");
    const token = getCookie("AuthToken"); // read token from cookie

    try {
        const response = await fetch(`/Admin/DeleteUserRole/${userroleIdToDelete}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`,  // attach token like SaveDropdown
                "Content-Type": "application/json"
            }
        });

        if (response.ok) {
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById("deleteConfirmModal"));
            if (modal) modal.hide();

            // Refresh page
            location.reload();
        } else {
            const data = await response.json();
            alert("Failed to delete role: " + (data.message || "Unknown error"));
        }
    } catch (error) {
        alert("Error deleting role: " + error.message);
    }

    userroleIdToDelete = null;
});
