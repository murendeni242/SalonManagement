using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Services;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.Services.DeleteServiceHandlerTests.Failure_Cases
{
    public class DeleteServiceHandlerTests_FailureCases
    {
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteServiceHandler _handler;

        public DeleteServiceHandlerTests_FailureCases()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new DeleteServiceHandler(
                _serviceRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ServiceNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _serviceRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Service?)null);

            // Act
            var act = () => _handler.Handle(99);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Service*");
        }
    }
}
