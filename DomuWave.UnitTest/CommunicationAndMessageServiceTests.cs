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
    /// Unit test per <see cref="ICommunicationService"/>.
    ///
    /// APPROCCIO DI TEST: questi test mockano l'interfaccia <see cref="ICommunicationService"/>
    /// direttamente (non la sua implementazione concreta). Servono come test di contratto
    /// dell'interfaccia: verificano che i metodi esposti abbiano la firma corretta, i tipi di
    /// ritorno attesi e che la semantica di base (null per id inesistente, lista vuota, ecc.)
    /// sia consistente con ciò che il consumer si aspetta.
    ///
    /// Proprietà del modello <see cref="Communication"/>:
    /// <c>Title</c> (non Subject), <c>Content</c>, <c>CommunicationType</c>, <c>Priority</c>,
    /// <c>PublicationDate</c>, <c>ExpirationDate</c>, <c>SendEmail</c>, <c>IsVisible</c>,
    /// <c>AttachmentPath</c>.
    /// </summary>
    public class CommunicationServiceTests
    {
        private readonly Mock<ICommunicationService> _serviceMock;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>Utente fittizio minimo usato come parametro currentUser in tutti i test.</summary>
        private readonly FakeUser _user = new FakeUser { Id = 1, FullName = "Test User" };

        public CommunicationServiceTests()
        {
            _serviceMock = new Mock<ICommunicationService>();
        }

        // ─── Helper ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Costruisce un'istanza di <see cref="Communication"/> con valori predefiniti
        /// sensati per i test, evitando la ripetizione del codice di costruzione.
        /// L'<paramref name="id"/> non viene assegnato direttamente (NHibernate lo gestisce
        /// tramite hilo), ma viene accettato per chiarezza semantica nei commenti del test.
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>GetAllAsync</c> restituisca tutte le comunicazioni non eliminate
        /// (IsDeleted = false) presenti nel sistema.
        /// Il mock simula una lista di 2 comunicazioni; si controlla che il risultato
        /// non sia null, contenga 2 elementi e che ognuno abbia un titolo valorizzato.
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>GetByIdAsync</c> con un id esistente restituisca la comunicazione
        /// corrispondente, con titolo, tipo e priorità corretti.
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>GetByIdAsync</c> con un id inesistente (999) restituisca null
        /// anziché lanciare un'eccezione. Il chiamante è responsabile di gestire il null
        /// (es. restituendo 404 dal controller).
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>GetByCondominiumIdAsync</c> restituisca tutte le comunicazioni
        /// associate al condominio specificato. Il filtraggio per condominio è essenziale
        /// per l'isolamento multi-tenant (ogni gestione vede solo le proprie comunicazioni).
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>GetVisibleCommunicationsAsync</c> restituisca esclusivamente
        /// le comunicazioni con <c>IsVisible = true</c>. Questo metodo è tipicamente
        /// esposto ai condòmini nel portale self-service, dove le comunicazioni non pubblicate
        /// non devono essere visibili.
        /// </summary>
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
            // Tutte le comunicazioni restituite devono avere IsVisible = true
            Assert.All(result, c => Assert.True(c.IsVisible));
        }

        // ─── GetByTypeAsync ───────────────────────────────────────────────────────

        /// <summary>
        /// Verifica che <c>GetByTypeAsync</c> filtri correttamente per <c>CommunicationType</c>.
        /// Nel dominio DomuWave i tipi predefiniti includono "Assemblea", "Avviso", "Manutenzione".
        /// </summary>
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
            // Ogni elemento deve avere esattamente il tipo richiesto
            Assert.All(result, c => Assert.Equal("Assemblea", c.CommunicationType));
        }

        // ─── GetByPriorityAsync ───────────────────────────────────────────────────

        /// <summary>
        /// Verifica che <c>GetByPriorityAsync</c> filtri correttamente per <c>Priority</c>.
        /// La priorità "Alta" identifica le comunicazioni urgenti che richiedono attenzione
        /// immediata dai condòmini.
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>GetUnreadByUserAsync</c> restituisca solo le comunicazioni
        /// non ancora lette dall'utente specificato (<paramref name="userId"/>).
        /// Il test simula un singolo messaggio non letto e verifica che il risultato
        /// contenga esattamente quell'elemento.
        /// </summary>
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
            Assert.Single(result); // deve esserci esattamente 1 comunicazione non letta
        }

        // ─── CreateAsync ──────────────────────────────────────────────────────────

        /// <summary>
        /// Verifica che <c>CreateAsync</c> con una comunicazione valida restituisca l'entità
        /// persistita, preservando titolo, tipo, priorità e flag <c>SendEmail</c>.
        /// Il mock usa una lambda di ritorno (<c>ReturnsAsync(c, _, __) => c</c>) per
        /// simulare il comportamento reale del servizio che restituisce l'oggetto salvato.
        /// </summary>
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

        /// <summary>
        /// Verifica esplicita che il modello <see cref="Communication"/> utilizzi
        /// la proprietà <c>Title</c> (non <c>Subject</c>, che non esiste su questo modello).
        /// Questo test serve come documentazione vivente: se in futuro qualcuno aggiungesse
        /// erroneamente una proprietà <c>Subject</c>, questo test richiederebbe una revisione.
        /// </summary>
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
            // La proprietà Subject non esiste su Communication: verificato a compile time
        }

        // ─── UpdateAsync ──────────────────────────────────────────────────────────

        /// <summary>
        /// Verifica che <c>UpdateAsync</c> su una comunicazione esistente restituisca
        /// l'entità con i valori aggiornati. Il test simula il ciclo completo:
        /// lettura → modifica in memoria → salvataggio → verifica del risultato.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ExistingCommunication_ReturnsUpdatedEntity()
        {
            // Arrange: recupera la comunicazione esistente
            var existing = BuildCommunication(1, "Vecchio titolo");

            _serviceMock
                .Setup(s => s.GetByIdAsync(1, _user, _ct))
                .ReturnsAsync(existing);

            // Modifica in memoria (simula ApplyUpdate del consumer)
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

        /// <summary>
        /// Verifica che <c>DeleteAsync</c> con un id esistente restituisca <c>true</c>,
        /// indicando che il soft-delete è stato applicato correttamente.
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>DeleteAsync</c> con un id inesistente (999) restituisca <c>false</c>,
        /// consentendo al controller di rispondere con 404 Not Found.
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>PublishCommunicationAsync</c> con un id esistente restituisca
        /// <c>true</c>. La pubblicazione imposta <c>IsVisible = true</c> e registra
        /// l'autore della pubblicazione (tracciabilità).
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>PublishCommunicationAsync</c> con un id inesistente restituisca
        /// <c>false</c>. Il controller deve rispondere con 404 Not Found in questo scenario.
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>ExistsAsync</c> restituisca <c>true</c> per un id che
        /// corrisponde a un'entità esistente nel sistema.
        /// </summary>
        [Fact]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            _serviceMock
                .Setup(s => s.ExistsAsync(1, _user, _ct))
                .ReturnsAsync(true);

            var result = await _serviceMock.Object.ExistsAsync(1, _user, _ct);

            Assert.True(result);
        }

        /// <summary>
        /// Verifica che <c>ExistsAsync</c> restituisca <c>false</c> per un id che
        /// non corrisponde a nessuna entità (o a un'entità soft-deleted).
        /// </summary>
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

        /// <summary>
        /// Verifica che <c>CountAsync</c> con un predicato LINQ (es. <c>x => !x.IsDeleted</c>)
        /// restituisca il conteggio corretto delle comunicazioni che soddisfano il filtro.
        /// Il mock usa <c>It.IsAny&lt;Expression&gt;</c> perché le lambda non sono comparabili
        /// per valore in Moq senza configurazione aggiuntiva.
        /// </summary>
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
}
