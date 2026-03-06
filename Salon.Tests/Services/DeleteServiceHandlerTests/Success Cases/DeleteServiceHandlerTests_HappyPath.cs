using Moq;
using Salon.Application.UseCases.Services;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Services.DeleteServiceHandlerTests.Success_Cases
{
    public class DeleteServiceHandlerTests_HappyPath
    {
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteServiceHandler _handler;

        public DeleteServiceHandlerTests_HappyPath()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new DeleteServiceHandler(
                _serviceRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ExistingService_CallsDeleteAsync()
        {
            // Arrange
            var service = new ServiceBuilder().WithId(1).Build();

            _serviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(service);
            _serviceRepo.Setup(r => r.DeleteAsync(service)).Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(1);

            // Assert — DeleteAsync triggers soft delete via DbContext interceptor
            _serviceRepo.Verify(r => r.DeleteAsync(service), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingService_WritesAuditLogWithSoftDeletedAction()
        {
            // Arrange
            var service = new ServiceBuilder().WithId(1).Build();

            _serviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(service);
            _serviceRepo.Setup(r => r.DeleteAsync(service)).Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(1);

            // Assert
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "SoftDeleted" &&
                           log.EntityName == "Service")),
                Times.Once);
        }
    }
}
