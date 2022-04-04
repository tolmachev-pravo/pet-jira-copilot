﻿using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Jira
{
    public interface IJiraService
    {
        Task<IEnumerable<DailyWorklogSummary>> GetUserDayWorklogs(DateTime fromDate, DateTime toDate, int issueCount);
        Task AddWorklogAsync(AddedWorklogDto worklog);
        Task<LoginResponse> Login(LoginRequest request);
        Task<string> GetCurrentUserAvatar(CancellationToken cancellationToken = default);
    }
}
