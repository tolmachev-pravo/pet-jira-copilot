##### What is the purpose of this automation?
Automation of time logging for Jira issues. Calculations are advisory and approximate

##### Who is this for?
This is for the development team. Currently, the calculations mostly take into account the dev-workflow and are tailored to changes in task statuses, where the employee is responsible

##### What time zone is used for time fields?
The time zone of the employee from Jira is used for time fields

##### Is it possible to log arbitrary time?
Yes, the time calculation is purely advisory

##### Where is the information I entered stored?
The application does not have a centralized permanent database. The source of data is the local storage of the browser

##### How is the logged time associated with a task?
Currently, it is associated with time. The time of the status change is compared with the time of logging. If there is a mismatch, then the association does not occur
##### How to remove worklog?
By Jira UI

##### I enter the correct username and password, but I get an error. Why?
Sometimes Jira after a certain number of errors may require a captcha. You need to login via Jira UI

##### Wishes, suggestions, comments, questions?
- https://github.com/tolmachev-pravo/pet-jira-workflow/issues
- https://github.com/tolmachev-pravo/pet-jira-workflow/discussions