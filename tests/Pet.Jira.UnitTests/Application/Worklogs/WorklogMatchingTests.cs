using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.UnitTests.Application.Mock;

namespace Pet.Jira.UnitTests.Application.Worklogs
{
	class WorklogMatchingTests
	{
		private IIssue[] _issues;
		private DateTime _date;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var i = 0;
			_date = DateTime.Now.Date;
			_issues = new[]
			{
				new MockIssue($"{i++}"),
				new MockIssue($"{i++}")
			};
		}

		[Test]
		public void Match_WhenParentsAndChildrenAreEmpty_ShouldNotThrow()
		{
			// Arrange
			var parents = Enumerable.Empty<WorkingDayWorklog>();
			var children = Enumerable.Empty<WorkingDayWorklog>();

			// Act
			// Assert
			Assert.DoesNotThrow(() => WorklogMatching.Match(parents, children));
		}

		[Test]
		public void Match_WhenParentsAreEmpty_ShouldNotThrow()
		{
			// Arrange
			var parents = Enumerable.Empty<WorkingDayWorklog>();
			var children = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog { Issue = _issues[0] }
			};

			// Act
			// Assert
			Assert.DoesNotThrow(() => WorklogMatching.Match(parents, children));
		}

		[Test]
		public void Match_WhenChildrenAreEmpty_ShouldNotThrow()
		{
			// Arrange
			var parents = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog { Issue = _issues[0] }
			};
			var children = Enumerable.Empty<WorkingDayWorklog>();

			// Act
			// Assert
			Assert.DoesNotThrow(() => WorklogMatching.Match(parents, children));
		}

		[Test]
		public void Match_WhenChildrenAndParentsAreNotEmpty_ShouldMatchByIssueKey()
		{
			// Arrange
			var parents = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog { Issue = _issues[0] }
			};
			var children = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog { Issue = _issues[0] }
			};

			// Act
			WorklogMatching.Match(parents, children);

			// Assert
			Assert.That(children[0].Parent, Is.EqualTo(parents[0]));
		}

		[Test]
		public void Match_WhenChildrenAndParentsAreNotEmpty_ShouldMatchByStartDate()
		{
			// Arrange
			var parents = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(9),
					RawCompleteDate = _date.AddHours(12)
				},
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(14),
					RawCompleteDate = _date.AddHours(16)
				}
			};
			var children = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(15),
					RawCompleteDate = _date.AddHours(17)
				}
			};

			// Act
			WorklogMatching.Match(parents, children);

			// Assert
			Assert.That(children[0].Parent, Is.EqualTo(parents[1]));
		}

		[Test]
		public void Match_WhenChildrenAndParentsAreNotEmpty_ShouldMatchByCompleteDate()
		{
			// Arrange
			var parents = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(9),
					RawCompleteDate = _date.AddHours(12)
				},
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(14),
					RawCompleteDate = _date.AddHours(16)
				}
			};
			var children = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(13),
					RawCompleteDate = _date.AddHours(15)
				}
			};

			// Act
			WorklogMatching.Match(parents, children);

			// Assert
			Assert.That(children[0].Parent, Is.EqualTo(parents[1]));
		}

		[Test]
		public void Match_WhenChildrenAndParentsHaveDifferentIssue_ShouldNotMatch()
		{
			// Arrange
			var parents = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[1],
					RawStartDate = _date.AddHours(9),
					RawCompleteDate = _date.AddHours(12)
				},
				new WorkingDayWorklog
				{
					Issue = _issues[1],
					RawStartDate = _date.AddHours(14),
					RawCompleteDate = _date.AddHours(16)
				}
			};
			var children = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(14),
					RawCompleteDate = _date.AddHours(16)
				}
			};

			// Act
			WorklogMatching.Match(parents, children);

			// Assert
			Assert.That(children[0].Parent, Is.Null);
		}

		[Test]
		public void Match_WhenChildrenAndParentsAreNotEmpty_ShouldMatchByCompleteDateNesting()
		{
			// Arrange
			var parents = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(9),
					RawCompleteDate = _date.AddHours(12)
				},
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(14),
					RawCompleteDate = _date.AddHours(16)
				}
			};
			var children = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(13),
					RawCompleteDate = _date.AddHours(17)
				}
			};

			// Act
			WorklogMatching.Match(parents, children);

			// Assert
			Assert.That(children[0].Parent, Is.EqualTo(parents[1]));
		}

		[Test]
		public void Match_WhenChildrenAndParentsAreNotEmpty_ShouldMatchByStartDateNesting()
		{
			// Arrange
			var parents = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(9),
					RawCompleteDate = _date.AddHours(12)
				},
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(14),
					RawCompleteDate = _date.AddHours(16)
				}
			};
			var children = new List<WorkingDayWorklog>
			{
				new WorkingDayWorklog
				{
					Issue = _issues[0],
					RawStartDate = _date.AddHours(8),
					RawCompleteDate = _date.AddHours(10)
				}
			};

			// Act
			WorklogMatching.Match(parents, children);

			// Assert
			Assert.That(children[0].Parent, Is.EqualTo(parents[0]));
		}
	}
}
