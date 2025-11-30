// src/app/app-config-loader.js
(function () {
  try {
    var xhr = new XMLHttpRequest();
    xhr.open('GET', '/assets/app-config.json', false); // synchronous so Angular gets config before bootstrap
    xhr.send(null);
    if (xhr.status === 200) {
      window.appConfig = JSON.parse(xhr.responseText);
      console.log('appConfig loaded', window.appConfig);
    } else {
      console.warn('app-config.json not found, status:', xhr.status);
      window.appConfig = {};
    }
  } catch (e) {
    console.error('Failed to load runtime config', e);
    window.appConfig = {};
  }
})();
