let config;

// elements
const elApiKey = document.getElementById("apikey");
const elForceClockSet = document.getElementById("force_clock_set");
const elLoopFlavor = document.getElementById("loop_flavor");
const elRunFlavor = document.getElementById("run_flavor");
const elLocationName = document.getElementById("location_name");
const elLocation = document.getElementById("location");
const elSwitches = document.getElementById("switches");
const elZones = document.getElementById("zones");
const elNearbyCity0Name = document.getElementById("nearby_city_0_name");
const elNearbyCity1Name = document.getElementById("nearby_city_1_name");
const elNearbyCity2Name = document.getElementById("nearby_city_2_name");
const elNearbyCity3Name = document.getElementById("nearby_city_3_name");
const elNearbyCity4Name = document.getElementById("nearby_city_4_name");
const elNearbyCity5Name = document.getElementById("nearby_city_5_name");
const elNearbyCity6Name = document.getElementById("nearby_city_6_name");
const elNearbyCity0 = document.getElementById("nearby_city_0");
const elNearbyCity1 = document.getElementById("nearby_city_1");
const elNearbyCity2 = document.getElementById("nearby_city_2");
const elNearbyCity3 = document.getElementById("nearby_city_3");
const elNearbyCity4 = document.getElementById("nearby_city_4");
const elNearbyCity5 = document.getElementById("nearby_city_5");
const elNearbyCity6 = document.getElementById("nearby_city_6");
const elRegionalCity0Name = document.getElementById("regional_city_0_name");
const elRegionalCity1Name = document.getElementById("regional_city_1_name");
const elRegionalCity2Name = document.getElementById("regional_city_2_name");
const elRegionalCity3Name = document.getElementById("regional_city_3_name");
const elRegionalCity4Name = document.getElementById("regional_city_4_name");
const elRegionalCity5Name = document.getElementById("regional_city_5_name");
const elRegionalCity6Name = document.getElementById("regional_city_6_name");
const elRegionalCity0 = document.getElementById("regional_city_0");
const elRegionalCity1 = document.getElementById("regional_city_1");
const elRegionalCity2 = document.getElementById("regional_city_2");
const elRegionalCity3 = document.getElementById("regional_city_3");
const elRegionalCity4 = document.getElementById("regional_city_4");
const elRegionalCity5 = document.getElementById("regional_city_5");
const elRegionalCity6 = document.getElementById("regional_city_6");

async function loadConfigPage() {
    await loadConfig();
    buildHtml();
}

async function loadControlPage() {
    await loadConfig();

    // Populate flavors select
    elRunFlavor.innerHTML += "";
    for (const flavorsKey in config.flavors.Flavor) {
        const flavor = config.flavors.Flavor[flavorsKey];
        elRunFlavor.innerHTML += "<option value=\"" + flavor.Name + "\">" + flavor.Name + "</option>";
    }

}

async function loadConfig() {

    // Make http request to get config
    const response = await fetch('/getConfig');

    // Make sure it loaded okay
    if (!response.ok) {
        alert("Unable to load config");
        return;
    }

    // Read json response
    config = await response.json();
}

function buildHtml() {
    // Set inputs to what the current config is
    elApiKey.value = config.config.apikey;
    elForceClockSet.checked = config.config.force_clock_set;

    // Clear out anything in there first
    elLoopFlavor.innerHTML = "";

    // Populate flavors select
    elLoopFlavor.innerHTML += "<option value=''>Disabled</option>";
    for (const flavorsKey in config.flavors.Flavor) {
        const flavor = config.flavors.Flavor[flavorsKey];
        // Add this flavor to the select. If loop_flavor is set to this flavor, set it as selected
        elLoopFlavor.innerHTML += "<option value=\"" + flavor.Name + "\" " + (config.config.loop_flavor === flavor.Name ? "selected" : "") + ">" + flavor.Name + "</option>";
    }

    // Change the flavor selector
    elLoopFlavor.value = config.config.loop_flavor;

    // Empty the tabs
    const elStarSelector = document.getElementById("star-selector");
    elStarSelector.innerHTML = "";

    // Populate star selector
    let firstStar = true;
    for (const starIndex in config.config.stars) {

        const star = config.config.stars[starIndex];

        // Add this star to the nav pills, give the first one the active class 
        elStarSelector.innerHTML +=
            "<li class=\"nav-item\">" +
            "<button type=\"button\" class=\"nav-link" + (firstStar ? " active" : "") + "\" data-index=\"" + starIndex + "\" href=\"#\">" + star.location_name + "</button>" +
            "</li>";

        // Click handler for the new tabs
        const elTabButtons = document.querySelectorAll('#star-selector button')
        elTabButtons.forEach(elTabButton => {
            const tabTrigger = new bootstrap.Tab(elTabButton);
            elTabButton.addEventListener('click', event => {
                // Prevent the buton from doing any form things
                event.preventDefault();
                // Trigger the bootstrap tab change
                tabTrigger.show();
                // Change the html to show the new values
                setHtmlValues();
            })
        });

        // No longer first star
        firstStar = false;
    }

    // Set values on all the input elements
    setHtmlValues();
}

function setHtmlValues() {
    // Find the selected tab, and index of it
    const selectedStar = document.querySelector("#star-selector .active");
    const selectedStarIndex = selectedStar.dataset.index;
    const starConfig = config.config.stars[selectedStarIndex];

    // Basic vars
    elLocationName.value = starConfig.location_name ?? "";
    elLocation.value = starConfig.location ?? "";
    elSwitches.value = starConfig.switches ?? "";
    elZones.value = starConfig.zones ?? "";
    // Nearby cities
    elNearbyCity0Name.value = starConfig.nearby_cities.location_name[0] ?? "";
    elNearbyCity1Name.value = starConfig.nearby_cities.location_name[1] ?? "";
    elNearbyCity2Name.value = starConfig.nearby_cities.location_name[2] ?? "";
    elNearbyCity3Name.value = starConfig.nearby_cities.location_name[3] ?? "";
    elNearbyCity4Name.value = starConfig.nearby_cities.location_name[4] ?? "";
    elNearbyCity5Name.value = starConfig.nearby_cities.location_name[5] ?? "";
    elNearbyCity6Name.value = starConfig.nearby_cities.location_name[6] ?? "";
    elNearbyCity0.value = starConfig.nearby_cities.geocode[0] ?? "";
    elNearbyCity1.value = starConfig.nearby_cities.geocode[1] ?? "";
    elNearbyCity2.value = starConfig.nearby_cities.geocode[2] ?? "";
    elNearbyCity3.value = starConfig.nearby_cities.geocode[3] ?? "";
    elNearbyCity4.value = starConfig.nearby_cities.geocode[4] ?? "";
    elNearbyCity5.value = starConfig.nearby_cities.geocode[5] ?? "";
    elNearbyCity6.value = starConfig.nearby_cities.geocode[6] ?? "";
    // Regional cities
    elRegionalCity0Name.value = starConfig.regional_cities.location_name[0] ?? "";
    elRegionalCity1Name.value = starConfig.regional_cities.location_name[1] ?? "";
    elRegionalCity2Name.value = starConfig.regional_cities.location_name[2] ?? "";
    elRegionalCity3Name.value = starConfig.regional_cities.location_name[3] ?? "";
    elRegionalCity4Name.value = starConfig.regional_cities.location_name[4] ?? "";
    elRegionalCity5Name.value = starConfig.regional_cities.location_name[5] ?? "";
    elRegionalCity6Name.value = starConfig.regional_cities.location_name[6] ?? "";
    elRegionalCity0.value = starConfig.regional_cities.geocode[0] ?? "";
    elRegionalCity1.value = starConfig.regional_cities.geocode[1] ?? "";
    elRegionalCity2.value = starConfig.regional_cities.geocode[2] ?? "";
    elRegionalCity3.value = starConfig.regional_cities.geocode[3] ?? "";
    elRegionalCity4.value = starConfig.regional_cities.geocode[4] ?? "";
    elRegionalCity5.value = starConfig.regional_cities.geocode[5] ?? "";
    elRegionalCity6.value = starConfig.regional_cities.geocode[6] ?? "";

}

// onchange events for all inputs
function updateStar() {

    // Update global vars
    config.config.apikey = elApiKey.value;
    config.config.force_clock_set = elForceClockSet.checked;
    config.config.loop_flavor = elLoopFlavor.value;

    // Find the selected tab, and index of it
    const selectedStar = document.querySelector("#star-selector .active");
    const selectedStarIndex = selectedStar.dataset.index;

    // Init objects if they don't exist
    if (config.config.stars.indexOf(selectedStarIndex) === -1)
        config.config.stars[selectedStarIndex] = {
            nearby_cities: {
                location_name: [],
                geocode: []
            },
            regional_cities: {
                location_name: [],
                geocode: []
            },
        };

    // Change tab name to location name
    selectedStar.innerText = elLocationName.value;

    // Parse zone list
    let zoneList = [];
    if (elZones.value !== "") {
        zoneList = elZones.value.split(",");
    }

    config.config.stars[selectedStarIndex].location_name = elLocationName.value;
    config.config.stars[selectedStarIndex].location = elLocation.value;
    config.config.stars[selectedStarIndex].switches = elSwitches.value;
    config.config.stars[selectedStarIndex].zones = zoneList;
    // Nearby cities
    config.config.stars[selectedStarIndex].nearby_cities.location_name[0] = elNearbyCity0Name.value;
    config.config.stars[selectedStarIndex].nearby_cities.location_name[1] = elNearbyCity1Name.value;
    config.config.stars[selectedStarIndex].nearby_cities.location_name[2] = elNearbyCity2Name.value;
    config.config.stars[selectedStarIndex].nearby_cities.location_name[3] = elNearbyCity3Name.value;
    config.config.stars[selectedStarIndex].nearby_cities.location_name[4] = elNearbyCity4Name.value;
    config.config.stars[selectedStarIndex].nearby_cities.location_name[5] = elNearbyCity5Name.value;
    config.config.stars[selectedStarIndex].nearby_cities.location_name[6] = elNearbyCity6Name.value;
    config.config.stars[selectedStarIndex].nearby_cities.geocode[0] = elNearbyCity0.value;
    config.config.stars[selectedStarIndex].nearby_cities.geocode[1] = elNearbyCity1.value;
    config.config.stars[selectedStarIndex].nearby_cities.geocode[2] = elNearbyCity2.value;
    config.config.stars[selectedStarIndex].nearby_cities.geocode[3] = elNearbyCity3.value;
    config.config.stars[selectedStarIndex].nearby_cities.geocode[4] = elNearbyCity4.value;
    config.config.stars[selectedStarIndex].nearby_cities.geocode[5] = elNearbyCity5.value;
    config.config.stars[selectedStarIndex].nearby_cities.geocode[6] = elNearbyCity6.value;
    // Regional cities
    config.config.stars[selectedStarIndex].regional_cities.location_name[0] = elRegionalCity0Name.value;
    config.config.stars[selectedStarIndex].regional_cities.location_name[1] = elRegionalCity1Name.value;
    config.config.stars[selectedStarIndex].regional_cities.location_name[2] = elRegionalCity2Name.value;
    config.config.stars[selectedStarIndex].regional_cities.location_name[3] = elRegionalCity3Name.value;
    config.config.stars[selectedStarIndex].regional_cities.location_name[4] = elRegionalCity4Name.value;
    config.config.stars[selectedStarIndex].regional_cities.location_name[5] = elRegionalCity5Name.value;
    config.config.stars[selectedStarIndex].regional_cities.location_name[6] = elRegionalCity6Name.value;
    config.config.stars[selectedStarIndex].regional_cities.geocode[0] = elRegionalCity0.value;
    config.config.stars[selectedStarIndex].regional_cities.geocode[1] = elRegionalCity1.value;
    config.config.stars[selectedStarIndex].regional_cities.geocode[2] = elRegionalCity2.value;
    config.config.stars[selectedStarIndex].regional_cities.geocode[3] = elRegionalCity3.value;
    config.config.stars[selectedStarIndex].regional_cities.geocode[4] = elRegionalCity4.value;
    config.config.stars[selectedStarIndex].regional_cities.geocode[5] = elRegionalCity5.value;
    config.config.stars[selectedStarIndex].regional_cities.geocode[6] = elRegionalCity6.value;
}

function addStar() {
    // Add a basic schema for the new star
    config.config.stars.push({
        location_name: "New Star",
        nearby_cities: {
            location_name: [],
            geocode: []
        },
        regional_cities: {
            location_name: [],
            geocode: []
        },
    });
    // Rebuild html to show it
    buildHtml();
}

function removeStar() {

    // Make sure they can't delete the only one
    if (config.config.stars.length <= 1) {
        alert("You cannot delete the only star");
        return;
    }

    // Make sure they really want to
    if (!confirm("Are you sure you want to delete this star?"))
        return;

    // Find the selected tab, and index of it
    const selectedStar = document.querySelector("#star-selector .active");
    const selectedStarIndex = selectedStar.dataset.index;

    // Remove it from the config array
    config.config.stars.splice(selectedStarIndex, 1);

    // Rebuild html
    buildHtml();
}

async function saveConfig() {

    // Make http request to get config
    const response = await fetch('/setConfig', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(config)
    });

    // Make sure it loaded okay
    if (!response.ok) {
        alert("Unable to set config");
        return;
    }

    // Read json response
    respJson = await response.json();
    alert(respJson.message);
}

async function runLF() {

    if (elRunFlavor.value.trim() === "") {
        alert("Flavor is required");
        return;
    }

    // Make http request
    const response = await fetch('/runLocalPresentation', {
        method: 'POST',
        body: elRunFlavor.value
    });

    // Make sure it loaded okay
    if (!response.ok) {
        alert("Unable to cancel LF");
        return;
    }

    // Read json response
    respJson = await response.json();
    alert(respJson.message);
}

async function cancelLF() {
    // Make http request
    const response = await fetch('/cancelLocalPresentation', {
        method: 'POST'
    });

    // Make sure it loaded okay
    if (!response.ok) {
        alert("Unable to cancel LF");
        return;
    }

    // Read json response
    respJson = await response.json();
    alert(respJson.message);
}

async function sendAlert() {
    
    const elAlertText = document.getElementById('alert_text');
    const alertText = elAlertText.value;
    
    const elAlertType = document.getElementById('alert_type');
    const alertType = elAlertType.value;
    
    // Make http request
    const formData = new FormData();
    formData.append('alertText', alertText);
    formData.append('alertType', alertType);
    const response = await fetch('/sendAlert', {
        method: 'POST',
        body: formData
    });

    // Make sure it loaded okay
    if (!response.ok) {
        alert("Unable to cancel LF");
        return;
    }

    // Read json response
    respJson = await response.json();
    alert(respJson.message);
}