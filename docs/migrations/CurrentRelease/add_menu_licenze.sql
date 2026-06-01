-- Aggiunge la voce "La mia licenza" al menu per gli amministratori tenant
INSERT INTO dbo.base_menues (MenuId, ParentMenuId, Icon, Description, Action, AuthorizationCode, PopulateEvent, IsEnabled, OrderKey, Tags)
SELECT 67, NULL, 'pi-id-card', 'La mia licenza', '/licenze', NULL, NULL, 1, 65, 'tenant'
WHERE NOT EXISTS (SELECT 1 FROM dbo.base_menues WHERE MenuId = 67);
