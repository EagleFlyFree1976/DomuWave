                                                        -- Add optional ChartOfAccounts FK to ConsumptionType.
                                                        -- When set, approving a ConsumptionCharge will automatically create an Expense
                                                        -- on this account for the total amount of the charge.

                                                        ALTER TABLE ConsumptionType
                                                            ADD AccountId INT NULL;

                                                        ALTER TABLE ConsumptionType
                                                            ADD CONSTRAINT FK_ConsumptionType_ChartOfAccounts
                                                                FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(Id);
