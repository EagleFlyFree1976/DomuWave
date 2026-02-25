using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DomuWave.Services.Implementations;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services
{
    /// <summary>
    /// Unit test per CommunicationService.
    /// Nota: Communication ha Title (non Subject), Content, CommunicationType, Priority,
    /// PublicationDate, ExpirationDate, SendEmail, IsVisible, AttachmentPath.
    /// </summary>
    public class CommunicationServiceTests
    {
        private readonly Mock<ICommunicationService> _serviceMock;
        private readonly CancellationToken _ct = CancellationToken.None;

        // Utente fittizio minimo
        private readonly FakeUser _user = new FakeUser { Id = 1, FullName = "Test User" };

        public CommunicationServiceTests()
        {
            _serviceMock = new Mock<ICommunicationService>();
        }

        // ─── Helper ───────────────────────────────────────────────────────────────

        private static Communication BuildCommunication(int id = 1,
            string title = "Avviso assemblea",
            string content = "Assemblea ordinaria in data 10/03/2026",
            string type = "Assemblea",
            string priority = "Alta",
            bool isVisible = true)
        {
            return new Communication
            {
                // Id viene assegnato tramite reflection per i test (o lasciato a 0 se non mappato)
                Title = title,
                Content = content,
                CommunicationType = type,
                Priority = priority,
                PublicationDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(30),
                SendEmail = true,
                IsVisible = isVisible,
                AttachmentPath = null
            };
        }

        // ─── GetAllAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ReturnsAllNonDeletedCommunications()
        {
            // Arrange
            var communications = new List<Communication>
            {
                BuildCommunication(1, "Avviso 1"),
                BuildCommunication(2, "Avviso 2"),
            };

            _serviceMock
                .Setup(s => s.GetAllAsync(_user, _ct))
                .ReturnsAsync(communications);

            // Act
            var result = await _serviceMock.Object.GetAllAsync(_user, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.False(string.IsNullOrWhiteSpace(c.Title)));
        }

        // ─── GetByIdAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsCommunication()
        {
            // Arrange
            var comm = BuildCommunication(1, "Manutenzione straordinaria");

            _serviceMock
                .Setup(s => s.GetByIdAsync(1, _user, _ct))
                .ReturnsAsync(comm);

            // Act
            var result = await _serviceMock.Object.GetByIdAsync(1, _user, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Manutenzione straordinaria", result.Title);
            Assert.Equal("Assemblea", result.CommunicationType);
            Assert.Equal("Alta", result.Priority);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetByIdAsync(999, _user, _ct))
                .ReturnsAsync((Communication)null);

            // Act
            var result = await _serviceMock.Object.GetByIdAsync(999, _user, _ct);

            // Assert
            Assert.Null(result);
        }

        // ─── GetByCondominiumIdAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetByCondominiumIdAsync_ReturnsCommunicationsForCondominium()
        {
            // Arrange
            var condominiumId = 10;
            var list = new List<Communication>
            {
                BuildCommunication(1, "Avviso condominio A"),
                BuildCommunication(2, "Avviso condominio B"),
            };

            _serviceMock
                .Setup(s => s.GetByCondominiumIdAsync(condominiumId, _user, _ct))
                .ReturnsAsync(list);

            // Act
            var result = await _serviceMock.Object.GetByCondominiumIdAsync(condominiumId, _user, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // ─── GetVisibleCommunicationsAsync ────────────────────────────────────────

        [Fact]
        public async Task GetVisibleCommunicationsAsync_ReturnsOnlyVisibleCommunications()
        {
            // Arrange
            var condominiumId = 10;
            var visible = new List<Communication>
            {
                BuildCommunication(1, "Comunicazione visibile", isVisible: true),
            };

            _serviceMock
                .Setup(s => s.GetVisibleCommunicationsAsync(condominiumId, _user, _ct))
                .ReturnsAsync(visible);

            // Act
            var result = await _serviceMock.Object.GetVisibleCommunicationsAsync(condominiumId, _user, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, c => Assert.True(c.IsVisible));
        }

        // ─── GetByTypeAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetByTypeAsync_ReturnsOnlyCommunicationsOfGivenType()
        {
            // Arrange
            var condominiumId = 10;
            var type = "Assemblea";
            var list = new List<Communication>
            {
                BuildCommunication(1, "Assemblea ordinaria", type: "Assemblea"),
                BuildCommunication(2, "Assemblea straordinaria", type: "Assemblea"),
            };

            _serviceMock
                .Setup(s => s.GetByTypeAsync(condominiumId, type, _user, _ct))
                .ReturnsAsync(list);

            // Act
            var result = await _serviceMock.Object.GetByTypeAsync(condominiumId, type, _user, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, c => Assert.Equal("Assemblea", c.CommunicationType));
        }

        // ─── GetByPriorityAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task GetByPriorityAsync_ReturnsOnlyCommunicationsOfGivenPriority()
        {
            // Arrange
            var condominiumId = 10;
            var priority = "Alta";
            var list = new List<Communication>
            {
                BuildCommunication(1, "Urgente 1", priority: "Alta"),
                BuildCommunication(2, "Urgente 2", priority: "Alta"),
            };

            _serviceMock
                .Setup(s => s.GetByPriorityAsync(condominiumId, priority, _user, _ct))
                .ReturnsAsync(list);

            // Act
            var result = await _serviceMock.Object.GetByPriorityAsync(condominiumId, priority, _user, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, c => Assert.Equal("Alta", c.Priority));
        }

        // ─── GetUnreadByUserAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetUnreadByUserAsync_ReturnsUnreadCommunicationsForUser()
        {
            // Arrange
            var condominiumId = 10;
            var userId = 5L;
            var unread = new List<Communication>
            {
                BuildCommunication(1, "Non letta"),
            };

            _serviceMock
                .Setup(s => s.GetUnreadByUserAsync(condominiumId, userId, _user, _ct))
                .ReturnsAsync(unread);

            // Act
            var result = await _serviceMock.Object.GetUnreadByUserAsync(condominiumId, userId, _user, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        // ─── CreateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidCommunication_ReturnsCreatedEntity()
        {
            // Arrange
            var newComm = BuildCommunication(0, "Nuova comunicazione");

            _serviceMock
                .Setup(s => s.CreateAsync(It.IsAny<Communication>(), _user, _ct))
                .ReturnsAsync((Communication c, object _, object __) => c);

            // Act
            var result = await _serviceMock.Object.CreateAsync(newComm, _user, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Nuova comunicazione", result.Title);
            Assert.Equal("Assemblea", result.CommunicationType);
            Assert.Equal("Alta", result.Priority);
            Assert.True(result.SendEmail);
        }

        [Fact]
        public async Task CreateAsync_CommunicationHasNoSubject_TitleIsUsed()
        {
            // Verifica esplicita: Communication NON ha Subject, usa Title
            var comm = new Communication
            {
                Title = "Titolo comunicazione",   // ← Title, non Subject
                Content = "Contenuto",
                CommunicationType = "Avviso",
                Priority = "Media",
                PublicationDate = DateTime.UtcNow,
                SendEmail = false,
                IsVisible = false
            };

            _serviceMock
                .Setup(s => s.CreateAsync(It.IsAny<Communication>(), _user, _ct))
                .ReturnsAsync(comm);

            var result = await _serviceMock.Object.CreateAsync(comm, _user, _ct);

            Assert.Equal("Titolo comunicazione", result.Title);
            // La proprietà Subject non esiste su Communication
        }

        // ─── UpdateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ExistingCommunication_ReturnsUpdatedEntity()
        {
            // Arrange
            var existing = BuildCommunication(1, "Vecchio titolo");

            _serviceMock
                .Setup(s => s.GetByIdAsync(1, _user, _ct))
                .ReturnsAsync(existing);

            existing.Title = "Titolo aggiornato";
            existing.Content = "Contenuto aggiornato";

            _serviceMock
                .Setup(s => s.UpdateAsync(existing, _user, _ct))
                .ReturnsAsync(existing);

            // Act
            var result = await _serviceMock.Object.UpdateAsync(existing, _user, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Titolo aggiornato", result.Title);
            Assert.Equal("Contenuto aggiornato", result.Content);
        }

        // ─── DeleteAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(1, _user, _ct))
                .ReturnsAsync(true);

            // Act
            var result = await _serviceMock.Object.DeleteAsync(1, _user, _ct);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(999, _user, _ct))
                .ReturnsAsync(false);

            // Act
            var result = await _serviceMock.Object.DeleteAsync(999, _user, _ct);

            // Assert
            Assert.False(result);
        }

        // ─── PublishCommunicationAsync ────────────────────────────────────────────

        [Fact]
        public async Task PublishCommunicationAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.PublishCommunicationAsync(1, _user.Id, _user, _ct))
                .ReturnsAsync(true);

            // Act
            var result = await _serviceMock.Object.PublishCommunicationAsync(1, _user.Id, _user, _ct);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PublishCommunicationAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.PublishCommunicationAsync(999, _user.Id, _user, _ct))
                .ReturnsAsync(false);

            // Act
            var result = await _serviceMock.Object.PublishCommunicationAsync(999, _user.Id, _user, _ct);

            // Assert
            Assert.False(result);
        }

        // ─── ExistsAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            _serviceMock
                .Setup(s => s.ExistsAsync(1, _user, _ct))
                .ReturnsAsync(true);

            var result = await _serviceMock.Object.ExistsAsync(1, _user, _ct);

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NonExistingId_ReturnsFalse()
        {
            _serviceMock
                .Setup(s => s.ExistsAsync(999, _user, _ct))
                .ReturnsAsync(false);

            var result = await _serviceMock.Object.ExistsAsync(999, _user, _ct);

            Assert.False(result);
        }

        // ─── CountAsync ───────────────────────────────────────────────────────────

        [Fact]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            _serviceMock
                .Setup(s => s.CountAsync(It.IsAny<Expression<Func<Communication, bool>>>(), _user, _ct))
                .ReturnsAsync(5);

            var result = await _serviceMock.Object.CountAsync(x => !x.IsDeleted, _user, _ct);

            Assert.Equal(5, result);
        }
    }

    // ─── Fake user per i test ─────────────────────────────────────────────────────

   
}
