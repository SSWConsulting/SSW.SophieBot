# Deployment

-   [Deploy with Bot Framework Composer](#deploy-with-bot-framework-composer)
-   [Automatic deployment with GitHub Actions](#automatic-deployment-with-github-actions)

## Manual way - Deploy with Bot Framework Composer

~~1. Open the repo with Bot Framework Composer.~~

~~2. Go to **Publish** section, select the publishing file.~~
   ![composer-choose-publishing-file](images/composer-choose-publishing-file.png)
   **Figure: Choose the publishing file**

~~3. Check the bot you want to publish. Click **Publish selected bots**.~~
   ![publish-the-bot](images/composer-publish-the-bot.png)
   **Figure: Publish the bot**

## Automatic way - Deployment with GitHub Actions

Use GitHub Actions to deploy bot to staging and production environments.

For staging deployment, please refer to [SSW.SophieBot.dev.yml](https://github.com/SSWConsulting/SSW.SophieBot/blob/main/.github/workflows/SSW.SophieBot.dev.yml)

For production deployment, please refer to [SSW.SophieBot.prod.yml](https://github.com/SSWConsulting/SSW.SophieBot/blob/main/.github/workflows/SSW.SophieBot.prod.yml)

This is as per [SSW Rule - Environments](https://www.ssw.com.au/rules/do-you-have-separate-development-testing-and-production-environments)
