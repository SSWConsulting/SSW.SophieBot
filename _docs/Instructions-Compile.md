## Prerequisites

In order to get set up to work with SSW.SophieBot, you will need the following:

1. [Bot Framework Composer](https://docs.microsoft.com/en-us/composer/install-composer?tabs=macos)
2. [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/blob/master/README.md)
3. [Installing & Configuring ngrok](<https://github.com/Microsoft/BotFramework-Emulator/wiki/Tunneling-(ngrok)>)

## Initial Setup

1. [Clone the repo](#step-1-clone-the-repo)
2. [Open the repo with Composer](#step-2-open-the-repo-with-composer)
3. [Configure your bot](#step-3-configure-your-bot)
4. [Test run](#step-4-test-run)

### Step 1. Clone the repo

Go to [SSW.SophieBot](https://github.com/SSWConsulting/SSW.SophieBot) and clone the repo.
![azure-devops-clone-repo](images/github-devops-clone-repo.png)
**Figure: Click Copy to get download link**

### Step 2. Open the repo with Composer

Open the Bot Framework Composer.

![composer-open-solution](images/composer-open-solution.png)
**Figure: Click Open on Composer's homepage**

![composer-select-solution](images/composer-select-solution.png)
**Figure: Select repo directory**

### Step 3. Configure your bot

In Bot Framework Composer, go to Configure | Development resources.

1. Set up Language Understanding.

![composer-set-up-luis](images/composer-set-up-luis.png)
**Figure: Click Set up Language Understanding**

![composer-select-luis-resource-type](images/composer-select-luis-resource-type.png)
**Figure: Select Use existing resources and click next**

![composer-select-subscription-and-luis-resource](images/composer-select-subscription-and-luis-resource.png)
**Figure: Select the subscription and luis resource and click next**

2. Set up App ID. Still in the Configure | Development resources, scroll to the bottom.

![composer-retrieve-app-id](images/composer-retrieve-app-id.png)
**Figure: Click Retrieve App ID**

![composer-select-publishing-profile](images/composer-select-publishing-profile.png)
**Figure: Select publishing profile**

### Step 4. Test run

![composer-start-bot](images/composer-start-bot.png)
**Figure: Click Start bot**

![composer-test-in-emulator](images/composer-test-in-emulator.png)
**Figure: Click Test in Emulator**

![emulator-welcome-message-after-login](images/emulator-welcome-message.png)
**Figure: Get welcome message**
