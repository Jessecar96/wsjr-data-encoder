let config;

async function loadConfig() {

    // Make http request to get config
    const response = await fetch('/get-config');

    // Make sure it loaded okay
    if (!response.ok) {
        alert("Unable to load config");
        return;
    }

    // Read json response
    config = await response.json();

    // Set inputs to what the current config is
    document.getElementById("apikey").value = config.config.apikey;
    document.getElementById("force_clock_set").checked = config.config.force_clock_set;

    const elFlavors = document.getElementById("loop_flavor");

    // Clear out anything in there first
    elFlavors.innerHTML = "";

    // Populate flavors select
    elFlavors.innerHTML += "<option selected>Disabled</option>";
    for (const flavorsKey in config.flavors.Flavor) {
        const flavor = config.flavors.Flavor[flavorsKey];
        // Add this flavor to the select. If loop_flavor is set to this flavor, set it as selected
        elFlavors.innerHTML += "<option " + (config.config.loop_flavor === flavor.Name ? "selected" : "") + ">" + flavor.Name + "</option>";
    }

    const elStarSelector = document.getElementById("star-selector");
    elStarSelector.innerHTML = "";

    // Populate star selector
    let firstStar = true;
    for (const starsKey in config.config.stars) {
        const star = config.config.stars[starsKey];
        // Add this star to the nav pills, give the first one the active class 
        elStarSelector.innerHTML += "<li class=\"nav-item\"><a class=\"nav-link" + (firstStar ? " active" : "") + "\" data-index=\"" + starsKey + "\" href=\"#\">" + star.location_name + "</a></li>";
        firstStar = false;
    }

    // Trigger change to switch elements
    changeStar();

}

function changeStar() {
    const selectedStar = document.querySelector("#star-selector .active");
    const selectedStarIndex = selectedStar.dataset.index;
    const starConfig = config.config.stars[selectedStarIndex];

    document.getElementById("location_name").value = starConfig.location_name ?? "";
    document.getElementById("location").value = starConfig.location ?? "";
    document.getElementById("switches").value = starConfig.switches ?? "";
    document.getElementById("zones").value = starConfig.zones ?? "";
    // Nearby cities
    document.getElementById("nearby_city_0_name").value = starConfig.nearby_cities.location_name[0];
    document.getElementById("nearby_city_1_name").value = starConfig.nearby_cities.location_name[1];
    document.getElementById("nearby_city_2_name").value = starConfig.nearby_cities.location_name[2];
    document.getElementById("nearby_city_3_name").value = starConfig.nearby_cities.location_name[3];
    document.getElementById("nearby_city_4_name").value = starConfig.nearby_cities.location_name[4];
    document.getElementById("nearby_city_5_name").value = starConfig.nearby_cities.location_name[5];
    document.getElementById("nearby_city_6_name").value = starConfig.nearby_cities.location_name[6];
    document.getElementById("nearby_city_0").value = starConfig.nearby_cities.geocode[0];
    document.getElementById("nearby_city_1").value = starConfig.nearby_cities.geocode[1];
    document.getElementById("nearby_city_2").value = starConfig.nearby_cities.geocode[2];
    document.getElementById("nearby_city_3").value = starConfig.nearby_cities.geocode[3];
    document.getElementById("nearby_city_4").value = starConfig.nearby_cities.geocode[4];
    document.getElementById("nearby_city_5").value = starConfig.nearby_cities.geocode[5];
    document.getElementById("nearby_city_6").value = starConfig.nearby_cities.geocode[6];
    // Regional cities
    document.getElementById("regional_city_0_name").value = starConfig.regional_cities.location_name[0];
    document.getElementById("regional_city_1_name").value = starConfig.regional_cities.location_name[1];
    document.getElementById("regional_city_2_name").value = starConfig.regional_cities.location_name[2];
    document.getElementById("regional_city_3_name").value = starConfig.regional_cities.location_name[3];
    document.getElementById("regional_city_4_name").value = starConfig.regional_cities.location_name[4];
    document.getElementById("regional_city_5_name").value = starConfig.regional_cities.location_name[5];
    document.getElementById("regional_city_6_name").value = starConfig.regional_cities.location_name[6];
    document.getElementById("regional_city_0").value = starConfig.regional_cities.geocode[0];
    document.getElementById("regional_city_1").value = starConfig.regional_cities.geocode[1];
    document.getElementById("regional_city_2").value = starConfig.regional_cities.geocode[2];
    document.getElementById("regional_city_3").value = starConfig.regional_cities.geocode[3];
    document.getElementById("regional_city_4").value = starConfig.regional_cities.geocode[4];
    document.getElementById("regional_city_5").value = starConfig.regional_cities.geocode[5];
    document.getElementById("regional_city_6").value = starConfig.regional_cities.geocode[6];

}