# Front End Development

1. Download and install Visual Studio Code
2. Open the workspace /UT4MasterServer/UT4MasterServer.Web/ut4ms.code-workspace
3. Click the settings icon (gear in the bottom left corner of visual studio) -> Profiles -> Import from file
4. Import /UT4MasterServer/UT4MasterServer.Web/ut4ms.code-profile. This will install the appropriate extensions and settings necessary for smooth development.
5. In terminal, run "npm ci"
6. Run "npm run dev"

This will serve the application from vite with hot reload at localhost:8080. The API will need to be running separately and it will need to be accessible from whatever the value of the VITE_API_URL environment variable in .env.development is set to (e.g. http://localhost)
