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

// ====== Cookie Helper ======
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(";").shift();
    return null;
}

console.log("UserRoleId from cookie:", getCookie("UserRoleId"));
const token = getCookie("AuthToken");


const apiBase = 'https://api.titentechnology.com';
let dropdownIdToDelete = null;



// ====== Fetch Helper ======
//async function fetchJson(url) {
//    const res = await fetch(url, {
//        credentials: "include",
//        headers: { "Accept": "application/json" }
//    });
//    if (!res.ok) throw new Error("HTTP " + res.status);
//    return await res.json();
//}

// ====== Load Role-Permission Table ======

async function loadRolePermissionTable() {
    try {
        const response = await fetch(`/Admin/GetUserRolePermission`, {
            method: 'GET',
            credentials: 'include',
            headers: { "Accept": "application/json" }
        });

        if (!response.ok) throw new Error("HTTP " + response.status);
        const data = await response.json();

        const $tbody = $("#dropdownTableBody");
        $tbody.empty();

        const grouped = {};
        data.forEach(p => {
            const roleName = p.role?.roleName || '';
            const roleId = p.role?.id;
            if (!grouped[roleId]) grouped[roleId] = { roleName, permissions: [] };
            grouped[roleId].permissions.push(p.permission?.permissionName || '');
        });

        Object.keys(grouped).forEach(roleId => {
            const { roleName, permissions } = grouped[roleId];
            const permsStr = permissions.join(', ');
            const displayText = permsStr.length > 50 ? permsStr.slice(0, 50) + '...' : permsStr;

            $tbody.append(`
                <tr>
                    <td>${roleName}</td>
                    <td title="${permsStr.replace(/"/g, '&quot;')}">${displayText}</td>
                    <td>
                        <button class="btn btn-primary me-2" onclick="editRolePermissions(${roleId})">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn-icon border-0 me-2 text-danger"
                                onclick="openDeleteModal(${roleId})" title="Delete">
                          <i class="fas fa-trash-alt fa-lg"></i>
                        </button>
                    </td>
                </tr>
            `);
        });
    } catch (err) {
        console.error("Error loading table:", err);
        alert("Error loading role-permission data: " + err.message);
    }
}


// ====== Load Roles ======
async function loadRolesDropdown(selectSelector, selectedRoleId = null) {

    const response = await fetch(`/Admin/GetUserRolesForPermission`, {
        method: "GET",
        credentials: "include",
        headers: { "Accept": "application/json" }
    });

    const roles = await response.json();

    const $select = $(selectSelector);
    $select.empty();

    roles.forEach(r => {
        $select.append(new Option(r.roleName, r.id));
    });

    // enable Select2
    if ($.fn.select2) {
        if ($select.hasClass("select2-hidden-accessible")) {
            $select.select2("destroy");
        }

        $select.select2({
            placeholder: "Select role...",
            width: "100%",
            allowClear: true
        });
    }

    // preselect role
    if (selectedRoleId) {
        $select.val(selectedRoleId).trigger("change");
    }
}

// ====== Load Permissions ======
async function loadPermissionDropdown(selectSelector, preSelected = []) {

    try {
        const response = await fetch(`/Admin/GetAllRolePermission`, {
            method: "GET",
            credentials: "include",
            headers: { "Accept": "application/json" }
        });

        if (!response.ok) {
            throw new Error("Failed to load permissions");
        }

        const items = await response.json();

        // Ensure items is an array
        if (!Array.isArray(items)) {
            console.error("Invalid JSON received:", items);
            alert("Error: Permission API returned invalid format.");
            return;
        }

        const $select = $(selectSelector);
        $select.empty();

        // Add permission options
        items.forEach(p => {
            $select.append(new Option(p.permissionName, p.id));
        });

        // Apply select2
        if ($.fn.select2) {
            if ($select.hasClass("select2-hidden-accessible")) {
                $select.select2("destroy");
            }

            $select.select2({
                placeholder: "Select permissions...",
                width: "100%",
                closeOnSelect: false,
                allowClear: true
            });

            // Preselect existing permissions
            if (preSelected.length) {
                $select.val(preSelected).trigger("change");
            }
        }

    } catch (err) {
        console.error("Load Permission Error:", err);
        alert("Failed to load permissions: " + err.message);
    }
}

// ====== Utility ======
function getSelectedPermissionIds(selectSelector) {
    return $(selectSelector).val() || [];
}

// ====== Save Role Permissions ======
// ====== Save Role Permissions ======
async function saveRolePermissions() {
    const roleId = $("#roleSelect").val();
    const selected = getSelectedPermissionIds("#dropdownpermitMulti").map(Number);

    if (!roleId) return alert("Please select a role");
    if (!selected.length) return alert("Please select at least one permission");

    try {
        // ---- get existing ----
        const existingResponse = await fetch(`/Admin/GetUserPermission`, {
            method: "GET",
            credentials: "include",
            headers: { "Accept": "application/json" }
        });

        const existingData = await existingResponse.json();

        // ---- filter only NEW ----
        const newPermissions = selected.filter(permId =>
            !existingData.some(rp => {
                const existingRoleId = rp.role?.id ?? rp.roleId;
                const existingPermissionId = rp.permission?.id ?? rp.permissionId;
                return existingRoleId == roleId && existingPermissionId == permId;
            })
        );

        if (!newPermissions.length)
            return alert("All selected permissions already exist for this role.");

        // ---- API EXPECTS THIS JSON FORMAT ----
        const payload = {
            id: 0,
            roleId: parseInt(roleId),
            permissionIds: newPermissions  // array of ints
        };

        // ---- send to API ----
        const saveResponse = await fetch(`/Admin/SaveUserRolePermission`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify(payload)
        });

        if (!saveResponse.ok) {
            const errorText = await saveResponse.text();
            throw new Error(errorText);
        }

        alert("Saved successfully!");
        location.reload();

    } catch (err) {
        alert("Save failed: " + err.message);
    }
}

// ====== Update Role Permissions ======
// ====== Update Role Permissions ======
//async function updateRolePermissions() {
//    const roleId = parseInt($("#updateRoleId").val(), 10);
//    const selected = getSelectedPermissionIds("#updateDropdownpermitMulti").map(Number);

//    if (!roleId) return alert("Please select a role");

//    try {
//        // ---- GET ALL EXISTING ROLE-PERMISSIONS ----
//        const existingResponse = await fetch(`/Admin/GetUserPermission`, {
//            method: "GET",
//            headers: { "Accept": "application/json" },
//            credentials: "include"
//        });

//        const existingData = await existingResponse.json();

//        // ---- FILTER PERMISSIONS FOR SELECTED ROLE ----
//        const rolePermissions = existingData.filter(rp => rp.role?.id === roleId);
//        const existingIds = rolePermissions.map(rp => rp.permission.id);

//        // ---- CALCULATE WHAT TO ADD AND REMOVE ----
//        const toAdd = selected.filter(id => !existingIds.includes(id));
//        const toRemove = rolePermissions
//            .filter(rp => !selected.includes(rp.permission.id))
//            .map(rp => rp.id); // <-- record Id for delete endpoint

//        // ---- ADD NEW PERMISSIONS ----
//        if (toAdd.length > 0) {
//            const payload = {
//                id: 0,
//                roleId: roleId,
//                permissionIds: toAdd
//            };

//            const addResponse = await fetch(`/Admin/SaveUserRolePermission`, {
//                method: "POST",
//                headers: { "Content-Type": "application/json" },
//                credentials: "include",
//                body: JSON.stringify(payload)
//            });

//            if (!addResponse.ok)
//                throw new Error(await addResponse.text());
//        }

//        // ---- REMOVE UNSELECTED PERMISSIONS ----
//        //for (const id of toRemove) {
//        //    const delResponse = await fetch(`/Admin/DeleteUserRolePermission/${id}`, {
//        //        method: "DELETE",
//        //        headers: { "Accept": "application/json" },
//        //        credentials: "include"
//        //    });

//        //    if (!delResponse.ok)
//        //        throw new Error(await delResponse.text());
//        //}

//        alert("Role permissions updated successfully!");
//        location.reload();

//    } catch (err) {
//        alert("Update failed: " + err.message);
//    }
//}


// ====== Edit Role ======

async function updateRolePermissions() {
    const roleId = parseInt($("#updateRoleId").val(), 10);
    const selected = getSelectedPermissionIds("#updateDropdownpermitMulti").map(Number);

    if (!roleId) return alert("Please select a role");

    try {
        //  GET ALL EXISTING ROLE-PERMISSIONS
        const existingResponse = await fetch(`/Admin/GetUserPermission`, {
            method: "GET",
            headers: { "Accept": "application/json" },
            credentials: "include"
        });
        const existingData = await existingResponse.json();

        //  FILTER EXISTING PERMISSIONS FOR THIS ROLE
        const rolePermissions = existingData.filter(rp => rp.role?.id === roleId);

        const existingIdsMap = {};
        rolePermissions.forEach(rp => {
            existingIdsMap[rp.permission.id] = rp.id; // Map: PermissionId -> RolePermission record Id
        });

        //  CALCULATE WHAT TO ADD OR UPDATE
        const payload = {
            id: 0, // Backend can handle 0 or null for insert
            roleId: roleId,
            permissionIds: selected, // array of selected Permission IDs
        };

        // Optionally, if your backend supports sending existing record IDs, include them:
        // payload.existingRecordIds = Object.values(existingIdsMap);

        //  SEND TO BACKEND
        const updateResponse = await fetch(`/Admin/SaveUserRolePermission`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify(payload)
        });

        if (!updateResponse.ok) {
            throw new Error(await updateResponse.text());
        }

        alert("Role permissions updated successfully!");
        location.reload();

    } catch (err) {
        alert("Update failed: " + err.message);
    }
}


async function editRolePermissions(roleId) {
    showSection("updateDropdownSection");
    $("#updateRoleId").val(roleId);

    await loadRolesDropdown("#updateDynamicDropdown", "#updateDropdownInput", "#updateRoleId", roleId);
    await loadPermissionDropdown("#updateDropdownpermitMulti");

    // --- GET EXISTING ROLE PERMISSIONS ---
    const response = await fetch(`/Admin/GetUserRolePermission`, {
        method: 'GET',
        credentials: 'include',
        headers: { "Accept": "application/json" }
    });

    const existingData = await response.json();   // <-- REQUIRED FIX

    const role = existingData.find(rp => rp.role?.id === roleId)?.role;
    if (role) {
        $("#updateDropdownInput").val(role.roleName);  // <-- Set the role name here
        $("#updateRoleId").val(role.id);
    }

    // Filter only permissions belonging to this role
    const rolePermissions = existingData
        .filter(rp => rp.role?.id === roleId)
        .map(rp => rp.permission?.id);

    // Pre-select permissions in the dropdown
    $("#updateDropdownpermitMulti").val(rolePermissions).trigger("change");
}

// ====== Delete Role Permission ======
//async function deleteRolePermission(id) {
//    dropdownIdToDelete = id;
//    const modal = new bootstrap.Modal(document.getElementById("deleteConfirmModal"));
//    modal.show();
//}

let userroleIdToDelete = null; // store id temporarily

// Open modal instead of confirm()
function openDeleteModal(roleId) {
    userroleIdToDelete = roleId;
    const modal = new bootstrap.Modal(document.getElementById("deleteConfirmModal"));
    modal.show();
}

document.getElementById("confirmDeleteBtn").addEventListener("click", async function () {
    if (!userroleIdToDelete) return;

    const userRoleId = getCookie("UserRoleId");
    const token = getCookie("AuthToken"); 

    try {
        const response = await fetch(`/Admin/DeleteUserRolePermission/${userroleIdToDelete}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Accept": "application/json"
            },
            credentials: "include"
        });

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById("deleteConfirmModal")).hide();
            location.reload();
        } else {
            let message = "Unknown error";

            try {
                const text = await response.text();
                const json = JSON.parse(text);
                message = json.message || text;
            } catch {
                // Fallback when no JSON is returned
                message = "Server returned an invalid error response.";
            }

            alert("Failed to delete role: " + message);
        }

    } catch (error) {
        alert("Error deleting role: " + error.message);
    }

    userroleIdToDelete = null;
});

// ====== Initialize Page ======
$(async function () {
    loadRolesDropdown("#roleSelect");
    await loadRolesDropdown("#dynamicDropdown", "#dropdownInput", "#selectedRoleId");
    await loadPermissionDropdown("#dropdownpermitMulti");
    await loadRolePermissionTable();

    $("#btnSave").on("click", saveRolePermissions);
    $("#btnCancel").on("click", () => showSection("dropdownListSection"));
    $("#btnUpdate").on("click", updateRolePermissions);
    $("#btnUpdateCancel").on("click", () => showSection("dropdownListSection"));
});
