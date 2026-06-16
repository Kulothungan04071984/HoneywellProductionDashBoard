-- ============================================================
-- ProductionDashboard – SQL Server Schema & Seed
-- ============================================================

CREATE TABLE ProductionMetrics (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Product     NVARCHAR(50)  NOT NULL,
    Shift       NVARCHAR(20)  NOT NULL,   -- 'Night' | 'Morning' | 'Afternoon'
    HourSlot    INT           NOT NULL,   -- 1-8
    FCT1_Target DECIMAL(10,2) NOT NULL DEFAULT 0,
    FCT1_Actual DECIMAL(10,2) NOT NULL DEFAULT 0,
    FCT2_Target DECIMAL(10,2) NOT NULL DEFAULT 0,
    FCT2_Actual DECIMAL(10,2) NOT NULL DEFAULT 0,
    FCT3_Target DECIMAL(10,2) NOT NULL DEFAULT 0,
    FCT3_Actual DECIMAL(10,2) NOT NULL DEFAULT 0,
    RF1_Target  DECIMAL(10,2) NOT NULL DEFAULT 0,
    RF1_Actual  DECIMAL(10,2) NOT NULL DEFAULT 0,
    RF2_Target  DECIMAL(10,2) NOT NULL DEFAULT 0,
    RF2_Actual  DECIMAL(10,2) NOT NULL DEFAULT 0,
    RTC1_Target DECIMAL(10,2) NOT NULL DEFAULT 0,
    RTC1_Actual DECIMAL(10,2) NOT NULL DEFAULT 0,
    VOL1_Target DECIMAL(10,2) NOT NULL DEFAULT 0,
    VOL1_Actual DECIMAL(10,2) NOT NULL DEFAULT 0,
    RecordedAt  DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Metrics UNIQUE (Product, Shift, HourSlot)
);

CREATE INDEX IX_Metrics_Product_Shift ON ProductionMetrics (Product, Shift);

-- ── Sample seed data for V200 ──────────────────────────────
INSERT INTO ProductionMetrics
  (Product, Shift, HourSlot,
   FCT1_Target, FCT1_Actual, FCT2_Target, FCT2_Actual,
   FCT3_Target, FCT3_Actual, RF1_Target,  RF1_Actual,
   RF2_Target,  RF2_Actual,  RTC1_Target, RTC1_Actual,
   VOL1_Target, VOL1_Actual)
VALUES
  -- Night Shift  (8 PM – 4 AM)
  ('V200','Night',1, 100,98,  95,92,  90,88,  120,118, 110,107, 85,82,  200,195),
  ('V200','Night',2, 100,102, 95,90,  90,93,  120,122, 110,108, 85,84,  200,202),
  ('V200','Night',3, 100,95,  95,97,  90,86,  120,115, 110,112, 85,80,  200,198),
  ('V200','Night',4, 100,105, 95,94,  90,91,  120,125, 110,109, 85,88,  200,208),
  ('V200','Night',5, 100,99,  95,96,  90,89,  120,119, 110,111, 85,83,  200,197),
  ('V200','Night',6, 100,101, 95,93,  90,92,  120,121, 110,106, 85,86,  200,204),
  ('V200','Night',7, 100,97,  95,98,  90,87,  120,116, 110,113, 85,81,  200,196),
  ('V200','Night',8, 100,103, 95,91,  90,94,  120,123, 110,110, 85,87,  200,206),
  -- Morning Shift (4 AM – 12 PM)
  ('V200','Morning',1, 100,96,  95,94,  90,91,  120,117, 110,108, 85,83,  200,193),
  ('V200','Morning',2, 100,104, 95,92,  90,88,  120,124, 110,111, 85,85,  200,207),
  ('V200','Morning',3, 100,98,  95,99,  90,90,  120,118, 110,107, 85,82,  200,199),
  ('V200','Morning',4, 100,100, 95,96,  90,93,  120,120, 110,109, 85,84,  200,200),
  ('V200','Morning',5, 100,94,  95,93,  90,87,  120,114, 110,112, 85,80,  200,191),
  ('V200','Morning',6, 100,106, 95,91,  90,95,  120,126, 110,106, 85,89,  200,210),
  ('V200','Morning',7, 100,99,  95,97,  90,89,  120,119, 110,110, 85,83,  200,198),
  ('V200','Morning',8, 100,102, 95,95,  90,92,  120,122, 110,108, 85,86,  200,203),
  -- Afternoon Shift (12 PM – 8 PM)
  ('V200','Afternoon',1, 100,97,  95,95,  90,90,  120,116, 110,109, 85,81,  200,194),
  ('V200','Afternoon',2, 100,103, 95,91,  90,88,  120,123, 110,112, 85,84,  200,206),
  ('V200','Afternoon',3, 100,99,  95,98,  90,91,  120,118, 110,107, 85,82,  200,197),
  ('V200','Afternoon',4, 100,101, 95,94,  90,93,  120,121, 110,110, 85,87,  200,201),
  ('V200','Afternoon',5, 100,95,  95,96,  90,87,  120,115, 110,113, 85,80,  200,190),
  ('V200','Afternoon',6, 100,107, 95,92,  90,95,  120,127, 110,106, 85,90,  200,212),
  ('V200','Afternoon',7, 100,98,  95,97,  90,89,  120,117, 110,111, 85,83,  200,196),
  ('V200','Afternoon',8, 100,104, 95,93,  90,94,  120,124, 110,108, 85,88,  200,208);

-- ── Stored procedure for upsert from production system ────
CREATE OR ALTER PROCEDURE usp_UpsertMetric
    @Product NVARCHAR(50), @Shift NVARCHAR(20), @HourSlot INT,
    @FCT1_Target DECIMAL(10,2), @FCT1_Actual DECIMAL(10,2),
    @FCT2_Target DECIMAL(10,2), @FCT2_Actual DECIMAL(10,2),
    @FCT3_Target DECIMAL(10,2), @FCT3_Actual DECIMAL(10,2),
    @RF1_Target  DECIMAL(10,2), @RF1_Actual  DECIMAL(10,2),
    @RF2_Target  DECIMAL(10,2), @RF2_Actual  DECIMAL(10,2),
    @RTC1_Target DECIMAL(10,2), @RTC1_Actual DECIMAL(10,2),
    @VOL1_Target DECIMAL(10,2), @VOL1_Actual DECIMAL(10,2)
AS
BEGIN
    MERGE ProductionMetrics AS tgt
    USING (SELECT @Product AS Product, @Shift AS Shift, @HourSlot AS HourSlot) AS src
    ON tgt.Product=src.Product AND tgt.Shift=src.Shift AND tgt.HourSlot=src.HourSlot
    WHEN MATCHED THEN UPDATE SET
        FCT1_Target=@FCT1_Target, FCT1_Actual=@FCT1_Actual,
        FCT2_Target=@FCT2_Target, FCT2_Actual=@FCT2_Actual,
        FCT3_Target=@FCT3_Target, FCT3_Actual=@FCT3_Actual,
        RF1_Target=@RF1_Target,   RF1_Actual=@RF1_Actual,
        RF2_Target=@RF2_Target,   RF2_Actual=@RF2_Actual,
        RTC1_Target=@RTC1_Target, RTC1_Actual=@RTC1_Actual,
        VOL1_Target=@VOL1_Target, VOL1_Actual=@VOL1_Actual,
        RecordedAt=GETDATE()
    WHEN NOT MATCHED THEN INSERT
        (Product,Shift,HourSlot,
         FCT1_Target,FCT1_Actual,FCT2_Target,FCT2_Actual,
         FCT3_Target,FCT3_Actual,RF1_Target,RF1_Actual,
         RF2_Target,RF2_Actual,RTC1_Target,RTC1_Actual,
         VOL1_Target,VOL1_Actual)
    VALUES
        (@Product,@Shift,@HourSlot,
         @FCT1_Target,@FCT1_Actual,@FCT2_Target,@FCT2_Actual,
         @FCT3_Target,@FCT3_Actual,@RF1_Target,@RF1_Actual,
         @RF2_Target,@RF2_Actual,@RTC1_Target,@RTC1_Actual,
         @VOL1_Target,@VOL1_Actual);
END;
