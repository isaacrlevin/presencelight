
## Configure an Entra ID Application

1. Sign in to the [Microsoft Entra admin center](https://entra.microsoft.com/) using either a work or school account or a personal Microsoft account.
1. If your account gives you access to more than one tenant, select your account in the top right corner, and set your portal session to the desired Azure AD tenant
   (using **Switch Directory**).
1. In the left-hand navigation pane, select the **Entra ID** service, and then select **App registrations**.

#### Register the client app (WpfApp)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `Presence Light`.
   - In the **Supported account types** section, select **Accounts in this organizational directory only (YOUR_TENANT_NAME only - Single tenant)**.
   - In the **Redirect URI (optional)** section, select **Public client/native (mobile & desktop)** and enter http://localhost for the value.
    - Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later.
1. On the app **Overview** page, find the **Directory (tenant) ID** value and record it for later.<br>![Ids](../static/id.png)
1. In the list of pages select **API permissions**
   - Select **Add a permission**
   - Ensure that the **Microsoft APIs** tab is selected
   - In the **Commonly used Microsoft APIs** section, click on **Microsoft Graph**
   - Select **Delegated permissions**.
   - Ensure that the right permissions are checked: **Presence.Read, User.Read**. Use the search box if necessary. See the screenshot below.
   - Select **Add permissions**
   - You can consent for your entire organization by selecting **Grant admin consent for YOUR_TENANT_NAME**
     - In the **Grant admin consent confirmation** section select **Yes**

   ![Api Permissions](..//static/api-perms.png)

#### Configuring PresenceLight (Desktop)

1. Start `PresenceLight`.
1. Select **Settings**
  1. Enter your **Directory (tenant) ID** or `common` if you elected to support Multitenant account types.
  1. Enter your **Application (client) ID**
  1. Select **SAVE SETTINGS**
1. Select **Team Status**
  1. Select **SIGN IN**
  1. Complete authentication in your browser.