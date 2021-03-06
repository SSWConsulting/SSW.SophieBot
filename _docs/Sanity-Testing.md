# Sanity Testing

**Please make sure we test all these queries on SophieBot before deploying to Production.**

- Type **'Hello'** - It will return Welcome message.
- Type **'Who is Adam Cogan?'** - It will return Profile Card of the employee. Please ensure that data on the Today field is correct and test all the button on the card importantly 'View in bookings' and 'View Project' button.
- Type **'Who is in Sydney?'** - It will return a list of people in requested location.
- Type **'Who is in {your current office} right now?'** - It will return a list of people who are present in office and check if data is correct or not.
- Type **'Who is on client work {any date} in {any office}?'** - It will return a list of people who are on client work on specific data and in that office. Please verify this data in Booked in Days in [PowerBI](https://app.powerbi.com/groups/456358f7-5b12-46f5-b952-2a37fa9bb5e8/reports/a4069dc4-86c1-4cba-bf81-161bb108c5c2/ReportSection).
- Type **'Who is free { any date } in { any office }?'** - It will a list of people who are not on client work on that day in that office. Please verify this data in Booked in Days in [PowerBI](https://app.powerbi.com/groups/456358f7-5b12-46f5-b952-2a37fa9bb5e8/reports/a4069dc4-86c1-4cba-bf81-161bb108c5c2/ReportSection).
- Type **'Which client is { employee name } is booked for?'** - It will return a client profile card. Please ensure it returns correct client that employee working for and verify those client informaion in CRM.
- Please test following query about the Intranet. Please verify all the reponse with this [SharePoint Page](https://sswcom.sharepoint.com/SitePages/where-do-you-want-to-go-today.aspx).
  - **Which client is Calum booked for?**
  - **Who is Calum working for?**
  - **Which client is Calum working for?**
  - **Where can I find CRM?**
  - **Where is SharePoint?**
  - **Where can I find rules?**
  - **Where is TimePro?**
  - **Where can I find employee responsibilities?**
  - **Where to see reports?**
  - **Where is SugarLearning?**
  - **Where can I find the SSW TV Website?**
  - **Where can I find SSW rules?**
  - **Where is the SSW website?**
  - **Where can I find the SSW design library?**
  - **Where is the SSW DevOps?**
