// Retrieve the opened tabs from localStorage (if any)
let openedTabs = {};

document.addEventListener("DOMContentLoaded", function () {
    const menuLinks = document.querySelectorAll('.app-container-anchor,.MenucollapseInnerMenuListItem, .nav-link_anchor');

    // Retrieve the opened tabs URLs from localStorage (if any)
    let openedTabsURLs = [];
    try {
        const savedTabs = localStorage.getItem('openedTabsURLs');
        openedTabsURLs = savedTabs ? JSON.parse(savedTabs) : [];
    } catch (error) {
        console.error("Error retrieving openedTabsURLs from localStorage:", error);
    }

    // Try to restore opened tabs from the saved URLs in openedTabsURLs
    openedTabsURLs.forEach(url => {
        // Check if the window for the URL is still open
        if (openedTabs[url] && !openedTabs[url].closed) {
            console.log(`Restored tab for ${url}.`);
        } else {
            // If the window is not open anymore, remove it from the openedTabs
            console.log(`No open tab found for ${url}, removing from openedTabsURLs.`);
            openedTabsURLs = openedTabsURLs.filter(openedUrl => openedUrl !== url);
        }
    });

    // Save the cleaned-up openedTabsURLs back to localStorage
    try {
        localStorage.setItem('openedTabsURLs', JSON.stringify(openedTabsURLs));
        console.log('Cleaned up and saved openedTabsURLs to localStorage');
    } catch (error) {
        console.error("Error saving cleaned openedTabsURLs to localStorage:", error);
    }

    menuLinks.forEach(link => {
        link.addEventListener('click', function (event) {
            event.preventDefault(); // Prevent default anchor behavior

            const url = link.getAttribute('data-url'); // Get the URL from data-url attribute

            // Handle case for "/" where the current page should be reloaded instead of opening a new tab
            if (url === '/') {
                console.log('Reloading current page');
                window.location.href = '/'; // Reload the current page without opening a new tab
                return;
            }

            // Check if the tab for this URL is already open
            if (openedTabs[url] && !openedTabs[url].closed) {
                console.log(`Tab for ${url} already open. Refreshing...`);
                openedTabs[url].focus();
                openedTabs[url].location.reload(); // Refresh the tab
            } else {
                // If not open, open a new tab and store the reference
                const newTab = window.open(url, '_blank');
                openedTabs[url] = newTab;

                console.log(`Opening a new tab for ${url}`);

                // Check if the URL is already in the openedTabsURLs array
                if (!openedTabsURLs.includes(url)) {
                    // If not, add it to the array
                    openedTabsURLs.push(url);

                    // Save the updated opened tabs URLs to localStorage
                    try {
                        localStorage.setItem('openedTabsURLs', JSON.stringify(openedTabsURLs));
                        console.log('Successfully saved openedTabsURLs to localStorage');
                    } catch (error) {
                        console.error("Error saving openedTabsURLs to localStorage:", error);
                    }
                }

                // Listen for tab close events to remove from openedTabs and localStorage
                newTab.addEventListener('beforeunload', () => {
                    console.log(`Tab for ${url} is being closed.`);
                    delete openedTabs[url];

                    // Remove the URL from openedTabsURLs and update localStorage
                    openedTabsURLs = openedTabsURLs.filter(openedUrl => openedUrl !== url);
                    try {
                        localStorage.setItem('openedTabsURLs', JSON.stringify(openedTabsURLs));
                    } catch (error) {
                        console.error("Error updating localStorage after tab close:", error);
                    }
                });
            }
        });
    });

    // Clean up closed tabs from openedTabs and localStorage on page load (to ensure accuracy)
    openedTabsURLs.forEach(url => {
        const tab = openedTabs[url];
        if (tab && tab.closed) {
            console.log(`Removing closed tab for ${url} from openedTabs.`);
            delete openedTabs[url];

            // Remove from openedTabsURLs if the tab is closed
            openedTabsURLs = openedTabsURLs.filter(openedUrl => openedUrl !== url);
        }
    });

    // Save the cleaned-up openedTabsURLs to localStorage
    try {
        localStorage.setItem('openedTabsURLs', JSON.stringify(openedTabsURLs));
        console.log('Cleaned up and saved openedTabsURLs to localStorage');
    } catch (error) {
        console.error("Error saving cleaned openedTabsURLs to localStorage:", error);
    }
});