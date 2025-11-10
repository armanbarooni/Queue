-- Create table-valued type for bulk status updates (drop/recreate to keep schema in sync)
IF TYPE_ID(N'dbo.StatusUpdateType') IS NOT NULL
    DROP TYPE dbo.StatusUpdateType;
GO

CREATE TYPE dbo.StatusUpdateType AS TABLE
(
    RefundRowId  BIGINT        NOT NULL,
    RefundState  INT           NOT NULL,
    BodyResponse NVARCHAR(MAX) NULL
);
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetPendingRefundTransactions
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        rt.id                 AS RefundRowId,
        rtaRefundId.attrValue AS RefundId,
        rtaTracking.attrValue AS RefundTransactionId,
        ta.attrValue          AS BaseId
    FROM OnlinePay.dbo.RefundTransaction rt WITH (NOLOCK)
    INNER JOIN OnlinePay.dbo.[Transaction] t WITH (NOLOCK)
        ON rt.transactionId = t.transactionId
    INNER JOIN OnlinePay.dbo.TransactionAttribute ta WITH (NOLOCK)
        ON ta.transactionId = t.id AND ta.attrName = 'SaleReferenceId'
    INNER JOIN OnlinePay.dbo.RefundTransactionAttribute rtaTracking WITH (NOLOCK)
        ON rtaTracking.refundTransactionId = rt.id AND rtaTracking.attrName = 'trackingCode'
    INNER JOIN OnlinePay.dbo.RefundTransactionAttribute rtaRefundId WITH (NOLOCK)
        ON rtaRefundId.refundTransactionId = rt.id AND rtaRefundId.attrName = 'refundId'
    WHERE rt.refundStatus IN (0, 1)
      AND rt.bankSettingsId = 35
      AND rt.state = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateRefundTransactionStates
    @RefundUpdates dbo.StatusUpdateType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE rt
        SET rt.refundStatus = upd.RefundState
    FROM OnlinePay.dbo.RefundTransaction rt
    INNER JOIN @RefundUpdates upd ON upd.RefundRowId = rt.id;

    MERGE OnlinePay.dbo.RefundTransactionAttribute AS target
    USING @RefundUpdates AS source
        ON target.refundTransactionId = source.RefundRowId
       AND target.attrName = 'responsebody'
    WHEN MATCHED THEN
        UPDATE SET target.attrValue = ISNULL(source.BodyResponse, '')
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (refundTransactionId, attrName, attrValue)
        VALUES (source.RefundRowId, 'responsebody', ISNULL(source.BodyResponse, ''));
END
GO

