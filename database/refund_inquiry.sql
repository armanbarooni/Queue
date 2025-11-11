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
        rt.id                  AS RefundRowId,
        rtaTracking.attrValue  AS RefundTransactionId,
        ta.attrValue           AS BaseId
    FROM OnlinePay.dbo.RefundTransaction rt WITH (NOLOCK)
    INNER JOIN OnlinePay.dbo.[Transaction] t WITH (NOLOCK)
        ON rt.transactionId = t.transactionId
    INNER JOIN OnlinePay.dbo.TransactionAttribute ta WITH (NOLOCK)
        ON ta.transactionId = t.id AND ta.attrName = 'SaleReferenceId'
    INNER JOIN OnlinePay.dbo.RefundTransactionAttribute rtaTracking WITH (NOLOCK)
        ON rtaTracking.refundTransactionId = rt.id AND rtaTracking.attrName = 'trackingCode'
    WHERE rt.refundStatus IN (0, 1, 3)
      AND rt.bankSettingsId = 35
      AND rt.state = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateRefundTransactionStates
    @RefundUpdates dbo.StatusUpdateType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Update status and refundDesc (concatenated from JSON body: statusDescription + statusDetailCode + statusDetailDesc)
    UPDATE rt
        SET rt.refundStatus = upd.RefundState,
            rt.refundDesc = LTRIM(RTRIM(
                CONCAT(
                    ISNULL(JSON_VALUE(upd.BodyResponse, '$.StatusDescription'), N''),
                    CASE WHEN ISNULL(JSON_VALUE(upd.BodyResponse, '$.StatusDetailCode'), N'') = N'' THEN N'' ELSE N' ' + JSON_VALUE(upd.BodyResponse, '$.StatusDetailCode') END,
                    CASE WHEN ISNULL(JSON_VALUE(upd.BodyResponse, '$.StatusDetailDesc'), N'') = N'' THEN N'' ELSE N' ' + JSON_VALUE(upd.BodyResponse, '$.StatusDetailDesc') END
                )
            ))
    FROM OnlinePay.dbo.RefundTransaction rt
    INNER JOIN @RefundUpdates upd ON upd.RefundRowId = rt.id;

    -- Always insert a new attribute row for full response body (no updates)
    INSERT INTO OnlinePay.dbo.RefundTransactionAttribute (refundTransactionId, attrName, attrValue)
    SELECT
        source.RefundRowId,
        'bodyresponse',
        ISNULL(source.BodyResponse, '')
    FROM @RefundUpdates AS source
    WHERE source.BodyResponse IS NOT NULL;
END
GO

