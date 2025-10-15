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


const apiBase = 'https://localhost:44368';

async function loadDropdownMaster() {
    const response = await fetch(`${apiBase}/api/DropdownMaster`);
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
                  <td>${p.name}</td>
                  <td>${p.value}</td>
                  <td class="text-center">
                       <button class="btn-icon border-0 me-2" style="color:#1b3f6f;"
                               onclick="editDropdown('${p.id}', '${p.name}', '${p.value}')">
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
                if (seen.has(p.name)) return false;
                seen.add(p.name);
                return true;
            })
            .sort((a, b) => a.name.localeCompare(b.name));

        dropdownMenu.empty();

        if (items.length === 0) {
            dropdownMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                dropdownMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.name}">${p.name}</a></li>
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
                if (seen.has(p.name)) return false;
                seen.add(p.name);
                return true;
            })
            .sort((a, b) => a.name.localeCompare(b.name));

        updatedropdownMenu.empty();

        if (items.length === 0) {
            updatedropdownMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                updatedropdownMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.name}">${p.name}</a></li>
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
                if (seen.has(p.name)) return false;
                seen.add(p.name);
                return true;
            })
            .sort((a, b) => a.name.localeCompare(b.name));

        dropdownMenu.empty();

        if (items.length === 0) {
            dropdownMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                dropdownMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.name}">${p.name}</a></li>
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
                if (seen.has(p.name)) return false;
                seen.add(p.name);
                return true;
            })
            .sort((a, b) => a.name.localeCompare(b.name));

        dropdownMenu.empty();

        if (items.length === 0) {
            dropdownMenu.append(`<li><span class="dropdown-item-text text-muted">No options found</span></li>`);
        } else {
            items.forEach(p => {
                dropdownMenu.append(`
                    <li><a class="dropdown-item" href="#" data-value="${p.name}">${p.name}</a></li>
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

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}
console.log("UserRoleId from cookie:", getCookie("UserRoleId"));


async function saveDropdown(name, value) {
    const now = new Date().toISOString();

    // get userroleid from cookie
    const userRoleId = getCookie("UserRoleId");

    const model = {
        id: 0,                       // new item
        name: name,                  // display name
        value: value,        // fallback to name if value not provided
        isActive: true,
        createdBy: userRoleId ? parseInt(userRoleId) : 0,
        createdDateTime: now,
        updatedBy: null,             // explicitly null
        updatedDateTime: null        // explicitly null
    };

    console.log(model);

    const response = await fetch(`${apiBase}/api/DropdownMaster`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(model)
    });

    if (!response.ok) throw new Error("Save failed: " + response.status);
    return await response.json();
}



$(document).ready(function () {

    // Save button handler
    $("#btnSave").on("click", async function () {
        const name = $("#dropdownInput").val().trim();
        const value = $("#value").val().trim();

        if (!name) {
            alert("Please enter or select a name");
            return;
        }
        if (!value) {
            alert("Please enter a value");
            return;
        }

        try {
            await saveDropdown(name, value);
            alert("Saved successfully!");

            window.location.reload();
            if (typeof showSection === 'function') showSection('dropdownListSection');
            // Optional: clear fields
            $("#dropdownInput").val("");
            $("#value").val("");

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
        $("#value").val("");
    });
});

$("#btnCancel").on("click", function () {
    $("#dropdownInput").val("");
    $("#value").val("");
    showSection('dropdownListSection'); // go back to list
});


// When clicking edit button in table
function editDropdown(id, name, value) {
    $("#updateId").val(id);
    $("#updateDropdownInput").val(name);
    $("#updateValue").val(value);

    showSection('updateDropdownSection');
    refreshUpdateDropdown();
}

// Save updated record
async function updateDropdown(id, name, value) {
    const now = new Date().toISOString();
    const userRoleId = getCookie("UserRoleId");

    const model = {
        id: id,
        name: name,
        value: value,
        isActive: true,
        updatedBy: userRoleId ? parseInt(userRoleId) : 0,
        updatedDateTime: now
    };

    const response = await fetch(`${apiBase}/api/DropdownMaster`, {
        method: "POST", // your backend merges insert/update
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(model)
    });

    if (!response.ok) throw new Error("Update failed: " + response.status);
    return await response.json();
}

$(document).ready(function () {
    // Update button handler
    $("#btnUpdate").on("click", async function () {
        const id = $("#updateId").val();
        const name = $("#updateDropdownInput").val().trim();
        const value = $("#updateValue").val().trim();

        if (!name || !value) {
            alert("Please fill all fields");
            return;
        }

        try {
            await updateDropdown(id, name, value);
            alert("Updated successfully!");

            showSection('dropdownListSection');
            await refreshDropdown();
            window.location.reload();

        } catch (err) {
            console.error("Error updating:", err);
            alert("Failed to update: " + err.message);
        }
    });

    // Cancel update
    $("#btnUpdateCancel").on("click", function () {
        $("#updateId").val("");
        $("#updateDropdownInput").val("");
        $("#updateValue").val("");
        showSection('dropdownListSection');
    });
});


let dropdownIdToDelete = null; // store id temporarily

// Open modal instead of confirm()
function openDeleteModal(id) {
    dropdownIdToDelete = id;
    const modal = new bootstrap.Modal(document.getElementById("deleteConfirmModal"));
    modal.show();
}

// On confirm button click
document.getElementById("confirmDeleteBtn").addEventListener("click", async function () {
    if (!dropdownIdToDelete) return;
    const userRoleId = getCookie("UserRoleId");
    // Make sure you have userRoleId defined somewhere
    const updatedBy = userRoleId ? parseInt(userRoleId) : 0;

    if (updatedBy <= 0) {
        alert("Invalid user role.");
        return;
    }

    try {
        const response = await fetch(`${apiBase}/api/DropdownMaster/${dropdownIdToDelete}?updatedBy=${updatedBy}`, {
            method: "DELETE"
        });

        if (response.ok) {
            // Close modal
            bootstrap.Modal.getInstance(document.getElementById("deleteConfirmModal")).hide();
            alert("Dropdown deleted successfully.");
            location.reload();
        } else {
            const data = await response.json();
            alert("Failed to delete dropdown: " + data.message);
        }
    } catch (error) {
        alert("Error deleting dropdown: " + error.message);
    }

    dropdownIdToDelete = null;
});