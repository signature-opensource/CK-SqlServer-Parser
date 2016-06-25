
         CREATE PROCEDURE INS_ARRIVEE_PIECE_PRO
         			@DAT_ARRIVEE_PIECE_PRO		DATETIME = NULL, 
         			@BLN_ACTIF					TINYINT, 
         			@BLN_CONFORME				TINYINT, 
         			@COM_ARRIVEE_PIECE_PRO		VARCHAR(255),
         			@ID_PIECE_PRO				INT, 
         			@ID_CONTRAT_PRO				INT, 
         			@ID_MODULE_PRO				INT, 
         			@ID_SESSION_PRO				INT, 
         			@DAT_RELANCE_ENGAGE_1		DATETIME, 
         			@DAT_RELANCE_ENGAGE_2		DATETIME, 
         			@DAT_RELANCE_ENGAGE_3		DATETIME, 
         			@DAT_RELANCE_REGLE_1		DATETIME, 
         			@DAT_RELANCE_REGLE_2		DATETIME, 
         			@DAT_RELANCE_REGLE_3		DATETIME,
         			@NUM_FILE					tinyint,
         			@ID_UTILISATEUR				int,
         			@NOM_FICHIER				varchar(100),
         			@NUM_LOT					varchar(17) = NULL

         	AS
         	BEGIN

         		IF @DAT_ARRIVEE_PIECE_PRO IS NULL
         			SET @DAT_ARRIVEE_PIECE_PRO = GETDATE ()

         		INSERT INTO ARRIVEE_PIECE_PRO
         			(
         				COD_ARRIVEE_PIECE_PRO, 
         				DAT_ARRIVEE_PIECE_PRO, 
         				BLN_ACTIF, 
         				BLN_CONFORME, 
         				COM_ARRIVEE_PIECE_PRO, 
         				ID_PIECE_PRO,
         				ID_CONTRAT_PRO, 
         				ID_MODULE_PRO, 
         				ID_SESSION_PRO, 
         				DAT_RELANCE_ENGAGE_1, 
         				DAT_RELANCE_ENGAGE_2, 
         				DAT_RELANCE_ENGAGE_3, 
         				DAT_RELANCE_REGLE_1, 
         				DAT_RELANCE_REGLE_2, 
         				DAT_RELANCE_REGLE_3,
         				NUM_FILE,
         				ID_UTILISATEUR,
         				NOM_FICHIER,
         				NUM_LOT
         			)
         		VALUES
         			(
         				0,
         				@DAT_ARRIVEE_PIECE_PRO, 
         				@BLN_ACTIF, 
         				@BLN_CONFORME, 
         				@COM_ARRIVEE_PIECE_PRO,
         				@ID_PIECE_PRO, 
         				@ID_CONTRAT_PRO, 
         				@ID_MODULE_PRO, 
         				@ID_SESSION_PRO, 
         				@DAT_RELANCE_ENGAGE_1, 
         				@DAT_RELANCE_ENGAGE_2, 
         				@DAT_RELANCE_ENGAGE_3, 
         				@DAT_RELANCE_REGLE_1, 
         				@DAT_RELANCE_REGLE_2, 
         				@DAT_RELANCE_REGLE_3,
         				@NUM_FILE,
         				@ID_UTILISATEUR,
         				@NOM_FICHIER,
         				@NUM_LOT
         			)

         		UPDATE	ARRIVEE_PIECE_PRO
         		SET		COD_ARRIVEE_PIECE_PRO = SCOPE_IDENTITY() 
         		WHERE	ID_ARRIVEE_PIECE_PRO = SCOPE_IDENTITY() 

         		return SCOPE_IDENTITY() 

         	END

         CREATE PROCEDURE [dbo].[LEC_GRP_STAGIAIRE_MODULE_PEC_FICHEDOSSIER]
         	@ID_MODULE		int
         AS
         BEGIN
         	DECLARE @ID_PREV_SESSION	int
         	DECLARE @COUNT				int
         	DECLARE @NB_STAGIAIRE		int
         	DECLARE @STAGIAIRE		VARCHAR(254)
         	DECLARE @COD_MOD			varchar(12)

         	SET @COUNT = 0

         	SELECT @COUNT	= count(ID_SESSION_PEC)	from SESSION_PEC	where ID_MODULE_PEC = @ID_MODULE
         	SELECT @COD_MOD = COD_MODULE_PEC	from MODULE_PEC		where ID_MODULE_PEC = @ID_MODULE

         	SELECT top 1 @ID_PREV_SESSION = ID_SESSION_PEC from SESSION_PEC
         		where ID_MODULE_PEC = @ID_MODULE AND BLN_ACTIF = 1
         		order by ID_SESSION_PEC desc

         	DECLARE @EXISTS_DESENGAGEMENT INT
         	SELECT @EXISTS_DESENGAGEMENT = ISNULL(COUNT(*),0)
         	FROM POSTE_COUT_ENGAGE
         	LEFT JOIN	ENGAGEMENT ON ENGAGEMENT.ID_ENGAGEMENT = POSTE_COUT_ENGAGE.ID_ENGAGEMENT AND
         				ENGAGEMENT.DAT_BAE IS NOT NULL 
         	WHERE 
         	POSTE_COUT_ENGAGE.ID_MODULE_PEC = @ID_MODULE				AND
         	POSTE_COUT_ENGAGE.ID_ENGAGEMENT = ENGAGEMENT.ID_ENGAGEMENT	AND 
         	POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NOT NULL

         CREATE TABLE #MES_STAGIAIRES (ID_STAGIAIRE_PEC INT)
         INSERT 	INTO #MES_STAGIAIRES
         SELECT
         	STAGIAIRE_PEC.ID_STAGIAIRE_PEC
         FROM		 
         	STAGIAIRE_PEC
         WHERE
         		@ID_MODULE		is not null	AND 
         		STAGIAIRE_PEC.ID_MODULE_PEC = @ID_MODULE	AND 
         		STAGIAIRE_PEC.ID_SESSION_PEC is null

         SET @NB_STAGIAIRE =(Select Count(*)

         	FROM		 
         		#MES_STAGIAIRES
         		INNER JOIN STAGIAIRE_PEC ON STAGIAIRE_PEC.ID_STAGIAIRE_PEC=#MES_STAGIAIRES.ID_STAGIAIRE_PEC
         		INNER JOIN BRANCHE		ON BRANCHE.ID_BRANCHE = STAGIAIRE_PEC.ID_BRANCHE
         		INNER JOIN INDIVIDU		ON INDIVIDU.ID_INDIVIDU = STAGIAIRE_PEC.ID_INDIVIDU
         		LEFT JOIN UTILISATEUR	ON UTILISATEUR.ID_UTILISATEUR = INDIVIDU.ID_UTILISATEUR
         		LEFT JOIN NATIONALITE	ON NATIONALITE.ID_NATIONALITE = INDIVIDU.ID_NATIONALITE
         		INNER JOIN CSP			ON CSP.ID_CSP			= STAGIAIRE_PEC.ID_CSP
         		INNER JOIN ETABLISSEMENT ON ETABLISSEMENT.ID_ETABLISSEMENT	= STAGIAIRE_PEC.ID_ETABLISSEMENT
         		INNER JOIN ADHERENT		ON ADHERENT.ID_ADHERENT				= ETABLISSEMENT.ID_ADHERENT
         		LEFT JOIN GROUPE on (STAGIAIRE_PEC.ID_GROUPE = GROUPE.ID_GROUPE)

         		LEFT OUTER JOIN STAGIAIRE_PEC STP ON STAGIAIRE_PEC.ID_STAGIAIRE_TUTEUR = STP.ID_STAGIAIRE_PEC
         		LEFT OUTER JOIN INDIVIDU ISTP ON STP.ID_INDIVIDU = ISTP.ID_INDIVIDU
         		left join R19 on (R19.ID_ADHERENT = ADHERENT.ID_ADHERENT and R19.ID_ACTIVITE in (SELECT ID_ACTIVITE FROM GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN'))		
         						and R19.ID_PERIODE = (select ID_PERIODE from PERIODE where id_type_periode =1 and bln_en_cours = 1))

         	)

         	IF @NB_STAGIAIRE = 1
         	BEGIN
         	SET @STAGIAIRE=( Select Top 1
         		INDIVIDU.NOM_INDIVIDU + ' ' + INDIVIDU.PRENOM_INDIVIDU 
         	FROM		 
         		#MES_STAGIAIRES
         		INNER JOIN STAGIAIRE_PEC ON STAGIAIRE_PEC.ID_STAGIAIRE_PEC=#MES_STAGIAIRES.ID_STAGIAIRE_PEC
         		INNER JOIN BRANCHE		ON BRANCHE.ID_BRANCHE = STAGIAIRE_PEC.ID_BRANCHE
         		INNER JOIN INDIVIDU		ON INDIVIDU.ID_INDIVIDU = STAGIAIRE_PEC.ID_INDIVIDU
         		LEFT JOIN UTILISATEUR	ON UTILISATEUR.ID_UTILISATEUR = INDIVIDU.ID_UTILISATEUR
         		LEFT JOIN NATIONALITE	ON NATIONALITE.ID_NATIONALITE = INDIVIDU.ID_NATIONALITE
         		INNER JOIN CSP			ON CSP.ID_CSP			= STAGIAIRE_PEC.ID_CSP
         		INNER JOIN ETABLISSEMENT ON ETABLISSEMENT.ID_ETABLISSEMENT	= STAGIAIRE_PEC.ID_ETABLISSEMENT
         		INNER JOIN ADHERENT		ON ADHERENT.ID_ADHERENT				= ETABLISSEMENT.ID_ADHERENT
         		LEFT JOIN GROUPE on (STAGIAIRE_PEC.ID_GROUPE = GROUPE.ID_GROUPE)

         		LEFT OUTER JOIN STAGIAIRE_PEC STP ON STAGIAIRE_PEC.ID_STAGIAIRE_TUTEUR = STP.ID_STAGIAIRE_PEC
         		LEFT OUTER JOIN INDIVIDU ISTP ON STP.ID_INDIVIDU = ISTP.ID_INDIVIDU
         		left join R19 on (R19.ID_ADHERENT = ADHERENT.ID_ADHERENT and R19.ID_ACTIVITE in (SELECT ID_ACTIVITE FROM GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN'))		
         						and R19.ID_PERIODE = (select ID_PERIODE from PERIODE where id_type_periode =1 and bln_en_cours = 1))

         		)

         	END
         	ELSE 
         	BEGIN
         	SET @STAGIAIRE=@NB_STAGIAIRE 
         	END 

         SELECT @STAGIAIRE AS STAGIAIRE
         END   

         CREATE  PROCEDURE [dbo].[LEC_DET_IBAN_BY_TYPE_FACTURE] 
         	@ID_MODULE_PEC		AS INT,
         	@ID_ADHERENT		AS INT,
         	@ID_ACTION_PEC		AS INT,
         	@ID_TRANSACTION		AS INT OUT,
         	@NUM_IBAN			AS VARCHAR(34) OUT

         AS

         BEGIN

         	SET @ID_TRANSACTION = NULL
         	SET @NUM_IBAN		= 0

         	IF @ID_MODULE_PEC IS NOT  NULL
         		BEGIN

         			SELECT
         				@ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION,
         				@NUM_IBAN		= NUM_IBAN
         			FROM [TRANSACTION]
         				INNER JOIN MODULE_PEC				ON [TRANSACTION].ID_ETABLISSEMENT_OF_DEST = MODULE_PEC. ID_ETABLISSEMENT_OF
         														AND MODULE_PEC.ID_MODULE_PEC = @ID_MODULE_PEC
         				INNER JOIN SOUS_TYPE_TRANSACTION	ON SOUS_TYPE_TRANSACTION.ID_SOUS_TYPE_TRANSACTION = [TRANSACTION].ID_SOUS_TYPE_TRANSACTION
         														AND COD_SOUS_TYPE_TRANSACTION = 'REGLEMT'
         			WHERE
         				[TRANSACTION].BLN_TRANSACTION_REGLEMENT_PRINCIPAL = 1 AND [TRANSACTION].BLN_ACTIF = 1

         		END
         	ELSE
         		BEGIN

         			SELECT
         				@ID_TRANSACTION		= T.ID_TRANSACTION,
         				@NUM_IBAN			= T.NUM_IBAN,
         				@ID_ACTION_PEC		= N.ID_ACTION_PEC,
         				@ID_ADHERENT		= E.ID_ADHERENT
         			FROM 
         				[TRANSACTION] AS T
         				INNER JOIN NR140 AS N ON N.ID_ETABLISSEMENT = T.ID_ETABLISSEMENT_DEST
         				INNER JOIN ETABLISSEMENT AS E ON N.ID_ETABLISSEMENT = E.ID_ETABLISSEMENT
         				INNER JOIN SOUS_TYPE_TRANSACTION	ON SOUS_TYPE_TRANSACTION.ID_SOUS_TYPE_TRANSACTION = T.ID_SOUS_TYPE_TRANSACTION
         														AND COD_SOUS_TYPE_TRANSACTION = 'REGLEMT'
         			WHERE
         				T.BLN_TRANSACTION_REGLEMENT_PRINCIPAL = 1  AND T.BLN_ACTIF = 1 AND
         				E.ID_ADHERENT = @ID_ADHERENT AND
         				N.ID_ACTION_PEC = @ID_ACTION_PEC

         		END

         END

         CREATE PROCEDURE [dbo].[BATCH_CLOTURE_CONTRAT_PRO]
         AS   

         BEGIN  

         DECLARE   
           @ID_TYPE_EVENEMENT_CLOTURE  int,   
           @ID_ETAT_CONTRAT_PRO_TERMINE int,  
           @ID_ETAT_CONTRAT_PRO_CLOTURE int,  
           @LIB_EVENEMENT     varchar(50)  

          SELECT @ID_ETAT_CONTRAT_PRO_TERMINE = ID_ETAT_CONTRAT_PRO  
          FROM ETAT_CONTRAT_PRO  
          WHERE COD_ETAT_CONTRAT_PRO = 'TERMINE'  

          SELECT @ID_ETAT_CONTRAT_PRO_CLOTURE = ID_ETAT_CONTRAT_PRO  
          FROM  ETAT_CONTRAT_PRO  
          WHERE COD_ETAT_CONTRAT_PRO = 'CLOTURE'  

          SELECT @ID_TYPE_EVENEMENT_CLOTURE = ID_TYPE_EVENEMENT, @LIB_EVENEMENT = LIBL_TYPE_EVENEMENT  
          FROM TYPE_EVENEMENT  
          WHERE COD_TYPE_EVENEMENT = 'CLOT_PRO'  

          CREATE TABLE #TMP_CONTRAT_A_CLOTURER  
          (ID_CONTRAT_PRO int)  

          INSERT INTO #TMP_CONTRAT_A_CLOTURER  
          (ID_CONTRAT_PRO)  
          SELECT ID_CONTRAT_PRO  
          FROM CONTRAT_PRO   
          WHERE ID_ETAT_CONTRAT_PRO = @ID_ETAT_CONTRAT_PRO_TERMINE  
          AND  NOT EXISTS (  
               SELECT 1  
               FROM  SESSION_PRO  
               INNER JOIN MODULE_PRO ON SESSION_PRO.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO   
               WHERE  SESSION_PRO.BLN_ACTIF = 1  
               AND   MODULE_PRO.BLN_ACTIF = 1  
               AND   MODULE_PRO.ID_CONTRAT_PRO = CONTRAT_PRO.ID_CONTRAT_PRO   
               AND   ID_REGLEMENT_PRO_ADH IS NULL  
               AND   ID_REGLEMENT_PRO_OF IS NULL  
               )  

         union  

          SELECT ID_CONTRAT_PRO  
          FROM CONTRAT_PRO   
          WHERE ID_ETAT_CONTRAT_PRO = @ID_ETAT_CONTRAT_PRO_TERMINE  
          AND  NOT EXISTS (  
               SELECT 1  
               FROM  SESSION_PRO  
               INNER JOIN MODULE_PRO ON SESSION_PRO.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO   
               INNER JOIN REGLEMENT_PRO ON SESSION_PRO.ID_REGLEMENT_PRO_ADH  = REGLEMENT_PRO.ID_REGLEMENT_PRO   
                      OR SESSION_PRO.ID_REGLEMENT_PRO_OF  = REGLEMENT_PRO.ID_REGLEMENT_PRO  
               WHERE  SESSION_PRO.BLN_ACTIF = 1  
               AND   MODULE_PRO.BLN_ACTIF = 1  
               AND   MODULE_PRO.ID_CONTRAT_PRO = CONTRAT_PRO.ID_CONTRAT_PRO   
               AND   REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NULL  
               )  

          INSERT INTO EVENEMENT  
           (  
            ID_TYPE_EVENEMENT,    LIBL_EVENEMENT,  DAT_EVENEMENT, ID_CONTRAT_PRO  
           )  
          SELECT  
            @ID_TYPE_EVENEMENT_CLOTURE,  @LIB_EVENEMENT,  GETDATE(), ID_CONTRAT_PRO  
          FROM #TMP_CONTRAT_A_CLOTURER  
          WHERE NOT EXISTS (SELECT 1 FROM EVENEMENT WHERE ID_CONTRAT_PRO = #TMP_CONTRAT_A_CLOTURER.ID_CONTRAT_PRO AND ID_TYPE_EVENEMENT = @ID_TYPE_EVENEMENT_CLOTURE)  

          UPDATE CONTRAT_PRO   
          SET ID_ETAT_CONTRAT_PRO = @ID_ETAT_CONTRAT_PRO_CLOTURE, DAT_CLOTURE = GETDATE()  
          FROM #TMP_CONTRAT_A_CLOTURER  
          WHERE CONTRAT_PRO .ID_CONTRAT_PRO = #TMP_CONTRAT_A_CLOTURER .ID_CONTRAT_PRO   

          declare @ID_TYPE_EVEN_CLOMOPRO int
          SELECT @ID_TYPE_EVEN_CLOMOPRO = ID_TYPE_EVENEMENT,   
         		@LIB_EVENEMENT = LIBL_TYPE_EVENEMENT  
          FROM	TYPE_EVENEMENT  
          WHERE	COD_TYPE_EVENEMENT = 'CLOMOPRO'  

          INSERT INTO EVENEMENT  
          (ID_TYPE_EVENEMENT, LIBL_EVENEMENT, DAT_EVENEMENT, ID_CONTRAT_PRO, ID_MODULE_PRO)  
          select	@ID_TYPE_EVEN_CLOMOPRO, @LIB_EVENEMENT, MODULE_PRO.DAT_CLOTURE, MODULE_PRO.ID_CONTRAT_PRO, MODULE_PRO.ID_MODULE_PRO  
          from	MODULE_PRO
          where	dat_cloture is not null
          and	not exists (select id_evenement from evenement where EVENEMENT.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO and ID_TYPE_EVENEMENT = @ID_TYPE_EVEN_CLOMOPRO)

         END  

         CREATE PROCEDURE [dbo].[UPD_HISTO_CONTRAT_NON_CONFORME]
         	@COD_CONTRAT_PRO			VARCHAR(10),
         	@DAT_DEPOT_DDTE				DATETIME = null,
         	@DAT_ENREGISTREMENT_DDTE	DATETIME = null,
         	@CODE_TYPE					INT,
         	@MOTIF_CONTROL_CONFORMITE	TEXT = null,
         	@COMMENTAIRE_COMMANDITAIRE	TEXT = null
         AS

         BEGIN

         	DECLARE @ID_HISTO_CONTRAT_DGEFP INT
         	DECLARE @ID_CONTRAT_PRO INT
         	DECLARE @STATUT_HISTO INT

         	SELECT  @ID_HISTO_CONTRAT_DGEFP = HISTO_CONTRAT_DGEFP.ID_HISTO_CONTRAT_DGEFP,
         			@ID_CONTRAT_PRO			= CONTRAT_PRO.ID_CONTRAT_PRO
         	FROM	CONTRAT_PRO 
         			INNER JOIN HISTO_CONTRAT_DGEFP ON CONTRAT_PRO.ID_CONTRAT_PRO = HISTO_CONTRAT_DGEFP.ID_CONTRAT_PRO
         	WHERE	CONTRAT_PRO.COD_CONTRAT_PRO = @COD_CONTRAT_PRO
         			
         			AND HISTO_CONTRAT_DGEFP.ID_HISTO_CONTRAT_DGEFP = 
         					( 
         						SELECT	MAX(HCD.ID_HISTO_CONTRAT_DGEFP)
         						FROM	HISTO_CONTRAT_DGEFP HCD
         						WHERE	HCD.ID_CONTRAT_PRO = CONTRAT_PRO.ID_CONTRAT_PRO
         					  )

         	UPDATE	CONTRAT_PRO
         	SET		DAT_DEPOT_DDTE = @DAT_DEPOT_DDTE
         	WHERE	CONTRAT_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO

         	IF @CODE_TYPE = 18
         		BEGIN
         			SET @STATUT_HISTO = 30

         			UPDATE	CONTRAT_PRO
         			SET		DAT_ENREGISTREMENT_DDTE = @DAT_ENREGISTREMENT_DDTE
         			WHERE	CONTRAT_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO
         		END

         	ELSE IF @CODE_TYPE = 19
         		BEGIN
         			SET  @STATUT_HISTO = 100
         		END
         	ELSE IF @CODE_TYPE = 20
         		BEGIN
         			SET  @STATUT_HISTO = 50
         		END

         	UPDATE	HISTO_CONTRAT_DGEFP
         	SET		DAT_DEPOT_DDTE = @DAT_DEPOT_DDTE,
         			STATUT_HISTO = @STATUT_HISTO,
         			MOTIF_CONTROL_CONFORMITE = @MOTIF_CONTROL_CONFORMITE,
         			COMMENTAIRE_COMMANDITAIRE = @COMMENTAIRE_COMMANDITAIRE
         	WHERE	HISTO_CONTRAT_DGEFP.ID_HISTO_CONTRAT_DGEFP = @ID_HISTO_CONTRAT_DGEFP

         END

         CREATE PROCEDURE [dbo].[UPD_DATE_ENGAGEMENT_CONTRAT_PRO_ADH]
         	@CODS_CONTRAT_PRO TEXT	
         AS

         BEGIN

         	DECLARE @Item varchar(10);

         	CREATE TABLE #List(Item VARCHAR(10) collate French_CI_AI)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,1,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)-1))),
         			@CODS_CONTRAT_PRO=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)+1,DATALENGTH(@CODS_CONTRAT_PRO))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT cast(@Item as varchar)
         	END

         	IF DATALENGTH(@CODS_CONTRAT_PRO) > 0
         		begin
         			select @Item = cast(@CODS_CONTRAT_PRO as varchar(10))
         			INSERT INTO #List SELECT @Item 
         		end

         	UPDATE	ENGAGEMENT_PRO
         	SET		ENGAGEMENT_PRO.DAT_LETTRE_ENGAGEMENT_PRO_ADH = GETDATE()
         	FROM	CONTRAT_PRO 
         			INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
         			INNER JOIN ENGAGEMENT_PRO ON MODULE_PRO.ID_ENGAGEMENT_PRO = ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO
         	WHERE	CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)

         END

 CREATE PROCEDURE [dbo].[LEC_SUM_BUDGETAIRE_COMPTE]
          @ID_GROUPE    INT,
          @ID_PERIODE    INT,
          @ID_ENVELOPPE   INT,
          @VALUE     DECIMAL(18,2),
          @P_E_R     VARCHAR(1),
          @ID_TYPE_FINANCEMENT INT
         AS

         BEGIN
          DECLARE
           @ID_COMPTE INT,
           @DiffMvtBudgetaire DECIMAL(18,2)

          IF (@ID_PERIODE IS NULL)
          BEGIN
           SELECT
            @ID_PERIODE = ID_PERIODE
           FROM
            PERIODE
           WHERE 
            BLN_EN_COURS = 1
            AND BLN_ACTIF = 1
          END

          IF (@ID_ENVELOPPE IS NOT NULL)
          BEGIN

           SELECT 
            @DiffMvtBudgetaire = SUM(CASE WHEN (PARAMETRAGE_BUDGETAIRE.SENS = 'C') THEN ISNULL(MNT_MVT_BUDGETAIRE,0) WHEN (PARAMETRAGE_BUDGETAIRE.SENS = 'D') THEN ISNULL(-1 * MNT_MVT_BUDGETAIRE,0) ELSE 0 END)
           FROM
            MVT_BUDGETAIRE
            INNER JOIN PARAMETRAGE_BUDGETAIRE 
             ON PARAMETRAGE_BUDGETAIRE.ID_TYPE_MOUVEMENT = MVT_BUDGETAIRE.ID_TYPE_MOUVEMENT
              AND MVT_BUDGETAIRE.P_E_R = @P_E_R 
            INNER JOIN ENVELOPPE
             ON ENVELOPPE.ID_ENVELOPPE = MVT_BUDGETAIRE.ID_ENVELOPPE
              AND ENVELOPPE.ID_TYPE_ENVELOPPE = PARAMETRAGE_BUDGETAIRE.ID_TYPE_ENVELOPPE
           WHERE
            ENVELOPPE.ID_ENVELOPPE = @ID_ENVELOPPE
            AND @ID_ENVELOPPE IS NOT NULL

           SELECT
            @ID_TYPE_FINANCEMENT = BLN_SOLDE_POSITIF
           FROM
            ENVELOPPE
            INNER JOIN TYPE_ENVELOPPE
             ON TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = ENVELOPPE.ID_TYPE_ENVELOPPE
           WHERE
            ID_ENVELOPPE = @ID_ENVELOPPE
          END
          ELSE 
          BEGIN

           SELECT 
            @DiffMvtBudgetaire = SUM(CASE WHEN (PARAMETRAGE_BUDGETAIRE.SENS = 'C') THEN ISNULL(MNT_MVT_BUDGETAIRE,0) WHEN (PARAMETRAGE_BUDGETAIRE.SENS = 'D') THEN ISNULL(-1 * MNT_MVT_BUDGETAIRE,0) ELSE 0 END)
           FROM
            MVT_BUDGETAIRE
            INNER JOIN PARAMETRAGE_BUDGETAIRE 
             ON PARAMETRAGE_BUDGETAIRE.ID_TYPE_MOUVEMENT = MVT_BUDGETAIRE.ID_TYPE_MOUVEMENT
              AND MVT_BUDGETAIRE.P_E_R = @P_E_R 
            INNER JOIN COMPTE
             ON COMPTE.ID_COMPTE = MVT_BUDGETAIRE.ID_COMPTE
              AND COMPTE.ID_TYPE_COMPTE = PARAMETRAGE_BUDGETAIRE.ID_TYPE_COMPTE
           WHERE
            COMPTE.ID_GROUPE = @ID_GROUPE
            AND COMPTE.ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT
          END

          DECLARE
           @TOTAL AS DECIMAL(18,2)

          SET @TOTAL = (ISNULL(@DiffMvtBudgetaire,0) - ISNULL(@VALUE,0))

          SELECT
           @TOTAL AS TOTAL,
           1 AS PERMET_NEGATIF
         END

 CREATE PROCEDURE [dbo].[INS_OPERATION]
          @LIBL_OPERATION VARCHAR(50),  
          @ID_BRANCHE INT,
          @ID_AGENCE INT,
          @ID_DISPOSITIF INT,
          @ID_PERIODE INT,
          @COM_OPERATION VARCHAR(200),
          @DAT_DEBUT DATETIME,
          @DAT_FIN DATETIME,
          @ID_UTIL INT,
          @BLN_DEFAUT INT,
          @BLN_ACTIF INT,
          @ID_SCHEMAS_XML INT,
          @ID_OPERATION_FINANCIERE INT OUT
         AS 

         BEGIN
          IF (@BLN_DEFAUT IS NULL)
          BEGIN
           SET @BLN_DEFAUT = 0
          END
          IF (@BLN_ACTIF IS NULL)
          BEGIN
           SET @BLN_ACTIF = 0
          END

          IF (@BLN_DEFAUT = 1)
          BEGIN
           UPDATE OPERATION_FINANCIERE SET BLN_DEFAUT = 0 WHERE ID_PERIODE = @ID_PERIODE AND ISNULL(ID_DISPOSITIF, -1) = ISNULL(@ID_DISPOSITIF, -1)
           AND ID_AGENCE = @ID_AGENCE AND ISNULL(ID_BRANCHE, -1) = ISNULL(@ID_BRANCHE, -1) AND BLN_DEFAUT = 1
          END

          INSERT INTO OPERATION_FINANCIERE
           (
            COD_OPERATION_FINANCIERE,  
            LIBC_OPERATION_FINANCIERE,  
            LIBL_OPERATION_FINANCIERE,  
            ID_BRANCHE,
            ID_AGENCE,
            ID_DISPOSITIF,
            ID_PERIODE,
            DAT_DEBUT,
            DAT_FIN,
            DAT_CREATION,  
            DAT_MODIF,  
            BLN_ACTIF,
            BLN_DEFAUT,
            COM_MODE_FINANCEMENT,
            ID_UTILISATEUR,
            ID_SCHEMAS_XML
           )
           VALUES  
           (
            0,
            '',
            @LIBL_OPERATION,  
            @ID_BRANCHE,
            @ID_AGENCE,
            @ID_DISPOSITIF,
            @ID_PERIODE, 
            @DAT_DEBUT,
            @DAT_FIN,
            GETDATE(),
            GETDATE(),  
            @BLN_ACTIF,
            @BLN_DEFAUT,
            @COM_OPERATION,
            @ID_UTIL,
            @ID_SCHEMAS_XML
           )

          UPDATE OPERATION_FINANCIERE SET COD_OPERATION_FINANCIERE = @@IDENTITY WHERE ID_OPERATION_FINANCIERE = @@IDENTITY 

          SET @ID_OPERATION_FINANCIERE = @@IDENTITY

         END

CREATE PROCEDURE [dbo].[INS_NR67bis]

         	@ID_ETABLISSEMENT int,

         	@ID_DOCUMENT int

         AS

         BEGIN

         	IF (@ID_ETABLISSEMENT IS NULL)

         		BEGIN

         			insert into NR67bis

         				(ID_ETABLISSEMENT, ID_DOCUMENT)

         			select 

         				distinct

         				ETABLISSEMENT.ID_ETABLISSEMENT as ID_ETABLISSEMENT, @ID_DOCUMENT as ID_DOCUMENT

         			from

         				ETABLISSEMENT

         				LEFT OUTER JOIN NR67bis on NR67bis.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT and NR67bis.ID_DOCUMENT = @ID_DOCUMENT

         			where NR67bis.ID_ETABLISSEMENT is null

         			order by ETABLISSEMENT.ID_ETABLISSEMENT

         		END

         	ELSE

         		BEGIN

         			insert into NR67bis

         				(ID_ETABLISSEMENT, ID_DOCUMENT)

         			values

         				(@ID_ETABLISSEMENT, @ID_DOCUMENT)

         		END

         END

GO         

 CREATE PROCEDURE [dbo].[DEL_STAGIAIRE_MODULE_PEC]
          @ID_STAGIAIRE INT
         AS  

         BEGIN
          SET NOCOUNT ON

           DELETE PLAN_FINANCEMENT_US
           FROM   PLAN_FINANCEMENT_US
          inner join UNITE_STAGIAIRE on UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE = PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE  
           WHERE ID_STAGIAIRE_PEC = @ID_STAGIAIRE

           DELETE NR215 FROM NR215
          inner join UNITE_STAGIAIRE on UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE = NR215.ID_UNITE_STAGIAIRE  
           WHERE
              ID_STAGIAIRE_PEC = @ID_STAGIAIRE

           DELETE FROM UNITE_STAGIAIRE
           WHERE ID_STAGIAIRE_PEC = @ID_STAGIAIRE

           DELETE NR210 FROM   NR210
          inner join ARRIVEE_PIECE_PEC on ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC = NR210.ID_ARRIVEE_PIECE_PEC 
           WHERE
              ID_STAGIAIRE_PEC = @ID_STAGIAIRE

           DELETE FROM ARRIVEE_PIECE_PEC
           WHERE ID_STAGIAIRE_PEC = @ID_STAGIAIRE

           DELETE FROM STAGIAIRE_PEC_ATTRIBUTS 
           WHERE ID_STAGIAIRE_PEC = @ID_STAGIAIRE

           DELETE FROM STAGIAIRE_PEC_CDC 
           WHERE ID_STAGIAIRE_PEC = @ID_STAGIAIRE

           DELETE FROM STAGIAIRE_PEC 
           WHERE ID_STAGIAIRE_PEC = @ID_STAGIAIRE
          END

GO

         CREATE PROCEDURE [dbo].[INS_DEDUCTION]
         	@ID_ADHERENT	int, 
         	@ID_ACTIVITE	int, 
         	@ID_PERIODE		int, 
         	@MNT_HT			decimal(18,2)
         AS

         BEGIN
         	IF (@MNT_HT <> 0) 
         	BEGIN
         		INSERT INTO DEDUCTION
         			(
         				ID_ADHERENT, 
         				ID_ACTIVITE, 
         				ID_PERIODE, 
         				MNT_HT
         			)
         		VALUES
         			(
         				@ID_ADHERENT, 
         				@ID_ACTIVITE, 
         				@ID_PERIODE, 
         				@MNT_HT
         			)
         	END
         END 

GO

         CREATE PROCEDURE [dbo].[BATCH_PLURI_ANNUEL_PRO]
         AS
         BEGIN

         SET NOCOUNT ON;

         DECLARE @last_run_date datetime
         SELECT @last_run_date = max(DAT_CREATION) from MVT_BUDGETAIRE_PERIODE 
         	where TYPE_ENREGISTREMENT like '%P'
         	and TYPE_ENREGISTREMENT not like '%RP'
         IF (@last_run_date is null)
         	SELECT @last_run_date = min(DAT_MVT_BUDGETAIRE) from MVT_BUDGETAIRE

         select	distinct MVT_BUDGETAIRE.id_MODULE_PRO,
         		cast(null as DATETIME) AS eng_initial_date,
         		SALARIE_PRO.ID_BRANCHE
         into #tmp_bpa_modules
         from MVT_BUDGETAIRE
         	inner join MODULE_PRO on (MODULE_PRO.id_MODULE_PRO = MVT_BUDGETAIRE.id_MODULE_PRO)
         	inner join CONTRAT_PRO on (MODULE_PRO.id_CONTRAT_PRO = CONTRAT_PRO.id_CONTRAT_PRO)
         	inner join EVENEMENT	on (EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT)
         	INNER JOIN SALARIE_PRO ON (CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO)
         WHERE   MVT_BUDGETAIRE.ID_MODULE_PRO is not null
         		and DAT_MVT_BUDGETAIRE >= @last_run_date 
         		and coalesce(BLN_REPRISE_ADHOC,0)=0 

         		and coalesce(BLN_REPRISE_NESSIE,0)=0 

         		and MODULE_PRO.DAT_FIN > '20061231'

         		and ((ID_TYPE_MOUVEMENT = 21 and ID_TYPE_EVENEMENT not in (23, 24, 25, 26, 29, 30)) or ID_TYPE_MOUVEMENT <> 21) 

         		and P_E_R = 'E'

         		and CONTRAT_PRO.BLN_ACTIF = 1
         		and MODULE_PRO.BLN_ACTIF = 1

         DECLARE @ID_MODULE int
         DECLARE @ID_DISPOSITIF	int
         DECLARE @ID_BRANCHE int
         DECLARE @ID_ENVELOPPE int
         DECLARE @ID_COMPTE int
         DECLARE @ID_TYPE_FINANCEMENT int
         DECLARE @MNT decimal(18,2)
         DECLARE @ID_SESSION int

         declare initial_cursor cursor for
         	select	MVT_BUDGETAIRE.ID_DISPOSITIF, 
         			#tmp_bpa_modules.id_branche, 
         			id_enveloppe, 
         			MVT_BUDGETAIRE.id_MODULE_PRO,
         			sum(MNT_MVT_BUDGETAIRE) as MNT,
         			ID_TYPE_FINANCEMENT, 
         			ID_COMPTE
         	from	MVT_BUDGETAIRE
         	join	#tmp_bpa_modules
         	on		MVT_BUDGETAIRE.id_MODULE_PRO = #tmp_bpa_modules.id_MODULE_PRO

         	join	MODULE_PRO
         	on		MODULE_PRO.ID_MODULE_PRO = #tmp_bpa_modules.ID_MODULE_PRO
         	join	ENGAGEMENT_PRO
         	on		ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO = MODULE_PRO.ID_ENGAGEMENT_PRO
         	and		ENGAGEMENT_PRO.ID_TYPE_ENGAGEMENT_PRO = 1 
         	join	EVENEMENT
         	on		EVENEMENT.ID_ENGAGEMENT_PRO = ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO
         	and		EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT
         	and		EVENEMENT.ID_TYPE_EVENEMENT = 21 

         	where	MVT_BUDGETAIRE.P_E_R = 'E'	
         	group by MVT_BUDGETAIRE.ID_DISPOSITIF, #tmp_bpa_modules.id_branche, id_enveloppe,ID_TYPE_FINANCEMENT, ID_COMPTE, MVT_BUDGETAIRE.id_MODULE_PRO

         OPEN initial_cursor
         FETCH NEXT FROM initial_cursor
         INTO @ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @ID_MODULE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         WHILE @@FETCH_STATUS = 0
         BEGIN

         	exec INS_MVT_BUDG_PERIODE_PRO
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	@ID_COMPTE,
         	@ID_TYPE_FINANCEMENT,
         	'EIP',
         	@MNT,
         	null, 
         	null

         	FETCH NEXT FROM initial_cursor
         	INTO @ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @ID_MODULE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         END
         CLOSE initial_cursor
         DEALLOCATE initial_cursor

         DECLARE mvt_cursor CURSOR FOR
         	select 
         			MVT_BUDGETAIRE.ID_DISPOSITIF, 
         			#tmp_bpa_modules.id_branche, 
         			id_enveloppe, 
         			MVT_BUDGETAIRE.id_MODULE_PRO,
         			sum(MNT_MVT_BUDGETAIRE) as MNT,
         			ID_TYPE_FINANCEMENT, 
         			ID_COMPTE
         	from	MVT_BUDGETAIRE
         	join	#tmp_bpa_modules 
         	on		MVT_BUDGETAIRE.id_MODULE_PRO = #tmp_bpa_modules.id_MODULE_PRO

         	join	MODULE_PRO
         	on		MODULE_PRO.ID_MODULE_PRO = #tmp_bpa_modules.ID_MODULE_PRO
         	join	ENGAGEMENT_PRO
         	on		ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO = MODULE_PRO.ID_ENGAGEMENT_PRO
         	and		ENGAGEMENT_PRO.ID_TYPE_ENGAGEMENT_PRO = 2 
         	join	EVENEMENT
         	on		EVENEMENT.ID_ENGAGEMENT_PRO = ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO
         	and		EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT

         	WHERE  
         			((ID_TYPE_MOUVEMENT =21 and ID_TYPE_EVENEMENT not in (24, 25, 26, 27, 28, 29, 30)) or ID_TYPE_MOUVEMENT<>21) 
         	and		P_E_R = 'E'
         	group by MVT_BUDGETAIRE.ID_DISPOSITIF, #tmp_bpa_modules.id_branche, id_enveloppe,ID_TYPE_FINANCEMENT, ID_COMPTE, MVT_BUDGETAIRE.id_MODULE_PRO

         OPEN mvt_cursor
         FETCH NEXT FROM mvt_cursor
         INTO @ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @ID_MODULE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         WHILE @@FETCH_STATUS = 0
         BEGIN

         	if @MNT  <> 0
         	begin

         		exec INS_MVT_BUDG_PERIODE_PRO
         		@ID_MODULE,
         		@ID_DISPOSITIF ,
         		@ID_BRANCHE,
         		@ID_ENVELOPPE,
         		@ID_COMPTE,
         		@ID_TYPE_FINANCEMENT,
         		'ECP',
         		@MNT,
         		null, 
         		null
         	end
         	FETCH NEXT FROM mvt_cursor
         	INTO @ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @ID_MODULE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         END
         CLOSE mvt_cursor
         DEALLOCATE mvt_cursor

         DECLARE clotures_cursor CURSOR FOR
         select	MVT_BUDGETAIRE.ID_MODULE_PRO,
         		ID_DISPOSITIF, 
         		SALARIE_PRO.ID_BRANCHE, 
         		ID_ENVELOPPE, 
         		sum(MNT_MVT_BUDGETAIRE) as MNT,
         		ID_TYPE_FINANCEMENT, 
         		ID_COMPTE
         from dbo.MVT_BUDGETAIRE
         	inner join EVENEMENT	on (EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT)
         	inner join MODULE_PRO	on (MODULE_PRO.ID_MODULE_PRO = MVT_BUDGETAIRE.ID_MODULE_PRO)
         	inner join CONTRAT_PRO	on (MODULE_PRO.ID_CONTRAT_PRO = CONTRAT_PRO.ID_CONTRAT_PRO)
         	INNER JOIN SALARIE_PRO ON (CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO) 
         where 
         	ID_TYPE_EVENEMENT in(25) 
         	and CONTRAT_PRO.DAT_CLOTURE is not null 
         	and DAT_MVT_BUDGETAIRE >= @last_run_date 
         	and P_E_R = 'E' 

         	and coalesce(CONTRAT_PRO.BLN_REPRISE_ADHOC,0) = 0  
         	and coalesce(CONTRAT_PRO.BLN_REPRISE_NESSIE,0) = 0
         	and MODULE_PRO.DAT_FIN > '20061231' 
         group by MVT_BUDGETAIRE.ID_MODULE_PRO, ID_DISPOSITIF, SALARIE_PRO.ID_BRANCHE, ID_ENVELOPPE, ID_TYPE_FINANCEMENT, ID_COMPTE

         OPEN clotures_cursor
         FETCH NEXT FROM clotures_cursor
         INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         WHILE @@FETCH_STATUS = 0
         BEGIN
         	if @MNT  <> 0
         		exec INS_MVT_BUDG_PERIODE_PRO
         		@ID_MODULE,
         		@ID_DISPOSITIF ,
         		@ID_BRANCHE,
         		@ID_ENVELOPPE,
         		@ID_COMPTE,
         		@ID_TYPE_FINANCEMENT,
         		'CP',
         		@MNT,
         		null, 
         		null

         	FETCH NEXT FROM clotures_cursor
         	INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         END
         CLOSE clotures_cursor
         DEALLOCATE clotures_cursor

         declare @dat_REGLEMENT_PRO datetime

         declare @id_reglement_of int
         declare @id_reglement_adh int

         DECLARE REGLEMENT_PROs_cursor CURSOR FOR
         select	MVT_BUDGETAIRE.ID_MODULE_PRO,
         		ID_DISPOSITIF, 
         		SALARIE_PRO.ID_BRANCHE, 
         		ID_ENVELOPPE, 
         		sum(MNT_MVT_BUDGETAIRE) as MNT,
         		ID_TYPE_FINANCEMENT, 
         		ID_COMPTE,
         		SESSION_PRO.ID_SESSION_PRO,
         		min(SESSION_PRO.ID_REGLEMENT_PRO_OF),
         		min(SESSION_PRO.ID_REGLEMENT_PRO_ADH)
         from dbo.MVT_BUDGETAIRE
         	inner join MODULE_PRO	on (MODULE_PRO.ID_MODULE_PRO = MVT_BUDGETAIRE.ID_MODULE_PRO)
         	inner join CONTRAT_PRO	on (CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO)
         	INNER JOIN SALARIE_PRO ON (CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO) 
         	inner join EVENEMENT on (EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT)
         	inner join SESSION_PRO on (EVENEMENT.ID_SESSION_PRO = SESSION_PRO.ID_SESSION_PRO
         		and MVT_BUDGETAIRE.ID_SESSION_PRO = SESSION_PRO.ID_SESSION_PRO
         	)

         where DAT_MVT_BUDGETAIRE >= @last_run_date 
         	and P_E_R = 'R'
         	and MODULE_PRO.DAT_FIN > '20061231'

         	and CONTRAT_PRO.BLN_ACTIF = 1
         	and MODULE_PRO.BLN_ACTIF = 1
         	and EVENEMENT.ID_TYPE_EVENEMENT in (24,29,30) 
         	and (SESSION_PRO.ID_REGLEMENT_PRO_of is not null or SESSION_PRO.ID_REGLEMENT_PRO_adh is not null) 

         	and	(
         		(coalesce(CONTRAT_PRO.BLN_REPRISE_ADHOC, 0) = 1 and EVENEMENT.DAT_EVENEMENT >= '20090317') 
         		OR
         		(coalesce(CONTRAT_PRO.BLN_REPRISE_NESSIE, 0) = 1 and EVENEMENT.DAT_EVENEMENT >= '20130101') 
         		OR
         		(coalesce(CONTRAT_PRO.BLN_REPRISE_ADHOC, 0) = 0 and coalesce(CONTRAT_PRO.BLN_REPRISE_NESSIE, 0) = 0)
         		)
         group by MVT_BUDGETAIRE.ID_MODULE_PRO, ID_DISPOSITIF, SALARIE_PRO.ID_BRANCHE, ID_ENVELOPPE, ID_TYPE_FINANCEMENT, ID_COMPTE, SESSION_PRO.ID_SESSION_PRO

         OPEN REGLEMENT_PROs_cursor
         FETCH NEXT FROM REGLEMENT_PROs_cursor
         INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE, @ID_SESSION, @id_reglement_of, @id_reglement_adh
         WHILE @@FETCH_STATUS = 0
         BEGIN
         	if @MNT  <> 0
         	begin 
         		declare @dat_reglement datetime
         		select @dat_reglement = DAT_VALID_REGLEMENT
         		from REGLEMENT_PRO
         		where ID_REGLEMENT_PRO = @id_reglement_of
         		and BLN_ACTIF = 1
         		if (@dat_reglement is null)
         		begin
         			select @dat_reglement = DAT_VALID_REGLEMENT
         			from REGLEMENT_PRO
         			where ID_REGLEMENT_PRO = @id_reglement_adh
         			and BLN_ACTIF = 1
         		end

         		if (@dat_reglement is not null)
         			exec INS_MVT_BUDG_PERIODE_PRO
         			@ID_MODULE,
         			@ID_DISPOSITIF ,
         			@ID_BRANCHE,
         			@ID_ENVELOPPE,
         			@ID_COMPTE,
         			@ID_TYPE_FINANCEMENT,
         			'RP',
         			@MNT,
         			@ID_SESSION, 
         			null 
         	end
         	FETCH NEXT FROM REGLEMENT_PROs_cursor
         	INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE, @ID_SESSION, @id_reglement_of, @id_reglement_adh
         END
         CLOSE REGLEMENT_PROs_cursor
         DEALLOCATE REGLEMENT_PROs_cursor

         select distinct ID_MODULE_PRO
         into	#cp_table
         from	MODULE_PRO
         join 	CONTRAT_PRO 
         on		(MODULE_PRO.ID_CONTRAT_PRO = CONTRAT_PRO.ID_CONTRAT_PRO)
         where	CONTRAT_PRO.DAT_CLOTURE is not null 
         and		CONTRAT_PRO.DAT_CLOTURE > @last_run_date
         and		(BLN_REPRISE_ADHOC = 1 or BLN_REPRISE_NESSIE = 1)
         and		MODULE_PRO.DAT_FIN > '20061231' 

         DECLARE cp_cursor CURSOR FOR
         select  
         	MVT_BUDGETAIRE_PERIODE.ID_MODULE_PRO, 
         	ID_DISPOSITIF,
         	ID_BRANCHE,
         	ID_ENVELOPPE,
         	-1* sum(
         		case when TYPE_ENREGISTREMENT in  ('EIP', 'ECP', 'IRP') then MNT_MVT_BUDG_PERIODE
         		else -1*MNT_MVT_BUDG_PERIODE end) as MNT_CLOTURE,
         	ID_COMPTE, 
         	ID_TYPE_FINANCEMENT
         from MVT_BUDGETAIRE_PERIODE
         	inner join #cp_table on (#cp_table.ID_MODULE_PRO = MVT_BUDGETAIRE_PERIODE.ID_MODULE_PRO )
         where 
         	ID_MODULE_PEC is null
         	and MVT_BUDGETAIRE_PERIODE.TYPE_ENREGISTREMENT in  ('EIP', 'ECP', 'IRP', 'RP', 'RRP')
         group by 
         	MVT_BUDGETAIRE_PERIODE.ID_MODULE_PRO, 
         	ID_DISPOSITIF,
         	ID_BRANCHE,
         	ID_ENVELOPPE,
         	ID_COMPTE, 
         	ID_TYPE_FINANCEMENT

         OPEN cp_cursor
         FETCH NEXT FROM cp_cursor
         INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         WHILE @@FETCH_STATUS = 0
         BEGIN
         		exec INS_MVT_BUDG_PERIODE_PRO
         		@ID_MODULE,
         		@ID_DISPOSITIF ,
         		@ID_BRANCHE,
         		@ID_ENVELOPPE,
         		@ID_COMPTE,
          		@ID_TYPE_FINANCEMENT,
         		'CP',
         		@MNT,
         		null, 
         		null 

         	FETCH NEXT FROM cp_cursor
         	INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         END
         CLOSE cp_cursor
         DEALLOCATE cp_cursor

         DECLARE cmp_cursor CURSOR FOR
         select  
         	MVT_BUDGETAIRE.ID_MODULE_PRO, 
         	ID_DISPOSITIF,
         	SALARIE_PRO.ID_BRANCHE,
         	ID_ENVELOPPE,
         	sum(MNT_MVT_BUDGETAIRE), 
         	ID_COMPTE, 
         	ID_TYPE_FINANCEMENT
         from MVT_BUDGETAIRE
         	inner join EVENEMENT on (EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT)
         	inner join MODULE_PRO	on (MODULE_PRO.ID_MODULE_PRO = MVT_BUDGETAIRE.ID_MODULE_PRO)
         	inner join CONTRAT_PRO on (MODULE_PRO.id_CONTRAT_PRO = CONTRAT_PRO.id_CONTRAT_PRO)
         	INNER JOIN SALARIE_PRO ON (CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO)
         where 
         	MVT_BUDGETAIRE.P_E_R = 'E' 
         	and EVENEMENT.ID_TYPE_EVENEMENT = 26
         	and CONTRAT_PRO.BLN_REPRISE_ADHOC = 0 
         	and CONTRAT_PRO.BLN_REPRISE_NESSIE = 0
         	and MODULE_PRO.DAT_CLOTURE is not null
         	and MODULE_PRO.DAT_CLOTURE > @last_run_date
         group by 
         	MVT_BUDGETAIRE.ID_MODULE_PRO, 
         	ID_DISPOSITIF,
         	ID_BRANCHE,
         	ID_ENVELOPPE,
         	ID_COMPTE, 
         	ID_TYPE_FINANCEMENT

         OPEN cmp_cursor
         FETCH NEXT FROM cmp_cursor
         INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         WHILE @@FETCH_STATUS = 0
         BEGIN
         		exec INS_MVT_BUDG_PERIODE_PRO
         		@ID_MODULE,
         		@ID_DISPOSITIF ,
         		@ID_BRANCHE,
         		@ID_ENVELOPPE,
         		@ID_COMPTE,
          		@ID_TYPE_FINANCEMENT,
         		'CMP',
         		@MNT,
         		null, 
         		null 

         	FETCH NEXT FROM cmp_cursor
         	INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         END
         CLOSE cmp_cursor
         DEALLOCATE cmp_cursor

         select	distinct ID_MODULE_PRO
         into	#enp_table
         from	MVT_BUDGETAIRE_PERIODE
         where	ID_MODULE_PRO is not null
         and		DAT_CREATION > @last_run_date
         and		TYPE_ENREGISTREMENT in  ('EIP', 'ECP', 'IRP', 'RP', 'RRP', 'CP', 'CMP')

         delete	MVT_BUDGETAIRE_PERIODE
         from	MVT_BUDGETAIRE_PERIODE
         join	#enp_table 
         on		#enp_table.ID_MODULE_PRO = MVT_BUDGETAIRE_PERIODE.ID_MODULE_PRO
         where	MVT_BUDGETAIRE_PERIODE.TYPE_ENREGISTREMENT = 'ENP'

         insert into MVT_BUDGETAIRE_PERIODE
         (DAT_CREATION, MNT_MVT_BUDG_PERIODE, ID_PERIODE, ID_DISPOSITIF, ID_BRANCHE, ID_MODULE_PRO, ID_ENVELOPPE, ID_COMPTE, ID_TYPE_FINANCEMENT, TYPE_ENREGISTREMENT)
         select  
         		GETDATE(),
         		sum(
         			case when TYPE_ENREGISTREMENT in  ('EIP', 'ECP', 'IRP', 'CMP', 'CP') then MNT_MVT_BUDG_PERIODE
         			else -1*MNT_MVT_BUDG_PERIODE end) as MNT_ENP,
         		MVT_BUDGETAIRE_PERIODE.ID_PERIODE,
         		ID_DISPOSITIF,
         		ID_BRANCHE,
         		MVT_BUDGETAIRE_PERIODE.ID_MODULE_PRO, 
         		ID_ENVELOPPE,
         		ID_COMPTE, 
         		ID_TYPE_FINANCEMENT,
         		'ENP'
         from	MVT_BUDGETAIRE_PERIODE
         join	#enp_table 
         on		#enp_table.ID_MODULE_PRO = MVT_BUDGETAIRE_PERIODE.ID_MODULE_PRO
         where	MVT_BUDGETAIRE_PERIODE.TYPE_ENREGISTREMENT in  ('EIP', 'ECP', 'IRP', 'RP', 'RRP',  'CMP', 'CP')

         group by 
         	MVT_BUDGETAIRE_PERIODE.ID_MODULE_PRO, 
         	ID_PERIODE,
         	ID_DISPOSITIF,
         	ID_BRANCHE,
         	ID_ENVELOPPE,
         	ID_COMPTE, 
         	ID_TYPE_FINANCEMENT

         update	t1
         set		t1.BLN_EN_POS = 1
         from	MVT_BUDGETAIRE_PERIODE t1
         join	(
         		select	ID_MODULE_PRO, ID_DISPOSITIF
         		from	MVT_BUDGETAIRE_PERIODE
         		where	TYPE_ENREGISTREMENT = 'ENP'
         		group by ID_MODULE_PRO, ID_DISPOSITIF
         		having SUM(cast(MNT_MVT_BUDG_PERIODE as decimal(18,2))) > 0
         		) t2
         on		t1.ID_MODULE_PRO = t2.ID_MODULE_PRO
         and		t1.ID_DISPOSITIF = t2.ID_DISPOSITIF
         and		t1.TYPE_ENREGISTREMENT = 'ENP'

         update	MVT_BUDGETAIRE_PERIODE 
         set		BLN_EN_POS = 0
         where	TYPE_ENREGISTREMENT = 'ENP'
         and		BLN_EN_POS is null

         
         
         END

         CREATE PROCEDURE [dbo].[ProcGetModulePECId]
         	@COD_MODULE varchar(18) 
         AS
         BEGIN

         	if CHARINDEX('/',@COD_MODULE, 0) = 0 and LEN(@COD_MODULE) > 4
         	begin
         		set @COD_MODULE = SUBSTRING(@COD_MODULE,0, LEN(@COD_MODULE)-3)+'/'+SUBSTRING(@COD_MODULE, LEN(@COD_MODULE)-3,4)
         	end

         	if CHARINDEX(' ',@COD_MODULE, 0) = 0
         	begin
         		declare @n int
         		set @n = CHARINDEX('/',@COD_MODULE, 0)
         		if @n > 2
         			set @COD_MODULE = SUBSTRING(@COD_MODULE,0,@n-2)+' '+SUBSTRING(@COD_MODULE,@n-2,18)
         	end
         	if len(@COD_MODULE) < 14
         		set @COD_MODULE = replicate('0',14- len(@COD_MODULE)) + @COD_MODULE
         	declare @module_id int
         	select @module_id = ID_MODULE_PEC 
         	from MODULE_PEC where COD_MODULE_PEC = @COD_MODULE
         	select 'Return Value'= @module_id	
         END

         CREATE PROCEDURE [dbo].[EDT_LETTRE_CONTRAT_PRO_REMB_SAL_ADH_TUTORALE]
         @ID_ETABLISSEMENT	INT,
         	@ID_BENEFICIAIRE	INT,
         	@TYPE_BENEFICIAIRE	INT,
         	@ID_ADRESSE			INT,
         	@ID_CONTACT			INT,
         	@_ID_MODULE_PRO INT,
         	@_COD_CONTRAT_PRO	VARCHAR(10)

         AS
         BEGIN

         	DECLARE @RELANCE INT
         	SET @RELANCE = 0

         	SELECT	ADHERENT.ID_ADHERENT,
         			ADHERENT.ID_TYPE_TVA,
         			ADHERENT.COD_ADHERENT,
         			AGENCE.ID_AGENCE,
         			AGENCE.COD_POSTAL,
         			AGENCE.LIB_VILLE,
         			AGENCE.LIB_NOM_CONSEILLER,
         			AGENCE.LIB_PNM_CONSEILLER,
         			ADHERENT.LIB_RAISON_SOCIALE
         	INTO	#TEMP_REGLEMENT_REFERENCE
         	FROM	ADHERENT
         			INNER JOIN AGENCE			ON AGENCE.ID_AGENCE = ADHERENT.ID_AGENCE
         			INNER JOIN ETABLISSEMENT	ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         	WHERE	(ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT);

         	SELECT	CONTRAT_PRO.ID_CONTRAT_PRO,
         			CONTRAT_PRO.COD_CONTRAT_PRO,
         			CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_DEB_CONTRAT, 3) AS DAT_DEBUT,
         			CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_FIN_CONTRAT, 3) AS DAT_FIN,
         			INDIVIDU.NOM_INDIVIDU, 
         			INDIVIDU.PRENOM_INDIVIDU,
         			Case when INDIVIDU.BLN_MASCULIN=1 then 'Monsieur' else 'Madame' end as Title ,
         			CONTRAT_PRO.LIB_LIEU_FORMATION
         	INTO	#TMP_CONTRAT_PRO
         	FROM	CONTRAT_PRO
         			INNER JOIN SALARIE_PRO ON CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO 
         			INNER JOIN INDIVIDU ON SALARIE_PRO.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU
         	WHERE   CONTRAT_PRO.COD_CONTRAT_PRO = @_COD_CONTRAT_PRO;

         	Select INDIVIDU.NOM_INDIVIDU AS NOM_TUTEUR, 
         			INDIVIDU.PRENOM_INDIVIDU as PRENOM_TUTEUR,
         			Case when INDIVIDU.BLN_MASCULIN=1 then 'Monsieur' else 'Madame' end as TITLE_TUTEUR
         			INTO #TUTEUR
         			from INDIVIDU 
         			inner join  CONTRAT_PRO on CONTRAT_PRO.ID_TUTEUR=INDIVIDU.ID_INDIVIDU
         				AND   CONTRAT_PRO.COD_CONTRAT_PRO = @_COD_CONTRAT_PRO;

         		SELECT  ROUND(MODULE_PRO.TAUX_HORAIRE_MODULE,2) as TAUX_HORAIRE_MODULE 
         		INTO #TEMP_MODULE FROM MODULE_PRO
         		where MODULE_PRO.ID_MODULE_PRO=@_ID_MODULE_PRO;

         SELECT TAU_TVA
         INTO #TVA from R26
         inner join #TEMP_REGLEMENT_REFERENCE on #TEMP_REGLEMENT_REFERENCE.ID_TYPE_TVA = R26.ID_TYPE_TVA
         inner join PERIODE on PERIODE.ID_PERIODE=R26.ID_PERIODE and PERIODE.ID_TYPE_PERIODE=2 and (DATEDIFF(DAY,PERIODE.DAT_DEB_PERIODE,GETDATE()) >0)
         order by PERIODE.DAT_DEB_PERIODE desc

         	IF( (SELECT COUNT(*) FROM ECHEANCE_MODULE_PRO WHERE ECHEANCE_MODULE_PRO.ID_MODULE_PRO=@_ID_MODULE_PRO AND (DATEDIFF(DAY,ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE1_PREV,GETDATE()) >0) AND (ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE1_EFF IS NULL)) >0)
         			BEGIN
         				SET @RELANCE = 1
         			END
         		ELSE
         			IF ((SELECT COUNT(*) FROM ECHEANCE_MODULE_PRO WHERE ECHEANCE_MODULE_PRO.ID_MODULE_PRO=@_ID_MODULE_PRO AND (DATEDIFF(DAY,ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE2_PREV,GETDATE()) >0) AND (ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE2_EFF IS NULL)) >0)
         					BEGIN
         						SET @RELANCE = 2
         					END

         		ELSE
         			BEGIN
         				IF ((SELECT COUNT(*) FROM ECHEANCE_MODULE_PRO WHERE ECHEANCE_MODULE_PRO.ID_MODULE_PRO=@_ID_MODULE_PRO AND (DATEDIFF(DAY,ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE3_PREV,GETDATE()) >0) AND (ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE3_EFF IS NULL)) >0)
         					BEGIN
         						SET @RELANCE = 3
         					END
         			END;

         	WITH XMLNAMESPACES (
         		DEFAULT 'EDT_LETTRE_CONTRAT_PRO_REMB_SAL_ADH_TUTORALE'
         	)

         	SELECT
         				CASE WHEN (@RELANCE > 0) THEN 
         			(
         				SELECT
         				(
         					SELECT CASE WHEN @RELANCE=0 THEN null ELSE @RELANCE END
         					FOR XML RAW('NUM_RELANCE'), ELEMENTS, TYPE
         				),
         				( 
         					SELECT  CASE WHEN @RELANCE=0 THEN ''  WHEN @RELANCE=1 THEN 'Šre' ELSE 'Šme' END
         					FOR XML RAW('niŠme'), ELEMENTS, TYPE
         				),
         				( 
         					SELECT  CASE WHEN  @RELANCE=0 THEN '' ELSE ' RELANCE' END
         					FOR XML RAW('RELANCE'), ELEMENTS, TYPE
         				)
         				FOR XML RAW('TOP') ,ELEMENTS,TYPE
         			)end,
         			(

         				SELECT	              	

         					dbo.GetXmlAgenceContact(ENTETE.ID_AGENCE) as EMETTEUR,

         					dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) as BENEFICIAIRE,

         					(
         						SELECT	REFERENCE.LIB_VILLE,
         								dbo.GetFullDate(getDate()) as DATE

         						FROM #TEMP_REGLEMENT_REFERENCE AS REFERENCE     
         						FOR XML AUTO, ELEMENTS, TYPE
         					)
         				FROM #TEMP_REGLEMENT_REFERENCE AS ENTETE     
         				FOR XML AUTO, ELEMENTS, TYPE
         			),
         			(

         				SELECT	
         						DEMANDE.COD_ADHERENT,
         						DEMANDE.LIB_RAISON_SOCIALE AS LIB_RAISON_SOCIALE_ADHERENT,
         						(
         							SELECT 
         									CORPS.ID_CONTRAT_PRO,
         									CORPS.DAT_DEBUT,
         									CORPS.DAT_FIN,
         									CORPS.Title,
         									CORPS.PRENOM_INDIVIDU,
         									CORPS.NOM_INDIVIDU
         							FROM #TMP_CONTRAT_PRO  AS CORPS 
         							FOR XML PATH(''), ELEMENTS, TYPE
         						),	
         						(
         							SELECT 
         									TUTEUR.TITLE_TUTEUR,
         									TUTEUR.PRENOM_TUTEUR,
         									TUTEUR.NOM_TUTEUR
         							FROM #TUTEUR  AS TUTEUR 
         							FOR XML PATH(''), ELEMENTS, TYPE
         						)
         				FROM  #TEMP_REGLEMENT_REFERENCE AS DEMANDE
         				FOR XML AUTO, ELEMENTS, TYPE
         			),

         			(
         			select

         			(
         				SELECT	top 1
         						dbo.GetContactSalutation(@ID_CONTACT, 1)
         				FROM CIVILITE
         				FOR XML PATH('POLITESSE_HAUT'), TYPE
         			),
         			CAST(CORPS.TAUX_HORAIRE_MODULE AS decimal(10,2)) as TAUX_HORAIRE_MODULE,
         			(
         				SELECT 	top 1 CAST((TAU_TVA * 100) AS decimal(10,2)) as TAU_TVA from #TVA
         					FOR XML PATH(''), ELEMENTS, TYPE
         			),

         			(
         				SELECT	top 1
         						dbo.GetContactSalutation(@ID_CONTACT, 0)
         				FROM CIVILITE
         				FOR XML PATH('POLITESSE_BAS'), TYPE
         			)
         				FROM #TEMP_MODULE as CORPS
         				FOR XML AUTO, ELEMENTS, TYPE
         			),

         			(

         				SELECT	SIGNATURE.LIB_PNM_CONSEILLER,
         						SIGNATURE.LIB_NOM_CONSEILLER
         				FROM
         						#TEMP_REGLEMENT_REFERENCE as SIGNATURE
         				FOR XML AUTO, ELEMENTS, TYPE
         			)

         	FROM	#TEMP_REGLEMENT_REFERENCE as LETTRE
         	FOR XML AUTO, ELEMENTS

         END

         CREATE PROCEDURE [dbo].[UPD_DATE_ENGAGEMENT_CONTRAT_PRO_OF]
         	@CODS_CONTRAT_PRO TEXT	
         AS

         BEGIN

         	DECLARE @Item varchar(10);

         	CREATE TABLE #List(Item VARCHAR(10) collate French_CI_AI)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,1,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)-1))),
         			@CODS_CONTRAT_PRO=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)+1,DATALENGTH(@CODS_CONTRAT_PRO))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT cast(@Item as varchar)
         	END

         	IF DATALENGTH(@CODS_CONTRAT_PRO) > 0
         		begin
         			select @Item = cast(@CODS_CONTRAT_PRO as varchar(10))
         			INSERT INTO #List SELECT @Item 
         		end

         	UPDATE	ENGAGEMENT_PRO
         	SET		ENGAGEMENT_PRO.DAT_LETTRE_ENGAGEMENT_PRO_OF = GETDATE()
         	FROM	CONTRAT_PRO 
         			INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
         			INNER JOIN ENGAGEMENT_PRO ON MODULE_PRO.ID_ENGAGEMENT_PRO = ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO
         	WHERE	CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)

         END

         CREATE PROCEDURE dbo.BATCH_MISE_A_JOUR_ETATS_ADP
         	@NOMBRE_JOURS	INT			
         AS
         BEGIN
         	DECLARE
         		@DATE	DATETIME 
         	SET
         		@DATE = DATEADD(DAY,@NOMBRE_JOURS, GETDATE());

         	UPDATE
         		AUTORISATION_PRELEVEMENT 
         	SET
         		ID_ETAT = 3,
         		DAT_VALIDATION = GETDATE()
         	WHERE
         		ID_ETAT = 2
         		AND (DATEADD(DAY,@NOMBRE_JOURS, DAT_SIGNATURE)<=GETDATE())

         	;WITH AUTORISATION_CADUQUE
         	AS
         	(
         		SELECT
         			AP.ID_AUTORISATION_PRELEVEMENT
         		FROM
         			AUTORISATION_PRELEVEMENT AP
         			LEFT JOIN ETAT_AUTORISATION_PRELEVEMENT EAP
         				ON EAP.ID_ETAT = AP.ID_ETAT
         			LEFT JOIN
         			(
         				SELECT
         					PC.ID_AUTORISATION_PRELEVEMENT,
         					MAX(PC.DAT_GENERATION_SEPA) AS DAT_LAST_SEPA
         				FROM
         					PRELEVEMENT_COLLECTE PC
         				GROUP BY
         					PC.ID_AUTORISATION_PRELEVEMENT
         			) LAST_PRELEVEMENT
         				ON LAST_PRELEVEMENT.ID_AUTORISATION_PRELEVEMENT = AP.ID_AUTORISATION_PRELEVEMENT
         		WHERE
         			(
         				LAST_PRELEVEMENT.DAT_LAST_SEPA IS NULL
         				OR
         				(DATEDIFF(DAY,LAST_PRELEVEMENT.DAT_LAST_SEPA, DATEADD(MONTH, -36, GETDATE())) > 0)
         			)
         			AND
         			(DATEDIFF(DAY,AP.DAT_CREATION, DATEADD(MONTH, -36, GETDATE())) > 0)
         			AND
         			(
         				AP.DAT_SIGNATURE IS NULL
         				OR
         				(DATEDIFF(DAY,AP.DAT_SIGNATURE, DATEADD(MONTH, -36, GETDATE())) > 0)
         			)
         			AND
         			UPPER(EAP.LIBL_ETAT) NOT LIKE '%ANNULATION%'
         	)
         	UPDATE
         		AUTORISATION_PRELEVEMENT
         	SET
         		ID_ETAT = 6,
         		MOTIF_ANNULATION = 'Mandat Caduque',
         		DAT_ANNULATION = GETDATE()
         	FROM
         		AUTORISATION_PRELEVEMENT AP
         		INNER JOIN AUTORISATION_CADUQUE AC ON AC.ID_AUTORISATION_PRELEVEMENT = AP.ID_AUTORISATION_PRELEVEMENT

            INSERT INTO COMMENTAIRES
             (
                 TYPE_OBJET,
                 ID_OBJET,
                 [DATE],
                 COMMENTAIRE,
                 ID_UTILISATEUR
             )
             SELECT
                 14 as TYPE_OBJET,
                 ID_AUTORISATION_PRELEVEMENT as ID_OBJET,
                 GETDATE() as [DATE],
                 'MANDAT CADUQUE' as COMMENTAIRE,
                 82 as ID_UTILISATEUR
         	FROM
         		AUTORISATION_PRELEVEMENT AP
         	WHERE
         		AP.ID_ETAT = 6
         		AND
         		UPPER(AP.MOTIF_ANNULATION) = 'MANDAT CADUQUE'
         		AND
         		NOT EXISTS
         		(
         			SELECT
         				1
         			FROM
         				COMMENTAIRES C
         			WHERE
         				C.TYPE_OBJET = 14
         				AND C.ID_OBJET = AP.ID_AUTORISATION_PRELEVEMENT
         				AND C.COMMENTAIRE = 'MANDAT CADUQUE'
         		)
         END

 CREATE PROCEDURE [dbo].[UPD_ACTION_ENGAGER]
          @ID_ACTION_PEC INT,
          @MNT_ENGAGE  DECIMAL(18,2),
          @ID_UTILISATEUR INT
         AS

         BEGIN
          DECLARE
           @NUM   INT,
           @ID_TYPE  INT,
           @IdEngagement INT

          SELECT
           @NUM = count(*)
          FROM
           POSTE_COUT_ENGAGE
           INNER JOIN MODULE_PEC
            ON MODULE_PEC.ID_MODULE_PEC = POSTE_COUT_ENGAGE.ID_MODULE_PEC
             AND MODULE_PEC.ID_ACTION_PEC = @ID_ACTION_PEC
          WHERE
           POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NOT NULL
           AND MODULE_PEC.BLN_ACTIF = 1
           AND MODULE_PEC.BLN_IMPUTABLE = 1
           AND MODULE_PEC.DAT_CLOTURE IS NULL

          IF(@NUM = 0)
           SET @ID_TYPE = 1
          ELSE
           SET @ID_TYPE = 3

          INSERT INTO ENGAGEMENT
          (
           COD_ENGAGEMENT,
           DAT_BAE,
           ID_ACTION_PEC,
           MNT_ENGAGE_HT,
           BLN_ACTIF,
           ID_TYPE_ENGAGEMENT
          )
          VALUES
          (
           0,
           getdate(),
           @ID_ACTION_PEC,
           @MNT_ENGAGE,
           1,
           @ID_TYPE
          )

          SET @IdEngagement = @@IDENTITY

          UPDATE
           ENGAGEMENT
          SET
           COD_ENGAGEMENT = @IdEngagement
          WHERE
           ID_ENGAGEMENT = @IdEngagement;

          UPDATE
           POSTE_COUT_ENGAGE
          SET
           ID_ENGAGEMENT = @IdEngagement
          WHERE
           POSTE_COUT_ENGAGE.ID_MODULE_PEC IN
           (
            SELECT
             ID_MODULE_PEC
            FROM
             MODULE_PEC
            WHERE
             ID_ACTION_PEC = @ID_ACTION_PEC
             AND MODULE_PEC.BLN_IMPUTABLE = 1
             AND MODULE_PEC.BLN_ACTIF = 1
             AND MODULE_PEC.DAT_CLOTURE IS NULL
           )
           AND POSTE_COUT_ENGAGE.ID_ENGAGEMENT IS NULL

           AND POSTE_COUT_ENGAGE.MNT_PREVISIONNEL_HT >= 0

          UPDATE
           ACTION_PEC
          SET
           BLN_ACCORD = 1,
           ID_UTILISATEUR_BAE = @ID_UTILISATEUR
          WHERE
           ID_ACTION_PEC = @ID_ACTION_PEC;

          EXEC INS_MVT_BUDGETAIRE_ENGAGEMENT
           @ID_ACTION_PEC,
           @ID_UTILISATEUR,
           @IdEngagement

          EXEC dbo.DEL_EXPORT_D3R
           @ID_ACTION_PEC,
           'PEC'
         END

GO 

         CREATE PROCEDURE [dbo].[MAJ_PARAMETRES_COLLECTE]
         @ANNEE INT
         AS
         BEGIN
         DECLARE
         @ID_PERIODE_ANNEE AS INT,
         @ANNEE_TEXTE AS VARCHAR(4)
         SELECT
         @ID_PERIODE_ANNEE = ID_PERIODE
         FROM PERIODE
         WHERE
         ID_TYPE_PERIODE = 1
         AND
         NUM_ANNEE = @ANNEE
         IF EXISTS (SELECT * FROM PARAMETRE_GLOBAL WHERE ID_PERIODE = @ID_PERIODE_ANNEE)
         BEGIN
         Print 'Les paramŠtres existent d‚j…'
         RETURN
         END
         INSERT INTO PARAMETRE_GLOBAL
         (ID_TYPE_ASSUJETISSEMENT
         ,ID_ACTIVITE
         ,COD_PARAM_GLO
         ,TAU_GLO_APPEL
         ,TAU_MIN
         ,MNT_MIN
         ,BLN_ACTIF
         ,COM_PARAM_GLO
         ,ID_PERIODE
         ,ID_REGIME)

         SELECT
         12, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         13, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         14, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         15, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         16, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         17, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         18, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         19, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         20, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         21, 1, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME

         UNION ALL
         SELECT
         23, 4, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         24, 4, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         25, 4, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         26, 4, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         27, 4, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         28, 4, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         29, 4, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL

         SELECT
         30, 5, 0, 0, 15, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME

         UNION ALL
         SELECT
         14, 2, 0, 0.4, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         15, 2, 0, 0.4, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         16, 2, 0, 0.4, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         21, 2, 0, 0.4, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL

         SELECT
         12, 3, 0, 0.5, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         13, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         14, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         15, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         16, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         17, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         18, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         19, 3, 0, 0.2, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         20, 3, 0, 0.35, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         21, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         22, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         23, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         24, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         25, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         26, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         27, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         28, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         29, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME
         UNION ALL
         SELECT
         30, 3, 0, 0.15, 100, 0, 1, NULL, @ID_PERIODE_ANNEE, REGIME.ID_REGIME
         FROM
         REGIME

         UPDATE PARAMETRE_GLOBAL SET
         COD_PARAM_GLO = ID_PARAM_GLO
         WHERE
         COD_PARAM_GLO = 0
         SELECT @ANNEE_TEXTE = convert(varchar(4), @ANNEE)
         INSERT INTO PARAMETRE_APPEL
         	(ID_PARAM_GLO
         	,ID_OPTION
         	,COD_PARAM_APPEL
         	,DAT_APPEL
         	,DAT_ECHEANCE
         	,PRC_APPELE
         	,BLN_ACTIF
         	,COM_PARAM_APPEL
         	,ID_TYPE_VERSEMENT)
         SELECT
         ID_PARAM_GLO,
         NULL,
         0,
         '01/01/' + @ANNEE_TEXTE + ' 00:00:00',
         '31/12/' + @ANNEE_TEXTE + ' 00:00:00',
         0,
         1,
         NULL,
         3
         FROM PARAMETRE_GLOBAL
         WHERE
         ID_PERIODE = @ID_PERIODE_ANNEE
         UNION ALL
         SELECT
         ID_PARAM_GLO,
         NULL,
         0,
         '15/01/' + @ANNEE_TEXTE + ' 00:00:00',
         '28/02/' + @ANNEE_TEXTE + ' 00:00:00',
         100,
         1,
         NULL,
         4
         FROM PARAMETRE_GLOBAL
         WHERE
         ID_PERIODE = @ID_PERIODE_ANNEE
         UPDATE PARAMETRE_APPEL SET
         COD_PARAM_APPEL = ID_PARAM_APPEL
         WHERE
         COD_PARAM_APPEL = 0
         END

         CREATE PROCEDURE [dbo].[BATCH_PLURI_ANNUEL_REPRISE]
         AS
         BEGIN
         DECLARE @ID_MODULE int
         DECLARE @ID_DISPOSITIF int
         DECLARE @ID_BRANCHE int
         DECLARE @MNT float
         DECLARE @ID_ENVELOPPE int
         DECLARE @ID_COMPTE int
         DECLARE @ID_TYPE_FINANCEMENT int
         DECLARE @dat_debut datetime
         DECLARE @dat_fin datetime
         DECLARE @jours_total int
         DECLARE @id_periode int
         DECLARE @jours_periode int

         DECLARE plan_fi_cursor CURSOR FOR
         SELECT	MODULE_PEC.ID_MODULE_PEC,
         		UNITE_STAGIAIRE.ID_DISPOSITIF, 
         		STAGIAIRE_PEC.ID_BRANCHE, 
         		sum(PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US) as MNT,
         		PLAN_FINANCEMENT_US.ID_ENVELOPPE,
         		PLAN_FINANCEMENT_US.ID_TYPE_FINANCEMENT
         FROM    ACTION_PEC 
         JOIN	MODULE_PEC 
         ON		dbo.ACTION_PEC.ID_ACTION_PEC = dbo.MODULE_PEC.ID_ACTION_PEC
         JOIN	STAGIAIRE_PEC 
         ON		MODULE_PEC.ID_MODULE_PEC = STAGIAIRE_PEC.ID_MODULE_PEC 
         JOIN	UNITE_STAGIAIRE 
         ON		UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC 
         JOIN	PLAN_FINANCEMENT_US 
         ON		PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE = UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE 

         JOIN	POSTE_COUT_ENGAGE 
         ON		POSTE_COUT_ENGAGE.id_MODULE_PEC=MODULE_PEC.id_MODULE_PEC
         AND		PLAN_FINANCEMENT_US.id_POSTE_COUT_ENGAGE=POSTE_COUT_ENGAGE.id_POSTE_COUT_ENGAGE

         JOIN	ENGAGEMENT	
         ON		ENGAGEMENT.ID_ENGAGEMENT = POSTE_COUT_ENGAGE.ID_ENGAGEMENT
         AND		ID_TYPE_ENGAGEMENT	is not null 
         AND		ID_TYPE_ENGAGEMENT	= 1
         AND		ENGAGEMENT.DAT_BAE is not null
         WHERE	(
         		(ACTION_PEC.BLN_REPRISE_ADHOC = 1 and MODULE_PEC.DAT_FIN > '20061231' and POSTE_COUT_ENGAGE.DAT_CREATION < '20090216')
         		OR
         		(ACTION_PEC.BLN_REPRISE_NESSIE = 1 and MODULE_PEC.DAT_FIN > '20091231' and POSTE_COUT_ENGAGE.DAT_CREATION < '20130106')
         		)

         		and ACTION_PEC.BLN_ACTIF = 1
         		and MODULE_PEC.BLN_ACTIF = 1
         		and PLAN_FINANCEMENT_US.ID_POSTE_COUT_REGLE is null

         GROUP BY 
         	UNITE_STAGIAIRE.ID_DISPOSITIF, 
         	STAGIAIRE_PEC.ID_BRANCHE,
         	MODULE_PEC.ID_MODULE_PEC,
         	PLAN_FINANCEMENT_US.ID_ENVELOPPE,
         	PLAN_FINANCEMENT_US.ID_TYPE_FINANCEMENT
         ORDER BY ID_MODULE_PEC

         OPEN plan_fi_cursor
         FETCH NEXT FROM plan_fi_cursor
         INTO @ID_MODULE, @ID_DISPOSITIF, @ID_BRANCHE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT

         WHILE @@FETCH_STATUS = 0
         BEGIN

         	exec INS_MVT_BUDG_PERIODE
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	NULL, 
         	@ID_TYPE_FINANCEMENT,
         	'EIR',
         	@MNT,
         	null, 
         	null

         	FETCH NEXT FROM plan_fi_cursor
         	INTO @ID_MODULE, @ID_DISPOSITIF, @ID_BRANCHE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT
         END
         CLOSE plan_fi_cursor
         DEALLOCATE plan_fi_cursor

         declare @ID_SESSION int
         DECLARE reglement_cursor CURSOR FOR
         SELECT	MODULE_PEC.ID_MODULE_PEC,

         		sum(MNT_PLAN_FINANCEMENT_US) as mnt,
         		PLAN_FINANCEMENT_US.ID_ENVELOPPE,
         		PLAN_FINANCEMENT_US.ID_TYPE_FINANCEMENT,
         		UNITE_STAGIAIRE.ID_DISPOSITIF,
         		STAGIAIRE_PEC.ID_BRANCHE,

         		POSTE_COUT_REGLE.ID_SESSION_PEC

         FROM    POSTE_COUT_REGLE 
         JOIN	MODULE_PEC 
         ON		MODULE_PEC.ID_MODULE_PEC = POSTE_COUT_REGLE.ID_MODULE_PEC
         JOIN	ACTION_PEC 
         ON		ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
         JOIN	REGLEMENT 
         ON		REGLEMENT.ID_REGLEMENT = POSTE_COUT_REGLE.ID_REGLEMENT
         JOIN	PLAN_FINANCEMENT_US 
         on		PLAN_FINANCEMENT_US.id_POSTE_COUT_REGLE=POSTE_COUT_REGLE.id_POSTE_COUT_REGLE
         JOIN	UNITE_STAGIAIRE 
         ON		UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE = PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE
         JOIN	STAGIAIRE_PEC 
         ON		STAGIAIRE_PEC.ID_STAGIAIRE_PEC = UNITE_STAGIAIRE.ID_STAGIAIRE_PEC
         WHERE   (
         		(ACTION_PEC.BLN_REPRISE_ADHOC = 1 and MODULE_PEC.DAT_FIN > '20061231' and DAT_VALID_REGLEMENT < '20090216')
         		OR
         		(ACTION_PEC.BLN_REPRISE_NESSIE = 1 and MODULE_PEC.DAT_FIN > '20091231' and DAT_VALID_REGLEMENT < '20130101')
         		)
         AND		PLAN_FINANCEMENT_US.bln_actif = 1
         group by	MODULE_PEC.ID_MODULE_PEC,
         			PLAN_FINANCEMENT_US.ID_ENVELOPPE,
         			PLAN_FINANCEMENT_US.ID_TYPE_FINANCEMENT, 
         			POSTE_COUT_REGLE.ID_MODULE_PEC,
         			UNITE_STAGIAIRE.ID_DISPOSITIF,
         			STAGIAIRE_PEC.ID_BRANCHE,
         			POSTE_COUT_REGLE.ID_SESSION_PEC
         having sum(MNT_PLAN_FINANCEMENT_US) > 0

         OPEN reglement_cursor
         FETCH NEXT FROM reglement_cursor
         INTO @ID_MODULE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT,@ID_DISPOSITIF, @ID_BRANCHE, @ID_SESSION

         WHILE @@FETCH_STATUS = 0
         BEGIN

         	exec INS_MVT_BUDG_PERIODE
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	NULL, 
         	@ID_TYPE_FINANCEMENT, 
         	'RR', 
         	@MNT,
         	@ID_SESSION,
         	null

         	FETCH NEXT FROM reglement_cursor
         	INTO @ID_MODULE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT, @ID_DISPOSITIF, @ID_BRANCHE, @ID_SESSION
         END
         CLOSE reglement_cursor
         DEALLOCATE reglement_cursor
         END

 CREATE PROCEDURE [dbo].[CHECK_GROUPE_ACTIVITE_SIREN]  
         	@ID_GROUPE int, 
         	@ID_ADHERENT int, 
         	@NUM_SIREN varchar(9),
         	@SAUF_ID_ETAB int
         AS

         BEGIN
         	SET NOCOUNT ON;

         	declare @ID_PERIODE_EN_COURS int
         	select	@ID_PERIODE_EN_COURS = ID_PERIODE 
         	from	PERIODE 
         	where	BLN_EN_COURS = 1 and ID_TYPE_PERIODE = 1

         	declare @ID_ACTIVITE_D_ADHERENT int
         	exec @ID_ACTIVITE_D_ADHERENT = GetActiviteAdherentByCode  @ID_ADHERENT, @ID_PERIODE_EN_COURS,'PLAN' 

         	SELECT TOP 1 @ID_ACTIVITE_D_ADHERENT as EXISTENT_AUTRES
         	from ETABLISSEMENT 
         		CROSS join ACTIVITE  
         		INNER join R19 on (	R19.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT and 
         							R19.ID_ACTIVITE in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 
         							 and 
         							R19.ID_PERIODE = @ID_PERIODE_EN_COURS)
         	where 
         		ETABLISSEMENT.ID_GROUPE = @ID_GROUPE and
         		ETABLISSEMENT.ID_ETABLISSEMENT <> coalesce(@SAUF_ID_ETAB, -1) and
         		ETABLISSEMENT.BLN_ACTIF = 1 and 
         		((@NUM_SIREN is null or (@ID_ACTIVITE_D_ADHERENT = 2 and left(ETABLISSEMENT.NUM_SIRET,9) <> @NUM_SIREN)) 
         		or
         		R19.ID_ACTIVITE <> @ID_ACTIVITE_D_ADHERENT)

         END

         CREATE PROCEDURE [dbo].[BATCH_PLURI_ANNUEL_REPRISE_PRO]
         AS
         BEGIN
         DECLARE @ID_MODULE int
         DECLARE @ID_DISPOSITIF int
         DECLARE @ID_BRANCHE int
         DECLARE @MNT float
         DECLARE @ID_ENVELOPPE int
         DECLARE @ID_COMPTE int
         DECLARE @ID_TYPE_FINANCEMENT int
         DECLARE @ID_SESSION int

         DECLARE @dat_debut datetime
         DECLARE @dat_fin datetime
         DECLARE @jours_total int
         DECLARE @id_periode int
         DECLARE @jours_periode int
         DECLARE @dat_valid_REGLEMENT datetime

         DECLARE eng_pro_cursor CURSOR FOR
         SELECT	MODULE_PRO.ID_MODULE_PRO,
         		ENGAGEMENT_PRO.ID_DISPOSITIF, 
         		SALARIE_PRO.ID_BRANCHE, 

         		round(sum(MODULE_PRO.MNT_MODULE_HT),2) as MNT, 
         		NULL as ID_ENVELOPPE,
         		NULL as ID_TYPE_FINANCEMENT
         FROM    CONTRAT_PRO 
                 INNER JOIN MODULE_PRO ON CONTRAT_PRO.id_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
         		INNER JOIN TYPE_FORMATION ON TYPE_FORMATION.ID_TYPE_FORMATION = MODULE_PRO.ID_TYPE_FORMATION  
         		INNER JOIN ENGAGEMENT_PRO on ENGAGEMENT_PRO.id_ENGAGEMENT_PRO=module_pro.id_ENGAGEMENT_PRO
         		INNER JOIN ETABLISSEMENT on ETABLISSEMENT.id_ETABLISSEMENT=CONTRAT_PRO.id_ETABLISSEMENT
         		INNER JOIN SALARIE_PRO ON (CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO) 
         WHERE   (
         		(CONTRAT_PRO.BLN_REPRISE_ADHOC = 1 and MODULE_PRO.DAT_FIN > '20061231')
         		OR
         		(CONTRAT_PRO.BLN_REPRISE_NESSIE = 1 and MODULE_PRO.DAT_FIN > '20091231') 
         		)

         	and CONTRAT_PRO.BLN_ACTIF=1
         	and MODULE_PRO.BLN_ACTIF=1
         GROUP BY
         	ENGAGEMENT_PRO.ID_DISPOSITIF, 

         	SALARIE_PRO.ID_BRANCHE,
         	MODULE_PRO.ID_MODULE_PRO

         OPEN eng_pro_cursor
         FETCH NEXT FROM eng_pro_cursor
         INTO @ID_MODULE, @ID_DISPOSITIF, @ID_BRANCHE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT

         WHILE @@FETCH_STATUS = 0
         BEGIN

         	exec INS_MVT_BUDG_PERIODE_PRO
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	NULL, 
         	@ID_TYPE_FINANCEMENT,
         	'IRP',
         	@MNT,
         	null, 
         	null

         	FETCH NEXT FROM eng_pro_cursor
         	INTO @ID_MODULE, @ID_DISPOSITIF, @ID_BRANCHE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT
         END
         CLOSE eng_pro_cursor
         DEALLOCATE eng_pro_cursor

         DECLARE regl_pro_cursor_adh CURSOR FOR
         SELECT	MODULE_PRO.ID_MODULE_PRO,
         		TYPE_FORMATION.ID_DISPOSITIF, 
         		ETABLISSEMENT.ID_BRANCHE,
         		round(sum(MNT_VERSE_HT_ADH),2) as MNT,
         		NULL as ID_ENVELOPPE,
         		NULL as ID_TYPE_FINANCEMENT,
         		DAT_VALID_REGLEMENT,
         		SESSION_PRO.ID_SESSION_PRO
         FROM    CONTRAT_PRO 
                 INNER JOIN MODULE_PRO ON CONTRAT_PRO.id_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
         		INNER JOIN SESSION_PRO ON SESSION_PRO.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO 
         		INNER JOIN TYPE_FORMATION ON TYPE_FORMATION.ID_TYPE_FORMATION = MODULE_PRO.ID_TYPE_FORMATION  
         		INNER JOIN REGLEMENT_PRO on REGLEMENT_PRO.id_REGLEMENT_PRO=SESSION_PRO.id_REGLEMENT_PRO_adh
         		and REGLEMENT_PRO.BLN_ACTIF=1
         		INNER JOIN ETABLISSEMENT on ETABLISSEMENT.id_ETABLISSEMENT=CONTRAT_PRO.id_ETABLISSEMENT
         WHERE	(
         		(CONTRAT_PRO.BLN_REPRISE_ADHOC = 1 and MODULE_PRO.DAT_FIN > '20061231' and REGLEMENT_PRO.DAT_VALID_REGLEMENT < '20090317')
         		OR
         		(CONTRAT_PRO.BLN_REPRISE_NESSIE = 1 and MODULE_PRO.DAT_FIN > '20091231' and REGLEMENT_PRO.DAT_VALID_REGLEMENT < '20130101')
         		)
         GROUP BY 
         	TYPE_FORMATION.ID_DISPOSITIF, 
         	ETABLISSEMENT.ID_BRANCHE,
         	MODULE_PRO.ID_MODULE_PRO,
         	DAT_VALID_REGLEMENT,
         	SESSION_PRO.ID_SESSION_PRO
         order by dat_valid_reglement

         OPEN regl_pro_cursor_adh
         FETCH NEXT FROM regl_pro_cursor_adh
         INTO @ID_MODULE, @ID_DISPOSITIF, @ID_BRANCHE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT, @DAT_VALID_REGLEMENT, @ID_SESSION

         WHILE @@FETCH_STATUS = 0
         BEGIN

         	exec INS_MVT_BUDG_PERIODE_PRO
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	NULL, 
         	@ID_TYPE_FINANCEMENT,
         	'RRP',
         	@MNT,
         	@ID_SESSION,    
         	null 

         	FETCH NEXT FROM regl_pro_cursor_adh
         	INTO @ID_MODULE, @ID_DISPOSITIF, @ID_BRANCHE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT, @DAT_VALID_REGLEMENT, @ID_SESSION
         END
         CLOSE regl_pro_cursor_adh
         DEALLOCATE regl_pro_cursor_adh

         DECLARE regl_pro_cursor_OF CURSOR FOR
         SELECT	MODULE_PRO.ID_MODULE_PRO,
         		TYPE_FORMATION.ID_DISPOSITIF, 
         		ETABLISSEMENT.ID_BRANCHE,
         		round(sum(MNT_VERSE_HT_OF),2) as MNT,
         		NULL as ID_ENVELOPPE,
         		NULL as ID_TYPE_FINANCEMENT,
         		DAT_VALID_REGLEMENT,
         		SESSION_PRO.ID_SESSION_PRO
         FROM    CONTRAT_PRO 
                 INNER JOIN MODULE_PRO ON CONTRAT_PRO.id_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
         		INNER JOIN SESSION_PRO ON SESSION_PRO.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO 
         		INNER JOIN TYPE_FORMATION ON TYPE_FORMATION.ID_TYPE_FORMATION = MODULE_PRO.ID_TYPE_FORMATION  
         		INNER JOIN REGLEMENT_PRO on REGLEMENT_PRO.id_REGLEMENT_PRO=SESSION_PRO.id_REGLEMENT_PRO_OF
         		and REGLEMENT_PRO.BLN_ACTIF=1
         		INNER JOIN ETABLISSEMENT on ETABLISSEMENT.id_ETABLISSEMENT=CONTRAT_PRO.id_ETABLISSEMENT
         WHERE   (CONTRAT_PRO.BLN_REPRISE_ADHOC = 1 and MODULE_PRO.DAT_FIN > '20061231' and REGLEMENT_PRO.DAT_VALID_REGLEMENT < '20090317')
         		OR
         		(CONTRAT_PRO.BLN_REPRISE_NESSIE = 1 and MODULE_PRO.DAT_FIN > '20091231' and REGLEMENT_PRO.DAT_VALID_REGLEMENT < '20130101')
         GROUP BY 
         	TYPE_FORMATION.ID_DISPOSITIF, 
         	ETABLISSEMENT.ID_BRANCHE,
         	MODULE_PRO.ID_MODULE_PRO, 
         	DAT_VALID_REGLEMENT,
         	SESSION_PRO.ID_SESSION_PRO
         order by dat_valid_reglement

         OPEN regl_pro_cursor_OF
         FETCH NEXT FROM regl_pro_cursor_OF
         INTO @ID_MODULE, @ID_DISPOSITIF, @ID_BRANCHE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT, @DAT_VALID_REGLEMENT, @ID_SESSION

         WHILE @@FETCH_STATUS = 0
         BEGIN
         	exec INS_MVT_BUDG_PERIODE_PRO
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	NULL, 
         	@ID_TYPE_FINANCEMENT,
         	'RRP',
         	@MNT,
         	@ID_SESSION, 
         	null 

         	FETCH NEXT FROM regl_pro_cursor_OF
         	INTO @ID_MODULE, @ID_DISPOSITIF, @ID_BRANCHE, @MNT, @ID_ENVELOPPE, @ID_TYPE_FINANCEMENT, @DAT_VALID_REGLEMENT, @ID_SESSION
         END
         CLOSE regl_pro_cursor_OF
         DEALLOCATE regl_pro_cursor_OF

         END

 CREATE PROCEDURE [dbo].[UPD_ENGAGER_CONTRAT_PRO]
          @ID_CONTRAT_PRO    AS INT,
          @ID_UTILISATEUR    AS INT,
          @ID_ADHERENT    AS INT
         AS

         BEGIN

          DECLARE @ID_AGENCE_CONTRAT INT

          DECLARE @NB_MOIS_ECHEANCIER INT
          SET  @NB_MOIS_ECHEANCIER = NULL
          SELECT  @NB_MOIS_ECHEANCIER = NB_MOIS_ECHEANCIER FROM PARAMETRES

          DECLARE @ID_ETAT_CONTRAT_PRO INT
          SET  @ID_ETAT_CONTRAT_PRO = NULL
          SELECT  @ID_ETAT_CONTRAT_PRO = ID_ETAT_CONTRAT_PRO FROM ETAT_CONTRAT_PRO WHERE ETAT_CONTRAT_PRO.COD_ETAT_CONTRAT_PRO = 'ENGAGE'

          DECLARE @DAT_DEB_CONTRAT DATETIME
          DECLARE @DAT_FIN_CONTRAT DATETIME

          SELECT  @DAT_DEB_CONTRAT = DAT_DEB_CONTRAT, 
            @DAT_FIN_CONTRAT = DAT_FIN_CONTRAT, 
            @ID_AGENCE_CONTRAT = CONTRAT_PRO.ID_AGENCE
          FROM CONTRAT_PRO 
          WHERE ID_CONTRAT_PRO = @ID_CONTRAT_PRO

          DECLARE @DAT_BAE_INITIALE DATETIME
          SET @DAT_BAE_INITIALE = NULL
          SELECT @DAT_BAE_INITIALE = DAT_BAE_INITIALE FROM CONTRAT_PRO WHERE ID_CONTRAT_PRO = @ID_CONTRAT_PRO

          DECLARE @ID_BRANCHE INT
          SET @ID_BRANCHE = NULL
          SELECT  @ID_BRANCHE = ID_BRANCHE FROM ADHERENT WHERE ID_ADHERENT = @ID_ADHERENT

          SELECT OPERATION_FINANCIERE.ID_OPERATION_FINANCIERE,
            OPERATION_FINANCIERE.ID_DISPOSITIF,
            OPERATION_FINANCIERE.ID_PERIODE
          INTO #OPE_FIN
          FROM OPERATION_FINANCIERE
            INNER JOIN DISPOSITIF ON DISPOSITIF.ID_DISPOSITIF = OPERATION_FINANCIERE.ID_DISPOSITIF
          WHERE  ID_BRANCHE = @ID_BRANCHE
            AND ID_AGENCE = @ID_AGENCE_CONTRAT
            AND ((COD_DISPOSITIF = 'CONTPRO') OR (COD_DISPOSITIF = 'FONCTUT'))
            AND BLN_DEFAUT = 1 

          SELECT  CONTRAT_PRO.ID_CONTRAT_PRO,
            DISPOSITIF.ID_DISPOSITIF,
            SUM(MODULE_PRO.MNT_MODULE_HT) as MNT_ENGAGE_HT,
            GETDATE() AS DAT_BAE,
            @ID_UTILISATEUR AS ID_UTILISATEUR,
            MODULE_PRO.ID_PERIODE AS ID_PERIODE,
            SUM(MODULE_PRO.NB_UNITE_TOTALE) AS NB_UNITE_TOTALE,
            CASE WHEN @DAT_BAE_INITIALE IS NULL THEN
              1 
             ELSE
              2
            END AS ID_TYPE_ENGAGEMENT_PRO,
            (SELECT ID_OPERATION_FINANCIERE FROM #OPE_FIN WHERE #OPE_FIN.ID_DISPOSITIF = DISPOSITIF.ID_DISPOSITIF AND #OPE_FIN.ID_PERIODE = MODULE_PRO.ID_PERIODE) AS ID_OPERATION_FINANCIERE

          INTO #TMP_LISTE
          FROM    CONTRAT_PRO 
            INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
            INNER JOIN TYPE_FORMATION ON MODULE_PRO.ID_TYPE_FORMATION = TYPE_FORMATION.ID_TYPE_FORMATION 
            INNER JOIN DISPOSITIF ON TYPE_FORMATION.ID_DISPOSITIF = DISPOSITIF.ID_DISPOSITIF
          WHERE  CONTRAT_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO
            AND MODULE_PRO.BLN_ACTIF = 1
            AND MODULE_PRO.ID_ENGAGEMENT_PRO  IS NULL
          GROUP BY CONTRAT_PRO.ID_CONTRAT_PRO,DISPOSITIF.ID_DISPOSITIF,MODULE_PRO.ID_PERIODE

          INSERT  dbo.ENGAGEMENT_PRO
          (
           COD_ENGAEMENT_PRO,
              DAT_BAE,
              MNT_ENGAGE_HT,
              NB_UNITE_ENGAGE,
              ID_UTILISATEUR,
              ID_OPERATION_FINANCIERE,
              ID_PERIODE,
              ID_TYPE_ENGAGEMENT_PRO,
              ID_DISPOSITIF,
              DAT_LETTRE_ENGAGEMENT_PRO_ADH,
              DAT_LETTRE_ENGAGEMENT_PRO_OF
          )
          SELECT 'COD_TEMP',
             #TMP_LISTE.DAT_BAE,
             #TMP_LISTE.MNT_ENGAGE_HT,
             #TMP_LISTE.NB_UNITE_TOTALE, 
             #TMP_LISTE.ID_UTILISATEUR,
             #TMP_LISTE.ID_OPERATION_FINANCIERE,
             #TMP_LISTE.ID_PERIODE, 
             #TMP_LISTE.ID_TYPE_ENGAGEMENT_PRO, 
             #TMP_LISTE.ID_DISPOSITIF,
             NULL,
             NULL
          FROM   #TMP_LISTE

          UPDATE MODULE_PRO
          SET  MODULE_PRO.ID_ENGAGEMENT_PRO = ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO
          FROM MODULE_PRO
            INNER JOIN TYPE_FORMATION ON MODULE_PRO.ID_TYPE_FORMATION = TYPE_FORMATION.ID_TYPE_FORMATION
            INNER JOIN  ENGAGEMENT_PRO ON TYPE_FORMATION.ID_DISPOSITIF = ENGAGEMENT_PRO .ID_DISPOSITIF
          WHERE MODULE_PRO.ID_ENGAGEMENT_PRO IS  NULL
          AND  MODULE_PRO.ID_PERIODE = ENGAGEMENT_PRO.ID_PERIODE
          AND  MODULE_PRO.BLN_ACTIF = 1
          AND  MODULE_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO
          AND  ENGAGEMENT_PRO.COD_ENGAEMENT_PRO = 'COD_TEMP'

          UPDATE ENGAGEMENT_PRO SET ENGAGEMENT_PRO.COD_ENGAEMENT_PRO = ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO WHERE COD_ENGAEMENT_PRO = 'COD_TEMP'

          UPDATE CONTRAT_PRO  
          SET CONTRAT_PRO.ID_ETAT_CONTRAT_PRO  = @ID_ETAT_CONTRAT_PRO,
           CONTRAT_PRO.BLN_CONFORME_ACCORD = 1,
           CONTRAT_PRO.BLN_PARTICIPATION_FINANCIERE = 1,
           CONTRAT_PRO.ID_UTILISATEUR = @ID_UTILISATEUR  
          WHERE CONTRAT_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO

          IF @DAT_BAE_INITIALE IS NULL 
           UPDATE CONTRAT_PRO SET CONTRAT_PRO.DAT_BAE_INITIALE = GETDATE()  WHERE CONTRAT_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO

          SELECT  CONTRAT_PRO.ID_CONTRAT_PRO,
            MODULE_PRO.ID_MODULE_PRO,
            MODULE_PRO.DAT_DEBUT,
            MODULE_PRO.DAT_FIN,
            DISPOSITIF.ID_DISPOSITIF
          INTO #TMP_MODULE_PRO
          FROM    CONTRAT_PRO 
            INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
            INNER JOIN TYPE_FORMATION ON MODULE_PRO.ID_TYPE_FORMATION = TYPE_FORMATION.ID_TYPE_FORMATION 
            INNER JOIN DISPOSITIF ON TYPE_FORMATION.ID_DISPOSITIF = DISPOSITIF.ID_DISPOSITIF 
            LEFT OUTER JOIN ENGAGEMENT_PRO ON MODULE_PRO.ID_ENGAGEMENT_PRO = ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO
                    AND MODULE_PRO.ID_PERIODE = ENGAGEMENT_PRO.ID_PERIODE
          WHERE  CONTRAT_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO
            AND MODULE_PRO.BLN_ACTIF = 1
            AND ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO IS NOT NULL
            AND TYPE_FORMATION.ID_TYPE_FORMATION IN (SELECT ID_TYPE_FORMATION FROM TYPE_FORMATION WHERE COD_TYPE_FORMATION NOT IN ('MAJOCDI','MAJOCDD'))

          INSERT INTO ECHEANCE_MODULE_PRO
             (
              DAT_CREATION, 
              BLN_ACTIF, 
              BLN_TRAITE, 
              BLN_DERNIER, 
              DAT_ECHEANCE, 
              DAT_ENVOI_DEMANDE_RBT, 
              DAT_ENVOI_RELANCE1_PREV, 
              DAT_ENVOI_RELANCE2_PREV, 
              DAT_ENVOI_RELANCE3_PREV, 
              DAT_ENVOI_RELANCE1_EFF, 
              DAT_ENVOI_RELANCE2_EFF, 
              DAT_ENVOI_RELANCE3_EFF, 
              DAT_PROP_RUPTURE,
              ID_MODULE_PRO
             )
          SELECT  GETDATE()    AS DAT_CREATION,
             1      AS BLN_ACTIF,
             0      AS BLN_TRAITE,
             0      AS BLN_DERNIER, 
             (CASE WHEN (DATEDIFF(MONTH,DAT_DEBUT,DAT_FIN)) > @NB_MOIS_ECHEANCIER
               THEN
                DATEADD(MONTH,@NB_MOIS_ECHEANCIER,DAT_DEBUT)
               ELSE
                DATEADD(MONTH,-1,DAT_FIN)
             END)     AS DAT_ECHEANCE,
             NULL     AS DAT_ENVOI_DEMANDE_RBT, 
             NULL     AS DAT_ENVOI_RELANCE1_PREV, 
             NULL     AS DAT_ENVOI_RELANCE2_PREV, 
             NULL     AS DAT_ENVOI_RELANCE3_PREV, 
             NULL     AS DAT_ENVOI_RELANCE1_EFF, 
             NULL     AS DAT_ENVOI_RELANCE2_EFF, 
             NULL     AS DAT_ENVOI_RELANCE3_EFF, 
             NULL     AS DAT_PROP_RUPTURE,
             ID_MODULE_PRO   AS ID_MODULE_PRO
          FROM  #TMP_MODULE_PRO

          EXEC dbo.DEL_EXPORT_D3R
           @ID_CONTRAT_PRO,
           'PRO'
         END

         CREATE PROCEDURE dbo.MAJ_ACTIVITE_ANNEE
         	@NUM_ANNEE INT  
         AS  
         BEGIN
         	DECLARE
         		@ID_PERIODE INT,
         		@ID_PERIODE_PASSEE INT

         	SELECT
         		@ID_PERIODE = ID_PERIODE  
         	FROM
         		PERIODE  
         	WHERE
         		ID_TYPE_PERIODE = 1 
         		AND NUM_ANNEE = @NUM_ANNEE

         	SELECT
         		T2.ID_ADHERENT,
         		T2.ID_ACTIVITE,
         		T2.ID_PERIODE
         	INTO
         		#TMP_ADH_ACTIVITE  
         	FROM
         		(
         		SELECT
         			ID_ADHERENT,
         			MAX(ID_PERIODE) AS ID_PERIODE
         		FROM
         			R19
         		WHERE
         			ID_ACTIVITE IN (SELECT ID_ACTIVITE FROM GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN'))		
         		GROUP BY
         			ID_ADHERENT  
         		) T1
         		INNER JOIN R19 T2
         			ON	T1.ID_ADHERENT = T2.ID_ADHERENT
         			AND	T1.ID_PERIODE = T2.ID_PERIODE  
         	WHERE
         		T2.ID_ACTIVITE IN (SELECT ID_ACTIVITE FROM GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN'))				
         	ORDER BY 1

         	INSERT INTO R19  
         	(
         		ID_ADHERENT,
         		ID_ACTIVITE,
         		ID_PERIODE
         	)  
         	SELECT
         		T1.ID_ADHERENT,
         		T1.ID_ACTIVITE,
         		@ID_PERIODE  
         	 FROM
         		#TMP_ADH_ACTIVITE T1  
         	 WHERE
         		NOT EXISTS
         		(
         			SELECT
         				1
         			FROM
         				R19  
         			WHERE
         				ID_ADHERENT = T1.ID_ADHERENT  
         				AND ID_ACTIVITE IN (SELECT ID_ACTIVITE FROM GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN'))	
         				AND ID_PERIODE = @ID_PERIODE  
         		)

         	DECLARE
         		@ID_ACTIVITE_PROF INT

         	SET
         		@ID_ACTIVITE_PROF = (SELECT ID_ACTIVITE FROM GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PROF'))

         	INSERT INTO R19  
         	(
         		ID_ADHERENT,
         		ID_ACTIVITE,
         		ID_PERIODE
         	)  
         	SELECT
         		T1.ID_ADHERENT,
         		@ID_ACTIVITE_PROF,
         		@ID_PERIODE  
         	FROM
         		ADHERENT T1  
         	WHERE
         		T1.BLN_ACTIF = 1  
         		AND NOT EXISTS (SELECT 1 FROM R19 WHERE ID_ADHERENT = T1.ID_ADHERENT AND ID_ACTIVITE = @ID_ACTIVITE_PROF AND ID_PERIODE = @ID_PERIODE)  
         		AND EXISTS (SELECT 1 FROM R19 WHERE ID_ADHERENT = T1.ID_ADHERENT AND ID_ACTIVITE = @ID_ACTIVITE_PROF)

         	UPDATE
         		T1
         	SET
         		T1.ID_ACTIVITE = T3.ID_CHAMP_APPLICATION
         	FROM
         		R19 T1
         		INNER JOIN R20 T2
         			ON T1.ID_ADHERENT = T2.ID_ADHERENT
         			AND T1.ID_PERIODE = T2.ID_PERIODE
         			AND T1.ID_PERIODE = @ID_PERIODE
         			AND T1.ID_ACTIVITE IN (SELECT ID_ACTIVITE FROM GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN'))					
         		INNER JOIN TYPE_ASSUJETISSEMENT T3
         			ON T3.ID_TYPE_ASSUJETISSEMENT = T2.ID_TYPE_ASSUJETISSEMENT
         	WHERE
         		T3.ID_CHAMP_APPLICATION <> T1.ID_ACTIVITE

         END

 CREATE PROCEDURE [dbo].[EDT_LETTRE_CONTRAT_PRO_REMB_SAL_OF]
         @ID_ETABLISSEMENT	INT,
         	@ID_BENEFICIAIRE	INT,
         	@TYPE_BENEFICIAIRE	INT,
         	@ID_ADRESSE			INT,
         	@ID_CONTACT			INT,
         	@_ID_MODULE_PRO INT,
         	@_COD_CONTRAT_PRO	VARCHAR(10)

         AS
         BEGIN

         	DECLARE @RELANCE INT
         	SET @RELANCE = 0

         	SELECT	CONTRAT_PRO.ID_CONTRAT_PRO,
         			CONTRAT_PRO.COD_CONTRAT_PRO,
         			CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_DEB_CONTRAT, 3) AS DAT_DEBUT,
         			CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_FIN_CONTRAT, 3) AS DAT_FIN,
         			INDIVIDU.NOM_INDIVIDU, 
         			INDIVIDU.PRENOM_INDIVIDU,
         			Case when INDIVIDU.BLN_MASCULIN=1 then 'Monsieur' else 'Madame' end as Title ,
         			CONTRAT_PRO.LIB_LIEU_FORMATION,
         			ADHERENT.ID_ADHERENT,
         			ADHERENT.ID_TYPE_TVA,
         			ADHERENT.COD_ADHERENT,
         			AGENCE.ID_AGENCE,
         			AGENCE.COD_POSTAL,
         			AGENCE.LIB_VILLE,
         			AGENCE.LIB_NOM_CONSEILLER,
         			AGENCE.LIB_PNM_CONSEILLER,
         			ADHERENT.LIB_RAISON_SOCIALE
         	INTO	#TMP_CONTRAT_PRO
         	FROM	CONTRAT_PRO
         			INNER JOIN SALARIE_PRO ON CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO 
         			INNER JOIN INDIVIDU ON SALARIE_PRO.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU
         			INNER JOIN ETABLISSEMENT ON CONTRAT_PRO.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT 
         			INNER JOIN ADHERENT ON ETABLISSEMENT.ID_ADHERENT = ADHERENT.ID_ADHERENT
         			INNER JOIN AGENCE ON CONTRAT_PRO.ID_AGENCE = AGENCE.ID_AGENCE
         			LEFT JOIN ETABLISSEMENT_OF ON ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_ETABLISSEMENT
         			LEFT JOIN ORGANISME_FORMATION ON ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF
         	WHERE   CONTRAT_PRO.COD_CONTRAT_PRO = @_COD_CONTRAT_PRO;

         		SELECT ORGANISME_FORMATION.LIB_RAISON_SOCIALE, ROUND(MODULE_PRO.TAUX_HORAIRE_RETENU_OF,2) as TAUX_HORAIRE_MODULE , TYPE_FORMATION.LIBL_TYPE_FORMATION
         		INTO #TEMP_MODULE_ORGANISME_OF FROM ORGANISME_FORMATION
         		INNER JOIN ETABLISSEMENT_OF  ON ETABLISSEMENT_OF.ID_OF=ORGANISME_FORMATION.ID_OF  and ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_BENEFICIAIRE
         		INNER join MODULE_PRO on MODULE_PRO.ID_ETABLISSEMENT_OF=ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF
         		INNER join TYPE_FORMATION on MODULE_PRO.ID_TYPE_FORMATION=TYPE_FORMATION.ID_TYPE_FORMATION
         		where MODULE_PRO.ID_MODULE_PRO=@_ID_MODULE_PRO;

         SELECT TAU_TVA
         INTO #TVA from R26
         inner join #TMP_CONTRAT_PRO on #TMP_CONTRAT_PRO.ID_TYPE_TVA = R26.ID_TYPE_TVA
         inner join PERIODE on PERIODE.ID_PERIODE=R26.ID_PERIODE and PERIODE.ID_TYPE_PERIODE=2 and (DATEDIFF(DAY,PERIODE.DAT_DEB_PERIODE,GETDATE()) >0)
         order by PERIODE.DAT_DEB_PERIODE desc

         	IF( (SELECT COUNT(*) FROM ECHEANCE_MODULE_PRO WHERE ECHEANCE_MODULE_PRO.ID_MODULE_PRO=@_ID_MODULE_PRO AND (DATEDIFF(DAY,ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE1_PREV,GETDATE()) >0) AND (ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE1_EFF IS NULL)) >0)
         			BEGIN
         				SET @RELANCE = 1
         			END
         		ELSE
         			IF ((SELECT COUNT(*) FROM ECHEANCE_MODULE_PRO WHERE ECHEANCE_MODULE_PRO.ID_MODULE_PRO=@_ID_MODULE_PRO AND (DATEDIFF(DAY,ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE2_PREV,GETDATE()) >0) AND (ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE2_EFF IS NULL)) >0)
         					BEGIN
         						SET @RELANCE = 2
         					END

         		ELSE
         			BEGIN
         				IF ((SELECT COUNT(*) FROM ECHEANCE_MODULE_PRO WHERE ECHEANCE_MODULE_PRO.ID_MODULE_PRO=@_ID_MODULE_PRO AND (DATEDIFF(DAY,ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE3_PREV,GETDATE()) >0) AND (ECHEANCE_MODULE_PRO.DAT_ENVOI_RELANCE3_EFF IS NULL)) >0)
         					BEGIN
         						SET @RELANCE = 3
         					END
         			END;

         	WITH XMLNAMESPACES (
         		DEFAULT 'EDT_LETTRE_PRO_REMBOURSEMENT_SALAIRES_OF'
         	)

         	SELECT
         			CASE WHEN (@RELANCE > 0) THEN 
         			(
         				SELECT
         				(
         					SELECT CASE WHEN @RELANCE=0 THEN null ELSE @RELANCE END
         					FOR XML RAW('NUM_RELANCE'), ELEMENTS, TYPE
         				),
         				( 
         					SELECT  CASE WHEN @RELANCE=0 THEN ''  WHEN @RELANCE=1 THEN 'Šre' ELSE 'Šme' END
         					FOR XML RAW('niŠme'), ELEMENTS, TYPE
         				),
         				( 
         					SELECT  CASE WHEN  @RELANCE=0 THEN '' ELSE ' RELANCE' END
         					FOR XML RAW('RELANCE'), ELEMENTS, TYPE
         				)
         				FOR XML RAW('TOP') ,ELEMENTS,TYPE
         			)end,
         			(

         				SELECT	              	

         					dbo.GetXmlAgenceContact(ENTETE.ID_AGENCE) as EMETTEUR,

         					dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) as BENEFICIAIRE,

         					(
         						SELECT	REFERENCE.LIB_VILLE,
         								dbo.GetFullDate(getDate()) as DATE

         						FROM #TMP_CONTRAT_PRO AS REFERENCE     
         						FOR XML AUTO, ELEMENTS, TYPE
         					)
         				FROM #TMP_CONTRAT_PRO AS ENTETE     
         				FOR XML AUTO, ELEMENTS, TYPE
         			),
         			(

         				SELECT	
         						DEMANDE.COD_ADHERENT,
         						DEMANDE.LIB_RAISON_SOCIALE AS LIB_RAISON_SOCIALE_ADHERENT,
         						(
         							SELECT 
         									CORPS.ID_CONTRAT_PRO,
         									CORPS.DAT_DEBUT,
         									CORPS.DAT_FIN,
         									CORPS.Title,
         									CORPS.PRENOM_INDIVIDU,
         									CORPS.NOM_INDIVIDU
         							FROM #TMP_CONTRAT_PRO  AS CORPS 
         							FOR XML PATH(''), ELEMENTS, TYPE
         						),	
         						(

         						SELECT	ORGANISME.LIBL_TYPE_FORMATION
         						FROM #TEMP_MODULE_ORGANISME_OF as ORGANISME
         						FOR XML PATH(''), ELEMENTS, TYPE
         						)
         				FROM  #TMP_CONTRAT_PRO AS DEMANDE
         				FOR XML AUTO, ELEMENTS, TYPE
         			),

         			(
         			select

         			(
         				SELECT	top 1
         						dbo.GetContactSalutation(@ID_CONTACT, 1)
         				FROM CIVILITE
         				FOR XML PATH('POLITESSE_HAUT'), TYPE
         			),
         						CAST(CORPS.TAUX_HORAIRE_MODULE AS decimal(10,2)) as TAUX_HORAIRE_MODULE,
         			(
         				SELECT 	top 1 CAST((TAU_TVA * 100) AS decimal(10,2)) as TAU_TVA from #TVA
         					FOR XML PATH(''), ELEMENTS, TYPE
         			),

         			(
         				SELECT	top 1
         						dbo.GetContactSalutation(@ID_CONTACT, 0)
         				FROM CIVILITE
         				FOR XML PATH('POLITESSE_BAS'), TYPE
         			)
         				FROM #TEMP_MODULE_ORGANISME_OF as CORPS
         				FOR XML AUTO, ELEMENTS, TYPE
         			),

         			(

         				SELECT	SIGNATURE.LIB_PNM_CONSEILLER,
         						SIGNATURE.LIB_NOM_CONSEILLER
         				FROM
         						#TMP_CONTRAT_PRO as SIGNATURE
         				FOR XML AUTO, ELEMENTS, TYPE
         			)

         	FROM	#TMP_CONTRAT_PRO as LETTRE
         	FOR XML AUTO, ELEMENTS

         END

         CREATE PROCEDURE [dbo].[LEC_GRP_STAGIAIRE_MODULE_PEC_CREATION]
         	@ID_MODULE		int,
         	@ID_SESSION		int = null,
         	@MODE_SESSION	int = null
         AS
         BEGIN
         	DECLARE @ID_PREV_SESSION	int
         	DECLARE @COUNT				int
         	DECLARE @COD_MOD			varchar(12)

         	SET @COUNT = 0

         	SELECT @COUNT	= count(*)			from SESSION_PEC	where ID_MODULE_PEC = @ID_MODULE
         	SELECT @COD_MOD = COD_MODULE_PEC	from MODULE_PEC		where ID_MODULE_PEC = @ID_MODULE

         	SELECT top 1 @ID_PREV_SESSION = ID_SESSION_PEC from SESSION_PEC
         		where ID_MODULE_PEC = @ID_MODULE AND BLN_ACTIF = 1
         		order by ID_SESSION_PEC desc

          	Select 
         		@ID_MODULE									AS ID_MODULE,
         		STAGIAIRE_PEC.ID_STAGIAIRE_PEC				AS ID,
         		INDIVIDU.ID_INDIVIDU						AS ID_INDIVIDU,
         		INDIVIDU.NIR 								AS NIF,
         		INDIVIDU.NOM_INDIVIDU 						AS NOM,
         		INDIVIDU.PRENOM_INDIVIDU 					AS PRENOM,
         		INDIVIDU.DAT_NAISSANCE 						AS NE,
         		INDIVIDU.NOM_JEUNE_FILLE 					AS NOM_JEUNE_FILLE,
         		CASE WHEN INDIVIDU.BLN_MASCULIN = 1 THEN 'Masculin' ELSE 'F‚minin' END AS SEXE,
         	    INDIVIDU.BLN_MASCULIN 						AS MASCULIN,
         		INDIVIDU.DAT_MODIF 							AS MODIF,
         		INDIVIDU.DAT_CREATION 						AS CREE,
         		STAGIAIRE_PEC.NB_HEURES_HORS_TT				AS NB_HEURES,
         		STAGIAIRE_PEC.NUM_DUREE_MENSUELLE_TRAVAIL	AS HORAIRE_MENSUEL,
         		COALESCE(STAGIAIRE_PEC.NB_HEURE_ENGAGE,0)	AS DUREE_PREVUE,
         		COALESCE(STAGIAIRE_PEC.NB_HEURE_REGLE,0)	AS DUREE_REALISE,
         		STAGIAIRE_PEC.MATRICULE_SALARIE				AS MATRICULE,
         		STAGIAIRE_PEC.ID_STATUT 					AS STATUT,
         		STAGIAIRE_PEC.LIB_AGENCE 					AS AGENCE,
         		STAGIAIRE_PEC.LIB_DR 						AS DR,
         		STAGIAIRE_PEC.TIME_STAMP					AS TIME_STAMP,
         		STAGIAIRE_PEC.SALAIRE_HORAIRE_CHARGE		AS BRUT_CHARGE,
         		STAGIAIRE_PEC.ID_TYPE_CONTRAT				AS CONTRAT,
         		CASE WHEN (@ID_MODULE IS NULL) THEN 1
         		ELSE STAGIAIRE_PEC.ID_CATEGORIE_ACTION END	AS CATEGORIE,
         		STAGIAIRE_PEC.ID_CSP 						AS CSP,
         		CSP.LIBL_CSP 								AS LIB_CSP,
         		STAGIAIRE_PEC.ID_CLASSIFICATION				AS CLASSIFICATION,
         		STAGIAIRE_PEC.BLN_FORMATEUR_INTERNE			AS FORMATEUR_INTERNE,
         		STAGIAIRE_PEC.ANALYTIQUE_STAGIAIRE			AS ANALYTIQUE_STAGIAIRE,
         		UTILISATEUR.COD_UTIL 						AS PAR,
         		ETABLISSEMENT.ID_ADHERENT 					AS ID_ADHERENT,
         		ETABLISSEMENT.ID_ETABLISSEMENT 				AS ETABLISSEMENT,
         		ADHERENT.COD_ADHERENT 						AS COD_ADHERENT,
         		ADHERENT.LIB_SIGLE_ADHERENT 				AS SIGLE_ADHERENT,		
         		ISNULL(STAGIAIRE_PEC.COM_STAGIAIRE_PEC,'')	AS COMMENTAIRES,
         		STAGIAIRE_PEC.BLN_TEMPS_PARTIEL as BLN_TEMPS_PARTIEL,
         		STAGIAIRE_PEC.CENTRE_COUT as CENTRE_COUT,
         		STAGIAIRE_PEC.ID_CODE_INSEE as ID_CODE_INSEE,
         		STAGIAIRE_PEC.ID_FAMILLE_PROFESSIONNELLE as ID_FAMILLE_PROFESSIONNELLE,
         		STAGIAIRE_PEC.SALAIRE_HORAIRE_NET as SALAIRE_HORAIRE_NET,
         		BRANCHE.LIBL_BRANCHE as LIB_BRANCHE,
         		STAGIAIRE_PEC.SALAIRE_HORAIRE_BRUT_CHARGE as SALAIRE_HORAIRE_BRUT_CHARGE,
         		STAGIAIRE_PEC.MONTANT_BRUT_CHARGE as MONTANT_BRUT_CHARGE,
         		STAGIAIRE_PEC.DATE_EMBAUCHE as DATE_EMBAUCHE,
         		NATIONALITE.LIBL_NATIONALITE as NATIONALITE,
         		STAGIAIRE_PEC.ID_NIVEAU_AVENANT as ID_NIVEAU_AVENANT,		
         		STAGIAIRE_PEC.ID_STAGIAIRE_PEC as ID_STAGIAIRE_PEC,		
         		ISTP.NOM_INDIVIDU as NOM_TUTEUR,
         		ISTP.PRENOM_INDIVIDU as PRENOM_TUTEUR,
         		STAGIAIRE_PEC.ID_BRANCHE,
         		STAGIAIRE_PEC.ID_GROUPE,
         		ISNULL((Select	Sum(US.NB_HEURE_ENGAGE)
         			FROM UNITE_STAGIAIRE US
         			LEFT JOIN	STAGIAIRE_PEC SP	ON SP.ID_STAGIAIRE_PEC = US.ID_STAGIAIRE_PEC  
         			WHERE		US.BLN_REFUS					<> 1							AND						
         						SP.ID_MODULE_PEC				=  @ID_MODULE
         						AND SP.ID_SESSION_PEC				IS NULL 
         						AND SP.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU
         			),0) 
         		AS NB_HEURE_PREVU,	
         		STAGIAIRE_PEC.NB_HEURE_REM,	

         		ISNULL((Select Sum(US.NB_HEURE_REGLE)AS DUREE_DEJA_REALISE		
         			FROM UNITE_STAGIAIRE US
         			LEFT JOIN	STAGIAIRE_PEC SP ON SP.ID_STAGIAIRE_PEC	= US.ID_STAGIAIRE_PEC  

         			left outer join session_pec on (SP.id_session_pec = session_pec.ID_SESSION_PEC)
         			WHERE				
         						SP.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU	
         						AND SP.ID_MODULE_PEC	=  @ID_MODULE 
         						AND (@ID_SESSION IS NULL OR (@ID_SESSION IS NOT NULL AND SP.ID_SESSION_PEC <> @ID_SESSION))
         						AND session_pec.bln_actif = 1 
         			),0) 
         		AS NB_HEURE_DEJA_REALISE,
         		STAGIAIRE_PEC.ID_ACTIVITE 

         	INTO #TMP_STAGIAIRE

         	FROM		 
         		STAGIAIRE_PEC
         		INNER JOIN INDIVIDU		ON INDIVIDU.ID_INDIVIDU = STAGIAIRE_PEC.ID_INDIVIDU
         		LEFT JOIN NATIONALITE	ON INDIVIDU.ID_NATIONALITE = NATIONALITE.ID_NATIONALITE
         		LEFT JOIN CSP			ON CSP.ID_CSP			= STAGIAIRE_PEC.ID_CSP
         		LEFT JOIN ETABLISSEMENT ON ETABLISSEMENT.ID_ETABLISSEMENT	= STAGIAIRE_PEC.ID_ETABLISSEMENT
         		LEFT JOIN ADHERENT		ON ADHERENT.ID_ADHERENT				= ETABLISSEMENT.ID_ADHERENT
         		LEFT JOIN UTILISATEUR	ON UTILISATEUR.ID_UTILISATEUR		= INDIVIDU.ID_UTILISATEUR
         		LEFT JOIN BRANCHE ON BRANCHE.ID_BRANCHE = STAGIAIRE_PEC.ID_BRANCHE		
         		LEFT OUTER JOIN STAGIAIRE_PEC STP ON STAGIAIRE_PEC.ID_STAGIAIRE_TUTEUR = STP.ID_STAGIAIRE_PEC
         		LEFT OUTER JOIN INDIVIDU ISTP ON STP.ID_INDIVIDU = ISTP.ID_INDIVIDU

         	WHERE

         		(@ID_SESSION	is not null AND STAGIAIRE_PEC.ID_SESSION_PEC = @ID_SESSION	AND 
         										STAGIAIRE_PEC.ID_SESSION_PEC is not null) 
         		OR
         		(@ID_MODULE		is not null	AND @ID_SESSION		is null						AND 
         										@MODE_SESSION	is null						AND
         										STAGIAIRE_PEC.ID_MODULE_PEC = @ID_MODULE	AND 
         										STAGIAIRE_PEC.ID_SESSION_PEC is null) 		

         	ORDER BY INDIVIDU.NOM_INDIVIDU

         	UPDATE #TMP_STAGIAIRE 
         	SET
         		DUREE_PREVUE = 
         	(
         	CASE WHEN (DUREE_PREVUE > (NB_HEURE_PREVU - NB_HEURE_DEJA_REALISE) ) THEN		
         			(NB_HEURE_PREVU - NB_HEURE_DEJA_REALISE)
         			ELSE
         			DUREE_PREVUE
         	END
         	)

         	SELECT	*, (NB_HEURE_PREVU - NB_HEURE_DEJA_REALISE) AS NB_HEURE_RESTE 
         	FROM	#TMP_STAGIAIRE	
         	WHERE	(NB_HEURE_PREVU - NB_HEURE_DEJA_REALISE) > 0
         	ORDER BY NOM

         END   

 CREATE PROCEDURE [dbo].[INS_PLAN_FINANCEMENT_US]
          @ID_UNITE_STAGIAIRE int,
          @ID_POSTE_COUT_ENGAGE int,
          @ID_DISPOSITIF int,
          @ID_MODULE int,
          @BLN_PLUS10 bit,
          @HOURS_ALL decimal(18,2),
          @HOURS_STAGIAIRE decimal(18,2),
          @MNT_POSTE_COUT_ENGAGE decimal (18, 2),
          @HOURS_US decimal(18,2),
          @BLN_MODIFICATION tinyint
         AS

         BEGIN
          DECLARE @ID_OPERATION_FINANCIERE int
          DECLARE @CIBLE_ACTION int
          DECLARE @ID_ACTION int
          DECLARE @ID_PERIODE int
          DECLARE @ID_AGENCE int
          DECLARE @ID_BRANCHE int
          DECLARE @BLN_DECOUVERT bit
          DECLARE @ID_TYPE_FINANCEMENT int
          DECLARE @ID_ENVELOPPE int
          DECLARE @MNT_PLAN_FINANCEMENT_US decimal(18,2)
          DECLARE @HOURS_STAGIAIRE_DISPOSITIF decimal(18,2)
          DECLARE @COUNT INT
          DECLARE @ID_ORIGINAL int

          IF @BLN_MODIFICATION IS NULL
          BEGIN
           SET @BLN_MODIFICATION = 0
          END 

          SELECT @COUNT = COUNT(*)
          FROM PLAN_FINANCEMENT_US
          WHERE
           ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE AND
           ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE

             SELECT @HOURS_STAGIAIRE_DISPOSITIF = NB_HEURE_ENGAGE from UNITE_STAGIAIRE 
          WHERE ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE
          AND BLN_REFUS =0

          if (@HOURS_STAGIAIRE_DISPOSITIF is null or @HOURS_STAGIAIRE_DISPOSITIF =0)
           return;

          SELECT 
           @ID_ACTION = MODULE_PEC.ID_ACTION_PEC,
           @ID_PERIODE = MODULE_PEC.ID_PERIODE,
           @ID_OPERATION_FINANCIERE = ACTION_PEC.ID_OPERATION_FINANCIERE,
           @CIBLE_ACTION = ACTION_PEC.CIBLE_ACTION
          FROM MODULE_PEC
          INNER JOIN ACTION_PEC ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
           AND MODULE_PEC.ID_MODULE_PEC = @ID_MODULE

          IF @CIBLE_ACTION = 1
          BEGIN

           SELECT @ID_BRANCHE =  STAGIAIRE_PEC.ID_BRANCHE
           FROM STAGIAIRE_PEC
           INNER JOIN UNITE_STAGIAIRE ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
                  AND UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE

           SELECT @ID_AGENCE = ACTION_PEC.ID_AGENCE
           FROM ACTION_PEC
           WHERE ACTION_PEC.ID_ACTION_PEC = @ID_ACTION

           SELECT 
            @ID_OPERATION_FINANCIERE = OPERATION_FINANCIERE.ID_OPERATION_FINANCIERE
           FROM OPERATION_FINANCIERE
           WHERE ID_BRANCHE = @ID_BRANCHE AND
             ID_AGENCE = @ID_AGENCE AND
             ID_DISPOSITIF = @ID_DISPOSITIF AND
             ID_PERIODE = @ID_PERIODE AND 
             BLN_DEFAUT=1
          END

          IF @BLN_PLUS10 = 1
          BEGIN

            SELECT
             @ID_TYPE_FINANCEMENT = ID_TYPE_FINANCEMENT,
             @ID_ENVELOPPE = ID_ENVELOPPE
            FROM PLAN_FINANCEMENT_OPERATION
            INNER JOIN POSTE_COUT_ENGAGE ON 
             PLAN_FINANCEMENT_OPERATION.ID_SOUS_TYPE_COUT = POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT AND 
             POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
            WHERE 
             ID_OPERATION_FINANCIERE = @ID_OPERATION_FINANCIERE

          END
          ELSE
          BEGIN
           SELECT
            @ID_TYPE_FINANCEMENT = PLAN_FINANCEMENT_OPERATION.ID_TYPE_FINANCEMENT,
            @ID_ENVELOPPE = ENVELOPPE.ID_ENVELOPPE
           FROM OPERATION_FINANCIERE
           INNER JOIN PLAN_FINANCEMENT_OPERATION ON 
            PLAN_FINANCEMENT_OPERATION.ID_OPERATION_FINANCIERE = OPERATION_FINANCIERE.ID_OPERATION_FINANCIERE AND 
            OPERATION_FINANCIERE.ID_OPERATION_FINANCIERE = @ID_OPERATION_FINANCIERE AND
            PLAN_FINANCEMENT_OPERATION.ID_TYPE_FINANCEMENT IS NULL AND
            ID_ENVELOPPE IS NOT NULL
           INNER JOIN POSTE_COUT_ENGAGE ON PLAN_FINANCEMENT_OPERATION.ID_SOUS_TYPE_COUT = POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT
            AND POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
           INNER JOIN ENVELOPPE ON ENVELOPPE.ID_ENVELOPPE = PLAN_FINANCEMENT_OPERATION.ID_ENVELOPPE
            AND ENVELOPPE.ID_PERIODE = @ID_PERIODE
           INNER JOIN TYPE_ENVELOPPE ON TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = ENVELOPPE.ID_TYPE_ENVELOPPE
            AND (TYPE_ENVELOPPE.ID_ACTIVITE IS NULL 
             OR TYPE_ENVELOPPE.ID_ACTIVITE in (2,3))

               ORDER BY ISNULL(TYPE_ENVELOPPE.ID_ACTIVITE, 10000) DESC

          END

             SET @MNT_PLAN_FINANCEMENT_US = 0

          IF @COUNT > 0
          BEGIN
           BEGIN
             UPDATE PLAN_FINANCEMENT_US
            SET MNT_PLAN_FINANCEMENT_US = @MNT_PLAN_FINANCEMENT_US
             WHERE 
            ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE AND
            ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE AND
            ID_ENVELOPPE =  @ID_ENVELOPPE AND 
            ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT
           END
           RETURN
          END

          IF @ID_TYPE_FINANCEMENT IS NULL 
            AND @ID_ENVELOPPE IS NULL
          BEGIN
           SET @MNT_PLAN_FINANCEMENT_US = 0
          END

          IF ((@ID_TYPE_FINANCEMENT IS NOT NULL) OR (@ID_ENVELOPPE IS NOT NULL))
          BEGIN
           INSERT INTO PLAN_FINANCEMENT_US
            (DAT_PLAN_FINANCEMENT_US, MNT_PLAN_FINANCEMENT_US,
            BLN_ACTIF, ID_UNITE_STAGIAIRE, ID_TYPE_FINANCEMENT,
            ID_ENVELOPPE, ID_POSTE_COUT_ENGAGE, BLN_ATTENTE_FONDS)
           VALUES
            (GETDATE(), @MNT_PLAN_FINANCEMENT_US, 1, @ID_UNITE_STAGIAIRE,@ID_TYPE_FINANCEMENT, @ID_ENVELOPPE, @ID_POSTE_COUT_ENGAGE, 1)

          END
         END

 CREATE PROCEDURE [dbo].[LEC_GRP_OPERATION]

          @ID_OPERATION_FINANCEUR  INT,
          @COD_OPERATION_FINANCEUR VARCHAR(50),
             @OPERATION_FINANCEUR  VARCHAR(50),
             @ID_BRANCHE     INT,
             @ID_DISPOSITIF    INT,
             @ID_AGENCE     INT,
             @ID_PERIODE     INT,
             @ENVELOPPE     INT,
             @DAT_DEBUT     DATETIME,
             @DAT_FIN     DATETIME,
             @BLN_ACTIF     INT
         AS
         BEGIN
          IF (@BLN_ACTIF IS NULL)
          BEGIN
           SET @BLN_ACTIF = 0
          END

          SELECT DISTINCT
           O_FIN.ID_OPERATION_FINANCIERE,
           O_FIN.LIBL_OPERATION_FINANCIERE AS NOM_OPERATION,
           O_FIN.COD_OPERATION_FINANCIERE,
           B.LIBL_BRANCHE     AS BRANCHE,
           D.LIBL_DISPOSITIF    AS DISPOSITIF,
           A.LIBL_AGENCE     AS AGENCE,
           P.NUM_ANNEE      AS PERIODE,
           O_FIN.BLN_ACTIF
          FROM
           OPERATION_FINANCIERE O_FIN
           LEFT JOIN NR152
            ON NR152.ID_OPERATION_FINANCIERE = O_FIN.ID_OPERATION_FINANCIERE
           LEFT JOIN PERIODE P
            ON P.ID_PERIODE = O_FIN.ID_PERIODE
           LEFT JOIN BRANCHE B
            ON B.ID_BRANCHE = O_FIN.ID_BRANCHE
           LEFT JOIN DISPOSITIF D
            ON D.ID_DISPOSITIF = O_FIN.ID_DISPOSITIF
           LEFT JOIN AGENCE A
            ON A.ID_AGENCE = O_FIN.ID_AGENCE
          WHERE
           (
            (RTRIM(@OPERATION_FINANCEUR) IS NULL)
            OR (O_FIN.LIBL_OPERATION_FINANCIERE LIKE @OPERATION_FINANCEUR + '%')
           )
           AND
           (
            (@ID_OPERATION_FINANCEUR IS NULL)
            OR (O_FIN.ID_OPERATION_FINANCIERE = @ID_OPERATION_FINANCEUR)
           )
           AND
           (
            (RTRIM(@COD_OPERATION_FINANCEUR) IS NULL)
            OR (O_FIN.COD_OPERATION_FINANCIERE = @COD_OPERATION_FINANCEUR)
           )
           AND
           (
            (@ID_PERIODE IS NULL)
            OR (O_FIN.ID_PERIODE = @ID_PERIODE)
           )
           AND
           (
            (@DAT_DEBUT IS NULL)
            OR (O_FIN.DAT_DEBUT >= @DAT_DEBUT))  
           AND
           (
            (@DAT_FIN IS NULL)
            OR (O_FIN.DAT_FIN <= @DAT_FIN)
           )
           AND
           (
            (@ID_BRANCHE IS NULL)
            OR (O_FIN.ID_BRANCHE = @ID_BRANCHE)
           )
           AND
           (
            (@ID_DISPOSITIF IS NULL)
            OR (O_FIN.ID_DISPOSITIF =  @ID_DISPOSITIF)
           )
           AND
           (
            (@ID_AGENCE IS NULL)
            OR (O_FIN.ID_AGENCE = @ID_AGENCE))
           AND
           (
            (@ENVELOPPE IS NULL)
            OR (NR152.ID_ENVELOPPE = @ENVELOPPE)
           )
           AND
           (
            O_FIN.BLN_ACTIF = 1
            OR @BLN_ACTIF = 0
           )
         END

 CREATE procedure [dbo].[INS_DEMANDES_REGLEMENT]
         @ID_UTILISATEUR   int,
         @ID_ACTION    int,
         @ID_MODULE    int,
         @ID_SOUS_TYPE_COUT  int,
         @ID_ADHERENT   int,
         @BLN_ACTIF    bit,
         @BLN_FACTURE_DIRECTE bit,
         @NUM_FACTURE   varchar(50),
         @ID_TYPE_FACTURE  int,
         @NUM_FACTURE_AVOIR  varchar(50),
         @MNT_DEMANDE_HT   decimal(18,2),
         @MNT_DEMANDE_TVA  decimal(18,2),
         @MNT_DEMANDE_TTC  decimal(18,2),
         @MNT_ACCORDE_HT   decimal(18,2),
         @MNT_ACCORDE_TVA  decimal(18,2),
         @MNT_ACCORDE_TTC  decimal(18,2),
         @TAUX_TVA    decimal(10,5),
         @ID_TRANSACTION   int,
         @BLN_OK_FINANCEMENT  tinyint,
         @BLN_OK_PIECE   tinyint,
         @ID_POSTE_COUT_REGLE int output,
         @ID_POSTE_COUT_ENGAGE int output,
         @ID_PCE_NEG    int output,
         @ID_PCE_NEW    int output,
         @ID_SESSION_PEC   int,
         @ID_ETABLISSEMENT  int,
         @ID_FACTURE    int,
         @ID_TYPE_TVA   int,
         @BLN_PAIEMENT_PARTIEL bit,
         @BLN_DEMANDE_CLOTURE_MODULE bit 
         AS

         BEGIN

          DECLARE @POSTE_COUT_ENG INT

          IF @BLN_FACTURE_DIRECTE IS NULL
           SET @BLN_FACTURE_DIRECTE = 0
          IF @BLN_ACTIF IS NULL
           SET @BLN_ACTIF = 0

          select
           @POSTE_COUT_ENG = ID_POSTE_COUT_ENGAGE
          from
           POSTE_COUT_ENGAGE
          WHERE
           ID_SOUS_TYPE_COUT = @ID_SOUS_TYPE_COUT
           AND DAT_DESENGAGEMENT is null
           AND ID_MODULE_PEC = @ID_MODULE

          DECLARE @BLN_CTRL INT
          SET  @BLN_CTRL = 0

          IF (@ID_TYPE_FACTURE = 2)
          BEGIN
           SET @BLN_CTRL = 1
           SET @BLN_OK_FINANCEMENT = 1
          END

          INSERT INTO POSTE_COUT_REGLE
             (COD_POSTE_COUT_REGLE, MNT_REGLE_HT, MNT_REGLE_TVA, MNT_REGLE_TTC, NUM_FACTURE,
           BLN_FACTURE_DIRECTE, TAU_TVA, BLN_ACTIF, ID_TRANSACTION, ID_SOUS_TYPE_COUT,
           ID_MODULE_PEC, MNT_DEMANDE_HT, ID_TYPE_FACTURE, MNT_DEMANDE_TVA,
           MNT_DEMANDE_TTC, DAT_CREATION, DAT_MODIF, ID_UTILISATEUR, NUM_FACTURE_AVOIR,
           BLN_OK_FINANCEMENT, ID_ETABLISSEMENT, BLN_OK_PIECE, ID_ETAT_PEC, BLN_A_CONTROLER, ID_POSTE_COUT_ENGAGE, ID_SESSION_PEC,ID_FACTURE,ID_TYPE_TVA,
           BLN_PAIEMENT_PARTIEL, BLN_DEMANDE_CLOTURE_MODULE )
           VALUES
             (0, @MNT_ACCORDE_HT, @MNT_ACCORDE_TVA, @MNT_ACCORDE_TTC, @NUM_FACTURE,
           @BLN_FACTURE_DIRECTE, @TAUX_TVA ,@BLN_ACTIF, @ID_TRANSACTION, @ID_SOUS_TYPE_COUT,
           @ID_MODULE, @MNT_DEMANDE_HT, @ID_TYPE_FACTURE, @MNT_DEMANDE_TVA,
           @MNT_DEMANDE_TTC, GETDATE(), GETDATE(), @ID_UTILISATEUR, @NUM_FACTURE_AVOIR,
           @BLN_OK_FINANCEMENT, @ID_ETABLISSEMENT, @BLN_OK_PIECE, 1, @BLN_CTRL, @POSTE_COUT_ENG, @ID_SESSION_PEC,@ID_FACTURE,@ID_TYPE_TVA,
           @BLN_PAIEMENT_PARTIEL, @BLN_DEMANDE_CLOTURE_MODULE)

          SET @ID_POSTE_COUT_REGLE = @@IDENTITY

          UPDATE POSTE_COUT_REGLE
           SET COD_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE
          WHERE ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE

          UPDATE
           POSTE_COUT_REGLE
          SET
           ID_POSTE_COUT_ENGAGE = @POSTE_COUT_ENG
          WHERE
           ID_MODULE_PEC = @ID_MODULE
           AND ID_SOUS_TYPE_COUT = @ID_SOUS_TYPE_COUT

         END

 CREATE procedure [dbo].[UPD_DEMANDES_REGLEMENT]
          @ID_UTILISATEUR    int,
          @ID_ACTION     int,
          @ID_MODULE     int,
          @ID_SOUS_TYPE_COUT   int,
          @ID_ADHERENT    int,
          @BLN_ACTIF     bit,
          @BLN_FACTURE_DIRECTE  bit,
          @NUM_FACTURE    varchar(50),
          @ID_TYPE_FACTURE   int,
          @NUM_FACTURE_AVOIR   varchar(50),
          @MNT_DEMANDE_HT    decimal(18,2),
          @MNT_DEMANDE_TVA   decimal(18,2),
          @MNT_DEMANDE_TTC   decimal(18,2),
          @MNT_ACCORDE_HT    decimal(18,2),
          @MNT_ACCORDE_TVA   decimal(18,2),
          @MNT_ACCORDE_TTC   decimal(18,2),
          @TAUX_TVA     decimal(10,5),
          @ID_TRANSACTION    int,
          @ID_POSTE_COUT_REGLE  int,
          @BLN_OK_FINANCEMENT   tinyint,
          @BLN_OK_PIECE    tinyint,
          @ID_POSTE_COUT_ENGAGE  int output,
          @ID_PCE_NEG     int output,
          @ID_PCE_NEW     int output,
          @ID_SESSION_PEC    int,
          @ID_ETABLISSEMENT   int,
          @ID_FACTURE     int,
          @ID_TYPE_TVA    int,
          @BLN_PAIEMENT_PARTIEL  bit,
          @BLN_DEMANDE_CLOTURE_MODULE bit
         AS

          BEGIN

           SET QUOTED_IDENTIFIER ON

           DECLARE @BLN_A_CONTROLER INT
           SET  @BLN_A_CONTROLER = -1

           IF (EXISTS(select 1 from POSTE_COUT_REGLE 
               INNER JOIN [TRANSACTION] ON [TRANSACTION].ID_TRANSACTION = POSTE_COUT_REGLE.ID_TRANSACTION
                where ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE AND 
                  POSTE_COUT_REGLE.ID_TYPE_VALIDATION = 8 AND 
                  ID_ETAT_PEC = 4 AND
                  [TRANSACTION].NUM_IBAN is not null))
           BEGIN
            SET @BLN_A_CONTROLER = 1
           END

           UPDATE 
            POSTE_COUT_REGLE
           SET
            MNT_REGLE_HT  = @MNT_ACCORDE_HT,
            ID_ETABLISSEMENT = @ID_ETABLISSEMENT,
            MNT_REGLE_TVA  = @MNT_ACCORDE_TVA,
            MNT_REGLE_TTC  = @MNT_ACCORDE_TTC,
            NUM_FACTURE   = @NUM_FACTURE,
            BLN_FACTURE_DIRECTE = @BLN_FACTURE_DIRECTE,
            TAU_TVA    = @TAUX_TVA,
            BLN_ACTIF   = @BLN_ACTIF,
            ID_TRANSACTION  = @ID_TRANSACTION,
            ID_SOUS_TYPE_COUT = @ID_SOUS_TYPE_COUT,
            ID_MODULE_PEC  = @ID_MODULE,
            MNT_DEMANDE_HT  = @MNT_DEMANDE_HT,
            ID_TYPE_FACTURE  = @ID_TYPE_FACTURE,
            MNT_DEMANDE_TVA  = @MNT_DEMANDE_TVA,
            MNT_DEMANDE_TTC  = @MNT_DEMANDE_TTC,
            DAT_MODIF   = GETDATE(),
            ID_UTILISATEUR  = @ID_UTILISATEUR,
            NUM_FACTURE_AVOIR = @NUM_FACTURE_AVOIR,
            BLN_OK_FINANCEMENT = @BLN_OK_FINANCEMENT,
            ID_ETAT_PEC   = 4,
            ID_SESSION_PEC  = @ID_SESSION_PEC,
            BLN_A_CONTROLER  = (case when @BLN_A_CONTROLER = -1 then BLN_A_CONTROLER else @BLN_A_CONTROLER end),
            ID_FACTURE   = @ID_FACTURE,
            ID_TYPE_TVA   = @ID_TYPE_TVA,
            BLN_OK_PIECE  = @BLN_OK_PIECE,
            BLN_PAIEMENT_PARTIEL  = @BLN_PAIEMENT_PARTIEL,
            BLN_DEMANDE_CLOTURE_MODULE = @BLN_DEMANDE_CLOTURE_MODULE
           WHERE ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE

          END

 CREATE PROCEDURE [dbo].[INS_ETABLISSEMENT_EXTRANET]
          @ID_ETABLISSEMENT INT,
          @ID_ADHERENT INT,
          @LIB_ENSEIGNE VARCHAR(50),
          @NUM_SIRET VARCHAR(14),
          @ID_NAF INT,
          @COD_IDCC VARCHAR(8),
          @LIB_ADRESSE_1 VARCHAR(100),
          @LIB_ADRESSE_2 VARCHAR(100),
          @LIB_ADRESSE_3 VARCHAR(100),
          @LIB_CP_CEDEX VARCHAR(5),
          @LIB_VIL_CEDEX VARCHAR(50),
          @DAT_CREATION DATETIME,
          @EMAIL_COLLECTE VARCHAR(250),
          @FROM_ADRESSE VARCHAR(50),
          @ID_ETAB_INSERE INT OUT
         AS

         BEGIN
          DECLARE
           @ID_BRANCHE   INT,
           @COD_ADHERENT  VARCHAR(6),
           @SUBJECT   NVARCHAR(255),
           @BODY    NVARCHAR(MAX),
           @ERROR_MESSAGE  NVARCHAR(4000),
           @NUM_SIRET_OPTI  VARCHAR(14),
           @LIB_ENSEIGNE_OPTI  VARCHAR(50),
           @COD_NAF_OPTI  VARCHAR(8),
           @COD_IDCC_OPTI  VARCHAR(8),
           @LIB_ADRESSE_1_OPTI VARCHAR(100),
           @LIB_ADRESSE_2_OPTI VARCHAR(100),
           @LIB_ADRESSE_3_OPTI VARCHAR(100),
           @LIB_CP_CEDEX_OPTI VARCHAR(5),
           @LIB_VIL_CEDEX_OPTI VARCHAR(50),
           @SEND_EMAIL   TINYINT = 0

          SET @ID_ETAB_INSERE = 0

          SELECT
           @ID_BRANCHE = ID_BRANCHE,
           @COD_ADHERENT = CAST(COD_ADHERENT AS VARCHAR(6))
          FROM
           ADHERENT
          WHERE
           ID_ADHERENT = @ID_ADHERENT

          SELECT
                 @ID_ETAB_INSERE = E.ID_ETABLISSEMENT,
           @NUM_SIRET_OPTI = E.NUM_SIRET,
           @LIB_ENSEIGNE_OPTI = E.LIB_ENSEIGNE,
           @COD_NAF_OPTI = N.COD_NAF,
           @COD_IDCC_OPTI = I.COD_IDCC,
           @LIB_ADRESSE_1_OPTI = A.LIB_ADR,
           @LIB_ADRESSE_2_OPTI = A.LIB_COMP_VOIE,
           @LIB_ADRESSE_3_OPTI = A.LIB_NOM_VOIE,
           @LIB_CP_CEDEX_OPTI = A.LIB_CP_CEDEX,
           @LIB_VIL_CEDEX_OPTI = A.LIB_VIL_CEDEX
          FROM
           ETABLISSEMENT E
           LEFT JOIN ADRESSE A
            ON A.ID_ADRESSE = E.ID_ADRESSE_PRINCIPALE
            AND A.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
           LEFT JOIN IDCC I
            ON I.ID_IDCC = E.ID_IDCC_APPLIQUEE
           LEFT JOIN NAF N
            ON N.ID_NAF = E.ID_NAF
          WHERE
           E.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
           OR E.NUM_SIRET = @NUM_SIRET

          IF (@ID_BRANCHE IS NOT NULL)
          BEGIN
           IF (@NUM_SIRET_OPTI IS NULL)
           BEGIN
            BEGIN TRY
             BEGIN TRAN TRAN_INS_ETABLISEMENT

             DECLARE
              @ID_ADRESSE INT,
              @ID_IDCC INT,
              @ID_GROUPE INT

             EXEC @ID_ADRESSE = dbo.INS_ADRESSE
              @ID_ADRESSE = NULL,
              @ID_ETABLISSEMENT_OF = NULL,
              @ID_TYPE_ADRESSE = 2,
              @ID_COMP_NUM_ADR = NULL,
              @ID_UO = NULL,
              @ID_ETABLISSEMENT = NULL,
              @ID_TIERS = NULL,
              @ID_TYPE_VOIE = 13,
              @NUM_VOIE = NULL,
              @NUM_TEL = NULL,
              @NUM_FAX = NULL,
              @BLN_ACTIF = 1,
              @COM_ADRESSE = 'Cr‚‚ par Extranet',
              @ID_UTILISATEUR = 82,
              @LIB_NOM_VOIE = @LIB_ADRESSE_1,
              @LIB_COMP_VOIE = @LIB_ADRESSE_2,
              @LIB_ADR = @LIB_ADRESSE_3,
              @BLN_PRINCIPAL = 1,
              @LIB_CP_CEDEX = @LIB_CP_CEDEX,
              @LIB_VIL_CEDEX = @LIB_VIL_CEDEX,
              @ID_PAYS = 1,
              @ID_MENTION_PARTICULIERE = NULL,
              @LIB_MENTION_PARTICULIERE = NULL,
              @ID_HEXAPOSTE = NULL,
              @EMAIL_PRO = NULL

             SET @ID_IDCC = (SELECT TOP 1 ID_IDCC FROM IDCC WHERE COD_IDCC = @COD_IDCC)
             SET @ID_GROUPE = (SELECT TOP 1 ID_GROUPE FROM ETABLISSEMENT WHERE ID_ADHERENT = @ID_ADHERENT AND BLN_PRINCIPAL = 1)

             EXEC @ID_ETAB_INSERE = dbo.INS_ETABLISSEMENT
              @ID_ADRESSE_PRINCIPALE = @ID_ADRESSE,
              @ID_ADHERENT = @ID_ADHERENT,
              @ID_ETAT_SIRET = 4,
              @ID_TYPE_TVA = 15,
              @COD_ETABLISSEMENT = NULL,
              @NUM_SIRET = @NUM_SIRET,
              @BLN_ACTIF = 0,
              @BLN_PRINCIPAL = 0,
              @COM_ETABLISSEMENT = 'Cr‚‚ par Extranet',
              @NUM_IBAN = NULL,
              @ID_UTILISATEUR = 82,
              @LIB_ENSEIGNE = @LIB_ENSEIGNE,
              @ID_UTILISATEUR_CREATEUR = 82,
              @ID_NAF = @ID_NAF,
              @ID_BRANCHE = @ID_BRANCHE,
              @ID_IDCC_APPLIQUEE = @ID_IDCC,
              @ID_ACTIVITE_ETABLISSEMENT = NULL,
              @ID_AGENCE = NULL,
              @ID_CHARGEE_MISSION = NULL,
              @ID_CHARGEE_RELATION = NULL,
              @ID_GROUPE = @ID_GROUPE,

              @BLN_CREATION = 1 

             UPDATE
              ADRESSE
             SET
              ID_ETABLISSEMENT = @ID_ETAB_INSERE
             WHERE
              ID_ADRESSE = @ID_ADRESSE

             insert into NR67bis (ID_ETABLISSEMENT, ID_DOCUMENT)
             select @ID_ETAB_INSERE, id_document from document where BLN_ABONNEMENT_ADHERENT = 1 and BLN_ACTIF =1

             SET @SUBJECT = N'Extranet : D‚claration de nouvel ‚tablissement' 
              + @NUM_SIRET + N' pour l''adh‚rent ' + @COD_ADHERENT  COLLATE French_CI_AI

             SET @BODY = N'<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"><html><head><meta http-equiv="content-type" content="text/html; charset=utf-8">' 
              + N'<title>Demande de cr‚ation d''''un nouvel ‚tablissement</title></head><body>' 
              + N'<p> Pour l''adh‚rent ' 
              + @COD_ADHERENT + N'<br>' 
              + N'Les informations synchronis‚es …ÿ partir de l''Extranet sont les suivantes : <br> ' 
              + N'Enseigne : ' + @LIB_ENSEIGNE + ' <br> ' 
              + N'SIRET : ' + @NUM_SIRET + ' <br> ' 
              + N'IDCC : ' + @COD_IDCC + ' <br> ' 
              + N'Adresse : ' + @LIB_ADRESSE_1 
              + N' <br> ' + @LIB_ADRESSE_2 
              + N' <br> ' + @LIB_ADRESSE_3 
              + N' <br> ' + @LIB_CP_CEDEX 
              + ' ' + @LIB_VIL_CEDEX + ' <br> ' 
              + '.</p></body></html>' COLLATE French_CI_AI;

             SET @SEND_EMAIL = 1

             COMMIT TRAN TRAN_INS_ETABLISEMENT
            END TRY
            BEGIN CATCH
             SELECT
              ERROR_NUMBER() AS  ERRORNUMBER,
              ERROR_MESSAGE() AS ERRORMESSAGE,
              ERROR_LINE(),
              ERROR_PROCEDURE();

             IF XACT_STATE() = -1
             BEGIN
              PRINT
               N'The transaction TRAN_INS_ETABLISEMENT is in an uncommittable state. ' + 'Rolling back transaction.'
              SET @SEND_EMAIL = 0
              ROLLBACK TRANSACTION CLOTURE_MODULE_PEC;
             END;

             IF XACT_STATE() = 1
             BEGIN
              PRINT
               N'The transaction TRAN_INS_ETABLISEMENT is committable. ' + 'Committing transaction.'
              SET @SEND_EMAIL = 1
              COMMIT TRANSACTION CLOTURE_MODULE_PEC;
             END;
            END CATCH
           END
           ELSE

           BEGIN
            SET @SUBJECT = 'Extranet : Modification d''‚tablissement SIRET ' 
             + @NUM_SIRET_OPTI + ' pour l''adh‚rent ' 
             + @COD_ADHERENT COLLATE French_CI_AI

            DECLARE
             @COD_NAF VARCHAR(8)

            SELECT
             @COD_NAF = COD_NAF
            FROM
             NAF
            WHERE
             ID_NAF = @ID_NAF     

            SET @BODY = N'<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"><html><head><meta http-equiv="content-type" content="text/html; charset=utf-8">' 
             + N'<title>Modification d''''un ‚tablissement</title></head><body>' 
             + N'<p> Une modification a ‚t‚ signal‚e dans l''Extranet sur l''Etablissement ' + @LIB_ENSEIGNE_OPTI
             + N'<br> pour l''adh‚rent : ' + @COD_ADHERENT

            IF(@NUM_SIRET_OPTI<>@NUM_SIRET)
            BEGIN
             SET @BODY = @BODY + N'<br> ANCIEN SIRET : ' + @NUM_SIRET_OPTI + N' / NOUVEAU SIRET : ' + @NUM_SIRET
             SET @SEND_EMAIL = 1
            END

            IF(@LIB_ENSEIGNE_OPTI<>@LIB_ENSEIGNE)
            BEGIN
             SET @BODY = @BODY + N'<br> ANCIENNE ENSEIGNE : ' + @LIB_ENSEIGNE_OPTI + N' / NOUVEAU ENSEIGNE : ' + @LIB_ENSEIGNE
             SET @SEND_EMAIL = 1
            END

            IF(@COD_NAF_OPTI<>@COD_NAF)
            BEGIN
             SET @BODY = @BODY + N'<br> ANCIEN NAF : ' + @COD_NAF_OPTI + N' / NOUVEAU NAF : ' + @COD_NAF
             SET @SEND_EMAIL = 1
            END

            IF(@COD_IDCC_OPTI<>@COD_IDCC)
            BEGIN
             SET @BODY = @BODY + '<br> ANCIEN IDCC : ' + @COD_IDCC_OPTI + ' / NOUVEAU IDCC : ' + @COD_IDCC
             SET @SEND_EMAIL = 1
            END

            IF(@LIB_ADRESSE_1_OPTI <> @LIB_ADRESSE_1
            OR @LIB_ADRESSE_2_OPTI <> @LIB_ADRESSE_2
            OR @LIB_CP_CEDEX_OPTI <> @LIB_CP_CEDEX
            OR @LIB_VIL_CEDEX_OPTI <> @LIB_VIL_CEDEX)
            BEGIN
             SET @BODY = @BODY + '<br><br> ANCIENNE ADRESSE : ' 
              + '<br>' + COALESCE(@LIB_ADRESSE_1_OPTI,'')
              + '<br>' + COALESCE(@LIB_ADRESSE_2_OPTI,'')
              + '<br>' + COALESCE(@LIB_CP_CEDEX_OPTI,'')
              + '<br>' + COALESCE(@LIB_VIL_CEDEX_OPTI,'')
              + '<br><br> NOUVELLE ADRESSE : '
              + '<br>' + COALESCE(@LIB_ADRESSE_1,'')
              + '<br>' + COALESCE(@LIB_ADRESSE_2,'')
              + '<br>' + COALESCE(@LIB_CP_CEDEX,'')
              + '<br>' + COALESCE(@LIB_VIL_CEDEX,'')
             SET @SEND_EMAIL = 1
            END 

            SET @BODY = @BODY + '<br><br>Merci de reporter les changements dans Optiform.</p></body></html>'  COLLATE French_CI_AI;
           END

           IF (@SEND_EMAIL = 1)
           EXEC dbo.ENVOIE_MAIL
            @RECIPIENTS = @EMAIL_COLLECTE,
            @COPY_RECIPIENTS = NULL,
            @BLIND_COPY_RECIPIENTS = NULL,
            @SUBJECT = @SUBJECT,
            @BODY_FORMAT = 'html',
            @BODY = @BODY,
            @FROM_ADRESS = @FROM_ADRESSE,
            @ATTACHMENT_PATH = NULL,
            @ERROR_MESSAGE = @ERROR_MESSAGE

           SELECT
            @ERROR_MESSAGE
          END
         END

 CREATE PROCEDURE [dbo].[UPD_HISTO_CONTRAT_DGEFP_ENVOYES]
          @LIST_NOM_FICHIER_XML TEXT 

         AS

         BEGIN

          DECLARE @Item varchar(255);

          CREATE TABLE #List(Item VARCHAR(255)  collate French_CI_AI)

          DECLARE @Delimiter char
          SET @Delimiter = ','
          WHILE CHARINDEX(@Delimiter,@LIST_NOM_FICHIER_XML,0) <> 0
          BEGIN
           SELECT
            @Item=RTRIM(LTRIM(SUBSTRING(@LIST_NOM_FICHIER_XML,1,CHARINDEX(@Delimiter,@LIST_NOM_FICHIER_XML,0)-1))),
            @LIST_NOM_FICHIER_XML=RTRIM(LTRIM(SUBSTRING(@LIST_NOM_FICHIER_XML,CHARINDEX(@Delimiter,@LIST_NOM_FICHIER_XML,0)+1,DATALENGTH(@LIST_NOM_FICHIER_XML))))

           IF LEN(@Item) > 0
            INSERT INTO #List SELECT cast(@Item as varchar(255) )
          END

          IF DATALENGTH(@LIST_NOM_FICHIER_XML) > 0
           begin
            select @Item = cast(@LIST_NOM_FICHIER_XML as varchar(255))
            INSERT INTO #List SELECT @Item 
           end

          UPDATE HISTO_CONTRAT_DGEFP
          SET  DAT_ENVOI_DGEFP = GETDATE(),
            STATUT_HISTO = 10

          WHERE (NOM_FICHIER_XML IN (SELECT Item FROM #List))

         END

 CREATE PROCEDURE [dbo].[INS_ETABLISSEMENT]
          @ID_ADRESSE_PRINCIPALE  INT,
          @ID_ADHERENT    INT,  
          @ID_ETAT_SIRET    INT,  
          @ID_TYPE_TVA    INT,  
          @COD_ETABLISSEMENT   VARCHAR(8),  
          @NUM_SIRET     VARCHAR(14),
          @BLN_ACTIF     TINYINT,
          @BLN_PRINCIPAL    TINYINT,
          @COM_ETABLISSEMENT   VARCHAR(255),
          @NUM_IBAN     VARCHAR(34),
          @ID_UTILISATEUR    INT,
          @LIB_ENSEIGNE    VARCHAR(50),
          @ID_UTILISATEUR_CREATEUR INT,
          @ID_NAF      INT,
          @ID_BRANCHE     INT,
          @ID_IDCC_APPLIQUEE   INT,
          @ID_ACTIVITE_ETABLISSEMENT INT,
          @ID_AGENCE     INT,
          @ID_CHARGEE_MISSION   INT,
          @ID_CHARGEE_RELATION  INT,
          @ID_GROUPE     INT,

          @BLN_CREATION    BIT = 0   
         AS

         BEGIN
          INSERT INTO ETABLISSEMENT
             (
           ID_ADRESSE_PRINCIPALE,
           ID_ADHERENT,
           ID_ETAT_SIRET,
           ID_TYPE_TVA,
           COD_ETABLISSEMENT,
           NUM_SIRET,
           BLN_ACTIF,
           BLN_PRINCIPAL,
           COM_ETABLISSEMENT,
           NUM_IBAN,
           DAT_MODIF,
           ID_UTILISATEUR,
           DAT_CREATION,
           LIB_ENSEIGNE,
           ID_UTILISATEUR_CREATEUR,
           ID_NAF,
           ID_BRANCHE,
           ID_IDCC_APPLIQUEE,
           ID_ACTIVITE_ETABLISSEMENT,
           ID_AGENCE,
           ID_CHARGEE_MISSION,
           ID_CHARGEE_RELATION,
           ID_GROUPE,
           BLN_CREATION
          )
          VALUES
          (
           @ID_ADRESSE_PRINCIPALE,
           @ID_ADHERENT,
           @ID_ETAT_SIRET,
           @ID_TYPE_TVA,
           @COD_ETABLISSEMENT,
           @NUM_SIRET,
           @BLN_ACTIF,
           @BLN_PRINCIPAL,
           @COM_ETABLISSEMENT,
           @NUM_IBAN,
           getDate(),
           @ID_UTILISATEUR,
           getDate(),
           @LIB_ENSEIGNE,
           @ID_UTILISATEUR_CREATEUR,
           @ID_NAF,
           @ID_BRANCHE,
           @ID_IDCC_APPLIQUEE,
           @ID_ACTIVITE_ETABLISSEMENT,
           @ID_AGENCE,
           @ID_CHARGEE_MISSION,
           @ID_CHARGEE_RELATION,
           @ID_GROUPE,
           @BLN_CREATION
          )

          IF (@COD_ETABLISSEMENT IS NULL )
          BEGIN
           UPDATE
            ETABLISSEMENT
           SET
            COD_ETABLISSEMENT = @@IDENTITY
           WHERE
            ID_ETABLISSEMENT = @@IDENTITY
          END

          IF (@BLN_PRINCIPAL = 1 AND @ID_ADHERENT > 0)
          BEGIN
           UPDATE
            ETABLISSEMENT
           SET
            BLN_PRINCIPAL = 0
           WHERE
            BLN_PRINCIPAL = 1
            AND ID_ADHERENT = @ID_ADHERENT
            AND ID_ETABLISSEMENT <> SCOPE_IDENTITY()

           UPDATE
            ADHERENT
           SET
            ID_TYPE_TVA = @ID_TYPE_TVA
           WHERE
            ID_ADHERENT = @ID_ADHERENT
          END

          RETURN @@IDENTITY
         END

 CREATE PROCEDURE [dbo].[MVT_BUDGETAIRE_PEC_INS_PLAN_FINANCEMENT]    
          @ID_EVENEMENT  INT,  
          @ID_TYPE_MOUVEMENT INT,  
          @P_E_R    varchar(1)  
         AS  
           
         BEGIN  
          DECLARE   
          @BLN_GESTION_GROUP  TINYINT,  
          @MNT_PFUS    DECIMAL(15,2),  
          @LIBL_MVT_BUDGETAIRE VARCHAR(60),  
          @COD_MODULE_PEC   VARCHAR(14),  
          @COD_TYPE_COUT   VARCHAR(8),  
          @COD_SOUS_TYPE_COUT  VARCHAR(8),  
          @ID_MODULE_PEC   INT,  
          @ID_PERIODE_MODULE_PEC INT ,  
          @ID_ACTION_PEC   INT,  
          @ID_ETABLISSEMENT  INT,  
          @ID_TYPE_FINANCEMENT INT,  
          @ID_ENVELOPPE   INT,  
          @ID_ADHERENT   INT,  
          @ID_PERIODE_FISC  INT,  
          @ID_PERIODE_CPT   INT,  
          @ID_COMPTE    INT,  
          @ID_DISPOSITIF   INT,  
          @ID_ACTIVITE   INT,  
          @ID_SOUS_TYPE_COUT  INT,  
          @ID_MVT_BUDGETAIRE  INT,  
          @ID_GROUPE_STAGIAIRE_PEC INT ,  
          @COD_TYPE_EVENEMENT  VARCHAR(8),  
          @ID_POSTE_COUT_ENGAGE INT,  
          @ID_POSTE_COUT_REGLE INT,  
          @DAT_EVENEMENT   DATETIME,  
          @N_R     VARCHAR(1),  
          @COD_FACTURE   VARCHAR(50),  
          @INVERSION_SIGNE_MVT TINYINT,  
          @MNT_ENGAGEMENT_RESIDUEL DECIMAL(15,2)  

          SELECT @ID_POSTE_COUT_ENGAGE = ID_POSTE_COUT_ENGAGE,  
            @ID_POSTE_COUT_REGLE = ID_POSTE_COUT_REGLE,  
            @DAT_EVENEMENT   = DAT_EVENEMENT,  
            @COD_TYPE_EVENEMENT  = COD_TYPE_EVENEMENT  
          FROM EVENEMENT  
          INNER JOIN TYPE_EVENEMENT ON EVENEMENT.ID_TYPE_EVENEMENT =TYPE_EVENEMENT.ID_TYPE_EVENEMENT  
          WHERE ID_EVENEMENT = @ID_EVENEMENT  

          SET @ID_COMPTE = NULL  
          SET @ID_ACTIVITE = NULL  
          SET @ID_DISPOSITIF = NULL  

          IF @ID_POSTE_COUT_REGLE IS NOT NULL  
          BEGIN  
           SELECT   
           @ID_MODULE_PEC   = MODULE_PEC.ID_MODULE_PEC,   
           @COD_MODULE_PEC   = COD_MODULE_PEC,  
           @ID_PERIODE_MODULE_PEC = ID_PERIODE,   
           @ID_ACTION_PEC   = ID_ACTION_PEC,  
           @ID_SOUS_TYPE_COUT  = ID_SOUS_TYPE_COUT,  
           @COD_FACTURE   =  FACTURE.COD_FACTURE  
           FROM POSTE_COUT_REGLE   
           INNER JOIN MODULE_PEC ON  MODULE_PEC.ID_MODULE_PEC = POSTE_COUT_REGLE.ID_MODULE_PEC  
           INNER JOIN FACTURE  ON  FACTURE.ID_FACTURE= POSTE_COUT_REGLE.ID_FACTURE  
           WHERE ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE  
          END  
          ELSE  
          BEGIN  
           SELECT   
            @ID_MODULE_PEC   = MODULE_PEC.ID_MODULE_PEC,   
            @COD_MODULE_PEC   = COD_MODULE_PEC,  
            @ID_PERIODE_MODULE_PEC = ID_PERIODE,   
            @ID_ACTION_PEC   = ID_ACTION_PEC,  
            @ID_SOUS_TYPE_COUT  = ID_SOUS_TYPE_COUT  
           FROM POSTE_COUT_ENGAGE  
           INNER JOIN MODULE_PEC ON MODULE_PEC.ID_MODULE_PEC = POSTE_COUT_ENGAGE.ID_MODULE_PEC  
           WHERE ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE  
          END  

          SELECT @COD_TYPE_COUT = tc.COD_TYPE_COUT,   
          @COD_SOUS_TYPE_COUT= stc.COD_SOUS_TYPE_COUT  
          FROM SOUS_TYPE_COUT stc  
          INNER JOIN TYPE_COUT tc ON tc.ID_TYPE_COUT = stc.ID_TYPE_COUT  
          WHERE stc.ID_SOUS_TYPE_COUT = @ID_SOUS_TYPE_COUT  

          SET @N_R = 'N'  
          SET @INVERSION_SIGNE_MVT = 0  
          IF @COD_TYPE_EVENEMENT = 'CHIF_PEC' 
          BEGIN  
           SET @LIBL_MVT_BUDGETAIRE = 'Chiffrage PEC ' + @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT  
          END  

          IF @COD_TYPE_EVENEMENT = 'ENGAGMT' 
          BEGIN  
           SET @LIBL_MVT_BUDGETAIRE = 'Engagement '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT  
          END  

          IF @COD_TYPE_EVENEMENT = 'REGLEMT' 
          BEGIN  
           IF @ID_TYPE_MOUVEMENT = 21 
           BEGIN  
            SET @INVERSION_SIGNE_MVT = 1  
            SET @LIBL_MVT_BUDGETAIRE = 'Annul eng suite Reg '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT + ' Fact. ' + @COD_FACTURE  
           END  
           ELSE IF @ID_TYPE_MOUVEMENT = 22 
           BEGIN  
            SET @LIBL_MVT_BUDGETAIRE = 'Reglement '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT + ' Fact. ' + @COD_FACTURE  
           END  
          END  

          IF (@COD_TYPE_EVENEMENT IS NULL OR @ID_TYPE_MOUVEMENT IS NULL)  
          BEGIN  
           SELECT PB = 'Type d evenement / Type de mouvement non definis', COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT, ID_TYPE_MOUVEMENT = @ID_TYPE_MOUVEMENT  
          END  
          ELSE  
          BEGIN  

          IF @ID_POSTE_COUT_REGLE IS NOT NULL  
          BEGIN  
           DECLARE CURSOR_PFUS CURSOR FOR  
           SELECT s_s.ID_ETABLISSEMENT,   
             pus.ID_TYPE_FINANCEMENT,   
             pus.ID_ENVELOPPE,   
             SUM(ISNULL(CAST(MNT_PLAN_FINANCEMENT_US AS DECIMAL(15,2)),0)) AS MNT_PFUS,  
             s_m.ID_GROUPE,  
             s_m.ID_ACTIVITE  
           FROM UNITE_STAGIAIRE us  
           INNER JOIN PLAN_FINANCEMENT_US pus ON pus.ID_UNITE_STAGIAIRE = us.ID_UNITE_STAGIAIRE  
           INNER JOIN STAGIAIRE_PEC s_s   ON s_s.ID_STAGIAIRE_PEC = us.ID_STAGIAIRE_PEC  
           INNER JOIN STAGIAIRE_PEC s_m  ON s_m.ID_MODULE_PEC = s_s.ID_MODULE_PEC   
             AND s_m.ID_ETABLISSEMENT = s_s.ID_ETABLISSEMENT   
             AND s_m.ID_INDIVIDU = s_s.ID_INDIVIDU   
             AND s_m.id_SESSION_PEC is null 
           WHERE pus.ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE  
           GROUP BY s_s.ID_ETABLISSEMENT,   
              pus.ID_TYPE_FINANCEMENT,   
              pus.ID_ENVELOPPE,  
              s_m.ID_GROUPE,   
              s_m.ID_ACTIVITE    
          END  
          ELSE  
          BEGIN  
           DECLARE CURSOR_PFUS CURSOR FOR  
           SELECT s.ID_ETABLISSEMENT,   
             pus.ID_TYPE_FINANCEMENT,   
             pus.ID_ENVELOPPE,   
             SUM(ISNULL(CAST(MNT_PLAN_FINANCEMENT_US AS DECIMAL(15,2)),0)) AS MNT_PFUS,  
             s.ID_GROUPE,  
             s.ID_ACTIVITE  
           FROM UNITE_STAGIAIRE us  
           INNER JOIN PLAN_FINANCEMENT_US pus ON pus.ID_UNITE_STAGIAIRE = us.ID_UNITE_STAGIAIRE  
           INNER JOIN STAGIAIRE_PEC s    ON s.ID_STAGIAIRE_PEC = us.ID_STAGIAIRE_PEC  
          WHERE pus.ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE  
           GROUP BY s.ID_ETABLISSEMENT,   
              pus.ID_TYPE_FINANCEMENT,   
              pus.ID_ENVELOPPE,  
              s.ID_GROUPE,   
              s.ID_ACTIVITE   
          END  

          OPEN CURSOR_PFUS  
          FETCH NEXT FROM CURSOR_PFUS INTO  
          @ID_ETABLISSEMENT,  
          @ID_TYPE_FINANCEMENT,  
          @ID_ENVELOPPE,  
          @MNT_PFUS,  
          @ID_GROUPE_STAGIAIRE_PEC,  
          @ID_ACTIVITE  

          WHILE (@@Fetch_Status <> -1)  
          BEGIN  

           SELECT @ID_ADHERENT = ID_ADHERENT  
           FROM ETABLISSEMENT   
           WHERE ID_ETABLISSEMENT = @ID_ETABLISSEMENT        

           IF (@ID_ENVELOPPE IS NULL)  
           BEGIN 

           SET @ID_PERIODE_FISC = @ID_PERIODE_MODULE_PEC  

           SET @ID_PERIODE_CPT = @ID_PERIODE_MODULE_PEC  

           EXEC @ID_COMPTE = LEC_COMPTE @ID_GROUPE_STAGIAIRE_PEC,   
             @ID_PERIODE_FISC, 
             @ID_TYPE_FINANCEMENT, @ID_ACTIVITE, null      

           SELECT @ID_ACTIVITE   = ID_ACTIVITE   
           FROM COMPTE  
           WHERE ID_COMPTE  = @ID_COMPTE   

          END    
          ELSE  
          BEGIN  

           SELECT @ID_PERIODE_FISC = ID_PERIODE   
           FROM ENVELOPPE  
           WHERE ID_ENVELOPPE = @ID_ENVELOPPE  

           SET @ID_PERIODE_CPT = @ID_PERIODE_FISC  

           SET @ID_COMPTE = NULL 
           SET @ID_TYPE_FINANCEMENT = NULL  
          END   

          IF @COD_TYPE_EVENEMENT = 'REGLEMT' 
          AND @ID_TYPE_MOUVEMENT = 21 
          BEGIN  
           SET @MNT_ENGAGEMENT_RESIDUEL= NULL  

           SELECT @MNT_ENGAGEMENT_RESIDUEL = SUM(ISNULL(CAST(MNT_MVT_BUDGETAIRE AS DECIMAL(15,2)), 0))  
           FROM MVT_BUDGETAIRE  
           INNER JOIN EVENEMENT on EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT  
           LEFT JOIN POSTE_COUT_REGLE  on EVENEMENT.ID_POSTE_COUT_REGLE=POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE  
           LEFT JOIN POSTE_COUT_ENGAGE on EVENEMENT.ID_POSTE_COUT_ENGAGE=POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE  
           WHERE  MVT_BUDGETAIRE.ID_MODULE_PEC              = @ID_MODULE_PEC   
           AND  ISNULL(POSTE_COUT_REGLE.ID_SOUS_TYPE_COUT , POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT) = @ID_SOUS_TYPE_COUT  
           AND  MVT_BUDGETAIRE.P_E_R       = @P_E_R  
           AND  MVT_BUDGETAIRE.ID_GROUPE      = @ID_GROUPE_STAGIAIRE_PEC  
           AND  MVT_BUDGETAIRE.ID_ETABLISSEMENT    = @ID_ETABLISSEMENT  
           AND  MVT_BUDGETAIRE.ID_ADHERENT      = @ID_ADHERENT  
           AND  MVT_BUDGETAIRE.ID_PERIODE_FISC     = @ID_PERIODE_FISC  
           AND  MVT_BUDGETAIRE.ID_PERIODE_CPT     = @ID_PERIODE_CPT  
           AND  MVT_BUDGETAIRE.ID_ACTIVITE      = @ID_ACTIVITE  
           AND  ISNULL(MVT_BUDGETAIRE.ID_COMPTE, 0)   = ISNULL(@ID_COMPTE, 0)  
           AND  ISNULL(MVT_BUDGETAIRE.ID_ENVELOPPE, 0)   = ISNULL(@ID_ENVELOPPE, 0)  
           AND  ISNULL(MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT, 0)  = ISNULL(@ID_TYPE_FINANCEMENT, 0)  

           SET @MNT_ENGAGEMENT_RESIDUEL= ISNULL(@MNT_ENGAGEMENT_RESIDUEL, 0)  

           IF @MNT_PFUS > 0  
            SELECT @MNT_PFUS = CASE   
                 WHEN @MNT_PFUS > @MNT_ENGAGEMENT_RESIDUEL THEN @MNT_ENGAGEMENT_RESIDUEL  
                 ELSE @MNT_PFUS   
                 END  
           ELSE  
            SET @MNT_PFUS = 0  
           END  

           IF @INVERSION_SIGNE_MVT = 1  
           BEGIN  
            SET @MNT_PFUS = - @MNT_PFUS  
            END  

            IF (@P_E_R = 'E' OR @MNT_PFUS <> 0 )   
            BEGIN  

             EXEC @ID_MVT_BUDGETAIRE = INS_MVT_BUDGETAIRE  
             @ID_TYPE_MOUVEMENT, @ID_EVENEMENT, @ID_COMPTE, @ID_ENVELOPPE, @MNT_PFUS,  
             @P_E_R, @DAT_EVENEMENT, @N_R, @ID_PERIODE_FISC, @ID_PERIODE_CPT,   
             @LIBL_MVT_BUDGETAIRE, @ID_ACTIVITE, @ID_ADHERENT, @ID_GROUPE_STAGIAIRE_PEC,  
             @ID_ETABLISSEMENT, @ID_TYPE_FINANCEMENT,   
             @ID_DISPOSITIF, @ID_MODULE_PEC  
            END  

            FETCH NEXT FROM CURSOR_PFUS INTO  
            @ID_ETABLISSEMENT,  
            @ID_TYPE_FINANCEMENT,  
            @ID_ENVELOPPE,  
            @MNT_PFUS,  
            @ID_GROUPE_STAGIAIRE_PEC,  
            @ID_ACTIVITE  
           END 
           CLOSE CURSOR_PFUS  
           DEALLOCATE CURSOR_PFUS  
          END  
         END

         CREATE PROCEDURE [dbo].[DECLOTURER_PEC]
         
         @cod_action_pec   integer,
         @annee_action_pec  integer,
         @cod_module_pec   varchar(14),
         @bln_ok     tinyint   out,
         @message    varchar(7000) out
         AS
         BEGIN

          declare @id_module_pec   int,
            @id_action_pec   int,
            @ID_EVENEMENT_NEW  int,
            @bln_decloture_action int,
            @dat_cloture   datetime

          DECLARE 
            @ID_TYPE_EVENEMENT  int,
            @LIBL_TYPE_EVENEMENT varchar(50),
            @ID_POSTE_COUT_REGLE int,
            @DAT_EVENEMENT   datetime,
            @ID_POSTE_COUT_ENGAGE int,
            @ID_UTILISATEUR   int,
            @COM_EVENEMENT   varchar(7600),
            @COD_TYPE_EVENEMENT  varchar(8)

          SELECT @ID_TYPE_EVENEMENT = ID_TYPE_EVENEMENT, @LIBL_TYPE_EVENEMENT = LIBL_TYPE_EVENEMENT
          FROM TYPE_EVENEMENT
          WHERE COD_TYPE_EVENEMENT = 'ANCLOPEC'

          SET @ID_UTILISATEUR   = 82
          SET @COM_EVENEMENT   = 'BATCH MANUEL Reg‚n‚ration Evenement ANCLOPEC (DECLOTURE MODULE)'

          SET @ID_POSTE_COUT_REGLE =NULL
          SET @DAT_EVENEMENT  = GETDATE()

          SET @bln_ok = 0
          SET @message = ''

          IF @cod_module_pec is null AND @cod_action_pec is null AND @annee_action_pec is null
          BEGIN 
           SELECT @message = '!! ALERTE !! Aucun parametre d''entree n''est valorise. Decl“ture impossible'
          END
          ELSE IF @cod_module_pec is not null 
          BEGIN
           IF @cod_action_pec is not null OR @annee_action_pec is not null
           BEGIN
            SELECT @message = '!! ALERTE !! Lorsque la variable @cod_module_pec est renseign‚e, les variables @cod_action_pec et @annee_action_pec doivent etre valorisees a NULL'
           END
           ELSE
           BEGIN
            SELECT @id_module_pec = ID_MODULE_PEC, 
              @id_action_pec = ACTION_PEC.ID_ACTION_PEC,
              @dat_cloture = MODULE_PEC.DAT_CLOTURE
            FROM MODULE_PEC
            INNER JOIN ACTION_PEC ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC 
            WHERE COD_MODULE_PEC = @cod_module_pec 
            AND ACTION_PEC.BLN_ACTIF = 1 AND MODULE_PEC.BLN_ACTIF = 1

            IF @id_module_pec IS NULL
            BEGIN
             SELECT @message = '!! ALERTE !! Module avec COD_MODULE_PEC = ' + CAST(@cod_module_pec AS VARCHAR)+ ' inexistant. Decloture module impossible.'
            END
            ELSE IF @dat_cloture IS NULL
            BEGIN
             SELECT @message = '!! ALERTE !! Module avec COD_MODULE_PEC = ' + CAST(@cod_module_pec AS VARCHAR)+ ' non clotur‚. D‚cloture module impossible.'

             SELECT @dat_cloture = ACTION_PEC.DAT_CLOTURE, @cod_action_pec  = COD_ACTION_PEC, @annee_action_pec = ANNEE_ACTION_PEC
             FROM ACTION_PEC 
             WHERE ID_ACTION_PEC = @id_action_pec  

             IF @dat_cloture IS NOT NULL
             BEGIN
              SELECT @message  = @message + CHAR(13) + CHAR(10) + '!! INFO !! L Action PEC avec COD_ACTION_PEC = ' + CAST(@cod_action_pec AS VARCHAR)+ ' et ANNEE_ACTION_PEC = ' + CAST(@annee_action_pec AS VARCHAR) + ' est cloturee et peut etre decloture. (Parametres de lancement a modifier)'
             END
            END
            ELSE
            BEGIN
             SET @bln_ok = 1
            END
           END
          END
          ELSE IF @cod_action_pec is not null AND @annee_action_pec is not null 
          BEGIN
            SET @id_module_pec = NULL

            SELECT  @id_action_pec = ID_ACTION_PEC, @dat_cloture = ACTION_PEC.DAT_CLOTURE
            FROM ACTION_PEC 
            WHERE COD_ACTION_PEC = @cod_action_pec  AND  ANNEE_ACTION_PEC = @annee_action_pec
            AND ACTION_PEC.BLN_ACTIF = 1

            IF @id_action_pec IS NULL
            BEGIN
             SELECT @message = '!! ALERTE !! Action non trouvee pour COD_ACTION_PEC = ' + CAST(@cod_action_pec AS VARCHAR)+ ' et ANNEE_ACTION_PEC = ' + CAST(@annee_action_pec AS VARCHAR) + ' . Decloture action impossible.'
            END
            ELSE IF @dat_cloture IS NULL
            BEGIN
             SELECT @message = '!! ALERTE !! Action PEC pour COD_ACTION_PEC = ' + CAST(@cod_action_pec AS VARCHAR)+ ' et ANNEE_ACTION_PEC = ' + CAST(@annee_action_pec AS VARCHAR) + ' non cloturee . Decloture impossible.'
            END
            ELSE
            BEGIN
             SET @bln_ok = 1
            END 
          END
          ELSE
          BEGIN
           SELECT @message = '!! ALERTE !! Lorsque la variable @cod_module_pec est renseign‚e, les variables @cod_action_pec et @annee_action_pec doivent etre valorisees a NULL'
                +  CHAR(13) + CHAR(10) +
                '!! ALERTE !! Lorsque la variable @cod_module_pec n''est pas renseign‚e, les variables @cod_action_pec et @annee_action_pec doivent etre les deux valorisees et non NULL'
          END

          IF @bln_ok = 1
          BEGIN
           DECLARE cu_pce_module CURSOR FOR
           SELECT MODULE_PEC.ID_MODULE_PEC, DER_PCE.ID_POSTE_COUT_ENGAGE
           FROM MODULE_PEC
           INNER JOIN (SELECT ID_MODULE_PEC, ID_SOUS_TYPE_COUT, ID_POSTE_COUT_ENGAGE = MAX(ID_POSTE_COUT_ENGAGE)
              FROM POSTE_COUT_ENGAGE
              GROUP BY ID_MODULE_PEC, ID_SOUS_TYPE_COUT) 
              DER_PCE ON DER_PCE .ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC   
           INNER JOIN POSTE_COUT_ENGAGE ON POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE = DER_PCE.ID_POSTE_COUT_ENGAGE  
           WHERE MODULE_PEC.ID_MODULE_PEC = ISNULL(@id_module_pec , MODULE_PEC.ID_MODULE_PEC)
           AND  MODULE_PEC.ID_ACTION_PEC = @id_action_pec
           AND  MODULE_PEC.BLN_ACTIF = 1

           OPEN cu_pce_module 

           FETCH cu_pce_module INTO @id_module_pec, @ID_POSTE_COUT_ENGAGE

           SET @bln_decloture_action = 0

           WHILE @@FETCH_STATUS <> -1
           BEGIN
            SET @ID_EVENEMENT_NEW  = NULL
            EXEC MVT_BUDGETAIRE_PEC_INS_EVENEMENT_PEC
            @LIBL_TYPE_EVENEMENT, @DAT_EVENEMENT, @ID_TYPE_EVENEMENT, @ID_POSTE_COUT_ENGAGE, @ID_POSTE_COUT_REGLE, @ID_UTILISATEUR, @COM_EVENEMENT, @ID_EVENEMENT_NEW out

            FETCH cu_pce_module INTO @id_module_pec, @ID_POSTE_COUT_ENGAGE
           END

           CLOSE  cu_pce_module 
           DEALLOCATE cu_pce_module 

           SELECT TOP 1 @COD_TYPE_EVENEMENT = COD_TYPE_EVENEMENT 
           from EVENEMENT
           INNER JOIN TYPE_EVENEMENT ON EVENEMENT.ID_TYPE_EVENEMENT = TYPE_EVENEMENT .ID_TYPE_EVENEMENT
           LEFT JOIN POSTE_COUT_ENGAGE ON EVENEMENT.ID_POSTE_COUT_ENGAGE =POSTE_COUT_ENGAGE .ID_POSTE_COUT_ENGAGE 
           LEFT JOIN POSTE_COUT_REGLE ON EVENEMENT.ID_POSTE_COUT_REGLE =POSTE_COUT_REGLE .ID_POSTE_COUT_REGLE 
           WHERE ISNULL(POSTE_COUT_ENGAGE .ID_MODULE_PEC , POSTE_COUT_REGLE .ID_MODULE_PEC ) = @id_module_pec
           ORDER BY EVENEMENT.ID_EVENEMENT DESC

           IF @COD_TYPE_EVENEMENT = 'ANCLOPEC'
           BEGIN
            SET @bln_decloture_action = 1

            UPDATE MODULE_PEC
            SET DAT_CLOTURE = NULL, DAT_MODIF = GETDATE(), ID_UTILISATEUR = @ID_UTILISATEUR
            WHERE ID_MODULE_PEC = @id_module_pec

            IF @cod_module_pec is NOT NULL  
            BEGIN
             SELECT @message = 'Le Module PEC avec COD_MODULE_PEC = ' + CAST(@cod_module_pec AS VARCHAR)+ ' a ete DEcloture.'  
            END
           END

           SELECT @dat_cloture = ACTION_PEC.DAT_CLOTURE,  @cod_action_pec  = COD_ACTION_PEC, @annee_action_pec = ANNEE_ACTION_PEC
           FROM ACTION_PEC 
           WHERE ID_ACTION_PEC = @id_action_pec

           IF @bln_decloture_action = 1 OR (@dat_cloture IS NOT NULL)
           BEGIN
            UPDATE ACTION_PEC
            SET DAT_CLOTURE = NULL, DAT_MODIF = GETDATE(), ID_UTILISATEUR = @ID_UTILISATEUR, ID_MOTIF_CLOTURE_PEC = NULL
            WHERE ID_ACTION_PEC  = @id_action_pec

            IF @dat_cloture IS NOT NULL
            BEGIN
             SELECT @message = @message 
                +  CHAR(13) + CHAR(10) + 
                'L Action PEC avec COD_ACTION_PEC = ' + CAST(@cod_action_pec AS VARCHAR)+ ' et ANNEE_ACTION_PEC = ' + CAST(@annee_action_pec AS VARCHAR) + ' a ete DEcloturee.'  
            END
           END  
          END

         END

GO

 CREATE PROCEDURE [dbo].[EDT_LETTRE_RELANCE_COMP_ETAB_CPRO]
          @ID_ETABLISSEMENT int,
          @ID_BENEFICIAIRE int,
          @TYPE_BENEFICIAIRE int,
          @ID_ADRESSE   int,
          @ID_CONTACT   int,
          @COD_CONTRAT_PRO varchar(10)
         AS

         BEGIN
          DECLARE
           @ID_CONTRAT_PRO INT,
           @ID_DOCUMENT INT,
           @RELANCE INT,
           @TAUX_TVA decimal(10,5),
           @ID_TYPE_TVA INT,
           @ID_REF_UTIL INT,
           @SEUIL_RELANCE_ADH decimal(18,2),
           @DELAI_RELANCE_1 INT,
           @DELAI_RELANCE_2 INT,
           @DELAI_RELANCE_3 INT

          SET @RELANCE = 1

          IF (@ID_CONTACT IS NULL)
          BEGIN
           SELECT
            @ID_CONTACT = NR31.ID_CONTACT
           FROM
            NR31
            INNER JOIN CONTACT
             ON CONTACT.ID_CONTACT = NR31.ID_CONTACT
           WHERE
            BLN_PRINCIPAL = 1
            AND BLN_ACTIF = 1
            AND ID_ETABLISSEMENT = @ID_BENEFICIAIRE
            AND CONTACT.LIB_NOM_CONTACT <> '.'
          END

          SELECT
           @ID_DOCUMENT = ID_DOCUMENT
          FROM
           DOCUMENT
          WHERE
           COD_DOCUMENT = 'LET_REL_CETAB'

          SELECT
           @DELAI_RELANCE_1 = DELAI_RELANCE_1,
           @DELAI_RELANCE_2 = DELAI_RELANCE_2,
           @DELAI_RELANCE_3 = DELAI_RELANCE_3,
           @SEUIL_RELANCE_ADH = SEUIL_RELANCE_ADH
          FROM
           PARAM_EDITION_CPRO
          WHERE
           ID_DOCUMENT   = @ID_DOCUMENT

          DECLARE
           @MAX_DAT_EVENEMENT_ADMIN DATETIME,
           @MAX_DAT_BAP_OF DATETIME

          SELECT
           @MAX_DAT_BAP_OF = CONVERT(DATETIME,MAX(SP.DAT_BAP_OF),10)   
          FROM
           CONTRAT_PRO CP
           INNER JOIN MODULE_PRO MP
            ON MP.ID_CONTRAT_PRO = CP.ID_CONTRAT_PRO
           INNER JOIN SESSION_PRO SP
            ON SP.ID_MODULE_PRO = MP.ID_MODULE_PRO
          WHERE
           CP.COD_CONTRAT_PRO = @COD_CONTRAT_PRO

          SELECT
           @MAX_DAT_EVENEMENT_ADMIN  = CONVERT(DATETIME,MAX(EA.DAT_EVENEMENT_ADMIN),10) 
          FROM
           EVENEMENT_ADMIN EA
           INNER JOIN CONTRAT_PRO CP
            ON EA.ID_CONTRAT_PRO = CP.ID_CONTRAT_PRO
             AND CP.COD_CONTRAT_PRO = @COD_CONTRAT_PRO
          WHERE
           EA.ID_TYPE_EVENEMENT_ADMIN IN (15,16,17)

          SELECT
           ADHERENT.COD_ADHERENT,
           ETABLISSEMENT.NUM_SIRET,   
           CM.LIB_NOM   AS LIB_NOM_CONSEILLER,
           CM.LIB_PNM   AS LIB_PNM_CONSEILLER,
           CR.ID_UTILISATEUR AS ID_UTIL,
           CR.LIB_PNM   AS LIB_PRENOM_CHARGE_RELATION,
           CR.LIB_NOM   AS LIB_NOM_CHARGE_RELATION,
           CR.LIB_VILLE,
           CR.EMAIL   AS EMAIL_CHARGE_RELATION
          INTO
           #TEMP_REFERENCE
          FROM
           ADHERENT
           INNER JOIN ETABLISSEMENT
            ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
           INNER JOIN ADRESSE
            ON ETABLISSEMENT.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE
           LEFT JOIN UTILISATEUR CR 
            ON ETABLISSEMENT.ID_CHARGEE_RELATION = CR.ID_UTILISATEUR
           LEFT JOIN UTILISATEUR CM 
            ON ETABLISSEMENT.ID_CHARGEE_MISSION = CM.ID_UTILISATEUR
          WHERE
           ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT

          SELECT
           @ID_REF_UTIL = ID_UTIL
          FROM
           #TEMP_REFERENCE

          SELECT
           @ID_TYPE_TVA = ETABLISSEMENT.ID_TYPE_TVA
          FROM
           ETABLISSEMENT
          WHERE
           ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT

          SELECT DISTINCT
           ADHERENT.COD_ADHERENT,
           AGENCE.ID_AGENCE,
           AGENCE.COD_POSTAL,
           AGENCE.LIB_VILLE,
           AGENCE.LIB_NOM_CONSEILLER,
           AGENCE.LIB_PNM_CONSEILLER
          INTO
           #TEMP_REGLEMENT_REFERENCE
          FROM
           ADHERENT
           INNER JOIN AGENCE
            ON AGENCE.ID_AGENCE = ADHERENT.ID_AGENCE
           INNER JOIN ETABLISSEMENT
            ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
          WHERE
           ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT

          SELECT
           CONTRAT_PRO.ID_CONTRAT_PRO,
           CONTRAT_PRO.COD_CONTRAT_PRO,
           CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_DEB_CONTRAT, 3) AS DAT_DEBUT,
           CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_FIN_CONTRAT, 3) AS DAT_FIN,
           INDIVIDU.NOM_INDIVIDU NOM,
           INDIVIDU.PRENOM_INDIVIDU PRENOM,
           INDIVIDU.ID_INDIVIDU,
           INDIVIDU.ID_NATIONALITE,
           CONTRAT_PRO.LIB_LIEU_FORMATION,
           CONTRAT_PRO.ID_SALARIE_PRO
          INTO
           #TMP_CONTRAT_PRO
          FROM
           CONTRAT_PRO
           INNER JOIN SALARIE_PRO
            ON CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO
           INNER JOIN INDIVIDU
            ON SALARIE_PRO.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU
          WHERE
           CONTRAT_PRO.COD_CONTRAT_PRO = @COD_CONTRAT_PRO

          SELECT
           @ID_CONTRAT_PRO = ID_CONTRAT_PRO
          FROM
           #TMP_CONTRAT_PRO

          DECLARE
           @LIB_PRENOM_CHARGE_RELATION_SIGNE VARCHAR(100),
           @LIB_NOM_CHARGE_RELATION_SIGNE VARCHAR(100)

          SELECT
           @LIB_NOM_CHARGE_RELATION_SIGNE = isnull(LIB_NOM_CHARGE_RELATION, ''),
           @LIB_PRENOM_CHARGE_RELATION_SIGNE = isnull(LIB_PRENOM_CHARGE_RELATION, '')
          FROM
           #TEMP_REFERENCE as SIGNATURE

          SELECT
           MP1.ID_MODULE_PRO,
           MP1.LIBL_MODULE_PRO,
           ISNULL(ISNULL(SP1.MNT_VERSE_HT_ADH,0)/ISNULL(SP1.NB_UNITE_TOTALE,1),0) AS TAUX_HORAIRE_MODULE, 
           SP1.DAT_DEBUT,
           SP1.DAT_FIN,
           ISNULL(SP1.NB_UNITE_TOTALE,0)   AS NB_UNITE_TOTALE,         
           ISNULL(SP1.MNT_VERSE_HT_ADH,0)  AS MNT_VERSE_HT_ADH,     
           ISNULL(SP1.MNT_VERSE_TVA_ADH,0) AS MNT_VERSE_TVA_ADH,    
           SP1.DAT_BAP_OF, 
           ISNULL(R26.TAU_TVA,0) AS TAU_TVA,   
           dbo.GetNumRelanceCompEtabPRO(@ID_DOCUMENT, @ID_CONTRAT_PRO, @MAX_DAT_BAP_OF, @MAX_DAT_EVENEMENT_ADMIN) AS NUM_RELANCE
          INTO
           #LST_MODULES_TOUTE
          FROM
           MODULE_PRO MP1
           LEFT JOIN SESSION_PRO  SP1
            ON SP1.ID_MODULE_PRO = MP1.ID_MODULE_PRO
           LEFT JOIN ETABLISSEMENT E
            ON E.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
           LEFT JOIN R26
            ON R26.ID_TYPE_TVA = E.ID_TYPE_TVA
           INNER JOIN PERIODE
            ON PERIODE.ID_TYPE_PERIODE = 2
             AND GETDATE()>=PERIODE.DAT_DEB_PERIODE
             AND GETDATE()<=PERIODE.DAT_FIN_PERIODE
             AND R26.ID_PERIODE = PERIODE.ID_PERIODE
          WHERE
           MP1.ID_CONTRAT_PRO = @ID_CONTRAT_PRO
           AND SP1.ID_FACTURE_OF IS NOT NULL
           AND SP1.ID_REGLEMENT_PRO_OF IS NOT NULL
           AND SP1.ID_FACTURE_ADHERENT IS NULL

          SELECT TOP 1
           @TAUX_TVA = TAU_TVA
          FROM
           #LST_MODULES_TOUTE

          PRINT @TAUX_TVA PRINT 'OK'

          SELECT TOP 1
           @RELANCE = NUM_RELANCE
          FROM
           #LST_MODULES_TOUTE

          SELECT DISTINCT
           ID_MODULE_PRO,
           LIBL_MODULE_PRO
          INTO
           #LST_MODULES
          FROM
           #LST_MODULES_TOUTE

          DECLARE
           @PRENOM_PARA1 VARCHAR(35),
           @NOM_PARA1 VARCHAR(35)

          SELECT TOP 1
           @PRENOM_PARA1 = PRENOM,
           @NOM_PARA1 = NOM
          FROM
           #TMP_CONTRAT_PRO

          IF (EXISTS(SELECT 1 FROM SYS.TABLES WHERE NAME = '#TABLE_PARAGRAPHE'))
          BEGIN
           DROP TABLE #TABLE_PARAGRAPHE
          END

          CREATE TABLE #TABLE_PARAGRAPHE
          (        
           PARAGRAPHE VARCHAR(MAX),
           NUM_ORDRE INTEGER,             
          )     

          INSERT INTO  #TABLE_PARAGRAPHE 
          VALUES
          (
           'Afin de pouvoir proc‚der au rŠglement du compl‚ment de forfait* relatif aux heures de formation r‚alis‚es et justifi‚es par les attestations de pr‚sence du contrat de professionnalisation de ' + @PRENOM_PARA1 +' '+ @NOM_PARA1 + ', nous vous remercions de bien vouloir nous retourner une facture selon le modŠle ci-joint d–ment compl‚t‚e et sign‚e.',
           1
          )

          INSERT INTO  #TABLE_PARAGRAPHE 
          VALUES
          (
           'Nous vous remercions de nous l''envoyer dans le  mois en cours, pour la prendre en compte dans la clture de nos comptes mensuels.',
           2
          )

          IF (@RELANCE >= 3)
          BEGIN
           INSERT INTO  #TABLE_PARAGRAPHE 
           VALUES
           (
            'Sans r‚ponse de votre part sous 1 mois, le dossier sera cl“tur‚ et la cr‚ance annul‚e.',
            3
           )
          END

          INSERT INTO  #TABLE_PARAGRAPHE 
          VALUES
          (
           'Nous restons … votre disposition pour tout renseignement compl‚mentaire et, vous prions d''agr‚er, Madame, Monsieur, l''expression de nos sincŠres salutations.',
           4
          )

          SELECT 
           (
            SELECT 
             ( 
              SELECT dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) 
              FOR XML RAW('BENEFICIAIRE'), ELEMENTS, TYPE
             ),  
             isnull(COD_ADHERENT, '')    as COD_ADH,     
             isnull(LIB_PRENOM_CHARGE_RELATION, '') as LIB_PRENOM_CHARGE_RELATION,     
             isnull(LIB_NOM_CHARGE_RELATION, '')  as LIB_NOM_CHARGE_RELATION,
             isnull(EMAIL_CHARGE_RELATION, '')  as EMAIL,

             (
              SELECT dbo.GetXmlAdrUtilAvecTel(ENTETE.ID_UTIL, dbo.DossierEstTSA(@COD_CONTRAT_PRO, 0), 1) FOR XML RAW('EMETTEUR_LETTRE'), ELEMENTS, TYPE
             ),
             (
              SELECT 
                (CASE WHEN @RELANCE = 1 THEN ' PremiŠre relance'
                  WHEN @RELANCE = 2 THEN ' DeuxiŠme relance'
                  WHEN @RELANCE = 3 THEN ' DerniŠre relance' END) 
              FOR XML RAW('NUM_RELANCE'), ELEMENTS, TYPE 
             ),

             (
              SELECT #TMP_CONTRAT_PRO.COD_CONTRAT_PRO
              FROM #TMP_CONTRAT_PRO
              FOR XML RAW('DOSSIER'), ELEMENTS, TYPE 
             ),
             ENTETE.NUM_SIRET AS SIRET,
             rtrim(case when patindex('%CEDEX%', LIB_VILLE) <> 0 then left(LIB_VILLE, patindex('%CEDEX%', LIB_VILLE)-1) else LIB_VILLE end) as LIB_VILLE, 
             dbo.GetFullDate(getDate()) as DATE,    
             (
              SELECT top 1
              dbo.GetContactSalutation(@ID_CONTACT, 1)
              FROM CIVILITE
              FOR XML PATH('POLITESSE_HAUT'), TYPE
             )
            FROM #TEMP_REFERENCE AS ENTETE     
            FOR XML AUTO, ELEMENTS, TYPE    
           ),

           (
            SELECT PARAGRAPHE
            FROM #TABLE_PARAGRAPHE AS TABLE_PARAGRAPHE ORDER BY NUM_ORDRE     
            FOR XML RAW('CORPS_DE_LETTRE'),ELEMENTS, TYPE
           ),

           (
            SELECT          
            @LIB_PRENOM_CHARGE_RELATION_SIGNE AS LIB_PRENOM_CHARGE_RELATION_SIGNE,          
            @LIB_NOM_CHARGE_RELATION_SIGNE  AS LIB_NOM_CHARGE_RELATION_SIGNE
            FOR XML RAW('SIGNATURE_LETTRE'),ELEMENTS, TYPE
           ),

           (
           SELECT 
            (
             SELECT         

              ( SELECT dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) 
               FOR XML RAW('BENEFICIAIRE'), ELEMENTS, TYPE       
              ),

              (SELECT dbo.GetXmlAdrUtilAvecTel(@ID_REF_UTIL,0, 1) 
               FOR XML RAW('EMETTEUR'), ELEMENTS, TYPE),

              ISNULL(COD_ADHERENT, '') AS COD_ADH,

              ISNULL(@COD_CONTRAT_PRO, '') AS COD_CONTRAT_PRO,   

              (SELECT CORPS2.PRENOM, CORPS2.NOM
               FROM #TMP_CONTRAT_PRO AS CORPS2 FOR XML RAW('SALARIE_FORME'),ELEMENTS, TYPE)
             FOR XML RAW('ENTETE'),ELEMENTS, TYPE      
            ),
            (
             SELECT
              (
               SELECT 
               ISNULL(LMPT.LIBL_MODULE_PRO,'') AS LIBL_MODULE_PRO,
                (SELECT          
                dbo.GetShortDate(ses.DAT_DEBUT)             AS DAT_DEBUT,
                dbo.GetShortDate(ses.DAT_FIN)              AS DAT_FIN,
                dbo.GetFrenchCurrencyFormat(CAST(COALESCE(ses.NB_UNITE_TOTALE,0.00)AS MONEY))  AS NB_UNITE_TOTALE,
                dbo.GetFrenchCurrencyFormat(CAST(COALESCE(ses.TAUX_HORAIRE_MODULE,0.00)AS MONEY)) AS TAUX_HORAIRE_MODULE,             
                dbo.GetFrenchCurrencyFormat(CAST(COALESCE(ses.MNT_VERSE_HT_ADH ,0.00)AS MONEY)) AS MNT_VERSE_HT_ADH            
                FROM #LST_MODULES_TOUTE AS ses
                WHERE ses.ID_MODULE_PRO = LMPT.ID_MODULE_PRO
                for xml raw('SESSION_TABLE'),ELEMENTS, TYPE
                )
               FROM #LST_MODULES_TOUTE AS LMPT 
               group by ID_MODULE_PRO, LMPT.LIBL_MODULE_PRO                           
               FOR XML RAW('MODULE_TABLE'),ELEMENTS, TYPE
              )      
            ),
            (
             SELECT dbo.GetFrenchCurrencyFormat(CAST(COALESCE(SUM(LMPT2.MNT_VERSE_HT_ADH),0.00)AS MONEY))       AS TOTAL_HT,
             dbo.GetFrenchCurrencyFormat(CAST(COALESCE(@TAUX_TVA*100,0.00) AS MONEY))          AS TAUX_TVA,
             dbo.GetFrenchCurrencyFormat(CAST(COALESCE(SUM(LMPT2.MNT_VERSE_HT_ADH * @TAUX_TVA )+SUM(LMPT2.MNT_VERSE_TVA_ADH),0.00) AS MONEY)) AS TOTAL_TVA,
             dbo.GetFrenchCurrencyFormat(CAST(COALESCE(SUM(LMPT2.MNT_VERSE_HT_ADH * @TAUX_TVA )+SUM(LMPT2.MNT_VERSE_HT_ADH),0.00) AS MONEY)) AS TOTAL_TTC
             FROM #LST_MODULES_TOUTE AS LMPT2        
             FOR XML RAW('TOTAL_ANNEX'),ELEMENTS, TYPE
            )
           FROM #TEMP_REFERENCE AS LETTRE_ANNEX 

           FOR XML RAW('LETTRE_ANNEX'), ELEMENTS, TYPE
          )   

          FROM #TEMP_REFERENCE as LETTRE
          FOR XML AUTO, ELEMENTS     
         END

         CREATE PROCEDURE [dbo].[CHEMIN_PIECE]
         	@TYPE_PIECE VARCHAR(50),
         	@ID_FACTURE INT,
         	@ID_DOSSIER INT,
          	@COD_DOSSIER varchar (12),
         	@TYPE_DOSSIER VARCHAR(3),
         	@RESULT_CHEMIN_PIECE varchar(255) OUTPUT

         AS
         BEGIN

         	DECLARE @ID_ACTION INT
         	DECLARE @ID_CONTRAT_PRO INT
         	DECLARE @ID_MODULE INT
         	DECLARE @ID_SESSION INT
         	DECLARE @COD_MODULE VARCHAR(14)
         	DECLARE @COD_SESSION_PEC VARCHAR(20)
         	DECLARE @IS_CREATION BIT
         	DECLARE @ID_POSTE_COUT INT
         	DECLARE @REPERTOIRE VARCHAR(300)
         	DECLARE @COD_PIECE varchar(10)

         	SELECT @COD_PIECE = DBO.GET_TYPE_PIECE_PEC(@TYPE_PIECE)
         	IF @ID_FACTURE IS  NULL AND @COD_PIECE = 'FACT'	
         	BEGIN
         		SET @RESULT_CHEMIN_PIECE = NULL
         		RETURN
         	END	

         	IF @TYPE_DOSSIER = 'PEC'
         	BEGIN

         		IF @ID_FACTURE IS NOT NULL AND @COD_PIECE = 'FACT'		 		 
         		 BEGIN
         			SELECT @REPERTOIRE = REPERTOIRE + CASE WHEN PATINDEX('%\', REPERTOIRE) = LEN(REPERTOIRE) THEN '' ELSE '\' END
         			FROM PIECE_PEC 
         			WHERE COD_PIECE_PEC = @COD_PIECE
         			SELECT @RESULT_CHEMIN_PIECE = 
         				@REPERTOIRE + CAST(YEAR(DAT_CREATION) AS VARCHAR(4)) + '\'+
         				REPLACE(COD_FACTURE, '/', '')+'_'+@COD_PIECE
         			FROM FACTURE
         			WHERE ID_FACTURE = @ID_FACTURE
         		 END
         		ELSE
         			BEGIN

         			CREATE TABLE #TMP_MODULE_PEC 
         			(
         				ID_MODULE INT,
         				ID_SESSION_PEC INT,
         				ID_PIECE_PEC INT,
         				LIBL_PIECE_PEC VARCHAR(250),
         				COD_PIECE_PEC VARCHAR(250),
         				ID_ADHERENT INT,
         				ID_ARRIVEE_PIECE_PEC INT,
         				PRESENT BIT,
         				CONFORME BIT,
         				DAT_RELANCE_ENGAGE_1 DATE,
         				DAT_RELANCE_ENGAGE_2 DATE,
         				DAT_RELANCE_ENGAGE_3 DATE,
         				DAT_RELANCE_REGLE_1 DATE,
         				DAT_RELANCE_REGLE_2 DATE,
         				DAT_RELANCE_REGLE_3 DATE,
         				LIB_RELANCE_ENGAGE VARCHAR(250),
         				LIB_RELANCE_REGLE VARCHAR(250),
         				TYPE_PIECE VARCHAR(250),
         				BLN_ADHERENT BIT,
         				BLN_SESSION BIT,
         				ID_STAGIAIRE INT,
         				INFORMATION VARCHAR(250),
         				REPERTOIRE VARCHAR(250),
         				ANNEE_ACTION_PEC INT,
         				NOM_FICHIER VARCHAR(250),
         				ID_DISPOSIIF INT,
         				ID_INDIVIDU INT,
         				ID_POSTE_COUT_REGLE INT,
         				BLN_POSTE_COUT_REGLE BIT,
         				BLN_AUTRES BIT,
         				NUM_FILE INT,
         				ID_UTILISATEUR INT
         			)

         			CREATE TABLE #TMP_SESSION_PEC 
         			(
         				ID_MODULE INT,
         				ID_SESSION_PEC INT,
         				ID_PIECE_PEC INT,
         				LIBL_PIECE_PEC VARCHAR(250),
         				COD_PIECE_PEC VARCHAR(250),
         				ID_ADHERNT INT,
         				ID_ARRIVEE_PIECE_PEC INT,
         				PRESENT BIT,
         				CONFORME BIT,
         				DAT_RELANCE_ENGAGE_1 DATE,
         				DAT_RELANCE_ENGAGE_2 DATE,
         				DAT_RELANCE_ENGAGE_3 DATE,
         				DAT_RELANCE_REGLE_1 DATE,
         				DAT_RELANCE_REGLE_2 DATE,
         				DAT_RELANCE_REGLE_3 DATE,
         				LIB_RELANCE_ENGAGE VARCHAR(250),
         				LIB_RELANCE_REGLE VARCHAR(250),
         				TYPE_PIECE VARCHAR(250),
         				BLN_ADHERENT BIT,
         				BLN_SESSION BIT,
         				ID_STAGIAIRE INT,
         				INFORMATION VARCHAR(250),
         				LIBL_PIECE_PEC1 VARCHAR(250),
         				REPERTOIRE VARCHAR(250),
         				NOM_FICHIER VARCHAR(250),
         				ID_DISPOSIIF INT,
         				ID_INDIVIDU INT,
         				BLN_AUTRES BIT
         			)

         			CREATE TABLE #TMP_REGLEMENT 
         			(
         				REPERTOIRE VARCHAR(250),
         				NOM_FICHIER VARCHAR(250),
         				ID_ARRIVEE_PIECE_PEC INT,
         				ID_POSTE_COUT_REGLE INT,
         				ID INT,
         				ID_PIECE_PEC INT,
         				BLN_POSTE_COUT_REGLE BIT,
         				TYPE_PIECE VARCHAR(250),
         				PRESENT BIT,
         				CONFORME BIT,
         				LIB_RELANCE_ENGAGE VARCHAR(250),
         				LIB_RELANCE_REGLE VARCHAR(250),
         				ID_MODULE_PEC INT,
         				COD_PIECE_PEC VARCHAR(250),
         				BLN_AUTRES BIT,
         				NUM_FILE int
         			)

         			 SET @ID_ACTION = @ID_DOSSIER

         			 DECLARE @ANNEE_ACTION_PEC VARCHAR(4)
         			 SELECT @ANNEE_ACTION_PEC = CAST(ANNEE_ACTION_PEC AS VARCHAR(4)) FROM ACTION_PEC WHERE ID_ACTION_PEC = @ID_ACTION 

         			 DECLARE CURSOR_SESSION CURSOR FOR

         				 SELECT SESSION_PEC.ID_SESSION_PEC, 
         						MODULE_PEC.ID_ACTION_PEC,  
         						MODULE_PEC.COD_MODULE_PEC,
         						SESSION_PEC.COD_SESSION_PEC
         				 FROM	SESSION_PEC INNER JOIN 
         						MODULE_PEC ON SESSION_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC 
         						INNER JOIN ACTION_PEC ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC 
         						INNER JOIN VUE_MODULES_VISIBLES_PANIERS MODULES_VISIBLES ON MODULES_VISIBLES.id_module_pec = MODULE_PEC.ID_MODULE_PEC
         						AND ACTION_PEC.ID_ACTION_PEC = @ID_DOSSIER

         				OPEN CURSOR_SESSION
         				FETCH NEXT FROM CURSOR_SESSION INTO @ID_SESSION, @ID_MODULE, @COD_MODULE, @COD_SESSION_PEC
         				WHILE @@FETCH_STATUS = 0   
         				BEGIN
         					INSERT INTO #TMP_SESSION_PEC EXECUTE [LEC_GRP_PIECES_SESSION_NOUVEAU]  
         					   @ID_ACTION
         					  ,@ID_MODULE
         					  ,@ID_SESSION
         					  ,@COD_SESSION_PEC

         					INSERT INTO #TMP_REGLEMENT EXECUTE [LEC_GRP_PIECES_REGLEMENT_NOUVEAU] 
         					   @IS_CREATION
         					  ,@ID_POSTE_COUT
         					  ,@ID_MODULE
         					  ,@COD_SESSION_PEC
         					FETCH NEXT FROM CURSOR_SESSION INTO @ID_SESSION, @ID_MODULE, @COD_MODULE, @COD_SESSION_PEC
         				END
         				CLOSE CURSOR_SESSION
         			DEALLOCATE CURSOR_SESSION
         			DECLARE @BLN_CREER_PIECES_SESSION BIT = 0
         			IF NOT EXISTS (SELECT 1 FROM #TMP_REGLEMENT )
         				SET @BLN_CREER_PIECES_SESSION = 1

         			DECLARE CURSOR_MODULE CURSOR FOR
         				SELECT	MODULE_PEC.ID_MODULE_PEC, COD_MODULE_PEC

         				FROM	MODULE_PEC INNER JOIN
         						ACTION_PEC ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         						INNER JOIN VUE_MODULES_VISIBLES_PANIERS MODULES_VISIBLES ON MODULES_VISIBLES.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         						AND ACTION_PEC.ID_ACTION_PEC = @ID_DOSSIER

         				OPEN CURSOR_MODULE
         				FETCH NEXT FROM CURSOR_MODULE INTO @ID_MODULE, @COD_MODULE
         				WHILE @@FETCH_STATUS = 0   
         				BEGIN

         				INSERT INTO #TMP_MODULE_PEC EXECUTE [LEC_GRP_PIECES_MODULE_NOUVEAU] 
         					   @ID_DOSSIER
         					  ,@ID_MODULE
         					  ,NULL
         					  ,@COD_MODULE
         				IF @BLN_CREER_PIECES_SESSION =1
         					INSERT INTO #TMP_REGLEMENT EXECUTE [LEC_GRP_PIECES_REGLEMENT_NOUVEAU] 
         						   1
         						  ,NULL
         						  ,@ID_MODULE
         						  ,@COD_MODULE

         					FETCH NEXT FROM CURSOR_MODULE INTO @ID_MODULE, @COD_MODULE
         				END
         			CLOSE CURSOR_MODULE
         			DEALLOCATE CURSOR_MODULE

         			SELECT @RESULT_CHEMIN_PIECE = CHEMIN_PIECE FROM
         			(
         			SELECT REPERTOIRE + @ANNEE_ACTION_PEC + '\'+ dbo.REMOVE_EXTENSION(NOM_FICHIER) AS CHEMIN_PIECE 
         			FROM #TMP_SESSION_PEC 
         			WHERE #TMP_SESSION_PEC.COD_PIECE_PEC LIKE @COD_PIECE+'%'
         			UNION
         			SELECT REPERTOIRE + @ANNEE_ACTION_PEC + '\'+ dbo.REMOVE_EXTENSION(NOM_FICHIER) AS CHEMIN_PIECE 
         			FROM #TMP_MODULE_PEC 
         			WHERE #TMP_MODULE_PEC.COD_PIECE_PEC LIKE @COD_PIECE+'%'
         			UNION
         			SELECT REPERTOIRE + @ANNEE_ACTION_PEC + '\'+ dbo.REMOVE_EXTENSION(NOM_FICHIER) AS CHEMIN_PIECE 
         			FROM #TMP_REGLEMENT 
         			WHERE #TMP_REGLEMENT.COD_PIECE_PEC LIKE @COD_PIECE+'%'
         			) AS A
         		END
         	END

         	IF @TYPE_DOSSIER = 'PRO'
         	BEGIN

         		CREATE TABLE #TMP_PRO 
         		(
         			ID_PIECE_PRO INT,
         			LIBL_PIECE_PRO VARCHAR(250),
         			ID_ARRIVEE_PIECE_PRO INT,
         			COD_ARRIVEE_PIECE_PRO VARCHAR(250),
         			DAT_ARRIVEE_PIECE_PRO DATE,
         			BLN_ACTIF BIT,
         			BLN_CONFORME BIT,
         			COM_ARRIVEE_PIECE_PRO VARCHAR(250),
         			ID_CONTRAT_PRO INT,
         			ID_MODULE_PRO INT,
         			ID_SESSION_PRO INT,
         			DAT_RELANCE_ENGAGE_1 DATE,
         			DAT_RELANCE_ENGAGE_2 DATE,
         			DAT_RELANCE_ENGAGE_3 DATE,
         			DAT_RELANCE_REGLE_1 DATE,
         			DAT_RELANCE_REGLE_2 DATE,
         			DAT_RELANCE_REGLE_3 DATE,
         			TIME_STAMP VARBINARY(8),
         			LIB_RELANCE_ENGAGE VARCHAR(250),
         			LIB_RELANCE_REGLE VARCHAR(250),
         			COD_PIECE_PRO VARCHAR(250),
         			BLN_BLOQUANT_ENGAGEMENT BIT,
         			BLN_BLOQUANT_REGLEMENT BIT,
         			REPERTOIRE VARCHAR(250),
         			INFORMATION VARCHAR(250),
         			TYPE_PIECE VARCHAR(250),
         			COD_CONTRAT_PRO VARCHAR(250),
         			COD_MODULE_PRO VARCHAR(250),
         			COD_SESSION_PRO VARCHAR(250),
         			DAT_CREATION DATE,
         			BLN_AUTRES BIT,
         			NUM_FILE INT,
         			ID_UTILISATEUR INT,
         			NOM_FICHIER VARCHAR(250)
         		)
         		SET @ID_CONTRAT_PRO = @ID_DOSSIER

         		INSERT INTO #TMP_PRO EXECUTE LEC_GRP_ARRIVEE_PIECES_PRO 
         			   @ID_CONTRAT_PRO
         			  ,1
         			  ,1
         			  ,1
         			  ,1	

         		SELECT	@RESULT_CHEMIN_PIECE = REPERTOIRE + CASE WHEN PATINDEX('%\', REPERTOIRE) = LEN(REPERTOIRE) THEN '' ELSE '\' END + 
         		CASE 
         			WHEN COD_PIECE_PRO IN ('FACTADH', 'FACTOF') 
         			THEN 
         				(
         					SELECT 
         						CAST(YEAR(DAT_CREATION) AS VARCHAR(4)) +
         						'\' +
         						SUBSTRING(ISNULL(COD_FACTURE, 'NULL'), 1, CHARINDEX('/', ISNULL(COD_FACTURE, 'NULL')) - 1) +
         						SUBSTRING(ISNULL(COD_FACTURE, 'NULL'), CHARINDEX('/', ISNULL(COD_FACTURE, 'NULL')) + 1, LEN(ISNULL(COD_FACTURE, 'NULL')))
         							COLLATE FRENCH_CI_AS
         					FROM FACTURE WHERE ID_FACTURE = @ID_FACTURE) + '_FACT' 
         			ELSE 
         				CAST(YEAR(DAT_CREATION) AS VARCHAR(4)) + '\' + REPLACE(REPLACE(REPLICATE('0',10-LEN(CAST(COD_CONTRAT_PRO as VARCHAR(10)))) + CAST(COD_CONTRAT_PRO as VARCHAR(10)) + '_' + 
         								CASE WHEN COD_MODULE_PRO IS NOT NULL THEN SUBSTRING(COD_MODULE_PRO, LEN(COD_MODULE_PRO) - 1, 2) ELSE 'NULL' END + '_' + 
         								CASE WHEN COD_SESSION_PRO IS NOT NULL THEN SUBSTRING(COD_SESSION_PRO, LEN(COD_SESSION_PRO) - 1, 2) ELSE 'NULL' END + '_' + 
         								COD_PIECE_PRO, ' ', ''),'/','')
         			END
         		FROM	#TMP_PRO 
         		WHERE	#TMP_PRO.COD_PIECE_PRO LIKE DBO.GET_TYPE_PIECE_PRO(@TYPE_PIECE)+'%'
         	END
         END

         CREATE PROCEDURE [dbo].[ACTION_COMPTEUR]
         	@ACTION_ANNEE int
         AS
         BEGIN

         	DECLARE	@@COMPTEUR int
         	DECLARE @@COD_CPT varchar(11)

         	SET @@COD_CPT = 'ACTION '+convert(varchar(4), @ACTION_ANNEE)

         	SELECT @@COMPTEUR = NUM_CPT 
         	FROM COMPTEUR 
         	WHERE COD_CPT = @@COD_CPT

         	IF @@COMPTEUR is null 
         		BEGIN
         			SET @@COMPTEUR = 1
         			insert into COMPTEUR (COD_CPT, NUM_CPT, BLN_BLOQUE)
         			values (@@COD_CPT, 1, 0)
         		END
         	else
         		BEGIN
         			SET @@COMPTEUR = @@COMPTEUR + 1
         			update COMPTEUR set NUM_CPT = @@COMPTEUR
         			where COD_CPT = @@COD_CPT
         		END
         RETURN  @@COMPTEUR
         END

 CREATE PROCEDURE [dbo].[EDT_GENERATION_RECU_DSP]
          @ID_DESTINATAIRE INT,
          @ID_BENEFICIAIRE INT,
          @ID_ADRESSE   INT,
          @ID_CONTACT   INT,
          @EDT_ID_PERIODE  INT,
          @_ANOMALIE   VARCHAR(3)
         AS

         BEGIN
          DECLARE
           @RAISON_SOCIALE VARCHAR(64),
           @ID_ADHERENT INT,
           @COD_RECU  VARCHAR(13),
           @ID_RECU  VARCHAR(10),
           @ID_BRANCHE  INT,
           @SIREN   VARCHAR(14)

          SELECT
           @ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT,
           @ID_BRANCHE = ETABLISSEMENT.ID_BRANCHE,
           @SIREN = CONVERT(VARCHAR(14),
           cast(ADHERENT.NUM_SIREN as money),  1) 
          FROM
           ETABLISSEMENT
           INNER JOIN ADHERENT
            ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
          WHERE
           ID_ETABLISSEMENT = @ID_DESTINATAIRE

          SELECT
           @RAISON_SOCIALE = ADHERENT.LIB_RAISON_SOCIALE
          FROM
           ETABLISSEMENT
           INNER JOIN ADHERENT
            ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
          WHERE
           ID_ETABLISSEMENT = @ID_BENEFICIAIRE

          SET
           @SIREN = REPLACE(LEFT(@SIREN, LEN(@SIREN) - 3),',','') 

          DECLARE
           @ID_ETAB_PLASTURGIE INT,
           @NOMBRE_ETABS INT 

          SELECT
           @ID_ETAB_PLASTURGIE = MIN(ID_ETABLISSEMENT),
           @NOMBRE_ETABS = COUNT(ID_ETABLISSEMENT)
          FROM
           ETABLISSEMENT
          WHERE
           ID_ADHERENT = @ID_ADHERENT
           AND ID_BRANCHE = 13
           AND BLN_ACTIF = 1

          IF (@ID_BRANCHE <> 13) 
          BEGIN
           IF (@ID_ETAB_PLASTURGIE IS NULL)
           BEGIN
            RETURN 
           END
           SET @ID_BENEFICIAIRE = @ID_ETAB_PLASTURGIE
          END

          IF (@_Anomalie = 'non' 
           AND NOT EXISTS
            (
             SELECT
              ID_RECU
             FROM
              RECU
             WHERE
              ID_ADHERENT = @ID_ADHERENT
              AND ID_PERIODE = @EDT_ID_PERIODE
              AND ID_TYPE_RECU = 4
              AND BLN_ACTIF = 2
            )
           )
          BEGIN
           EXEC dbo.INS_RECU
            @ID_ADHERENT,
            @EDT_ID_PERIODE,
            0,
            0,
            0,
            1
          END

          SELECT
           @ID_RECU = ID_RECU,
           @COD_RECU = COD_RECU
          FROM
           RECU
          WHERE
           ID_RECU_REMP IS NULL
           AND  ID_ADHERENT = @ID_ADHERENT
           AND  ID_PERIODE = @EDT_ID_PERIODE
           AND  ID_TYPE_RECU = 4

          DECLARE
           @MASSE_SALARIALE DECIMAL(18,2)

          SELECT
           @MASSE_SALARIALE = SUM(R21_BIS.MASSE_SALARIALE_REELLE)
          FROM
           R21_BIS
           INNER JOIN ETABLISSEMENT
            ON ETABLISSEMENT.ID_ETABLISSEMENT = R21_BIS.ID_ETABLISSEMENT
          WHERE
           ETABLISSEMENT.ID_BRANCHE = 13
           AND ETABLISSEMENT.BLN_ACTIF = 1
           AND R21_BIS.ID_PERIODE = @EDT_ID_PERIODE
           AND ETABLISSEMENT.ID_ADHERENT = @ID_ADHERENT

          DECLARE
           @CONTRIBUTION DECIMAL(18,2)

          SELECT
           ETABLISSEMENT.NUM_SIRET,
           SUM (POSTE_IMPUTATION.MNT_HT - POSTE_IMPUTATION.DONT_REGLE_FPSPP) AS CONTRIBUTION
          INTO
           #CONTRIB_ETABS
          FROM
           VERSEMENT
           INNER JOIN POSTE_VERSEMENT
            ON POSTE_VERSEMENT.ID_VERSEMENT = VERSEMENT.ID_VERSEMENT
           INNER JOIN POSTE_IMPUTATION
            ON POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT
           INNER JOIN TYPE_VERSEMENT
            ON POSTE_VERSEMENT.ID_TYPE_VERSEMENT = TYPE_VERSEMENT.ID_TYPE_VERSEMENT
           INNER JOIN ETABLISSEMENT
            ON ETABLISSEMENT.ID_ETABLISSEMENT = POSTE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE
          WHERE
           VERSEMENT.BLN_IMPAYE = 0
           AND VERSEMENT.BLN_ACTIF = 1
           AND POSTE_IMPUTATION.ID_VIREMENT_INTERNE_COLLECTE IS NOT NULL
           AND POSTE_VERSEMENT.ID_PERIODE = @EDT_ID_PERIODE
           AND POSTE_IMPUTATION.ID_ACTIVITE = 6
           AND ID_ADHERENT_BENEFICIAIRE = @ID_ADHERENT
           AND TYPE_VERSEMENT.GRP_TYPE_VERSEMENT = 'O'
          GROUP BY
           POSTE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE, ETABLISSEMENT.NUM_SIRET
          HAVING
           SUM (POSTE_IMPUTATION.MNT_HT - POSTE_IMPUTATION.DONT_REGLE_FPSPP) > 0

          SELECT
           CONVERT(VARCHAR(12), GETDATE(), 103) AS DAT_EDITION,
           @SIREN AS SIREN,
           NUM_ANNEE AS ANNEE
          INTO
           #PERIODE
          FROM
           PERIODE
          WHERE
           PERIODE.ID_PERIODE = @EDT_ID_PERIODE

          SELECT
           @CONTRIBUTION = SUM(CONTRIBUTION)
          FROM
           #CONTRIB_ETABS

          SELECT

           (
            SELECT
             @RAISON_SOCIALE AS LIB_RAISON_SOCIALE,
             ISNULL(CIVILITE.LIBL_CIVILITE, '') AS CONTACT_CIVILITE,
             ISNULL(CONTACT.LIB_NOM_CONTACT, '') AS LIB_NOM_CONTACT,
             ISNULL(CONTACT.LIB_PNM_CONTACT, '') AS LIB_PNM_CONTACT,
             ADRESSE.LIB_ADR,
             ISNULL(ADRESSE.LIB_COMP_VOIE, '') AS LIB_COMP_VOIE,
             ADRESSE.LIB_CP_CEDEX,
             ADRESSE.LIB_VIL_CEDEX
            FROM
             ADRESSE
             LEFT OUTER JOIN CONTACT
              ON CONTACT.ID_CONTACT = @ID_CONTACT
               AND  CONTACT.LIB_NOM_CONTACT <> '.'
               AND  CONTACT.LIB_NOM_CONTACT NOT LIKE 'contact%'
             LEFT OUTER JOIN CIVILITE
              ON CONTACT.ID_CIVILITE = CIVILITE.ID_CIVILITE
            WHERE
             ADRESSE.ID_ADRESSE = @ID_ADRESSE
            FOR XML AUTO, ELEMENTS, TYPE
           ),

           (
            SELECT
             DAT_EDITION,
             SIREN,
             ANNEE
            FROM
             #PERIODE
            FOR XML PATH('ENTETE'), TYPE
           ),

           (
            SELECT
             @NOMBRE_ETABS AS NOMBRE,
             (
              SELECT
               ANNEE
              FROM
               #PERIODE
             ) AS ANNEE,
             dbo.GetFrenchCurrencyFormat(@MASSE_SALARIALE) AS MASSE_SALARIALE,
             (
              SELECT
               NUM_SIRET,
               dbo.GetFrenchCurrencyFormat(CONTRIBUTION) AS CONTRIB_ETAB
              FROM
               #CONTRIB_ETABS
              FOR XML RAW('TABLEAU_ETABS'),ELEMENTS, TYPE
             ),
             dbo.GetFrenchCurrencyFormat(@CONTRIBUTION) as CONTRIBUTION_TOTALE
            FOR XML RAW('CORPS'), ELEMENTS, TYPE
           )
          FOR XML RAW('LETTRE'), ELEMENTS
         END

         CREATE PROCEDURE [dbo].[LEC_DET_CONTRAT_PRO]
         	@ID_CONTRAT_PRO				AS INT,
         	@ID_UTILISATEUR				AS INT = NULL
         AS
         BEGIN
         	DECLARE @CPT as INT, @NB as INT, @NB_UNITE_FORMATION_TOT decimal(18,2)
         	DECLARE @COD_UTILISATEUR_CREATEUR VARCHAR(10)
         	SET @CPT	= 0
         	SET @NB		= 0
         	SET @COD_UTILISATEUR_CREATEUR		= NULL

         	SELECT @CPT = count(*) 
         		FROM SESSION_PRO 
         			INNER JOIN MODULE_PRO		ON MODULE_PRO.ID_MODULE_PRO = SESSION_PRO.ID_MODULE_PRO

         			WHERE (DAT_BAP_OF IS NOT NULL OR DAT_BAP_ADH IS NOT NULL) AND MODULE_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO

         	SELECT @NB = count(C.ID_CONTRAT_PRO)
         		FROM CONTRAT_PRO C
         			INNER JOIN CONTRAT_PRO C1	on	C1.ID_ETABLISSEMENT = C.ID_ETABLISSEMENT AND C1.ID_TUTEUR = C.ID_TUTEUR 
         										AND DATEDIFF(day, C1.DAT_DEB_CONTRAT, C.DAT_FIN_CONTRAT) > 0
         										AND DATEDIFF(day, C.DAT_DEB_CONTRAT, C1.DAT_FIN_CONTRAT) > 0
         										AND C1.ID_CONTRAT_PRO = @ID_CONTRAT_PRO
         		WHERE
         			C.DAT_CLOTURE		is null	AND 
         			C.BLN_ACTIF			= 1		AND 
         			C.ID_ETAT_CONTRAT_PRO <> 7	AND
         			C.ID_CONTRAT_PRO <> C1.ID_CONTRAT_PRO

         	SELECT @NB_UNITE_FORMATION_TOT = SUM(MODULE_PRO.NB_UNITE_FORMATION) 
         		FROM MODULE_PRO
         			INNER JOIN TYPE_FORMATION	on	TYPE_FORMATION.ID_TYPE_FORMATION = MODULE_PRO.ID_TYPE_FORMATION
         		WHERE @ID_CONTRAT_PRO is not null	AND MODULE_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO AND
         				MODULE_PRO.BLN_ACTIF = 1	AND 
         				COD_TYPE_FORMATION <> 'FTUT' 

         	SELECT @COD_UTILISATEUR_CREATEUR = (SELECT UTILISATEUR.COD_UTIL
         		FROM UTILISATEUR 
         		INNER JOIN CONTRAT_PRO ON UTILISATEUR.ID_UTILISATEUR = CONTRAT_PRO.ID_UTILISATEUR_CREATEUR
         		WHERE CONTRAT_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO)

         DECLARE @DOB DATETIME

         SELECT @DOB = INDIVIDU.DAT_NAISSANCE 
         FROM CONTRAT_PRO
         LEFT JOIN INDIVIDU ON INDIVIDU.ID_INDIVIDU = CONTRAT_PRO.ID_TUTEUR
         WHERE CONTRAT_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO

         	SELECT 
         		CONTRAT_PRO.*,
         		@DOB					AS DAT_NAISSANCE_TUTEUR,
         		@CPT					as NB_SESSION_BAPE,
         		@NB						as NB_CONTRAT_PRO_TUTEUR,
         		@NB_UNITE_FORMATION_TOT as NB_UNITE_FORMATION_TOT,
         		UTILISATEUR.COD_UTIL	as COD_UTILISATEUR,
         		ADHERENT.ID_ADHERENT,
         		INDIVIDU.ID_INDIVIDU,
         		CLASSIFICATION.COD_CLASSIFICATION,
         		TYPE_CONTRAT_PRO.COD_TYPE_CONTRAT_PRO,
         		@COD_UTILISATEUR_CREATEUR as COD_UTILISATEUR_CREATEUR
         	FROM
         		CONTRAT_PRO
         		LEFT  JOIN UTILISATEUR			ON UTILISATEUR.ID_UTILISATEUR		= (case when @ID_CONTRAT_PRO is null then @ID_UTILISATEUR 
         																					else CONTRAT_PRO.ID_UTILISATEUR end)
         		LEFT  JOIN ETABLISSEMENT		ON ETABLISSEMENT.ID_ETABLISSEMENT	= CONTRAT_PRO.ID_ETABLISSEMENT
         		LEFT JOIN  ADHERENT				ON ADHERENT.ID_ADHERENT				= ETABLISSEMENT.ID_ADHERENT
         		LEFT  JOIN SALARIE_PRO			ON SALARIE_PRO.ID_SALARIE_PRO		= CONTRAT_PRO.ID_SALARIE_PRO
         		LEFT  JOIN INDIVIDU				ON INDIVIDU.ID_INDIVIDU				= SALARIE_PRO.ID_INDIVIDU
         		LEFT JOIN CLASSIFICATION		ON CONTRAT_PRO.ID_CLASSIFICATION = CLASSIFICATION.ID_CLASSIFICATION
         		INNER JOIN TYPE_CONTRAT_PRO		ON CONTRAT_PRO.ID_TYPE_CONTRAT_PRO = TYPE_CONTRAT_PRO.ID_TYPE_CONTRAT_PRO
         	WHERE
         		CONTRAT_PRO.ID_CONTRAT_PRO	= @ID_CONTRAT_PRO
         END

 CREATE PROCEDURE [dbo].[GET_COMPTEUR_SUIVANT]
         	@COD_CPT VARCHAR(25)
         AS
         BEGIN
         	DECLARE @compteur [int]

         	SELECT @compteur = NUM_CPT
         	FROM COMPTEUR
         	WHERE COD_CPT = @COD_CPT

         	UPDATE 	COMPTEUR
         		SET NUM_CPT = @compteur + 1
         		WHERE COD_CPT = @COD_CPT

         	RETURN @compteur
         END

         CREATE PROCEDURE [dbo].[EDT_LETTRE_PEC_ACCORD_DX97_CSP]
         	@ID_STAGIAIRE int,
         	@ID_ETABLISSEMENT int,
         	@ID_MODULE int
         AS
         BEGIN

         SELECT 
         	CASE WHEN i.BLN_MASCULIN = 1
         		THEN 'Monsieur'
         		ELSE 'Madame'
         	END AS CONTACT_CIVILITE,
         	i.NOM_INDIVIDU AS LIB_NOM_CONTACT,
         	i.PRENOM_INDIVIDU AS LIB_PNM_CONTACT,
         	CAST(a.NUM_VOIE as VARCHAR(5))  + isnull(' '+LIBL_COMP_NUM_ADR+' ',' ') + tv.LIBL_TYPE_VOIE + ' ' + a.LIB_NOM_VOIE AS LIB_ADR,
         	a.LIB_COMP_VOIE,
         	a.LIB_MENTION_PARTICULIERE, 
         	h.COD_POSTAL AS LIB_CP_CEDEX,
         	h.LIB_ACHEMINEMENT AS LIB_VIL_CEDEX
         	INTO #TMP_BENEF
         	FROM STAGIAIRE_PEC AS st
         	INNER JOIN SALARIE AS sa ON sa.ID_INDIVIDU = st.ID_INDIVIDU AND sa.ID_ETABLISSEMENT = st.ID_ETABLISSEMENT
         	INNER JOIN INDIVIDU AS i ON i.ID_INDIVIDU = st.ID_INDIVIDU
         	INNER JOIN ADRESSE AS a ON a.ID_ADRESSE = sa.ID_ADRESSE
         	INNER JOIN TYPE_VOIE AS tv ON tv.ID_TYPE_VOIE = a.ID_TYPE_VOIE
         	INNER JOIN HEXAPOSTE AS h ON h.ID_HEXAPOSTE = a.ID_HEXAPOSTE
         	left join COMP_NUM_ADR on a.ID_COMP_NUM_ADR = COMP_NUM_ADR.ID_COMP_NUM_ADR 
         	WHERE st.ID_STAGIAIRE_PEC = @ID_STAGIAIRE

         SELECT LTRIM(CR.LIB_PNM) AS LIB_PRENOM_CHARGE_RELATION,
         	LTRIM(CR.LIB_NOM) AS LIB_NOM_CHARGE_RELATION,
         	CR.EMAIL,
         	(SELECT TOP 1 FAF_NAME FROM PARAMETRES) as NOM,
         	CR.LIB_ADR1 AS ADRESSE1,
         	ISNULL(CR.LIB_ADR2, '') AS ADRESSE2,
         	CR.COD_POSTAL AS CP,
         	CR.LIB_VILLE AS VILLE,
         	CR.NUM_TEL AS TEL,
         	CR.LIB_VILLE,
         	dbo.GetFullDate(getDate()) as DATE,
         	MODULE_PEC.COD_MODULE_PEC AS REFERENCE,
         	MODULE_PEC.LIBL_MODULE_PEC AS NOM_FORMATION,
         	ORGANISME_FORMATION.LIB_RAISON_SOCIALE AS NOM_OF,
         	'du ' + CONVERT(VARCHAR(8), MODULE_PEC.DAT_DEBUT, 3) + ' au ' + CONVERT(VARCHAR(8), MODULE_PEC.DAT_FIN, 3) + ' ' + CONVERT(VARCHAR(15), MODULE_PEC.NUM_DUREE_HEURE) + ' h' AS DATE_DUREE,
         	dbo.GetFrenchCurrencyFormat(CAST(coalesce(POSTE_COUT_ENGAGE.MNT_ENGAGE_HT,0) as money)) + CONVERT(NVARCHAR(6), ' ? H.T') AS COUT,
         	(SELECT TOP 1 CONTACT_CIVILITE FROM #TMP_BENEF) AS POLITESSE
         	INTO	#TMP_GENERAL
         	FROM	ADHERENT
         	INNER JOIN ETABLISSEMENT ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT AND ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
         	INNER JOIN UTILISATEUR AS CR ON ETABLISSEMENT.ID_CHARGEE_RELATION = CR.ID_UTILISATEUR
         	INNER JOIN MODULE_PEC ON MODULE_PEC.ID_MODULE_PEC = @ID_MODULE
         	INNER JOIN ETABLISSEMENT_OF ON ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = MODULE_PEC.ID_ETABLISSEMENT_OF
         	INNER JOIN ORGANISME_FORMATION ON ORGANISME_FORMATION.ID_OF = ETABLISSEMENT_OF.ID_OF
         	INNER JOIN POSTE_COUT_ENGAGE ON POSTE_COUT_ENGAGE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL
         	INNER JOIN SOUS_TYPE_COUT ON SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT = POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT AND SOUS_TYPE_COUT.COD_SOUS_TYPE_COUT = 'CP';

         WITH XMLNAMESPACES (
         	DEFAULT 'EDT_LETTRE_PEC_ACCORD_DX97_CSP'
         )

         SELECT
         	(SELECT
         			CONTACT_CIVILITE,
         			LIB_NOM_CONTACT,
         			LIB_PNM_CONTACT,
         			LIB_ADR,
         			LIB_COMP_VOIE,
         			LIB_MENTION_PARTICULIERE,
         			LIB_CP_CEDEX,
         			LIB_VIL_CEDEX
         		FROM #TMP_BENEF AS ADRESSE
         		FOR XML AUTO, ELEMENTS, TYPE
         	) AS BENEFICIAIRE,
         	(SELECT
         			LIB_PRENOM_CHARGE_RELATION,
         			LIB_NOM_CHARGE_RELATION,
         			EMAIL,
         			(SELECT
         					NOM,
         					ADRESSE1,
         					ADRESSE2,
         					CP,
         					VILLE,
         					TEL
         				FROM #TMP_GENERAL AS EMETTEUR
         				FOR XML AUTO, ELEMENTS, TYPE),
         			LIB_VILLE,
         			[DATE],
         			REFERENCE,
         			POLITESSE AS POLITESSE_HAUT
         		FROM #TMP_GENERAL AS ENTETE     
         		FOR XML AUTO, ELEMENTS, TYPE
         	),
         	(SELECT
         			(SELECT
         					NOM_FORMATION,
         					NOM_OF,
         					DATE_DUREE,
         					COUT
         				FROM #TMP_GENERAL AS FORMATION
         				FOR XML AUTO, ELEMENTS, TYPE),
         			POLITESSE AS POLITESSE_BAS
         		FROM #TMP_GENERAL AS CORPS
         		FOR XML AUTO, ELEMENTS, TYPE)
         FROM	#TMP_GENERAL as LETTRE
         	FOR XML AUTO, ELEMENTS

         END

         CREATE PROCEDURE EDT_BORDEREAUX
         	@IDS_BORDEREAU varchar(500)	
         with recompile
         AS
         	BEGIN
         		DECLARE
         			@Item int,
         			@LIB_NOM varchar(35),
         			@LIB_PNM varchar(35),
         			@COD_BORDEREAU int,
         			@COD_ADHERENT int,
         			@COD_TIERS varchar(20),
         			@LIB_RAISON_SOCIALE varchar(50),
         			@NOM_TIERS varchar(50),
         			@ID_POSTE_VERSEMENT int,
         			@ID_VERSEMENT int,
         			@LIB_BANQUE varchar(50),
         			@NUM_CHEQUE varchar(10),
         			@DAT_SAISIE datetime,
         			@TOTAL_HT decimal(18,2),
         			@CHQ_CPT int,
         			@OLD_COD_BORDEREAU int,
         			@OLD_ID_POSTE_VERSEMENT int

         		SET @OLD_COD_BORDEREAU = 0
         		SET @OLD_ID_POSTE_VERSEMENT = 0
         		SET @CHQ_CPT = 0

         		CREATE TABLE #TMP_DATA
         		(
         			LIB_NOM varchar(35),
         			LIB_PNM varchar(35),
         			COD_BORDEREAU int,
         			COD_ADHERENT varchar(20),
         			LIB_RAISON_SOCIALE varchar(50),
         			ID_POSTE_VERSEMENT int,
         			LIB_BANQUE varchar(50),
         			NUM_CHEQUE varchar(10),
         			DAT_SAISIE datetime,
         			TOTAL_HT decimal(18,2),
         			NUM_CHEQ int
         		)

         		CREATE TABLE #List(Item int)

         		DECLARE @Delimiter char
         		SET @Delimiter = ','
         		WHILE CHARINDEX(@Delimiter,@IDS_BORDEREAU,0) <> 0
         			BEGIN
         				SELECT
         					@Item=RTRIM(LTRIM(SUBSTRING(@IDS_BORDEREAU,1,CHARINDEX(@Delimiter,@IDS_BORDEREAU,0)-1))),
         					@IDS_BORDEREAU=RTRIM(LTRIM(SUBSTRING(@IDS_BORDEREAU,CHARINDEX(@Delimiter,@IDS_BORDEREAU,0)+1,LEN(@IDS_BORDEREAU))))

         				IF LEN(@Item) > 0
         					INSERT INTO #List
         					SELECT @Item
         			END

         		IF LEN(@IDS_BORDEREAU) > 0
         			INSERT INTO #List
         			SELECT @IDS_BORDEREAU 

         		DECLARE CURSOR_DATA CURSOR FOR
         		SELECT
         			UTILISATEUR.LIB_NOM,
         			UTILISATEUR.LIB_PNM,
         			BORDEREAU.COD_BORDEREAU,	
         			ADHERENT.COD_ADHERENT,
         			TIERS.COD_TIERS,
         			ADHERENT.LIB_RAISON_SOCIALE,
         			TIERS.LIB_NOM,

         			VERSEMENT.ID_VERSEMENT,
         			VERSEMENT.LIB_BANQUE,
         			VERSEMENT.NUM_CHEQUE,
         			VERSEMENT.DAT_SAISIE,

         			SUM(POSTE_IMPUTATION.MNT_TTC) as TOTAL_HT
         		FROM
         			BORDEREAU
         			INNER JOIN UTILISATEUR		ON BORDEREAU.ID_UTILISATEUR = UTILISATEUR.ID_UTILISATEUR
         			INNER JOIN VERSEMENT		ON VERSEMENT.ID_BORDEREAU = BORDEREAU.ID_BORDEREAU
         			INNER JOIN POSTE_VERSEMENT	ON POSTE_VERSEMENT.ID_VERSEMENT = VERSEMENT.ID_VERSEMENT 
         			LEFT OUTER JOIN ADHERENT	ON VERSEMENT.ID_ADHERENT_EMETTEUR = ADHERENT.ID_ADHERENT
         			LEFT OUTER JOIN TIERS		ON VERSEMENT.ID_TIERS_EMETTEUR = TIERS.ID_TIERS
         			INNER JOIN POSTE_IMPUTATION	ON POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT
         		WHERE
         		    (BORDEREAU.ID_BORDEREAU in (select Item from #List))
         			AND (POSTE_VERSEMENT.BLN_ACTIF = 1)
         			AND (VERSEMENT.BLN_ACTIF = 1)
         		GROUP BY
         			UTILISATEUR.LIB_NOM,
         			UTILISATEUR.LIB_PNM,
         			BORDEREAU.COD_BORDEREAU,
         			ADHERENT.COD_ADHERENT,
         			TIERS.COD_TIERS,
         			ADHERENT.LIB_RAISON_SOCIALE,
         			TIERS.LIB_NOM,
         			VERSEMENT.ID_VERSEMENT,

         			VERSEMENT.LIB_BANQUE,
         			VERSEMENT.NUM_CHEQUE,
         			VERSEMENT.DAT_SAISIE
         		ORDER BY
         			BORDEREAU.COD_BORDEREAU,

         			VERSEMENT.ID_VERSEMENT,
         			ADHERENT.COD_ADHERENT

         		OPEN CURSOR_DATA
         		FETCH NEXT FROM CURSOR_DATA INTO @LIB_NOM, @LIB_PNM, @COD_BORDEREAU, @COD_ADHERENT, @COD_TIERS, @LIB_RAISON_SOCIALE, @NOM_TIERS,  @ID_VERSEMENT, @LIB_BANQUE, @NUM_CHEQUE, @DAT_SAISIE, @TOTAL_HT
         		WHILE @@FETCH_STATUS = 0
         			BEGIN
         				IF (@COD_BORDEREAU <> @OLD_COD_BORDEREAU)
         					BEGIN
         						SET @CHQ_CPT = 0
         						SET @OLD_COD_BORDEREAU = @COD_BORDEREAU
         					END

         				SET @CHQ_CPT = @CHQ_CPT + 1

         				INSERT INTO #TMP_DATA VALUES
         				(
         					@LIB_NOM,
         					@LIB_PNM,
         					@COD_BORDEREAU,
         					isnull(cast(@COD_ADHERENT as varchar(20)), @COD_TIERS),
         					isnull(@LIB_RAISON_SOCIALE,@NOM_TIERS),
         					0,
         					@LIB_BANQUE,
         					@NUM_CHEQUE,
         					@DAT_SAISIE,
         					@TOTAL_HT,
         					@CHQ_CPT
         				)
         				FETCH NEXT FROM CURSOR_DATA INTO @LIB_NOM, @LIB_PNM, @COD_BORDEREAU, @COD_ADHERENT, @COD_TIERS, @LIB_RAISON_SOCIALE, @NOM_TIERS,  @ID_VERSEMENT, @LIB_BANQUE, @NUM_CHEQUE, @DAT_SAISIE, @TOTAL_HT
         			END
         		CLOSE CURSOR_DATA
         		DEALLOCATE CURSOR_DATA

         		SELECT
         			#TMP_DATA.*,
         			TRANSIT.NUM_IBAN_TRANSIT as NUM_COMPTE,
         			TRANSIT.BIC_TRANSIT as BIC,
         			TRANSIT.LIB_COMPTE_BANQUE AS LIB_COMPT_BANQUE 
         		FROM
         			#TMP_DATA, TRANSIT
         	END

         CREATE PROCEDURE [dbo].[UPD_DATE_LETTRE_RELANCE_ENGAGEMENT_CONTRAT_PRO_ADH]
         	@CODS_CONTRAT_PRO TEXT	
         AS

         BEGIN

         	DECLARE @Item VARCHAR(12);

         	CREATE TABLE #List(Item VARCHAR(12)  collate French_CI_AI)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,1,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)-1))),
         			@CODS_CONTRAT_PRO=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)+1,DATALENGTH(@CODS_CONTRAT_PRO))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT @Item
         	END

         	IF DATALENGTH(@CODS_CONTRAT_PRO) > 0
         		INSERT INTO #List SELECT @CODS_CONTRAT_PRO

         	SELECT      CONTRAT_PRO.ID_CONTRAT_PRO, 
         				ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO, 
         				ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_1, 
         				ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_2, 
         				ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_3
         	INTO		#TEMP
         	FROM        CONTRAT_PRO 
         				INNER JOIN ARRIVEE_PIECE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = ARRIVEE_PIECE_PRO.ID_CONTRAT_PRO
         				INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         					AND PIECE_PRO.BLN_CONTRAT = 1 AND PIECE_PRO.BLN_ACTIF=1 AND PIECE_PRO.BLN_BLOQUANT_ENGAGEMENT = 1
         	WHERE			CONTRAT_PRO.BLN_OK_PIECE = 0
         				AND CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)
         				AND ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 0) OR (ARRIVEE_PIECE_PRO.BLN_conforme = 0) )
         and ARRIVEE_PIECE_PRO.id_module_pro is null and ARRIVEE_PIECE_PRO.id_session_pro is null
         AND	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO  NOT IN 
         				(
         					SELECT DISTINCT APP.ID_ARRIVEE_PIECE_PRO
         					FROM CONTRAT_PRO TP
         						 INNER JOIN MODULE_PRO MP ON TP.ID_CONTRAT_PRO = MP.ID_CONTRAT_PRO
         						 INNER JOIN ARRIVEE_PIECE_PRO APP ON MP.ID_MODULE_PRO = APP.ID_MODULE_PRO
         						 CROSS JOIN PARAMETRES
         					WHERE		MP.BLN_OK_PIECE = 0 
         							AND MP.BLN_SUBROGE != 1
         							AND (
         									(
         											(APP.DAT_RELANCE_ENGAGE_1 IS NOT NULL)
         										AND (APP.DAT_RELANCE_ENGAGE_2 IS NULL) 
         										AND (APP.DAT_RELANCE_ENGAGE_3 IS NULL)
         										AND (DATEDIFF(DAY,APP.DAT_RELANCE_ENGAGE_1,GETDATE()) < PARAMETRES.DELAI_RELANCE_PRO )
         									)
         								OR
         									(
         											(APP.DAT_RELANCE_ENGAGE_2 IS NOT NULL)
         										AND (APP.DAT_RELANCE_ENGAGE_3 IS NULL) 
         										AND (DATEDIFF(DAY,APP.DAT_RELANCE_ENGAGE_2,GETDATE()) < PARAMETRES.DELAI_RELANCE_PRO )
         									)
         								OR
         									(APP.DAT_RELANCE_ENGAGE_3 IS NOT NULL)														
         							)
         				)
         	UNION

         	SELECT  CONTRAT_PRO.ID_CONTRAT_PRO, 
         			ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_1, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_2, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_3
         	FROM	CONTRAT_PRO
         			INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         			INNER JOIN ARRIVEE_PIECE_PRO ON MODULE_PRO.ID_MODULE_PRO = ARRIVEE_PIECE_PRO.ID_MODULE_PRO
         			INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         					AND PIECE_PRO.BLN_MODULE = 1 AND PIECE_PRO.BLN_CONTRAT = 0 AND PIECE_PRO.BLN_ACTIF=1 AND PIECE_PRO.BLN_BLOQUANT_ENGAGEMENT = 1
         			CROSS JOIN PARAMETRES
         	WHERE		MODULE_PRO.BLN_OK_PIECE = 0 
         			AND MODULE_PRO.BLN_SUBROGE != 1
         			AND CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)
         			AND ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 0) OR (ARRIVEE_PIECE_PRO.BLN_conforme = 0) )
          and ARRIVEE_PIECE_PRO.id_session_pro is null
         AND	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO  NOT IN 
         				(
         					SELECT DISTINCT APP.ID_ARRIVEE_PIECE_PRO
         					FROM CONTRAT_PRO TP
         						 INNER JOIN MODULE_PRO MP ON TP.ID_CONTRAT_PRO = MP.ID_CONTRAT_PRO
         						 INNER JOIN ARRIVEE_PIECE_PRO APP ON MP.ID_MODULE_PRO = APP.ID_MODULE_PRO
         						 CROSS JOIN PARAMETRES
         					WHERE		MP.BLN_OK_PIECE = 0 
         							AND MP.BLN_SUBROGE != 1
         							AND (
         									(
         											(APP.DAT_RELANCE_ENGAGE_1 IS NOT NULL)
         										AND (APP.DAT_RELANCE_ENGAGE_2 IS NULL) 
         										AND (APP.DAT_RELANCE_ENGAGE_3 IS NULL)
         										AND (DATEDIFF(DAY,APP.DAT_RELANCE_ENGAGE_1,GETDATE()) < PARAMETRES.DELAI_RELANCE_PRO )
         									)
         								OR
         									(
         											(APP.DAT_RELANCE_ENGAGE_2 IS NOT NULL)
         										AND (APP.DAT_RELANCE_ENGAGE_3 IS NULL) 
         										AND (DATEDIFF(DAY,APP.DAT_RELANCE_ENGAGE_2,GETDATE()) < PARAMETRES.DELAI_RELANCE_PRO )
         									)
         								OR
         									(APP.DAT_RELANCE_ENGAGE_3 IS NOT NULL)														
         							)
         				)

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_1 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_ENGAGE_1 IS NULL	
         						AND #TEMP.DAT_RELANCE_ENGAGE_2 IS NULL
         						AND #TEMP.DAT_RELANCE_ENGAGE_3 IS NULL 
         			)

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_2 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_ENGAGE_1 IS NOT NULL	
         						AND	#TEMP.DAT_RELANCE_ENGAGE_2 IS NULL
         						AND #TEMP.DAT_RELANCE_ENGAGE_3 IS NULL
         			)

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_3 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_ENGAGE_2 IS NOT NULL
         						AND #TEMP.DAT_RELANCE_ENGAGE_3 IS NULL
         			)

         DROP TABLE #List
         DROP TABLE #TEMP
         END

 CREATE PROCEDURE [dbo].[EDT_ORDRES_VIREMENT_CONTRAT_PRO]
          @ID_ETABLISSEMENT  INT,
          @ID_BENEFICIAIRE  INT,
          @TYPE_BENEFICIAIRE  INT,
          @ID_ADRESSE    INT,
          @ID_CONTACT    INT,
          @TYPE_DESTINATAIRE  INT,
          @ID_TRANSACTION   INT,
          @ID_REGLEMENT_PRO  INT,
          @BLN_COPIE    BIT = 0
         AS
          BEGIN
           DECLARE
            @COD_ADHERENT_OF AS VARCHAR(8),
            @TOT AS MONEY

           IF (@TYPE_DESTINATAIRE = 1) 
            BEGIN
             SELECT
              @COD_ADHERENT_OF = ADHERENT.COD_ADHERENT
             FROM
              ETABLISSEMENT
              JOIN ADHERENT ON  ETABLISSEMENT.ID_ADHERENT = ADHERENT.ID_ADHERENT
             WHERE
              ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
           END

           IF (@TYPE_DESTINATAIRE = 2) 
            BEGIN
             SELECT
              @COD_ADHERENT_OF = ORGANISME_FORMATION.COD_OF
             FROM
              ETABLISSEMENT_OF
              JOIN ORGANISME_FORMATION ON ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF
             WHERE
              ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_ETABLISSEMENT
            END

           SELECT  TOP 1 
            CM.LIB_NOM   AS LIB_NOM_CONSEILLER,
            CM.LIB_PNM   AS LIB_PNM_CONSEILLER,
            CR.ID_UTILISATEUR AS ID_UTIL,
            CR.LIB_PNM   AS LIB_PRENOM_CHARGE_RELATION,
            CR.LIB_NOM   AS LIB_NOM_CHARGE_RELATION,
            CR.LIB_VILLE,
            CR.EMAIL   AS EMAIL_CHARGE_RELATION,
            REGLEMENT_PRO.ID_REGLEMENT_PRO,
            REGLEMENT_PRO.NUM_VIREMENT,
            REGLEMENT_PRO.MNT_REGLE_TTC, 
            REGLEMENT_PRO.MNT_REGLE_HT,  
            REGLEMENT_PRO.DAT_VALID_REGLEMENT,
            REGLEMENT_PRO.DAT_REGLEMENT,
            [TRANSACTION].NUM_IBAN,
            [TRANSACTION].BIC
           INTO
            #TEMP_INFO
           FROM
            REGLEMENT_PRO
            JOIN [TRANSACTION]  ON  REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
            JOIN SESSION_PRO   ON  (SESSION_PRO.ID_REGLEMENT_PRO_OF = REGLEMENT_PRO.ID_REGLEMENT_PRO OR SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO)
            JOIN MODULE_PRO   ON  MODULE_PRO.ID_MODULE_PRO = SESSION_PRO.ID_MODULE_PRO
            JOIN CONTRAT_PRO   ON  CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
            LEFT JOIN ETABLISSEMENT  ON  CONTRAT_PRO.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT 
            LEFT JOIN ADHERENT  ON  ETABLISSEMENT.ID_ADHERENT = ADHERENT.ID_ADHERENT
            LEFT JOIN UTILISATEUR CR 
                   ON  ETABLISSEMENT.ID_CHARGEE_RELATION = CR.ID_UTILISATEUR
            LEFT JOIN UTILISATEUR CM 
                   ON  ETABLISSEMENT.ID_CHARGEE_MISSION = CM.ID_UTILISATEUR
           WHERE
            REGLEMENT_PRO.ID_REGLEMENT_PRO = @ID_REGLEMENT_PRO
            AND  REGLEMENT_PRO.ID_TRANSACTION = @ID_TRANSACTION;

           SELECT DISTINCT
            CASE
             WHEN INDIVIDU.BLN_MASCULIN  = 0 THEN 'Madame'
             ELSE 'Monsieur'
            END AS CIVILITE,
            INDIVIDU.NOM_INDIVIDU, 
            INDIVIDU.PRENOM_INDIVIDU, 
            CONTRAT_PRO.ID_CONTRAT_PRO,
            CONTRAT_PRO.COD_CONTRAT_PRO,
            CONTRAT_PRO.LIBL_CONTRAT_PRO,
            @ID_REGLEMENT_PRO AS ID_REGLEMENT_PRO,
            CONTRAT_PRO.ID_ETABLISSEMENT 
           INTO
            #TEMP_CONTRAT_PRO
           FROM 
            SESSION_PRO
            JOIN MODULE_PRO  ON MODULE_PRO.ID_MODULE_PRO = SESSION_PRO.ID_MODULE_PRO
            JOIN CONTRAT_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
            JOIN SALARIE_PRO ON SALARIE_PRO.ID_SALARIE_PRO = CONTRAT_PRO.ID_SALARIE_PRO 
            JOIN INDIVIDU  ON SALARIE_PRO.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU
           where 
            (SESSION_PRO.ID_REGLEMENT_PRO_OF = @ID_REGLEMENT_PRO
            OR 
            SESSION_PRO.ID_REGLEMENT_PRO_ADH = @ID_REGLEMENT_PRO) 

           SELECT  #TEMP_CONTRAT_PRO.ID_REGLEMENT_PRO,
             #TEMP_CONTRAT_PRO.ID_CONTRAT_PRO,
             MODULE_PRO.ID_MODULE_PRO,
             MODULE_PRO.LIBL_MODULE_PRO,
             MODULE_PRO.TAUX_HORAIRE_RETENU_ADH,
             MODULE_PRO.TAUX_HORAIRE_RETENU_OF,
             ORGANISME_FORMATION.LIB_RAISON_SOCIALE,

             coalesce(dbo.IS_EMPTY(ETABLISSEMENT.LIB_ENSEIGNE), ADHERENT.LIB_RAISON_SOCIALE) as LIB_RAISON_SOCIALE_2
           INTO #TEMP_MODULE_PRO
           FROM #TEMP_CONTRAT_PRO 
           JOIN MODULE_PRO ON  #TEMP_CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
           LEFT JOIN ETABLISSEMENT_OF ON  MODULE_PRO.ID_ETABLISSEMENT_OF = ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF 
           LEFT JOIN ORGANISME_FORMATION ON  ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF
           LEFT JOIN ETABLISSEMENT  ON  #TEMP_CONTRAT_PRO.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT 
           LEFT JOIN ADHERENT  ON  ETABLISSEMENT.ID_ADHERENT = ADHERENT.ID_ADHERENT
           WHERE MODULE_PRO.BLN_ACTIF = 1

           SELECT  SESSION_PRO.ID_SESSION_PRO,
             SESSION_PRO.COD_SESSION_PRO,
             SESSION_PRO.ID_MODULE_PRO,
             SESSION_PRO.DAT_DEBUT, 
             SESSION_PRO.DAT_FIN, 
             SESSION_PRO.NB_UNITE_TOTALE, 
             CASE @TYPE_DESTINATAIRE
               WHEN 1 THEN FAC_ADH.NUM_FACTURE
               WHEN 2 THEN FAC_OF.NUM_FACTURE
             END AS NUM_FACTURE,
             SESSION_PRO.MNT_VERSE_HT_OF,
             SESSION_PRO.MNT_VERSE_HT_ADH,
             SESSION_PRO.MNT_VERSE_TVA_OF,
             SESSION_PRO.MNT_VERSE_TVA_ADH
           INTO #TEMP_SESSION_PRO
           FROM    #TEMP_MODULE_PRO 
           JOIN SESSION_PRO
           ON  #TEMP_MODULE_PRO.ID_MODULE_PRO = SESSION_PRO.ID_MODULE_PRO
           LEFT JOIN
             FACTURE FAC_ADH
           ON  SESSION_PRO.ID_FACTURE_ADHERENT = FAC_ADH.ID_FACTURE
           LEFT JOIN
             FACTURE FAC_OF
           ON  SESSION_PRO.ID_FACTURE_OF = FAC_OF.ID_FACTURE
           WHERE SESSION_PRO.BLN_ACTIF = 1
           AND  (
             ((@TYPE_DESTINATAIRE = 1) AND (SESSION_PRO.ID_REGLEMENT_PRO_ADH = #TEMP_MODULE_PRO.ID_REGLEMENT_PRO) AND (SESSION_PRO.MNT_VERSE_HT_ADH > 0))
             OR
             ((@TYPE_DESTINATAIRE = 2) AND (SESSION_PRO.ID_REGLEMENT_PRO_OF = #TEMP_MODULE_PRO.ID_REGLEMENT_PRO) AND (SESSION_PRO.MNT_VERSE_HT_OF > 0))
             )

           DELETE FROM #TEMP_MODULE_PRO
           WHERE #TEMP_MODULE_PRO.ID_MODULE_PRO NOT IN (SELECT #TEMP_SESSION_PRO.ID_MODULE_PRO FROM #TEMP_SESSION_PRO);

           DELETE FROM #TEMP_CONTRAT_PRO
           WHERE #TEMP_CONTRAT_PRO.ID_CONTRAT_PRO NOT IN (SELECT #TEMP_MODULE_PRO.ID_CONTRAT_PRO FROM #TEMP_MODULE_PRO);

           WITH XMLNAMESPACES (
            DEFAULT 'ORDRE_VIREMENT_CONTRAT_PRO'
           )
           SELECT

             dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) AS BENEFICIAIRE,

             (

              SELECT                

               CASE
                WHEN (@TYPE_DESTINATAIRE = 1) THEN 'ADHERENT'
                WHEN (@TYPE_DESTINATAIRE = 2) THEN 'OF'
               END AS LIB_ADHERENT_OF,
               @COD_ADHERENT_OF AS COD_ADHERENT_OF,
               ISNULL(LIB_PRENOM_CHARGE_RELATION, '') AS LIB_PRENOM_CHARGE_RELATION,
               ISNULL(LIB_NOM_CHARGE_RELATION, '')  AS LIB_NOM_CHARGE_RELATION,
               ISNULL(EMAIL_CHARGE_RELATION, '')  AS EMAIL,

               dbo.GetXmlAdrUtilAvecTel(ENTETE.ID_UTIL, 
                     (SELECT MAX(CAST(dbo.DossierEstTSA(COD_CONTRAT_PRO, 0) AS INT)) FROM #TEMP_CONTRAT_PRO), 
                     (
                      CASE
                       WHEN (@TYPE_DESTINATAIRE = 1) THEN 1
                       ELSE 0
                      END
                     )) AS EMETTEUR,
               CAST(NUM_VIREMENT AS VARCHAR) + case when @BLN_COPIE = 1 then ' Copie' else '' end  AS NUM_VIREMENT,

               (
                SELECT (      
                 SELECT DISTINCT COD_CONTRAT_PRO
                 FROM #TEMP_CONTRAT_PRO
                 FOR XML RAW (''), ELEMENTS, TYPE
                ) FOR XML RAW ('REFERENCE_CONTRAT'), ELEMENTS, TYPE
               ),

               RTRIM(CASE WHEN PATINDEX('%CEDEX%', LIB_VILLE) <> 0 THEN LEFT(LIB_VILLE, PATINDEX('%CEDEX%', LIB_VILLE)-1) ELSE LIB_VILLE END) AS LIB_VILLE, 
               dbo.GetFullDate(GETDATE()) AS DATE,    
               (
                SELECT TOP 1
                  dbo.GetContactSalutation(@ID_CONTACT, 1)
                FROM CIVILITE
                FOR XML PATH('POLITESSE_HAUT'), TYPE
               )
              FROM #TEMP_INFO AS ENTETE     
              FOR XML AUTO, ELEMENTS, TYPE
             ),

             (
              SELECT 
                dbo.GetShorterDate(#TEMP_INFO.DAT_REGLEMENT) AS DAT_REGLEMENT,
                COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_INFO.MNT_REGLE_TTC AS MONEY)), ' ') AS MNT_REGLE_TTC,
                #TEMP_INFO.NUM_IBAN,
                #TEMP_INFO.BIC,
                COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_INFO.MNT_REGLE_HT AS MONEY)), ' ') AS MNT_REGLE_HT,
                COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_INFO.MNT_REGLE_TTC-#TEMP_INFO.MNT_REGLE_HT  AS MONEY)), ' ') AS MNT_REGLE_TVA, 
                (
                 SELECT 'Intitul‚ du module' AS LIB_TITRE_TABLEAU_LIGNE_1,
                   CASE          
                    WHEN (@TYPE_DESTINATAIRE = 1) THEN 'Organisme Formation'
                    WHEN (@TYPE_DESTINATAIRE = 2) THEN 'Adh‚rent'
                   END AS LIB_TITRE_TABLEAU_LIGNE_2,
                   CASE          
                    WHEN (@TYPE_DESTINATAIRE = 1) THEN 'Taux horaire vers‚ … l''adh‚rent'
                    WHEN (@TYPE_DESTINATAIRE = 2) THEN 'Taux horaire vers‚ … l''OF'
                   END AS LIB_TITRE_TABLEAU_LIGNE_3
                 FOR XML RAW ('ENTETE_TABLEAU'), ELEMENTS, TYPE
                 ),
                 (
                  SELECT (      
                   SELECT #TEMP_CONTRAT_PRO.COD_CONTRAT_PRO,
                   #TEMP_CONTRAT_PRO.CIVILITE,
                   #TEMP_CONTRAT_PRO.PRENOM_INDIVIDU, 
                   #TEMP_CONTRAT_PRO.NOM_INDIVIDU
                   FROM #TEMP_CONTRAT_PRO
                   FOR XML RAW ('CONTRAT'), ELEMENTS, TYPE
                  ) FOR XML RAW ('RECAP_CONTRATS'), ELEMENTS, TYPE
                 ),
                 (
                 SELECT #TEMP_CONTRAT_PRO.COD_CONTRAT_PRO,
                 #TEMP_CONTRAT_PRO.CIVILITE,
                 #TEMP_CONTRAT_PRO.PRENOM_INDIVIDU, 
                 #TEMP_CONTRAT_PRO.NOM_INDIVIDU,
                 #TEMP_CONTRAT_PRO.LIBL_CONTRAT_PRO,
                 (
                  SELECT
                    (    
                     SELECT #TEMP_MODULE_PRO.LIBL_MODULE_PRO,
                       (
                        SELECT 
                          CASE 
                           WHEN (@TYPE_DESTINATAIRE = 2) THEN #TEMP_MODULE_PRO.LIB_RAISON_SOCIALE_2 
                           WHEN (@TYPE_DESTINATAIRE = 1) THEN #TEMP_MODULE_PRO.LIB_RAISON_SOCIALE
                          END AS LIB_RAISON_SOCIALE,

                          CASE
                           WHEN (@TYPE_DESTINATAIRE = 1) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_MODULE_PRO.TAUX_HORAIRE_RETENU_ADH AS MONEY)), ' ')
                           WHEN (@TYPE_DESTINATAIRE = 2) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_MODULE_PRO.TAUX_HORAIRE_RETENU_OF AS MONEY)), ' ')
                          END AS  TAUX_HORAIRE_RETENU,

                          dbo.GetShorterDate(#TEMP_SESSION_PRO.DAT_DEBUT) AS DAT_DEBUT,
                          dbo.GetShorterDate(#TEMP_SESSION_PRO.DAT_FIN) AS DAT_FIN,
                          COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_SESSION_PRO.NB_UNITE_TOTALE AS MONEY)), ' ') AS NB_UNITE_TOTALE,

                          CASE 
                           WHEN (@TYPE_DESTINATAIRE = 1) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_SESSION_PRO.MNT_VERSE_HT_ADH AS MONEY)), ' ')
                           WHEN (@TYPE_DESTINATAIRE = 2) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_SESSION_PRO.MNT_VERSE_HT_OF AS MONEY)), ' ')
                          END AS MNT_VERSE_HT,

                          CASE 
                           WHEN (@TYPE_DESTINATAIRE = 1) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_SESSION_PRO.MNT_VERSE_TVA_ADH AS MONEY)), ' ')
                           WHEN (@TYPE_DESTINATAIRE = 2) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_SESSION_PRO.MNT_VERSE_TVA_OF AS MONEY)), ' ')
                          END AS MNT_VERSE_TVA,

                          COALESCE(#TEMP_SESSION_PRO.NUM_FACTURE, '') AS NUM_FACTURE,
                          #TEMP_SESSION_PRO.COD_SESSION_PRO AS ID_SESSION_PRO,

                          CASE
                           WHEN (@TYPE_DESTINATAIRE = 1) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_SESSION_PRO.MNT_VERSE_HT_ADH + #TEMP_SESSION_PRO.MNT_VERSE_TVA_ADH AS MONEY)), ' ')
                           WHEN (@TYPE_DESTINATAIRE = 2) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(#TEMP_SESSION_PRO.MNT_VERSE_HT_OF + #TEMP_SESSION_PRO.MNT_VERSE_TVA_OF AS MONEY)), ' ')
                          END AS  MNT_TTC

                        FROM #TEMP_SESSION_PRO
                        WHERE #TEMP_MODULE_PRO.ID_MODULE_PRO = #TEMP_SESSION_PRO.ID_MODULE_PRO
                        FOR XML RAW ('SESSION'), ELEMENTS, TYPE
                       )
                     FROM #TEMP_MODULE_PRO
                     WHERE #TEMP_MODULE_PRO.ID_CONTRAT_PRO = #TEMP_CONTRAT_PRO.ID_CONTRAT_PRO
                     FOR XML RAW ('MODULE'), ELEMENTS, TYPE
                   )
                 )
                 FROM #TEMP_CONTRAT_PRO
                 FOR XML RAW ('CONTRAT'), ELEMENTS, TYPE
                ),
                (
                 SELECT CASE
                    WHEN (@TYPE_DESTINATAIRE = 1) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(SUM(#TEMP_SESSION_PRO.MNT_VERSE_HT_ADH + #TEMP_SESSION_PRO.MNT_VERSE_TVA_ADH) AS MONEY)), ' ')
                    WHEN (@TYPE_DESTINATAIRE = 2) THEN COALESCE(dbo.GetFrenchCurrencyFormat(CAST(SUM(#TEMP_SESSION_PRO.MNT_VERSE_HT_OF + #TEMP_SESSION_PRO.MNT_VERSE_TVA_OF) AS MONEY)), ' ')
                   END AS  TOTAL_MNT_TTC
                 FROM #TEMP_SESSION_PRO
                 FOR XML RAW(''), ELEMENTS,TYPE
                ),
                (
                 SELECT TOP 1
                   dbo.GetContactSalutation(@ID_CONTACT, 1) AS POLITESSE_BAS
                 FROM CIVILITE
                 FOR XML RAW(''), ELEMENTS,TYPE
                )
              FROM #TEMP_INFO
              FOR XML RAW('CORPS'), ELEMENTS,TYPE
             ),
             (

              SELECT SIGNATURE.LIB_PNM_CONSEILLER,
                SIGNATURE.LIB_NOM_CONSEILLER
              FROM #TEMP_INFO as SIGNATURE
              FOR XML AUTO, ELEMENTS, TYPE
             )

           FROM #TEMP_INFO AS LETTRE
           FOR XML RAW('') ,ROOT('LETTRE'),ELEMENTS
          END

         CREATE PROCEDURE [dbo].[LEC_GRP_ADHERENT_JAMAIS_PAYE]        
         @ID_PERIODE int,        
          @ID_AGENCE int,        
          @P10PlusOnly tinyint = 0        
         AS        

          
         DECLARE @NUM_ANNEE int      
         BEGIN      

          IF @ID_AGENCE = -1 SET @ID_AGENCE = NULL 

          SELECT @NUM_ANNEE = NUM_ANNEE      
          FROM PERIODE      
          WHERE ID_PERIODE = @ID_PERIODE       

          SELECT        
          ADHERENT.ID_ADHERENT,        
          ADHERENT.ID_ETABLISSEMENT_PRINCIPAL as ID_ETABLISSEMENT        

          FROM        
          ADHERENT        

          INNER JOIN R20  ON R20.ID_ADHERENT  = ADHERENT.ID_ADHERENT       
               AND R20.ID_PERIODE  = @ID_PERIODE      
          INNER JOIN PERIODE ON PERIODE.ID_PERIODE = @ID_PERIODE      

          LEFT JOIN       
           (       
           SELECT ID_ADHERENT, MNT_VERSE_HT = SUM(MNT_HT_VO)      
           FROM VUE_BO_Restit_Collecte_ETABLISSEMENT      
           WHERE ANNEE = @NUM_ANNEE      
           GROUP BY ID_ADHERENT      
           ) VERSE ON VERSE.ID_ADHERENT = ADHERENT.ID_ADHERENT      

          WHERE       

           ADHERENT.BLN_ACTIF = 1      

          AND ADHERENT.ID_AGENCE = COALESCE(@ID_AGENCE, ADHERENT.ID_AGENCE)        

          AND       
          ( ISNULL(dbo.GetMasseSalariale(ADHERENT.ID_ADHERENT, @ID_PERIODE, DEFAULT), 0) > 1        
           OR       
           YEAR(ISNULL(ADHERENT.DAT_CREATION, 0)) < YEAR( GETDATE() )
          )       

          AND ADHERENT.ID_SITUATION_ECONOMIQUE IN (SELECT ID_SITUATION_ECONOMIQUE       
                       FROM SITUATION_ECONOMIQUE       
                       WHERE COD_SITUATION_ECONOMIQUE       
                       IN       
                       (      
                       '01',   
                       '08', 
                       '09', 
                       '10', 
                       '12', 
                       '14', 
                       '15', 
                       '16' 
                       )        
                    )      

          AND ISNULL(VERSE.MNT_VERSE_HT, 0) <= 0      
          AND (       
             (@P10PlusOnly = 0) 
            OR      
           (
               ADHERENT.ID_ADHERENT IN       
             (        
              SELECT ID_ADHERENT        
              FROM R19        
              WHERE ID_PERIODE = @ID_PERIODE        
              AND  ID_ACTIVITE in (4, 10, 11)  
             )        
            )        
          )      
         END

 GO

         CREATE PROCEDURE [dbo].[INS_BORDEREAU]
         	@ID_LOT_REMISE_BANCAIRE	INT = NULL, 
         	@ID_UTILISATEUR			INT,
         	@ID_MODE_VERSEMENT		INT = 1,
         	@LISTE_ID_VERSEMENT_SELECTIONNE VARCHAR(8000) = NULL 
         AS
         BEGIN
         	DECLARE
         		@ID_BORDEREAU INT

         	IF EXISTS (SELECT 1 FROM tempdb.dbo.sysobjects(nolock)WHERE name like '%#VERSEMENTS%')
         		DROP TABLE #VERSEMENTS

         	CREATE TABLE #VERSEMENTS (ID_VERSEMENT INT)

         	IF(@LISTE_ID_VERSEMENT_SELECTIONNE IS NOT NULL)
         		BEGIN
         			INSERT INTO #VERSEMENTS
         			SELECT
         				ID_VERSEMENT 
         			FROM
         				VERSEMENT 
         				INNER JOIN dbo.F_SPLIT(@LISTE_ID_VERSEMENT_SELECTIONNE, ';') FS
         					ON FS.DATA = VERSEMENT.ID_VERSEMENT
         			WHERE
         				VERSEMENT.ID_BORDEREAU IS NULL
         				AND	VERSEMENT.BLN_PB_IMPUTATION = 0
         				AND VERSEMENT.ID_STATUT_VERSEMENT = 2
         				AND
         				(
         					BLN_ENCAISSEMENT_DIFFERE	= 0

         					OR
         					(
         						BLN_ENCAISSEMENT_DIFFERE = 1
         						AND BLN_DIFFERE_PRIS_EN_COMPTE = 1
         						AND DATEDIFF(DAY, GETDATE(), VERSEMENT.DAT_ENCAISSEMENT) <= 0
         					)
         				)
         				AND	VERSEMENT.BLN_ACTIF = 1
         				AND ID_MODE_VERSEMENT = @ID_MODE_VERSEMENT
         		END

         	ELSE
         		BEGIN
         			INSERT INTO #VERSEMENTS
         			SELECT
         				ID_VERSEMENT
         			FROM
         				dbo.GET_VERSEMENTS_POUR_BORDEREAU(@ID_UTILISATEUR,   @ID_MODE_VERSEMENT)
         		END

         	IF NOT EXISTS(SELECT 1 FROM #VERSEMENTS)
         		SELECT -1
         	ELSE
         		BEGIN
         			INSERT INTO BORDEREAU
         			(
         				ID_LOT_REMISE_BANCAIRE, 
         				COD_BORDEREAU, 
         				DAT_EDIT_BORDEREAU, 
         				DAT_COMPTA_BORDEREAU, 
         				BLN_VALIDATION, 
         				ID_UTILISATEUR
         			)
         			VALUES
         			(
         				@ID_LOT_REMISE_BANCAIRE, 
         				0, 
         				GETDATE(), 
         				NULL, 
         				0, 
         				@ID_UTILISATEUR
         			)

         			SET
         				@ID_BORDEREAU = @@IDENTITY

         			UPDATE
         				BORDEREAU
         			SET
         				COD_BORDEREAU = @ID_BORDEREAU
         			WHERE
         				ID_BORDEREAU = @ID_BORDEREAU

         			UPDATE
         				VERSEMENT	
         			SET 
         				VERSEMENT.ID_BORDEREAU			= @ID_BORDEREAU,
         				VERSEMENT.ID_STATUT_VERSEMENT	= 4,
         				SYNC_BLN_SYNCHRO_SSIS = 1,  
         				SYNC_DAT_SYNCHRO_SSIS = GETDATE() 
         			FROM
         				VERSEMENT
         				INNER JOIN #VERSEMENTS
         					ON #VERSEMENTS.ID_VERSEMENT = VERSEMENT.ID_VERSEMENT

         			SELECT @ID_BORDEREAU
         		END
         END

         CREATE PROCEDURE [dbo].[UPD_DATE_LETTRE_RELANCE_ENGAGEMENT_CONTRAT_PRO_OF]
         	@CODS_CONTRAT_PRO TEXT	
         AS

         BEGIN

         	DECLARE @Item VARCHAR(12);

         	CREATE TABLE #List(Item VARCHAR(12)  collate French_CI_AI)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,1,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)-1))),
         			@CODS_CONTRAT_PRO=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)+1,DATALENGTH(@CODS_CONTRAT_PRO))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT @Item
         	END

         	IF DATALENGTH(@CODS_CONTRAT_PRO) > 0
         		INSERT INTO #List SELECT @CODS_CONTRAT_PRO

         	SELECT  CONTRAT_PRO.ID_CONTRAT_PRO, 
         			ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_1, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_2, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_3
         	INTO	#TEMP
         	FROM	CONTRAT_PRO
         			INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         			INNER JOIN ARRIVEE_PIECE_PRO ON MODULE_PRO.ID_MODULE_PRO = ARRIVEE_PIECE_PRO.ID_MODULE_PRO
         			INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         					AND PIECE_PRO.BLN_MODULE = 1 AND PIECE_PRO.BLN_CONTRAT = 0 AND PIECE_PRO.BLN_ACTIF=1 AND PIECE_PRO.BLN_BLOQUANT_ENGAGEMENT = 1
         			CROSS JOIN PARAMETRES
         	WHERE		MODULE_PRO.BLN_OK_PIECE = 0 
         			AND MODULE_PRO.BLN_SUBROGE = 1
         			AND CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)
         			AND ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 1) AND ( EXISTS (select 1 from NR410 WHERE NR410.ID_ARRIVEE_PIECE_PRO = ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO))))

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_1 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_ENGAGE_1 IS NULL	
         						AND #TEMP.DAT_RELANCE_ENGAGE_2 IS NULL
         						AND #TEMP.DAT_RELANCE_ENGAGE_3 IS NULL 
         			)

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_2 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_ENGAGE_1 IS NOT NULL	
         						AND	#TEMP.DAT_RELANCE_ENGAGE_2 IS NULL
         						AND #TEMP.DAT_RELANCE_ENGAGE_3 IS NULL
         			)

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_3 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_ENGAGE_2 IS NOT NULL
         						AND #TEMP.DAT_RELANCE_ENGAGE_3 IS NULL
         			)

         END

         CREATE PROCEDURE [dbo].[INS_FORMATEUR_INTERNE_PEC]
          @COD_FORMATEUR_INTERNE_PEC varchar(8),
          @ID_TYPE_CONTRAT INT,
          @ID_BRANCHE int,
          @ID_CSP int,
          @ID_INDIVIDU int,
          @ID_ETABLISSEMENT int,
          @ID_MODULE_PEC int,
          @ID_CLASSIFICATION int,
          @SALAIRE_HORAIRE_CHARGE decimal(18,2),
          @NB_HEURES_HORS_TT decimal(18,2),
          @COM_FORMATEUR_INTERNE_PEC varchar(255),
          @NB_HEURE_DISPENSEE decimal(18,2),
          @SALAIRE_HORAIRE_NET decimal(18,2),
          @DATE_EMBAUCHE datetime,
          @MONTANT_BRUT_CHARGE decimal(18,2),
          @SALAIRE_HORAIRE_BRUT_CHARGE decimal(18,2),
          @ANALYTIQUE_STAGIAIRE varchar(20),
          @FONCTION varchar(100),
          @BLN_SUIVI_FORMATION_FORMATEUR tinyint,
          @BLN_SUIVI_FORMATION_TUTEUR tinyint,
          @BLN_FORME_REGULIEREMENT_INTERNE tinyint
         AS
         BEGIN
         	INSERT INTO [FORMATEUR_INTERNE_PEC]
                    ([COD_FORMATEUR_INTERNE_PEC]
                    ,[ID_TYPE_CONTRAT]
                    ,[ID_BRANCHE]
                    ,[ID_CSP]
                    ,[ID_INDIVIDU]
                    ,[ID_ETABLISSEMENT]
                    ,[ID_MODULE_PEC]
                    ,[ID_CLASSIFICATION]
                    ,[SALAIRE_HORAIRE_CHARGE]
                    ,[NB_HEURES_HORS_TT]
                    ,[COM_FORMATEUR_INTERNE_PEC]
                    ,[NB_HEURE_DISPENSEE]
                    ,[SALAIRE_HORAIRE_NET]
                    ,[DATE_EMBAUCHE]
                    ,[MONTANT_BRUT_CHARGE]
                    ,[SALAIRE_HORAIRE_BRUT_CHARGE]
                    ,[ANALYTIQUE_STAGIAIRE]
                    ,[FONCTION]
         		   ,BLN_SUIVI_FORMATION_FORMATEUR  
         		   ,BLN_SUIVI_FORMATION_TUTEUR 
         		   ,BLN_FORME_REGULIEREMENT_INTERNE)
              VALUES
                    (@COD_FORMATEUR_INTERNE_PEC,
                    @ID_TYPE_CONTRAT,
                    @ID_BRANCHE,
                    @ID_CSP, 
                    @ID_INDIVIDU, 
                    @ID_ETABLISSEMENT, 
                    @ID_MODULE_PEC,
                    @ID_CLASSIFICATION, 
                    @SALAIRE_HORAIRE_CHARGE, 
                    @NB_HEURES_HORS_TT, 
                    @COM_FORMATEUR_INTERNE_PEC,
                    @NB_HEURE_DISPENSEE, 
                    @SALAIRE_HORAIRE_NET, 
                    @DATE_EMBAUCHE, 
                    @MONTANT_BRUT_CHARGE,
                    @SALAIRE_HORAIRE_BRUT_CHARGE, 
                    @ANALYTIQUE_STAGIAIRE, 
                    @FONCTION,
         		   @BLN_SUIVI_FORMATION_FORMATEUR ,
         		   @BLN_SUIVI_FORMATION_TUTEUR,
         		   @BLN_FORME_REGULIEREMENT_INTERNE)

         		UPDATE POSTE_COUT_ENGAGE	SET BLN_OK_FINANCEMENT = 0
         		WHERE ID_MODULE_PEC = @ID_MODULE_PEC
         			AND DAT_DESENGAGEMENT IS NULL
         			AND ID_ENGAGEMENT IS NULL

            select @@IDENTITY as ID
         END

         CREATE PROCEDURE [dbo].[BATCH_PLURI_ANNUEL]
         AS
         BEGIN

         SET NOCOUNT ON;

         select	MVT_BUDGETAIRE.ID_MODULE_PEC,
         		MIN(MVT_BUDGETAIRE.DAT_MVT_BUDGETAIRE) AS eng_initial_date
         into	#PERIMETRE_MODULES
         from	MVT_BUDGETAIRE
         join	MODULE_PEC
         on		MODULE_PEC.ID_MODULE_PEC = MVT_BUDGETAIRE.ID_MODULE_PEC
         join	ACTION_PEC
         on		MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         where   MVT_BUDGETAIRE.ID_MODULE_PEC is not null
         and		coalesce(ACTION_PEC.BLN_REPRISE_ADHOC,0) = 0
         and		coalesce(ACTION_PEC.BLN_REPRISE_NESSIE,0) = 0
         and		P_E_R = 'E'
         and		ACTION_PEC.BLN_ACTIF = 1
         and		MODULE_PEC.BLN_ACTIF = 1
         group by MVT_BUDGETAIRE.ID_MODULE_PEC

         DECLARE @ID_MODULE int
         DECLARE @ID_DISPOSITIF int
         DECLARE @ID_BRANCHE int
         DECLARE @ID_ENVELOPPE int
         DECLARE @ID_COMPTE int
         DECLARE @ID_TYPE_FINANCEMENT int
         DECLARE @MNT float
         DECLARE @ID_ETABLISSEMENT int
         DECLARE @ID_COMPTE_EN int

         declare initial_cursor cursor for
         select	MVT_BUDGETAIRE.ID_DISPOSITIF, 
         		ID_BRANCHE = dbo.GetBrancheHistorise(MVT_BUDGETAIRE.ID_MODULE_PEC, MVT_BUDGETAIRE.ID_ETABLISSEMENT),
         		MVT_BUDGETAIRE.ID_ENVELOPPE, 
         		MVT_BUDGETAIRE.ID_MODULE_PEC,
         		MNT = sum(cast(MVT_BUDGETAIRE.MNT_MVT_BUDGETAIRE as decimal(18,6))),
         		MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT, 
         		MVT_BUDGETAIRE.ID_COMPTE
         from	MVT_BUDGETAIRE
         join	#PERIMETRE_MODULES
         on		MVT_BUDGETAIRE.ID_MODULE_PEC = #PERIMETRE_MODULES.ID_MODULE_PEC
         where	P_E_R = 'E'
         and		convert(varchar,MVT_BUDGETAIRE.DAT_MVT_BUDGETAIRE,112) = convert(varchar,#PERIMETRE_MODULES.eng_initial_date,112) 
         group by MVT_BUDGETAIRE.ID_DISPOSITIF, 
         		 dbo.GetBrancheHistorise(MVT_BUDGETAIRE.ID_MODULE_PEC, MVT_BUDGETAIRE.ID_ETABLISSEMENT),
         		 MVT_BUDGETAIRE.ID_ENVELOPPE, 
         		 MVT_BUDGETAIRE.ID_MODULE_PEC,
         		 MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT, 
         		 MVT_BUDGETAIRE. ID_COMPTE
         having sum(cast(MVT_BUDGETAIRE.MNT_MVT_BUDGETAIRE as decimal(18,6))) <> 0

         OPEN initial_cursor
         FETCH NEXT FROM initial_cursor
         INTO @ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @ID_MODULE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         WHILE @@FETCH_STATUS = 0
         BEGIN

         	exec INS_MVT_BUDG_PERIODE
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	@ID_COMPTE,
         	@ID_TYPE_FINANCEMENT,
         	'EIS',
         	@MNT,
         	null,	
         	null	

         	FETCH NEXT FROM initial_cursor
         	INTO @ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @ID_MODULE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         END
         CLOSE initial_cursor
         DEALLOCATE initial_cursor

         DECLARE mvt_cursor CURSOR FOR
         select	MVT_BUDGETAIRE.ID_DISPOSITIF, 
         		ID_BRANCHE = dbo.GetBrancheHistorise(MVT_BUDGETAIRE.ID_MODULE_PEC, MVT_BUDGETAIRE.ID_ETABLISSEMENT),
         		MVT_BUDGETAIRE.ID_ENVELOPPE, 
         		MVT_BUDGETAIRE.ID_MODULE_PEC,
         		MNT = sum(cast(MVT_BUDGETAIRE.MNT_MVT_BUDGETAIRE as decimal(18,6))),
         		MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT, 
         		MVT_BUDGETAIRE.ID_COMPTE
         from	MVT_BUDGETAIRE
         join	#PERIMETRE_MODULES
         on		MVT_BUDGETAIRE.ID_MODULE_PEC = #PERIMETRE_MODULES.ID_MODULE_PEC
         where	P_E_R in ('E','R')
         and		MVT_BUDGETAIRE.DAT_MVT_BUDGETAIRE >= dateadd(day, 1, convert(datetime, convert(varchar, #PERIMETRE_MODULES.eng_initial_date, 112), 112)) 
         group by MVT_BUDGETAIRE.ID_DISPOSITIF, 
         		 dbo.GetBrancheHistorise(MVT_BUDGETAIRE.ID_MODULE_PEC, MVT_BUDGETAIRE.ID_ETABLISSEMENT),
         		 MVT_BUDGETAIRE.ID_ENVELOPPE, 
         		 MVT_BUDGETAIRE.ID_MODULE_PEC,
         		 MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT, 
         		 MVT_BUDGETAIRE. ID_COMPTE
         having sum(cast(MVT_BUDGETAIRE.MNT_MVT_BUDGETAIRE as decimal(18,6))) <> 0

         OPEN mvt_cursor
         FETCH NEXT FROM mvt_cursor
         INTO @ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @ID_MODULE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         WHILE @@FETCH_STATUS = 0
         BEGIN

         	exec INS_MVT_BUDG_PERIODE
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	@ID_COMPTE,
         	@ID_TYPE_FINANCEMENT,
         	'EC',
         	@MNT,
         	null,	
         	null	

         	FETCH NEXT FROM mvt_cursor
         	INTO @ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @ID_MODULE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         END
         CLOSE mvt_cursor
         DEALLOCATE mvt_cursor

         DECLARE clotures_cursor CURSOR FOR
         select	MVT_BUDGETAIRE.ID_DISPOSITIF, 
         		ID_BRANCHE = dbo.GetBrancheHistorise(MVT_BUDGETAIRE.ID_MODULE_PEC, MVT_BUDGETAIRE.ID_ETABLISSEMENT),
         		MVT_BUDGETAIRE.ID_ENVELOPPE, 
         		MVT_BUDGETAIRE.ID_MODULE_PEC,
         		MNT = sum(cast(MVT_BUDGETAIRE.MNT_MVT_BUDGETAIRE as decimal(18,6))),
         		MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT, 
         		MVT_BUDGETAIRE.ID_COMPTE
         from	MVT_BUDGETAIRE
         join	#PERIMETRE_MODULES
         on		MVT_BUDGETAIRE.ID_MODULE_PEC = #PERIMETRE_MODULES.ID_MODULE_PEC
         join	EVENEMENT
         on		EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT
         join	TYPE_EVENEMENT
         on		TYPE_EVENEMENT.ID_TYPE_EVENEMENT = EVENEMENT.ID_TYPE_EVENEMENT
         where	P_E_R = 'E'
         and		TYPE_EVENEMENT.COD_TYPE_EVENEMENT in ('CLOTACT', 'CLOMOPEC', 'ANCLOPEC')
         group by MVT_BUDGETAIRE.ID_DISPOSITIF, 
         		 dbo.GetBrancheHistorise(MVT_BUDGETAIRE.ID_MODULE_PEC, MVT_BUDGETAIRE.ID_ETABLISSEMENT),
         		 MVT_BUDGETAIRE.ID_ENVELOPPE, 
         		 MVT_BUDGETAIRE.ID_MODULE_PEC,
         		 MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT, 
         		 MVT_BUDGETAIRE. ID_COMPTE
         having sum(cast(MVT_BUDGETAIRE.MNT_MVT_BUDGETAIRE as decimal(18,6))) <> 0

         OPEN clotures_cursor
         FETCH NEXT FROM clotures_cursor
         INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         WHILE @@FETCH_STATUS = 0
         BEGIN
         	exec INS_MVT_BUDG_PERIODE
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	@ID_COMPTE,
         	@ID_TYPE_FINANCEMENT,
         	'C',
         	@MNT,
         	null,	
         	null	

         	FETCH NEXT FROM clotures_cursor
         	INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE
         END
         CLOSE clotures_cursor
         DEALLOCATE clotures_cursor

         declare @dat_reglement datetime
         declare @id_session int

         DECLARE reglements_cursor CURSOR FOR
         select	MVT_BUDGETAIRE.ID_DISPOSITIF, 
         		ID_BRANCHE = dbo.GetBrancheHistorise(MVT_BUDGETAIRE.ID_MODULE_PEC, MVT_BUDGETAIRE.ID_ETABLISSEMENT),
         		MVT_BUDGETAIRE.ID_ENVELOPPE, 
         		MVT_BUDGETAIRE.ID_MODULE_PEC,
         		MNT = sum(cast(MVT_BUDGETAIRE.MNT_MVT_BUDGETAIRE as decimal(18,6))),
         		MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT, 
         		MVT_BUDGETAIRE.ID_COMPTE
         from	MVT_BUDGETAIRE
         join	#PERIMETRE_MODULES
         on		MVT_BUDGETAIRE.ID_MODULE_PEC = #PERIMETRE_MODULES.ID_MODULE_PEC
         where	P_E_R = 'R'
         group by MVT_BUDGETAIRE.ID_DISPOSITIF, 
         		 dbo.GetBrancheHistorise(MVT_BUDGETAIRE.ID_MODULE_PEC, MVT_BUDGETAIRE.ID_ETABLISSEMENT),
         		 MVT_BUDGETAIRE.ID_ENVELOPPE, 
         		 MVT_BUDGETAIRE.ID_MODULE_PEC,
         		 MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT, 
         		 MVT_BUDGETAIRE. ID_COMPTE
         having sum(cast(MVT_BUDGETAIRE.MNT_MVT_BUDGETAIRE as decimal(18,6))) <> 0

         OPEN reglements_cursor
         FETCH NEXT FROM reglements_cursor
         INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE, @dat_reglement, @id_session
         WHILE @@FETCH_STATUS = 0
         BEGIN
         	exec INS_MVT_BUDG_PERIODE
         	@ID_MODULE,
         	@ID_DISPOSITIF ,
         	@ID_BRANCHE,
         	@ID_ENVELOPPE,
         	@ID_COMPTE,
         	@ID_TYPE_FINANCEMENT,
         	'R',
         	@MNT,
         	@id_session, 
         	null	

         	FETCH NEXT FROM reglements_cursor
         	INTO @ID_MODULE,@ID_DISPOSITIF, @ID_BRANCHE, @ID_ENVELOPPE, @MNT, @ID_TYPE_FINANCEMENT, @ID_COMPTE, @dat_reglement, @id_session
         END
         CLOSE reglements_cursor
         DEALLOCATE reglements_cursor

         if exists (select 1 from sys.objects where name like 'enp_table')
         	drop table enp_table

         select	distinct ID_MODULE_PEC
         into	enp_table
         from	MVT_BUDGETAIRE_PERIODE
         where	ID_MODULE_PEC is not null

         and		TYPE_ENREGISTREMENT in  ('EIS', 'EC', 'EIR', 'R', 'RR', 'C')

         delete	MVT_BUDGETAIRE_PERIODE
         from	MVT_BUDGETAIRE_PERIODE
         join	enp_table 
         on		enp_table.ID_MODULE_PEC = MVT_BUDGETAIRE_PERIODE.ID_MODULE_PEC
         where	MVT_BUDGETAIRE_PERIODE.TYPE_ENREGISTREMENT = 'EN'

         insert into MVT_BUDGETAIRE_PERIODE
         (DAT_CREATION, MNT_MVT_BUDG_PERIODE, ID_PERIODE, ID_DISPOSITIF, ID_BRANCHE, ID_MODULE_PEC, ID_ENVELOPPE, ID_COMPTE, ID_TYPE_FINANCEMENT, TYPE_ENREGISTREMENT)
         select  
         		GETDATE(),
         		sum(
         			case when TYPE_ENREGISTREMENT in  ('EIS', 'EC', 'EIR', 'C') then MNT_MVT_BUDG_PERIODE
         			else -1*MNT_MVT_BUDG_PERIODE end) as MNT_EN,
         		ID_PERIODE,
         		ID_DISPOSITIF,
         		ID_BRANCHE,
         		MVT_BUDGETAIRE_PERIODE.ID_MODULE_PEC,
         		ID_ENVELOPPE,
         		ID_COMPTE, 
         		ID_TYPE_FINANCEMENT,
         		'EN'
         from	MVT_BUDGETAIRE_PERIODE
         join	enp_table 
         on		enp_table.ID_MODULE_PEC = MVT_BUDGETAIRE_PERIODE.ID_MODULE_PEC
         where	MVT_BUDGETAIRE_PERIODE.TYPE_ENREGISTREMENT in  ('EIS', 'EC', 'EIR', 'R', 'RR', 'C')

         group by 
         	MVT_BUDGETAIRE_PERIODE.ID_MODULE_PEC,
         	ID_PERIODE,
         	ID_DISPOSITIF,
         	ID_BRANCHE,
         	ID_ENVELOPPE,
         	ID_COMPTE, 
         	ID_TYPE_FINANCEMENT

         drop table enp_table

         print 'suppression des EN pour les modules et actions cl“tur‚s ou inactifs'
         update	t1
         set		t1.MNT_MVT_BUDG_PERIODE = 0
         from	MVT_BUDGETAIRE_PERIODE t1
         join	MODULE_PEC t2
         on		t1.ID_MODULE_PEC = t2.ID_MODULE_PEC
         join	ACTION_PEC t3
         on		t2.ID_ACTION_PEC = t3.ID_ACTION_PEC
         where	(t2.DAT_CLOTURE is not null or t3.DAT_CLOTURE is not null or t2.BLN_ACTIF = 0 or t3.BLN_ACTIF = 0)
         and		t1.TYPE_ENREGISTREMENT = 'EN'
         and		ABS(t1.MNT_MVT_BUDG_PERIODE) <> 0

         UPDATE	MVT_BUDGETAIRE_PERIODE
         SET		ID_DISPOSITIF =	(
         						SELECT	TYPE_ENVELOPPE.ID_DISPOSITIF 
         						FROM	ENVELOPPE 
         						JOIN	TYPE_ENVELOPPE 
         						ON		TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE=ENVELOPPE.ID_TYPE_ENVELOPPE
         						AND		ENVELOPPE.ID_ENVELOPPE =MVT_BUDGETAIRE_PERIODE.ID_ENVELOPPE
         						)
         WHERE	ID_DISPOSITIF IS NULL 
         AND		ID_COMPTE IS NULL 
         AND		ID_MODULE_PEC IS NOT NULL 
         AND		ID_ENVELOPPE IS NOT NULL

         UPDATE	MVT_BUDGETAIRE_PERIODE
         SET		ID_DISPOSITIF = 1 
         WHERE	ID_DISPOSITIF IS NULL 
         AND		ID_COMPTE IS NOT NULL 
         AND		ID_MODULE_PEC IS NOT NULL 
         AND		ID_ENVELOPPE IS NULL

         update	t1
         set		t1.ID_COMPTE = t2.id_compte
         from	MVT_BUDGETAIRE_PERIODE t1
         join	(
         		select	distinct 
         				t1.ID_MODULE_PEC,
         				id_compte=MAX(t2.id_compte)
         		from	(select distinct ID_MODULE_PEC from MVT_BUDGETAIRE_PERIODE where ID_TYPE_FINANCEMENT is not null and ID_COMPTE is null) t1
         		join	MVT_BUDGETAIRE_PERIODE t2
         		on		t1.ID_MODULE_PEC = t2.ID_MODULE_PEC
         		and		t2.ID_COMPTE is not null
         		group by t1.ID_MODULE_PEC
         		) t2
         on		t1.ID_MODULE_PEC = t2.ID_MODULE_PEC
         where	t1.ID_TYPE_FINANCEMENT is not null 
         and		t1.ID_COMPTE is null

         update	t1
         set		t1.id_activite = t2.id_activite
         from	MVT_BUDGETAIRE_PERIODE t1
         join	(
         		select	t1.ID_MODULE_PEC, min(t1.ID_ACTIVITE) as id_activite
         		from	STAGIAIRE_PEC t1
         		join	(select distinct ID_MODULE_PEC from MVT_BUDGETAIRE_PERIODE where ID_TYPE_FINANCEMENT is not null and ID_COMPTE is null) t2
         		on		t1.ID_MODULE_PEC = t2.ID_MODULE_PEC
         		where	t1.ID_SESSION_PEC is null
         		group by t1.ID_MODULE_PEC
         		) t2
         on		t1.ID_MODULE_PEC = t2.ID_MODULE_PEC
         where	t1.ID_TYPE_FINANCEMENT is not null 
         and		t1.ID_COMPTE is null

         END

 CREATE PROCEDURE [dbo].[LEC_GRP_ANNULATION_VIREMENT_PRO]
          @NUM_VIREMENT_PRO   VARCHAR(10),
          @COD_MODULE_PRO    VARCHAR(10),
          @COD_SESSION_BAP_PRO  VARCHAR(10),
          @COD_SESSION_PRO   VARCHAR(12)
         AS

         BEGIN

          IF (@COD_MODULE_PRO = '')
           BEGIN
            SET @COD_MODULE_PRO = NULL
           END

          IF (@NUM_VIREMENT_PRO = '')
           BEGIN
            SET @NUM_VIREMENT_PRO = NULL
           END

          IF (@COD_SESSION_PRO = '')
           BEGIN
            SET @COD_SESSION_PRO = NULL
           END

          IF (@COD_SESSION_BAP_PRO = '')
           BEGIN
            SET @COD_SESSION_BAP_PRO = NULL
           END

          SELECT DISTINCT
           0               AS ANNULATION,
           REGLEMENT_PRO.ID_REGLEMENT_PRO        AS ID_REGLEMENT_PRO,
           REGLEMENT_PRO.NUM_VIREMENT         AS NUM_VIREMENT,
           REGLEMENT_PRO.COD_REGLEMENT_PRO        AS COD_REGLEMENT_PRO,
              case when (SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO) 
            then SESSION_PRO.MNT_VERSE_HT_ADH
            else SESSION_PRO.MNT_VERSE_HT_OF
           end
                          AS MNT_REGLE_HT,
           case when (SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO) 
            then SESSION_PRO.MNT_VERSE_TVA_ADH 
            else SESSION_PRO.MNT_VERSE_TVA_OF
           end
                          AS MNT_REGLE_TVA,
           case when (SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO) 
            then SESSION_PRO.MNT_VERSE_HT_ADH+SESSION_PRO.MNT_VERSE_TVA_ADH        
            else SESSION_PRO.MNT_VERSE_HT_OF+SESSION_PRO.MNT_VERSE_TVA_OF
           end
                          AS MNT_REGLE_TTC,

           CONTRAT_PRO.ID_CONTRAT_PRO         AS ID_CONTRAT_PRO,
           CONTRAT_PRO.COD_CONTRAT_PRO         AS COD_CONTRAT_PRO,
           CONTRAT_PRO.LIBL_CONTRAT_PRO        AS LIBL_CONTRAT_PRO,

           MODULE_PRO.ID_MODULE_PRO         AS ID_MODULE_PRO,
           MODULE_PRO.COD_MODULE_PRO          AS COD_MODULE_PRO,
           MODULE_PRO.LIBL_MODULE_PRO          AS LIBL_MODULE_PRO,

           AGENCE.ID_AGENCE           AS ID_AGENCE,
           AGENCE.LIBC_AGENCE           AS LIBC_AGENCE,

           SESSION_PRO.ID_SESSION_PRO         AS ID_SESSION_PRO,
           SESSION_PRO.COD_SESSION_PRO         AS COD_SESSION_PRO,
           SESSION_PRO.DAT_DEBUT          AS DAT_DEBUT,
           SESSION_PRO.DAT_FIN           AS DAT_FIN,
            CASE 
            WHEN [TRANSACTION].ID_TIERS_BENEF   IS NOT NULL
             THEN TIERS.LIB_NOM  
            WHEN [TRANSACTION].ID_ETABLISSEMENT_BENEF IS NOT NULL
             THEN ADHERENT.LIB_SIGLE_ADHERENT
           END               AS BENEFICIAIRE,
           1               AS BLN_ANNULABLE

         INTO #TEMP
          FROM 
            REGLEMENT_PRO 
            INNER JOIN SESSION_PRO   ON SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO
                      AND (REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NULL) 
                      AND (REGLEMENT_PRO.DAT_EDITION IS NOT NULL)
            INNER JOIN MODULE_PRO    ON SESSION_PRO.ID_MODULE_PRO   = MODULE_PRO.ID_MODULE_PRO 
            INNER JOIN CONTRAT_PRO    ON MODULE_PRO.ID_CONTRAT_PRO   = CONTRAT_PRO.ID_CONTRAT_PRO
            INNER JOIN SESSION_BAP_PRO   ON SESSION_PRO.ID_SESSION_BAP_PRO_ADH = SESSION_BAP_PRO.ID_SESSION_BAP_PRO
            INNER JOIN AGENCE     ON AGENCE.ID_AGENCE     = REGLEMENT_PRO.ID_AGENCE
            INNER JOIN [TRANSACTION]   ON [TRANSACTION].ID_TRANSACTION  = REGLEMENT_PRO.ID_TRANSACTION
            inner JOIN ETABLISSEMENT    ON ETABLISSEMENT.ID_ETABLISSEMENT  = [TRANSACTION].ID_ETABLISSEMENT_BENEF
            inner JOIN ADHERENT     ON ADHERENT.ID_ADHERENT    = ETABLISSEMENT.ID_ADHERENT
            LEFT JOIN TIERS     ON TIERS.ID_TIERS    = [TRANSACTION].ID_TIERS_BENEF
         WHERE  (@COD_MODULE_PRO IS NULL  OR MODULE_PRO.COD_MODULE_PRO = @COD_MODULE_PRO) 
            AND (@NUM_VIREMENT_PRO IS NULL  OR REGLEMENT_PRO.NUM_VIREMENT = @NUM_VIREMENT_PRO) 
            AND (@COD_SESSION_PRO IS NULL  OR SESSION_PRO.COD_SESSION_PRO = @COD_SESSION_PRO)
            AND (@COD_SESSION_BAP_PRO IS NULL OR SESSION_BAP_PRO.COD_SESSION_BAP_PRO = @COD_SESSION_BAP_PRO)
            AND (REGLEMENT_PRO.BLN_ACTIF = 1)
            AND  SESSION_PRO.BLN_ACTIF = 1
            AND  MODULE_PRO.BLN_ACTIF = 1 
            AND REGLEMENT_PRO.NUM_VIREMENT IS NOT NULL

         union all

         SELECT DISTINCT
           0               AS ANNULATION,
           REGLEMENT_PRO.ID_REGLEMENT_PRO        AS ID_REGLEMENT_PRO,
           REGLEMENT_PRO.NUM_VIREMENT         AS NUM_VIREMENT,
           REGLEMENT_PRO.COD_REGLEMENT_PRO        AS COD_REGLEMENT_PRO,
           case when (SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO) 
            then SESSION_PRO.MNT_VERSE_HT_ADH
            else SESSION_PRO.MNT_VERSE_HT_OF
           end
                          AS MNT_REGLE_HT,
           case when (SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO) 
            then SESSION_PRO.MNT_VERSE_TVA_ADH 
            else SESSION_PRO.MNT_VERSE_TVA_OF
           end
                          AS MNT_REGLE_TVA,
           case when (SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO) 
            then SESSION_PRO.MNT_VERSE_HT_ADH+SESSION_PRO.MNT_VERSE_TVA_ADH        
            else SESSION_PRO.MNT_VERSE_HT_OF+SESSION_PRO.MNT_VERSE_TVA_OF
           end
                          AS MNT_REGLE_TTC,

           CONTRAT_PRO.ID_CONTRAT_PRO         AS ID_CONTRAT_PRO,
           CONTRAT_PRO.COD_CONTRAT_PRO         AS COD_CONTRAT_PRO,
           CONTRAT_PRO.LIBL_CONTRAT_PRO        AS LIBL_CONTRAT_PRO,

           MODULE_PRO.ID_MODULE_PRO         AS ID_MODULE_PRO,
           MODULE_PRO.COD_MODULE_PRO          AS COD_MODULE_PRO,
           MODULE_PRO.LIBL_MODULE_PRO          AS LIBL_MODULE_PRO,

           AGENCE.ID_AGENCE           AS ID_AGENCE,
           AGENCE.LIBC_AGENCE           AS LIBC_AGENCE,

           SESSION_PRO.ID_SESSION_PRO         AS ID_SESSION_PRO,
           SESSION_PRO.COD_SESSION_PRO         AS COD_SESSION_PRO,
           SESSION_PRO.DAT_DEBUT          AS DAT_DEBUT,
           SESSION_PRO.DAT_FIN           AS DAT_FIN,
           CASE 
            WHEN [TRANSACTION].ID_TIERS_BENEF   IS NOT NULL
             THEN TIERS.LIB_NOM  
            WHEN [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF IS NOT NULL
             THEN ORGANISME_FORMATION.LIB_SIGLE_OF
           END          AS BENEFICIAIRE,
           1          AS BLN_ANNULABLE

          FROM 
            REGLEMENT_PRO 
            INNER JOIN SESSION_PRO   ON SESSION_PRO.ID_REGLEMENT_PRO_OF = REGLEMENT_PRO.ID_REGLEMENT_PRO
                      AND (REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NULL) 
                      AND (REGLEMENT_PRO.DAT_EDITION IS NOT NULL)
            INNER JOIN MODULE_PRO    ON SESSION_PRO.ID_MODULE_PRO   = MODULE_PRO.ID_MODULE_PRO 
            INNER JOIN CONTRAT_PRO    ON MODULE_PRO.ID_CONTRAT_PRO   = CONTRAT_PRO.ID_CONTRAT_PRO
            INNER JOIN SESSION_BAP_PRO   ON SESSION_PRO.ID_SESSION_BAP_PRO_OF = SESSION_BAP_PRO.ID_SESSION_BAP_PRO
            INNER JOIN AGENCE     ON AGENCE.ID_AGENCE     = REGLEMENT_PRO.ID_AGENCE
            INNER JOIN [TRANSACTION]   ON [TRANSACTION].ID_TRANSACTION  = REGLEMENT_PRO.ID_TRANSACTION
            INNER JOIN ETABLISSEMENT_OF   ON ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF= [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF
            INNER JOIN ORGANISME_FORMATION  ON ORGANISME_FORMATION.ID_OF   = ETABLISSEMENT_OF.ID_OF
            left JOIN TIERS     ON TIERS.ID_TIERS    = [TRANSACTION].ID_TIERS_BENEF
         WHERE (@COD_MODULE_PRO IS NULL  OR MODULE_PRO.COD_MODULE_PRO = @COD_MODULE_PRO) 
            AND (@NUM_VIREMENT_PRO IS NULL  OR REGLEMENT_PRO.NUM_VIREMENT = @NUM_VIREMENT_PRO) 
            AND (@COD_SESSION_PRO IS NULL  OR SESSION_PRO.COD_SESSION_PRO = @COD_SESSION_PRO)
            AND (@COD_SESSION_BAP_PRO IS NULL OR SESSION_BAP_PRO.COD_SESSION_BAP_PRO = @COD_SESSION_BAP_PRO)
            AND (REGLEMENT_PRO.BLN_ACTIF = 1)
            AND  SESSION_PRO.BLN_ACTIF = 1
            AND  MODULE_PRO.BLN_ACTIF = 1
            AND REGLEMENT_PRO.NUM_VIREMENT IS NOT NULL

         union all

         SELECT DISTINCT
           0               AS ANNULATION,
           REGLEMENT_PRO.ID_REGLEMENT_PRO        AS ID_REGLEMENT_PRO,
           REGLEMENT_PRO.NUM_VIREMENT         AS NUM_VIREMENT,
           REGLEMENT_PRO.COD_REGLEMENT_PRO        AS COD_REGLEMENT_PRO,
           case when (SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO) 
            then SESSION_PRO.MNT_VERSE_HT_ADH
            else SESSION_PRO.MNT_VERSE_HT_OF
           end
                          AS MNT_REGLE_HT,
           case when (SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO) 
            then SESSION_PRO.MNT_VERSE_TVA_ADH 
            else SESSION_PRO.MNT_VERSE_TVA_OF
           end
                          AS MNT_REGLE_TVA,
           case when (SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO) 
            then SESSION_PRO.MNT_VERSE_HT_ADH+SESSION_PRO.MNT_VERSE_TVA_ADH        
            else SESSION_PRO.MNT_VERSE_HT_OF+SESSION_PRO.MNT_VERSE_TVA_OF
           end
                          AS MNT_REGLE_TTC,
           CONTRAT_PRO.ID_CONTRAT_PRO         AS ID_CONTRAT_PRO,
           CONTRAT_PRO.COD_CONTRAT_PRO         AS COD_CONTRAT_PRO,
           CONTRAT_PRO.LIBL_CONTRAT_PRO        AS LIBL_CONTRAT_PRO,

           MODULE_PRO.ID_MODULE_PRO         AS ID_MODULE_PRO,
           MODULE_PRO.COD_MODULE_PRO          AS COD_MODULE_PRO,
           MODULE_PRO.LIBL_MODULE_PRO          AS LIBL_MODULE_PRO,

           AGENCE.ID_AGENCE           AS ID_AGENCE,
           AGENCE.LIBC_AGENCE           AS LIBC_AGENCE,

           SESSION_PRO.ID_SESSION_PRO         AS ID_SESSION_PRO,
           SESSION_PRO.COD_SESSION_PRO         AS COD_SESSION_PRO,
           SESSION_PRO.DAT_DEBUT          AS DAT_DEBUT,
           SESSION_PRO.DAT_FIN           AS DAT_FIN,
           TIERS.LIB_NOM            AS BENEFICIAIRE,
           case when (exists(select ID_REGLEMENT_PRO from  REGLEMENT_PRO  AS REGLT_AUTRE 
                where (REGLT_AUTRE.ID_REGLEMENT_PRO=SESSION_PRO.ID_REGLEMENT_PRO_OF OR
              REGLT_AUTRE.ID_REGLEMENT_PRO=SESSION_PRO.ID_REGLEMENT_PRO_ADH) 
              AND REGLEMENT_PRO.ID_REGLEMENT_PRO<>REGLT_AUTRE.ID_REGLEMENT_PRO
              AND REGLT_AUTRE.DAT_VALID_REGLEMENT IS NOT NULL)
              ) 
           then 0 else 1
           end               AS BLN_ANNULABLE

          FROM 
            REGLEMENT_PRO 
            INNER JOIN SESSION_PRO   ON (SESSION_PRO.ID_REGLEMENT_PRO_OF = REGLEMENT_PRO.ID_REGLEMENT_PRO or SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO.ID_REGLEMENT_PRO)
                      AND (REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NULL) 
                      AND (REGLEMENT_PRO.DAT_EDITION IS NOT NULL)
            INNER JOIN MODULE_PRO    ON SESSION_PRO.ID_MODULE_PRO   = MODULE_PRO.ID_MODULE_PRO 
            INNER JOIN CONTRAT_PRO    ON MODULE_PRO.ID_CONTRAT_PRO   = CONTRAT_PRO.ID_CONTRAT_PRO
            INNER JOIN SESSION_BAP_PRO   ON (SESSION_PRO.ID_SESSION_BAP_PRO_OF = SESSION_BAP_PRO.ID_SESSION_BAP_PRO or SESSION_PRO.ID_SESSION_BAP_PRO_ADH = SESSION_BAP_PRO.ID_SESSION_BAP_PRO)
            INNER JOIN AGENCE     ON AGENCE.ID_AGENCE     = REGLEMENT_PRO.ID_AGENCE
            INNER JOIN [TRANSACTION]   ON [TRANSACTION].ID_TRANSACTION  = REGLEMENT_PRO.ID_TRANSACTION
            LEFT JOIN REGLEMENT_PRO  REGLT_AUTREOF ON REGLT_AUTREOF.id_REGLEMENT_PRO=SESSION_PRO.ID_REGLEMENT_PRO_OF
            LEFT JOIN REGLEMENT_PRO  REGLT_AUTREADH ON REGLT_AUTREADH.id_REGLEMENT_PRO=SESSION_PRO.ID_REGLEMENT_PRO_ADH
            left JOIN TIERS     ON TIERS.ID_TIERS    = [TRANSACTION].ID_TIERS_BENEF
         WHERE (@COD_MODULE_PRO IS NULL  OR MODULE_PRO.COD_MODULE_PRO = @COD_MODULE_PRO) 
            AND (@NUM_VIREMENT_PRO IS NULL  OR REGLEMENT_PRO.NUM_VIREMENT = @NUM_VIREMENT_PRO) 
            AND (@COD_SESSION_PRO IS NULL  OR SESSION_PRO.COD_SESSION_PRO = @COD_SESSION_PRO)
            AND (@COD_SESSION_BAP_PRO IS NULL OR SESSION_BAP_PRO.COD_SESSION_BAP_PRO = @COD_SESSION_BAP_PRO)
            AND (REGLEMENT_PRO.BLN_ACTIF = 1)
            AND  SESSION_PRO.BLN_ACTIF = 1
            AND  MODULE_PRO.BLN_ACTIF = 1
            AND REGLEMENT_PRO.NUM_VIREMENT IS NOT NULL
            and TIERS.LIB_NOM  is not null

         SELECT DISTINCT * FROM #TEMP ORDER BY NUM_VIREMENT ,COD_SESSION_PRO

         END

         CREATE PROCEDURE [INS_OFFRE_SERVICE]
         			@ID_FREQUENCE_DIFFUSION_AUTOMATIQUE ID,
         			@ID_CONTACT ID,
         			@ID_UTILISATEUR ID,
         			@ID_GROUPE ID,
         			@ID_GESTION_REMUNERATION ID,
         			@DAT_ENVOI_AUTO DATETIME
         AS  
         BEGIN
         DECLARE	@DAT_MODIF DATETIME
         DECLARE @ID_OFFRE_SERVICE ID
         SET @DAT_MODIF = GETDATE()
         INSERT INTO [OFFRE_SERVICE]
                    ([DAT_MODIF]
                    ,[ID_FREQUENCE_DIFFUSION_AUTOMATIQUE]
                    ,[ID_CONTACT]
                    ,[ID_UTILISATEUR]
                    ,[ID_GROUPE]
                    ,[ID_GESTION_REMUNERATION]
                    ,[DAT_CREATION]
                    ,[DAT_ENVOI_AUTO])
         VALUES (@DAT_MODIF
                ,@ID_FREQUENCE_DIFFUSION_AUTOMATIQUE
                ,@ID_CONTACT
                ,@ID_UTILISATEUR
                ,@ID_GROUPE
                ,@ID_GESTION_REMUNERATION
                ,@DAT_MODIF
                ,@DAT_ENVOI_AUTO)
         SET @ID_OFFRE_SERVICE = @@IDENTITY

         SELECT @ID_OFFRE_SERVICE AS ID_OFFRE_SERVICE
         END

         CREATE PROCEDURE [dbo].[UPD_DATE_LETTRE_RELANCE_REGLEMENT_CONTRAT_PRO_ADH]
         	@CODS_CONTRAT_PRO TEXT	
         AS

         BEGIN

         	DECLARE @Item VARCHAR(12);

         	CREATE TABLE #List(Item VARCHAR(12)  collate French_CI_AI)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,1,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)-1))),
         			@CODS_CONTRAT_PRO=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)+1,DATALENGTH(@CODS_CONTRAT_PRO))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT @Item
         	END

         	IF DATALENGTH(@CODS_CONTRAT_PRO) > 0
         		INSERT INTO #List SELECT @CODS_CONTRAT_PRO

         	SELECT      CONTRAT_PRO.ID_CONTRAT_PRO, 
         				ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO, 
         				ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_1, 
         				ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_2, 
         				ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_3
         	INTO		#TEMP
         	FROM        CONTRAT_PRO 
         				INNER JOIN ARRIVEE_PIECE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = ARRIVEE_PIECE_PRO.ID_CONTRAT_PRO
         				INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         					AND PIECE_PRO.BLN_CONTRAT = 1 AND PIECE_PRO.BLN_ACTIF=1 AND (PIECE_PRO.BLN_BLOQUANT_REGLEMENT = 1 or PIECE_PRO.BLN_BLOQUANT_ENGAGEMENT = 1)
         	WHERE			CONTRAT_PRO.BLN_OK_PIECE = 0
         				AND CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)
         				AND ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 1) AND ( EXISTS (select 1 from NR410 WHERE NR410.ID_ARRIVEE_PIECE_PRO = ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO))))
         				AND CONTRAT_PRO.BLN_ACTIF=1

         	UNION

         	SELECT  CONTRAT_PRO.ID_CONTRAT_PRO, 
         			ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_1, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_2, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_3
         	FROM	CONTRAT_PRO
         			INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         			INNER JOIN ARRIVEE_PIECE_PRO ON MODULE_PRO.ID_MODULE_PRO = ARRIVEE_PIECE_PRO.ID_MODULE_PRO
         			INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         				AND MODULE_PRO.BLN_ACTIF=1 	AND PIECE_PRO.BLN_MODULE = 1 AND PIECE_PRO.BLN_CONTRAT = 0 AND PIECE_PRO.BLN_ACTIF=1 AND (PIECE_PRO.BLN_BLOQUANT_REGLEMENT = 1 or PIECE_PRO.BLN_BLOQUANT_ENGAGEMENT = 1)
         			CROSS JOIN PARAMETRES
         	WHERE		MODULE_PRO.BLN_OK_PIECE = 0 
         			AND MODULE_PRO.BLN_SUBROGE != 1
         			AND CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)
         			AND ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 1) AND ( EXISTS (select 1 from NR410 WHERE NR410.ID_ARRIVEE_PIECE_PRO = ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO))))

         UNION

         	SELECT  CONTRAT_PRO.ID_CONTRAT_PRO, 
         			ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_1, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_2, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_3
         	FROM	CONTRAT_PRO
         			INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         			INNER JOIN SESSION_PRO ON SESSION_PRO.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO
         			INNER JOIN ARRIVEE_PIECE_PRO ON SESSION_PRO.ID_SESSION_PRO = ARRIVEE_PIECE_PRO.ID_SESSION_PRO
         			INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         			AND MODULE_PRO.BLN_ACTIF=1 AND SESSION_PRO.BLN_ACTIF=1	AND PIECE_PRO.BLN_SESSION = 1 	AND PIECE_PRO.BLN_MODULE = 0 AND PIECE_PRO.BLN_CONTRAT = 0 AND PIECE_PRO.BLN_ACTIF=1 AND (PIECE_PRO.BLN_BLOQUANT_REGLEMENT = 1 or PIECE_PRO.BLN_BLOQUANT_ENGAGEMENT = 1)
         			CROSS JOIN PARAMETRES
         	WHERE		SESSION_PRO.BLN_OK_PIECE = 0 
         			AND MODULE_PRO.BLN_SUBROGE != 1
         			AND CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)
         			AND ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 1) AND ( EXISTS (select 1 from NR410 WHERE NR410.ID_ARRIVEE_PIECE_PRO = ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO))))

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_1 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_REGLE_1 IS NULL	
         						AND #TEMP.DAT_RELANCE_REGLE_2 IS NULL
         						AND #TEMP.DAT_RELANCE_REGLE_3 IS NULL 
         			)

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_2 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_REGLE_1 IS NOT NULL	
         						AND	#TEMP.DAT_RELANCE_REGLE_2 IS NULL
         						AND #TEMP.DAT_RELANCE_REGLE_3 IS NULL
         			)

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_3 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_REGLE_2 IS NOT NULL
         						AND #TEMP.DAT_RELANCE_REGLE_3 IS NULL
         			)

         END

 CREATE PROCEDURE [dbo].[LEC_GRP_STAGIAIRE_MODULE_PEC_POSTE_COUT]
          @ID_MODULE int,
          @ID_POSTE_COUT_ENGAGE int
         AS

         BEGIN

          DECLARE @MNT_ENGAGE_HT decimal(18,2)
          DECLARE @NUM_DUREE_HEURE int
          DECLARE @SUM_HEURE_STAGIAIRE decimal(18,2)
          DECLARE @ID_SOUS_TYPE_COUT INT

          SELECT @NUM_DUREE_HEURE = NUM_DUREE_HEURE 
          FROM MODULE_PEC WHERE ID_MODULE_PEC = @ID_MODULE

          IF (@NUM_DUREE_HEURE IS NULL)
          BEGIN
           SET @NUM_DUREE_HEURE = 1
          END

          SELECT @MNT_ENGAGE_HT = MNT_ENGAGE_HT, @ID_SOUS_TYPE_COUT = ID_SOUS_TYPE_COUT 
          FROM POSTE_COUT_ENGAGE 
          WHERE ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE

          IF (@MNT_ENGAGE_HT IS NULL)
          BEGIN
           SET @MNT_ENGAGE_HT = 0
          END

          SELECT @SUM_HEURE_STAGIAIRE = SUM(NB_HEURE_ENGAGE)
          FROM STAGIAIRE_PEC
          WHERE ID_MODULE_PEC = @ID_MODULE
          AND ID_SESSION_PEC is NULL

          SELECT  DISTINCT(UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE),
          STAGIAIRE_PEC.ID_STAGIAIRE_PEC,
          ADHERENT.LIB_RAISON_SOCIALE AS ADHERENT,
          INDIVIDU.NOM_INDIVIDU + ' ' + INDIVIDU.PRENOM_INDIVIDU AS STAGIAIRE,
          DISPOSITIF.LIBL_DISPOSITIF AS DISPOSITIF,
          UNITE_STAGIAIRE.NB_HEURE_ENGAGE AS NB_HEURE,
          UNITE_STAGIAIRE.NB_HEURE_REGLE As NB_HEURE_REGLE,
          UNITE_STAGIAIRE.NB_HEURE_REM NB_HEURE_REM,
          (@MNT_ENGAGE_HT * UNITE_STAGIAIRE.NB_HEURE_ENGAGE) / @SUM_HEURE_STAGIAIRE AS CALCULATE_TOTAL,
          0 AS TOTAL,
          @ID_SOUS_TYPE_COUT AS ID_SOUS_TYPE_COUT,
          STAGIAIRE_PEC.NB_HEURE_ENGAGE AS NB_HEURE_TOTAL,
          STAGIAIRE_PEC.NB_HEURE_REGLE AS NB_HEURE_REGLE_TOTAL,
          STAGIAIRE_PEC.ID_ETABLISSEMENT,
          ETABLISSEMENT.ID_ADHERENT,
          DISPOSITIF.ID_DISPOSITIF,
          coalesce(GROUPE.LIB_SIGLE_GROUPE, left(GROUPE.LIB_GROUPE,15)) AS LIB_SIGLE_GROUPE
          FROM UNITE_STAGIAIRE
           INNER JOIN STAGIAIRE_PEC ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
           INNER JOIN DISPOSITIF ON DISPOSITIF.ID_DISPOSITIF = UNITE_STAGIAIRE.ID_DISPOSITIF
           LEFT JOIN  INDIVIDU ON INDIVIDU.ID_INDIVIDU = STAGIAIRE_PEC.ID_INDIVIDU
           LEFT JOIN ETABLISSEMENT ON ETABLISSEMENT.ID_ETABLISSEMENT = STAGIAIRE_PEC.ID_ETABLISSEMENT
           LEFT JOIN ADHERENT ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
           LEFT JOIN GROUPE ON GROUPE.ID_GROUPE = STAGIAIRE_PEC.ID_GROUPE
          WHERE
           STAGIAIRE_PEC.ID_MODULE_PEC = @ID_MODULE
           AND UNITE_STAGIAIRE.BLN_REFUS = 0
           AND ID_SESSION_PEC is NULL
           AND UNITE_STAGIAIRE.NB_HEURE_ENGAGE > 0
         END

CREATE PROCEDURE [dbo].[LEC_DET_MODULE_PEC] 
          @ID_MODULE int
         AS

         BEGIN

          DECLARE @DESENGAGEMENT INT
          DECLARE @DESENGAGEMENT_VERSION_ECRAN INT

          SELECT @DESENGAGEMENT = COUNT(*) 
          from MODULE_PEC
           LEFT JOIN POSTE_COUT_ENGAGE ON POSTE_COUT_ENGAGE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
           LEFT JOIN ENGAGEMENT ON ENGAGEMENT.ID_ENGAGEMENT = POSTE_COUT_ENGAGE.ID_ENGAGEMENT
          WHERE ID_TYPE_ENGAGEMENT <> 2   AND
            DAT_DESENGAGEMENT is null  AND
            ENGAGEMENT.DAT_BAE is not null AND
            MODULE_PEC.ID_MODULE_PEC = @ID_MODULE

          IF @DESENGAGEMENT > 0
          BEGIN
           SET @DESENGAGEMENT = 1
          END

          select
           @DESENGAGEMENT_VERSION_ECRAN = count(*) 
          from
           POSTE_COUT_ENGAGE 
            inner join ENGAGEMENT on POSTE_COUT_ENGAGE.ID_ENGAGEMENT = ENGAGEMENT.ID_ENGAGEMENT
          where
           POSTE_COUT_ENGAGE.ID_MODULE_PEC = @ID_MODULE
            AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL
            AND POSTE_COUT_ENGAGE.ID_ENGAGEMENT IS NOT NULL
            AND ENGAGEMENT.ID_TYPE_ENGAGEMENT <> 2

          select 
           MODULE_PEC.ID_MODULE_PEC,
           @DESENGAGEMENT AS DESENGAGEMENT,
           @DESENGAGEMENT_VERSION_ECRAN as DESENGAGEMENT_VERSION_ECRAN,
           ACTION_PEC.ID_ACTION_PEC,
           ACTION_PEC.COD_ACTION_PEC AS COD_ACTION, 
           ACTION_PEC.LIBL_ACTION_PEC AS LIBELLE_ACTION, 
           dbo.GetActionPECCode(ACTION_PEC.COD_ACTION_PEC,ACTION_PEC.ANNEE_ACTION_PEC) AS ACTION_CODE_11,
           MODULE_PEC.BLN_EXTERNE AS INTERNE_EXTERNE, 
           MODULE_PEC.DAT_CREATION AS CREE_LE,
           MODULE_PEC.DAT_MODIF AS MODIFIE_LE,
           UTILISATEUR.ID_UTILISATEUR, 
           UTILISATEUR.COD_UTIL AS PAR,
           UCREATEUR.COD_UTIL AS CREATEUR, 
           MODULE_PEC.BLN_ACTIF AS ACTIF,
           MODULE_PEC.COD_MODULE_PEC AS NUM_MODULE, 
           MODULE_PEC.LIBL_MODULE_PEC AS LIBELLE_MODULE,
           MODULE_PEC.ID_CRITERE_CHIFFRAGE,
           MODULE_PEC.ID_ETABLISSEMENT_OF AS ID_ETABLISSEMENT_OF,
           ETABLISSEMENT_OF.ID_OF,
           MODULE_PEC.ID_THEME AS ID_THEME,
           MODULE_PEC.ID_STAGE AS ID_STAGE,
           MODULE_PEC.ID_FORMACODE AS ID_FORMACODE,
           MODULE_PEC.DAT_DEBUT, 
           MODULE_PEC.DAT_FIN, 
           MODULE_PEC.NUM_DUREE_JOUR, 
           MODULE_PEC.NUM_DUREE_HEURE, 
           MODULE_PEC.MNT_CONVENTION, 
           MODULE_PEC.NUM_INTERNE,
           MODULE_PEC.ID_DEPARTEMENT,
           PERIODE.ID_PERIODE,
           PERIODE.NUM_ANNEE, 
           MODULE_PEC.BLN_IMPUTABLE,
           isnull(MODULE_PEC.COM_MODULE_PEC, '') as COM_MODULE_PEC,
           ACTION_PEC.CIBLE_ACTION AS CIBLE_ACTION,
           MODULE_PEC.TIME_STAMP,
           MODULE_PEC.BLN_INTRA as BLN_INTRA,
           MODULE_PEC.AXE_MODULE as AXE_MODULE,
           MODULE_PEC.DOMAINE_MODULE as DOMAINE_MODULE,
           MODULE_PEC.ID_MODALITE_FORMATION as ID_MODALITE_FORMATION,
           MODULE_PEC.BLN_DELEGATION_PAIEMENT as BLN_DELEGATION_PAIEMENT,
           ISNULL((SELECT SUM(MODULE_PEC.NUM_DUREE_HEURE)
              FROM MODULE_PEC
              WHERE MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
               AND MODULE_PEC.BLN_ACTIF = 1
               GROUP BY MODULE_PEC.ID_ACTION_PEC),0)
           AS TOTAL_DUREE_UTILISE,
           ID_DISPOSITIF_PAR_DEFAUT, 
           (SELECT COUNT(*) + 1
           FROM SESSION_PEC
           WHERE ID_MODULE_PEC=@ID_MODULE) AS NEXT_SESSION_NUMBER,
           case when MODULE_PEC.DAT_CLOTURE is null then 0 else 1 end as BLN_CLOTURE,
           MODULE_PEC.BLN_CATALOGUE,
           MODULE_PEC.ID_MODALITE_ENSEIGNEMENT,
           MODULE_PEC.CODE_CPF   
          from MODULE_PEC 
            INNER JOIN ACTION_PEC ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
            LEFT JOIN PERIODE ON PERIODE.ID_PERIODE = MODULE_PEC.ID_PERIODE
            LEFT JOIN ETABLISSEMENT_OF ON ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = MODULE_PEC.ID_ETABLISSEMENT_OF
            LEFT JOIN UTILISATEUR ON UTILISATEUR.ID_UTILISATEUR = MODULE_PEC.ID_UTILISATEUR
            LEFT JOIN UTILISATEUR UCREATEUR ON UCREATEUR.ID_UTILISATEUR = MODULE_PEC.ID_UTILISATEUR_CREATEUR

          WHERE MODULE_PEC.ID_MODULE_PEC = @ID_MODULE

         END

         CREATE PROCEDURE [dbo].[INS_COMMENTAIRE_WITH_OUTPUT] 
         	@TYPE_OBJET int,	
         	@ID_OBJET int,
         	@COMMENTAIRE varchar(7600),
         	@ID_UTILISATEUR int,
         	@RESULT_COMMENTAIRE int OUTPUT
         AS
         BEGIN
         	SET NOCOUNT ON;

         	INSERT INTO COMMENTAIRES
                    ([TYPE_OBJET]
                    ,[ID_OBJET]
                    ,[DATE]
                    ,[COMMENTAIRE]
                    ,[ID_UTILISATEUR])
              VALUES
                    (@TYPE_OBJET
                    ,@ID_OBJET
                    ,GetDate()
                    ,@COMMENTAIRE
                    ,@ID_UTILISATEUR)

         	SET @RESULT_COMMENTAIRE = @@IDENTITY

         END

         CREATE PROCEDURE [dbo].[UPD_DATE_LETTRE_RELANCE_REGLEMENT_CONTRAT_PRO_OF]
         	@CODS_CONTRAT_PRO TEXT	
         AS

         BEGIN

         	DECLARE @Item VARCHAR(12);

         	CREATE TABLE #List(Item VARCHAR(12)  collate French_CI_AI)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,1,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)-1))),
         			@CODS_CONTRAT_PRO=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)+1,DATALENGTH(@CODS_CONTRAT_PRO))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT @Item
         	END

         	IF DATALENGTH(@CODS_CONTRAT_PRO) > 0
         		INSERT INTO #List SELECT @CODS_CONTRAT_PRO

         	SELECT  CONTRAT_PRO.ID_CONTRAT_PRO, 
         			ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_1, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_2, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_3
         	INTO	#TEMP
         	FROM	CONTRAT_PRO
         			INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         			INNER JOIN ARRIVEE_PIECE_PRO ON MODULE_PRO.ID_MODULE_PRO = ARRIVEE_PIECE_PRO.ID_MODULE_PRO
         			INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         				AND MODULE_PRO.BLN_ACTIF=1	AND PIECE_PRO.BLN_MODULE = 1 AND PIECE_PRO.BLN_CONTRAT = 0 AND PIECE_PRO.BLN_ACTIF=1 AND (PIECE_PRO.BLN_BLOQUANT_REGLEMENT = 1 or PIECE_PRO.BLN_BLOQUANT_ENGAGEMENT = 1 )
         			CROSS JOIN PARAMETRES
         	WHERE		MODULE_PRO.BLN_OK_PIECE = 0 
         			AND MODULE_PRO.BLN_SUBROGE = 1
         			AND CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)
         			AND ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 1) AND ( EXISTS (select 1 from NR410 WHERE NR410.ID_ARRIVEE_PIECE_PRO = ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO))))
         			AND CONTRAT_PRO.BLN_ACTIF=1

         union 

         SELECT  CONTRAT_PRO.ID_CONTRAT_PRO, 
         			ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_1, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_2, 
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_3
         	FROM	CONTRAT_PRO
         			INNER JOIN MODULE_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         			INNER JOIN SESSION_PRO ON SESSION_PRO.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO
         			INNER JOIN ARRIVEE_PIECE_PRO ON SESSION_PRO.ID_SESSION_PRO = ARRIVEE_PIECE_PRO.ID_SESSION_PRO
         			INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         				AND MODULE_PRO.BLN_ACTIF=1 AND SESSION_PRO.BLN_ACTIF=1 AND PIECE_PRO.BLN_SESSION = 1	AND PIECE_PRO.BLN_MODULE = 0 AND PIECE_PRO.BLN_CONTRAT = 0 AND PIECE_PRO.BLN_ACTIF=1 AND (PIECE_PRO.BLN_BLOQUANT_REGLEMENT = 1 or PIECE_PRO.BLN_BLOQUANT_ENGAGEMENT = 1 )
         			CROSS JOIN PARAMETRES
         	WHERE		SESSION_PRO.BLN_OK_PIECE = 0 
         			AND MODULE_PRO.BLN_SUBROGE = 1
         			AND CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)
         			AND ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PRO.BLN_ACTIF = 1) AND ( EXISTS (select 1 from NR410 WHERE NR410.ID_ARRIVEE_PIECE_PRO = ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO))))

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_1 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_REGLE_1 IS NULL	
         						AND #TEMP.DAT_RELANCE_REGLE_2 IS NULL
         						AND #TEMP.DAT_RELANCE_REGLE_3 IS NULL 
         			)

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_2 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_REGLE_1 IS NOT NULL	
         						AND	#TEMP.DAT_RELANCE_REGLE_2 IS NULL
         						AND #TEMP.DAT_RELANCE_REGLE_3 IS NULL
         			)

         	UPDATE	ARRIVEE_PIECE_PRO
         	SET		ARRIVEE_PIECE_PRO.DAT_RELANCE_REGLE_3 = GETDATE()
         	WHERE	ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO	IN 
         			(	
         				SELECT	#TEMP.ID_ARRIVEE_PIECE_PRO
         				FROM	#TEMP
         				WHERE		#TEMP.DAT_RELANCE_REGLE_2 IS NOT NULL
         						AND #TEMP.DAT_RELANCE_REGLE_3 IS NULL
         			)

         END

 CREATE PROCEDURE [dbo].[UPD_ANNULATION_VIREMENT_PRO]
          @ID_REGLEMENT_PRO INT,
          @ID_SESSION_PRO INT
         AS

         BEGIN

          DECLARE @ID_REGLEMENT_PRO_OF AS INT,
            @ID_REGLEMENT_PRO_ADH AS INT,
            @DAT_VALID_REGLEMENT AS DATETIME

          SET @ID_REGLEMENT_PRO_OF = NULL
          SET @ID_REGLEMENT_PRO_ADH = NULL
          SET @DAT_VALID_REGLEMENT = NULL

          SELECT @DAT_VALID_REGLEMENT = DAT_VALID_REGLEMENT 
          FROM REGLEMENT_PRO 
          WHERE ID_REGLEMENT_PRO = @ID_REGLEMENT_PRO

          IF(@DAT_VALID_REGLEMENT IS NULL)
          BEGIN

           SELECT @ID_REGLEMENT_PRO_OF = ID_REGLEMENT_PRO_OF FROM SESSION_PRO WHERE ID_SESSION_PRO = @ID_SESSION_PRO AND
                              ID_REGLEMENT_PRO_OF IS NOT NULL
           SELECT @ID_REGLEMENT_PRO_ADH = ID_REGLEMENT_PRO_ADH FROM SESSION_PRO WHERE ID_SESSION_PRO = @ID_SESSION_PRO AND 
                              ID_REGLEMENT_PRO_ADH IS NOT NULL

           update REGLEMENT_PRO SET BLN_ACTIF=0 
           WHERE ID_REGLEMENT_PRO=@ID_REGLEMENT_PRO 
             and bln_actif=1

           if (@ID_REGLEMENT_PRO_OF = @ID_REGLEMENT_PRO) 
            update SESSION_PRO 
            SET 
              ID_REGLEMENT_PRO_OF = NULL,
              BLN_A_CONTROLER = 1,
              ID_TYPE_VALIDATION=4,
              DAT_BAP_OF = NULL,
              ID_SESSION_BAP_PRO_OF = NULL 
            WHERE ID_SESSION_PRO = @ID_SESSION_PRO

           if (@ID_REGLEMENT_PRO_ADH = @ID_REGLEMENT_PRO ) 
            update SESSION_PRO 
            SET 
              ID_REGLEMENT_PRO_ADH = NULL ,
              BLN_A_CONTROLER = 1,
              ID_TYPE_VALIDATION=4,
              DAT_BAP_ADH = NULL,
              ID_SESSION_BAP_PRO_ADH = NULL
            WHERE ID_SESSION_PRO = @ID_SESSION_PRO
          END 
         END 

         CREATE PROCEDURE INS_ROLE_CONTACT
         	@ID_CONTACT int,
         	@ID_ROLE int
         AS
         BEGIN
         	SET NOCOUNT ON;
         	if not exists(select 1 from CONTACT_ROLE where ID_CONTACT = @ID_CONTACT	and ID_ROLE = @ID_ROLE)
         		insert into CONTACT_ROLE (ID_CONTACT, ID_ROLE)
         		values (@ID_CONTACT, @ID_ROLE)
         END

         CREATE PROCEDURE [dbo].[UPD_DATE_LETTRE_ENGAGEMENT_ADH]
         	@IDS_ACTION_PEC TEXT	
         AS

         BEGIN

         	DECLARE @Item int;

         	CREATE TABLE #List(Item int)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@IDS_ACTION_PEC,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@IDS_ACTION_PEC,1,CHARINDEX(@Delimiter,@IDS_ACTION_PEC,0)-1))),
         			@IDS_ACTION_PEC=RTRIM(LTRIM(SUBSTRING(@IDS_ACTION_PEC,CHARINDEX(@Delimiter,@IDS_ACTION_PEC,0)+1,DATALENGTH(@IDS_ACTION_PEC))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT cast(@Item as int)
         	END

         	IF DATALENGTH(@IDS_ACTION_PEC) > 0
         		begin
         			select @Item = cast(@IDS_ACTION_PEC as varchar(50))
         			INSERT INTO #List SELECT @Item 
         		end

         	SELECT DISTINCT ENGAGEMENT.ID_ENGAGEMENT
         	INTO			#TEMP_ENGAMEMENT
         	FROM			ACTION_PEC 
         					INNER JOIN MODULE_PEC ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC 
         					INNER JOIN ENGAGEMENT ON ACTION_PEC.ID_ACTION_PEC = ENGAGEMENT.ID_ACTION_PEC 
         	WHERE			(ENGAGEMENT.DAT_BAE IS NOT NULL)
         					AND (ENGAGEMENT.ID_TYPE_ENGAGEMENT <> 2)
         					AND (ENGAGEMENT.DAT_LETTRE_PEC_ENGAGEMENT_ADH  IS NULL)
         					AND (ACTION_PEC.ID_ACTION_PEC IN (SELECT Item FROM #List))

         	UPDATE ENGAGEMENT
         	SET	DAT_LETTRE_PEC_ENGAGEMENT_ADH= GETDATE()
         	WHERE	ID_ENGAGEMENT IN (SELECT ID_ENGAGEMENT FROM #TEMP_ENGAMEMENT)

         END

         CREATE PROCEDURE [dbo].[UPD_NR31]  
         	@ID_ETABLISSEMENT	INTEGER,
         	@ID_CONTACT			INTEGER,
         	@ID_FONCTION		INTEGER,
         	@NUM_PORT			VARCHAR(10),
         	@NUM_TEL			VARCHAR(10),
         	@NUM_FAX			VARCHAR(10),
         	@BLN_PRINCIPAL		TINYINT,
         	@BLN_ACTIF			TINYINT,
         	@EMAIL_PRO			VARCHAR(100),
         	@ID_ADRESSE			INTEGER,
         	@LIB_TITRE			VARCHAR(100),
         	@EMAIL_PERS			VARCHAR(100),
         	@TIME_STAMP			timestamp  
         AS  
         	UPDATE NR31
         	SET  
         		ID_ETABLISSEMENT = @ID_ETABLISSEMENT,  
         		ID_CONTACT		= @ID_CONTACT,  
         		ID_FONCTION		= @ID_FONCTION,  
         		NUM_PORT		= @NUM_PORT,  
         		NUM_TEL			= @NUM_TEL,  
         		NUM_FAX			= @NUM_FAX,  
         		BLN_PRINCIPAL	= @BLN_PRINCIPAL,
         		BLN_ACTIF		= @BLN_ACTIF,
         		EMAIL_PRO		= @EMAIL_PRO,
         		ID_ADRESSE		= @ID_ADRESSE,
         		LIB_TITRE		= @LIB_TITRE,
         		EMAIL_PERS		= @EMAIL_PERS

         	FROM NR31

         	WHERE
         			ID_ETABLISSEMENT	= @ID_ETABLISSEMENT  
         		AND ID_CONTACT			= @ID_CONTACT
         		AND TIME_STAMP			= @TIME_STAMP  

         	IF @@ROWCOUNT = 0   
         		BEGIN  
         		   IF EXISTS(SELECT * FROM NR31 WHERE	ID_ETABLISSEMENT	= @ID_ETABLISSEMENT AND 
         												ID_CONTACT			= @ID_CONTACT		AND 
         												TIME_STAMP			!= @TIME_STAMP)      
         		   BEGIN  

         		      RAISERROR(50001, 16, 1)  
         		   END  
         		   ELSE  
         		   BEGIN  

         		      RAISERROR(50002, 16, 1)  
         		   END  
         		END       

         	IF (@BLN_ACTIF = 0 and  EXISTS (select * from [TRANSACTION]  where ID_ETABLISSEMENT_BENEF = @ID_ETABLISSEMENT 
         				AND ID_CONTACT = @ID_CONTACT AND BLN_ACTIF = 1 and BLN_TRANSACTION_REGLEMENT_PRINCIPAL = 0))
         				BEGIN
         				UPDATE [TRANSACTION]
         				SET BLN_ACTIF = 0
         				WHERE 
         				ID_ETABLISSEMENT_BENEF = @ID_ETABLISSEMENT 
         				AND ID_CONTACT = @ID_CONTACT AND BLN_ACTIF = 1
         				and BLN_TRANSACTION_REGLEMENT_PRINCIPAL = 0
         				END
         	IF (EXISTS (
         				select [TRANSACTION].ID_ADRESSE, [TRANSACTION].ID_CONTACT, [TRANSACTION].ID_ADRESSE
         				from [TRANSACTION]
         				where 
         					[TRANSACTION].ID_ETABLISSEMENT_DEST	=  @ID_ETABLISSEMENT	AND
         					[TRANSACTION].ID_CONTACT			=  @ID_CONTACT			AND
         					[TRANSACTION].ID_ADRESSE			<> @ID_ADRESSE
         				) )
         	BEGIN
         		UPDATE [TRANSACTION] set [TRANSACTION].ID_ADRESSE = @ID_ADRESSE
         		where 
         			[TRANSACTION].ID_ETABLISSEMENT_DEST	=  @ID_ETABLISSEMENT	AND
         			[TRANSACTION].ID_CONTACT			=  @ID_CONTACT
         		print 'transaction ok'
         	END

GO

 CREATE PROCEDURE [dbo].[BATCH_GENERATION_MVT_BUDGETAIRE_TRANSFERT]
          @ID_EVENEMENT INT
         AS

         BEGIN
          SET NOCOUNT ON;

          with transferts as
          (  
          SELECT
           TRANSFERT.ID_COMPTE,
           TRANSFERT.ID_ENVELOPPE,
           MNT_MVT = ISNULL(TRANSFERT.MNT_REEL, TRANSFERT.MNT_REEL_OUVRIER),
           TRANSFERT.BLN_COMPTE_VERS_ENVELOPPE ,

           COMPTE.ID_PERIODE,
           COMPTE.ID_ACTIVITE ,
           LIBL = CASE WHEN LEN(LTRIM(TRANSFERT.COM_TRANSFERT))>0 AND COD_TYPE_EVENEMENT != 'TRANSFER' THEN TRANSFERT.COM_TRANSFERT ELSE  TRANSFERT.LIBL_TRANSFERT END,
           EVENEMENT.ID_EVENEMENT, 
           COMPTE.ID_GROUPE,
           EVENEMENT.DAT_EVENEMENT,
           COMPTE.ID_TYPE_FINANCEMENT,

           ID_ETABLISSEMENT = TRANSFERT.ID_ETABLISSEMENT, 
           ETABLISSEMENT.ID_ADHERENT
          FROM    EVENEMENT
            INNER JOIN TRANSFERT  on (TRANSFERT.ID_TRANSFERT  = EVENEMENT.ID_TRANSFERT) 
            INNER JOIN COMPTE   on (COMPTE.ID_COMPTE   = TRANSFERT.ID_COMPTE)
            INNER JOIN TYPE_EVENEMENT on (EVENEMENT.ID_TYPE_EVENEMENT = TYPE_EVENEMENT.ID_TYPE_EVENEMENT)

            INNER JOIN ETABLISSEMENT on (ETABLISSEMENT.ID_ETABLISSEMENT= TRANSFERT.ID_ETABLISSEMENT)     
          WHERE ID_EVENEMENT NOT IN (SELECT ID_EVENEMENT FROM MVT_BUDGETAIRE) 
          AND   ID_EVENEMENT = ISNULL(@ID_EVENEMENT, ID_EVENEMENT)
          )

          
          INSERT INTO MVT_BUDGETAIRE
          (
           ID_ACTIVITE,
           ID_PERIODE_FISC,
           ID_PERIODE_CPT,
           ID_ETABLISSEMENT,
           ID_ADHERENT,
           P_E_R,
           ID_GROUPE,
           ID_TYPE_FINANCEMENT,
           N_R,
           DAT_MVT_BUDGETAIRE,
           ID_EVENEMENT,
           ID_TYPE_MOUVEMENT,
           MNT_MVT_BUDGETAIRE,
           ID_ENVELOPPE,
           ID_COMPTE,
           LIBL_MVT_BUDGETAIRE
          )
          SELECT
           transferts.ID_ACTIVITE,
           transferts.ID_PERIODE,
           transferts.ID_PERIODE,
           transferts.ID_ETABLISSEMENT,
           transferts.ID_ADHERENT,
           'R', 
           transferts.ID_GROUPE,
           transferts.ID_TYPE_FINANCEMENT,
           'N',
           transferts.DAT_EVENEMENT,
           transferts.ID_EVENEMENT,
           4,  
           case transferts.BLN_COMPTE_VERS_ENVELOPPE when 0 then transferts.MNT_MVT else -1 *transferts.MNT_MVT end, 
           null, 
           transferts.ID_COMPTE,
           transferts.LIBL
          FROM transferts

          UNION ALL
          
          SELECT
           transferts.ID_ACTIVITE,
           transferts.ID_PERIODE,
           transferts.ID_PERIODE,
           transferts.ID_ETABLISSEMENT,
           transferts.ID_ADHERENT,
           'R', 
           transferts.ID_GROUPE,
           null, 
           'N',
           transferts.DAT_EVENEMENT,
           transferts.ID_EVENEMENT,
           5,  
           case transferts.BLN_COMPTE_VERS_ENVELOPPE when 1 then transferts.MNT_MVT else -1 * transferts.MNT_MVT end, 
           transferts.ID_ENVELOPPE, 
           null, 
           transferts.LIBL
          FROM transferts

         END

         CREATE PROCEDURE [dbo].[EDT_LETTRE_REFUS_ADH_CONTRAT_PRO]
         		@ID_ETABLISSEMENT int,
         		@ID_BENEFICIAIRE int,
         		@TYPE_BENEFICIAIRE int,
         		@ID_ADRESSE int,
         		@ID_CONTACT int,
         		@COD_CONTRAT_PRO varchar(10)
         AS

         BEGIN

         	IF @ID_CONTACT IS NULL
         	SELECT @ID_CONTACT = NR31.ID_CONTACT
         	FROM NR31
         		INNER JOIN CONTACT ON CONTACT.ID_CONTACT = NR31.ID_CONTACT
         	WHERE BLN_PRINCIPAL = 1
         		AND BLN_ACTIF = 1
         		AND ID_ETABLISSEMENT = @ID_BENEFICIAIRE
         		AND CONTACT.LIB_NOM_CONTACT <> '.';

         	SELECT	ADHERENT.COD_ADHERENT,
         			ETABLISSEMENT.NUM_SIRET,
         			CM.LIB_NOM			as LIB_NOM_CONSEILLER,
         			CM.LIB_PNM			as LIB_PNM_CONSEILLER,
         			CR.ID_UTILISATEUR	as ID_UTIL,
         			CR.LIB_PNM			as LIB_PRENOM_CHARGE_RELATION,
         			CR.LIB_NOM			as LIB_NOM_CHARGE_RELATION,
         			CR.LIB_VILLE,
         			CR.EMAIL			as EMAIL_CHARGE_RELATION
         	INTO	#TEMP_REFERENCE
         	FROM	ADHERENT
         	JOIN	ETABLISSEMENT	
         	ON		ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         	JOIN	ADRESSE
         	ON		ETABLISSEMENT.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE
         	left join
         			utilisateur CR 
         	on		ETABLISSEMENT.id_chargee_relation = CR.id_utilisateur
         	left join
         			utilisateur CM 
         	on		ETABLISSEMENT.id_chargee_mission = CM.id_utilisateur
         	WHERE	ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT;

         	SELECT	CONTRAT_PRO.ID_CONTRAT_PRO,
         			CONTRAT_PRO.COD_CONTRAT_PRO,
         			CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_DEB_CONTRAT, 3) AS DAT_DEBUT,
         			CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_FIN_CONTRAT, 3) AS DAT_FIN,
         			INDIVIDU.NOM_INDIVIDU, 
         			INDIVIDU.PRENOM_INDIVIDU,
         			INDIVIDU.ID_INDIVIDU,
         			INDIVIDU.ID_NATIONALITE,
         			CONTRAT_PRO.LIB_LIEU_FORMATION,
         			CONTRAT_PRO.ID_SALARIE_PRO
         	INTO	#TEMP_CONTRAT_PRO
         	FROM	CONTRAT_PRO
         	JOIN	SALARIE_PRO ON CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO 
         	JOIN	INDIVIDU ON SALARIE_PRO.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU
         	WHERE   CONTRAT_PRO.COD_CONTRAT_PRO = @COD_CONTRAT_PRO;

         	WITH XMLNAMESPACES (
         		DEFAULT 'EDT_LETTRE_REFUS_ADH_CONTRAT_PRO'
         	)

         	SELECT

         			dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) as BENEFICIAIRE,

         			(

         				SELECT	              	

         					isnull(COD_ADHERENT, '')				as COD_ADH,
         					isnull(LIB_PRENOM_CHARGE_RELATION, '')	as LIB_PRENOM_CHARGE_RELATION,
         					isnull(LIB_NOM_CHARGE_RELATION, '')		as LIB_NOM_CHARGE_RELATION,
         					isnull(EMAIL_CHARGE_RELATION, '')		as EMAIL,

         					dbo.GetXmlAdrUtilAvecTel(ENTETE.ID_UTIL,0,1) as EMETTEUR,

         					(
         						SELECT	#TEMP_CONTRAT_PRO.COD_CONTRAT_PRO
         						FROM	#TEMP_CONTRAT_PRO
         						FOR XML PATH(''), TYPE 
         					),
         					ENTETE.NUM_SIRET AS SIRET,
         					rtrim(case when patindex('%CEDEX%', LIB_VILLE) <> 0 then left(LIB_VILLE, patindex('%CEDEX%', LIB_VILLE)-1) else LIB_VILLE end) as LIB_VILLE, 
         					dbo.GetFullDate(getDate()) as DATE,				
         					(
         						SELECT	top 1
         						dbo.GetContactSalutation(@ID_CONTACT, 1)
         						FROM CIVILITE
         						FOR XML PATH('POLITESSE_HAUT'), TYPE
         					)
         				FROM #TEMP_REFERENCE AS ENTETE     
         				FOR XML AUTO, ELEMENTS, TYPE
         			),

         			(

         				SELECT	CORPS.COD_CONTRAT_PRO,
         						CORPS.PRENOM_INDIVIDU,
         						CORPS.NOM_INDIVIDU,
         						CORPS.DAT_DEBUT,
         						CORPS.DAT_FIN,
         						(
         							SELECT	MOTIF_REFUS_PRO.LIBL_MOTIF_REFUS_PRO
         							FROM	#TEMP_CONTRAT_PRO
         							JOIN	MOTIF_REFUS_CONTRAT 
         							ON		#TEMP_CONTRAT_PRO.ID_CONTRAT_PRO = MOTIF_REFUS_CONTRAT.ID_CONTRAT_PRO 
         							JOIN	MOTIF_REFUS_PRO 
         							ON		MOTIF_REFUS_CONTRAT.ID_MOTIF_REFUS_PRO = MOTIF_REFUS_PRO.ID_MOTIF_REFUS_PRO
         							FOR XML RAW('MOTIF_REFUS'), ELEMENTS, TYPE, ROOT('MOTIFS')
         						),
         						(
         							SELECT	top 1
         							dbo.GetContactSalutation(@ID_CONTACT, 1)
         							FROM CIVILITE
         							FOR XML PATH('POLITESSE_BAS'), TYPE
         						)

         					FROM	#TEMP_CONTRAT_PRO AS CORPS
         					FOR XML AUTO, ELEMENTS, TYPE
         			),

         			(

         				SELECT	SIGNATURE.LIB_PNM_CONSEILLER,
         						SIGNATURE.LIB_NOM_CONSEILLER
         				FROM
         						#TEMP_REFERENCE as SIGNATURE
         				FOR XML AUTO, ELEMENTS, TYPE
         			)

         	FROM	#TEMP_CONTRAT_PRO AS LETTRE
         	FOR XML AUTO, ELEMENTS

         END

         CREATE PROCEDURE [dbo].[INS_SALARIE]
         	@ID_SALARIE INT OUTPUT,
         	@BLN_ACTIF  tinyint=1,
         	@ID_UTILISATEUR INT,
         	@ID_INDIVIDU INT,
         	@ID_ETABLISSEMENT INT,
         	@NUM_DUREE_MENSUELLE_TRAVAIL decimal(18,2),
         	@MATRICULE_SALARIE  [varchar](20),
         	@SALAIRE_HORAIRE_CHARGE decimal(18,2),
         	@SALAIRE_HORAIRE_BRUT_CHARGE  decimal(18,2),
         	@SALAIRE_HORAIRE_NET decimal(18,2),
         	@MONTANT_BRUT_CHARGE decimal(18,2),
         	@BLN_TEMPS_PARTIEL  tinyint,
         	@ID_TYPE_CONTRAT INT,
         	@ID_CSP  INT,
         	@ID_CLASSIFICATION  INT,
         	@ID_STATUT  INT,
         	@ID_CODE_INSEE  INT,
         	@ID_FAMILLE_PROFESSIONNELLE  INT,
         	@DATE_EMBAUCHE  datetime,
         	@ID_NIVEAU_AVENANT  INT,
         	@ANALYTIQUE_STAGIAIRE  varchar(20),
         	@CENTRE_COUT  varchar(50),
         	@DATE_SORTIE datetime ,
         	@FONCTION  varchar(120),
         	@BLN_SALARIE_REFERENCE  tinyint,
         	@ID_ADRESSE INT

         AS
         BEGIN
         DECLARE @ID_SALARIE_LOCAL INT 

         	INSERT INTO SALARIE 
         	(
         		COD_SALARIE,
         		DAT_CREATION,
         		ID_UTILISATEUR ,
         		ID_INDIVIDU ,
         		ID_ETABLISSEMENT,
         		NUM_DUREE_MENSUELLE_TRAVAIL,
         		MATRICULE_SALARIE,
         		SALAIRE_HORAIRE_CHARGE,
         		SALAIRE_HORAIRE_BRUT_CHARGE,
         		SALAIRE_HORAIRE_NET,
         		MONTANT_BRUT_CHARGE ,
         		BLN_TEMPS_PARTIEL,
         		ID_TYPE_CONTRAT ,
         		ID_CSP,
         		ID_CLASSIFICATION,
         		ID_STATUT,
         		ID_CODE_INSEE,
         		ID_FAMILLE_PROFESSIONNELLE,
         		DATE_EMBAUCHE,
         		ID_NIVEAU_AVENANT,
         		ANALYTIQUE_STAGIAIRE,
         		CENTRE_COUT ,
         		DATE_SORTIE,
         		FONCTION,
         		BLN_SALARIE_REFERENCE ,
         		BLN_ACTIF,
         		ID_ADRESSE
         	)
         	values
         	(
         		'0',
         		GETDATE(),
         		@ID_UTILISATEUR ,
         		@ID_INDIVIDU ,
         		@ID_ETABLISSEMENT ,
         		@NUM_DUREE_MENSUELLE_TRAVAIL ,
         		@MATRICULE_SALARIE ,
         		@SALAIRE_HORAIRE_CHARGE ,
         		@SALAIRE_HORAIRE_BRUT_CHARGE  ,
         		@SALAIRE_HORAIRE_NET ,
         		@MONTANT_BRUT_CHARGE ,
         		@BLN_TEMPS_PARTIEL  ,
         		@ID_TYPE_CONTRAT ,
         		@ID_CSP  ,
         		@ID_CLASSIFICATION  ,
         		@ID_STATUT  ,
         		@ID_CODE_INSEE  ,
         		@ID_FAMILLE_PROFESSIONNELLE  ,
         		@DATE_EMBAUCHE  ,
         		@ID_NIVEAU_AVENANT  ,
         		@ANALYTIQUE_STAGIAIRE ,
         		@CENTRE_COUT ,
         		@DATE_SORTIE  ,
         		@FONCTION ,
         		@BLN_SALARIE_REFERENCE ,
         		@BLN_ACTIF  ,
         		@ID_ADRESSE
         	)

         	SET @ID_SALARIE		= @@IDENTITY

         	UPDATE SALARIE SET COD_SALARIE = @ID_SALARIE WHERE ID_SALARIE = @ID_SALARIE

         	UPDATE SALARIE
         	SET BLN_SALARIE_REFERENCE = 0	
         	WHERE ID_INDIVIDU = @ID_INDIVIDU AND ID_SALARIE <> @ID_SALARIE AND BLN_SALARIE_REFERENCE <> 0

         	return @@IDENTITY	
         END

         CREATE PROCEDURE [dbo].[LEC_GRP_ADHERENT]
         	@ID_ADHERENT		int,
         	@COD_ADHERENT		int,
         	@NUM_SIREN			varchar(9),
         	@LIB_RAISON_SOCIALE varchar(50),
         	@COD_POSTAL			varchar(5),
         	@LIB_VILLE 			varchar(40),
         	@NUM_TEL 			varchar(10),
         	@NUM_FAX 			varchar(10),
         	@LIB_NOM_CONTACT	varchar(35),
         	@EMAIL				varchar(100),
         	@LIB_GROUPE 		varchar(35),
         	@BLN_ACTIF 			tinyint = 0,
         	@ID_AGENCE 			int,
         	@ID_ACTION_PEC		int = NULL,
         	@ID_ADHERENT_MIN	int = NULL,
         	@ID_ADHERENT_MAX	int = NULL,
         	@ID_NAF				int,
         	@ID_IDCC			int,
         	@ID_BRANCHE			int,
         	@ID_CHARGEE_MISSION		int,
         	@ID_CHARGEE_RELATION	int,
         	@ID_SYNDICAT			int,
         	@ID_GROUPE_STAT			int,
         	@BLN_CREATION			bit = 0 
         AS
         BEGIN
         	IF (@BLN_ACTIF IS NULL)
         	BEGIN
         		SET @BLN_ACTIF = 0
         	END

         SELECT 
         		ADHERENT.ID_ADHERENT,
         		ADHERENT.ID_AGENCE,
         		ADHERENT.COD_ADHERENT,
         		ADHERENT.LIB_RAISON_SOCIALE,
         		ADHERENT.NUM_SIREN,
         		ADHERENT.EMAIL,
         		ADHERENT.BLN_ACTIF, 
         		ADHERENT.LIB_SIGLE_ADHERENT,
         		ADHERENT.ID_TYPE_TVA,
         		ADRESSE.LIB_ADR,
         		ADRESSE.NUM_TEL,
         		ADRESSE.NUM_FAX,
         		ADRESSE.LIB_CP_CEDEX AS COD_POSTAL,
         		ADRESSE.LIB_VIL_CEDEX AS LIB_VILLE, 
         		CONTACT.LIB_NOM_CONTACT,
         		AGENCE.LIBL_AGENCE,
         		AGENCE.LIBC_AGENCE,
         		ADHERENT.ID_CHARGEE_MISSION,
         		ADHERENT.ID_SYNDICAT,
         		ADHERENT.ID_ETABLISSEMENT_PRINCIPAL
         FROM
         		ADHERENT 
         		left join ADRESSE		ON ADRESSE.ID_ADRESSE = ADHERENT.ID_ADRESSE_PRINCIPALE 
                 INNER JOIN AGENCE		ON ADHERENT.ID_AGENCE = AGENCE.ID_AGENCE
         		LEFT JOIN	NR31		ON	ADHERENT.ID_ETABLISSEMENT_PRINCIPAL = NR31.ID_ETABLISSEMENT
         									AND coalesce(NR31.BLN_PRINCIPAL, 1) = 1
         		LEFT OUTER JOIN CONTACT ON CONTACT.ID_CONTACT = NR31.ID_CONTACT
                 LEFT OUTER JOIN GROUPE	ON ADHERENT.ID_GROUPE = GROUPE.ID_GROUPE
         WHERE
         	(@ID_ADHERENT IS NULL		OR ADHERENT.ID_ADHERENT		= @ID_ADHERENT)
         	AND	(@COD_ADHERENT IS NULL	OR ADHERENT.COD_ADHERENT	= @COD_ADHERENT)
         	AND	(@NUM_SIREN IS NULL		OR ADHERENT.NUM_SIREN		LIKE @NUM_SIREN)
         	AND	(@LIB_RAISON_SOCIALE IS NULL OR ADHERENT.LIB_RAISON_SOCIALE LIKE @LIB_RAISON_SOCIALE)
         	AND	(@COD_POSTAL IS NULL	OR ADRESSE.LIB_CP_CEDEX		LIKE @COD_POSTAL)
         	AND	(@LIB_VILLE IS NULL		OR ADRESSE.LIB_VIL_CEDEX	LIKE @LIB_VILLE)
         	AND	(@NUM_TEL IS NULL		OR ADRESSE.NUM_TEL			LIKE @NUM_TEL)
         	AND	(@NUM_FAX IS NULL		OR ADRESSE.NUM_FAX			LIKE @NUM_FAX)
         	AND	(@LIB_NOM_CONTACT IS NULL OR CONTACT.LIB_NOM_CONTACT LIKE @LIB_NOM_CONTACT)
         	AND	(@EMAIL IS NULL			OR ADHERENT.EMAIL		LIKE @EMAIL)
         	AND	(@LIB_GROUPE IS NULL	OR GROUPE.LIB_GROUPE	LIKE @LIB_GROUPE)
         	AND (@ID_AGENCE IS NULL		OR ADHERENT.ID_AGENCE	= @ID_AGENCE) 
         	AND (@ID_NAF IS NULL		OR ADHERENT.ID_NAF	= @ID_NAF) 
         	AND (@ID_IDCC IS NULL		OR ADHERENT.ID_IDCC	= @ID_IDCC) 
         	AND (@ID_BRANCHE IS NULL	OR ADHERENT.ID_BRANCHE	= @ID_BRANCHE) 
         	AND	(ADHERENT.BLN_ACTIF >= @BLN_ACTIF)
         	AND (@ID_ACTION_PEC IS NULL		OR ID_ADHERENT IN (
         				SELECT ID_ADHERENT FROM NR140 INNER jOIN ETABLISSEMENT
         				ON  ETABLISSEMENT.ID_ETABLISSEMENT = NR140.ID_ETABLISSEMENT
         				WHERE NR140.ID_ACTION_PEC = @ID_ACTION_PEC
         				))
         	AND (
         			(@ID_ADHERENT_MIN IS NULL OR @ID_ADHERENT_MAX IS NULL) OR
         			(ADHERENT.ID_ADHERENT between @ID_ADHERENT_MIN AND @ID_ADHERENT_MAX)
         		)
         	AND (@ID_CHARGEE_MISSION IS NULL	OR ADHERENT.ID_CHARGEE_MISSION	= @ID_CHARGEE_MISSION)
         	AND (@ID_CHARGEE_RELATION IS NULL	OR ADHERENT.ID_CHARGEE_RELATION	= @ID_CHARGEE_RELATION)
         	AND (@ID_SYNDICAT IS NULL			OR ADHERENT.ID_SYNDICAT	= @ID_SYNDICAT)
         	AND (@ID_GROUPE_STAT IS NULL		OR ADHERENT.ID_GROUPE_STATISTIQUE = @ID_GROUPE_STAT)
         	AND ISNULL(ADHERENT.BLN_CREATION, 0) <= @BLN_CREATION
         order by ID_ADHERENT

         END

 CREATE PROCEDURE [dbo].[INS_TRANSFERT]

          @LIBL_TRANSFERT     VARCHAR(50),    
          @BLN_COMPTE_VERS_ENVELOPPE  TINYINT,    
          @ID_GROUPE      INT,  
          @ID_ENVELOPPE     INT,  
          @DAT_TRANSFERT     DATETIME,  
          @MNT_TRANSFERT     DECIMAL(18,2),  
          @ID_TYPE_FINANCEMENT   INT,   
          @ID_UTILISATEUR     INT,  
          @ID_PERIODE      INT,  
          @COM_TRANSFERT     VARCHAR(255),  
          @LIBL_MVT_BUDGETAIRE   VARCHAR(60),  
          @ID_TYPE_EVENEMENT    INT = 4,
          @ID_ETABLISSEMENT    INT = null 
         AS
         BEGIN  
          DECLARE
          @ID_TRANSFERT  INT,
          @ID_EVENEMENT  INT,
          @MNT_BUDGETAIRE  DECIMAL(18,2),
          @RETORNO   INT,
          @ID_COMPTE   INT,
          @NUM_ANNEE   SMALLINT,
          @COD_TYPE_EVENEMENT VARCHAR(8),
          @ID_ACTIVITE_ADH INT,
          @DAT    DATETIME  

          SELECT @COD_TYPE_EVENEMENT = COD_TYPE_EVENEMENT from TYPE_EVENEMENT where ID_TYPE_EVENEMENT = @ID_TYPE_EVENEMENT      

          IF @ID_PERIODE IS NULL  
          BEGIN  
           SELECT @ID_PERIODE = ID_PERIODE   
           FROM PERIODE   
           WHERE BLN_EN_COURS = 1   
           AND  ID_TYPE_PERIODE = 1  
          END  

          IF @ID_ETABLISSEMENT IS NULL
          BEGIN
           SELECT @ID_ETABLISSEMENT = dbo.GetEtabRepresentatifDuGroupe(@ID_GROUPE)  
          END 

          IF @COD_TYPE_EVENEMENT = 'TRAN_PRE'  OR @COD_TYPE_EVENEMENT = 'TRAN_DOT'

          BEGIN
           SET @ID_ACTIVITE_ADH = 2  
          END
          ELSE 
          BEGIN
           SET @ID_ACTIVITE_ADH = NULL  

           SELECT  TOP 1 @ID_ACTIVITE_ADH = ID_ACTIVITE  
           FROM  ETABLISSEMENT   
           INNER JOIN R19  on R19.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT  
           WHERE  ID_ETABLISSEMENT = @ID_ETABLISSEMENT   
           AND   ID_PERIODE = @ID_PERIODE  
           AND   ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 
           ORDER BY ID_ACTIVITE ASC  

          END

          SET @ID_COMPTE = NULL  

          EXEC @ID_COMPTE = LEC_COMPTE  
          @ID_GROUPE = @ID_GROUPE,  
          @ID_PERIODE_FISC = @ID_PERIODE,  
          @ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT,  
          @ID_ACTIVITE = @ID_ACTIVITE_ADH,  
          @COM_COMPTE = 'Cr‚ation par Transfert'  

          IF isnull(@ID_COMPTE,0) > 0  
          BEGIN

           INSERT INTO TRANSFERT  
           (
           COD_TRANSFERT,    
           LIBL_TRANSFERT,    
           BLN_ACTIF,    
           BLN_COMPTE_VERS_ENVELOPPE,  
           DAT_CREATION,  
           DAT_TRANSFERT,  
           ID_UTILISATEUR,    
           ID_ENVELOPPE,    
           ID_COMPTE,  
           MNT_REEL,  
           MNT_REEL_OUVRIER,  
           COM_TRANSFERT ,
           ID_ETABLISSEMENT
           )  
           VALUES    
           (  
           0,    
           @LIBL_TRANSFERT,    
           1,  
           @BLN_COMPTE_VERS_ENVELOPPE,    
           GETDATE(),    
           @DAT_TRANSFERT,  
           @ID_UTILISATEUR,  
           @ID_ENVELOPPE,  
           @ID_COMPTE,    
           @MNT_TRANSFERT,    
           NULL, 
           @COM_TRANSFERT ,
           @ID_ETABLISSEMENT
           ) 

           update TRANSFERT  
           set  COD_TRANSFERT =  SCOPE_IDENTITY()  
           where ID_TRANSFERT =  SCOPE_IDENTITY()  

           set @ID_TRANSFERT =  SCOPE_IDENTITY()  

           SET @DAT = @DAT_TRANSFERT

           EXEC @ID_EVENEMENT = INS_EVENEMENT  
             @LIBL_TRANSFERT, 
             @ID_TYPE_EVENEMENT, 
             null, 
             @ID_TRANSFERT, 
             null, 
             null, 
             null          

           IF (@COD_TYPE_EVENEMENT = 'TRANSFER')   
           BEGIN 
            EXEC dbo.BATCH_GENERATION_MVT_BUDGETAIRE_TRANSFERT @ID_EVENEMENT
           END

          END 

          RETURN isnull(@ID_TRANSFERT,0)  
         END

         CREATE PROCEDURE [dbo].[UPD_DATE_LETTRE_RELANCE_INSTRUCTION_ADH]
         	@IDS_ACTION_PEC TEXT	
         AS

         BEGIN

         	DECLARE @Item int;

         	CREATE TABLE #List(Item int)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@IDS_ACTION_PEC,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@IDS_ACTION_PEC,1,CHARINDEX(@Delimiter,@IDS_ACTION_PEC,0)-1))),
         			@IDS_ACTION_PEC=RTRIM(LTRIM(SUBSTRING(@IDS_ACTION_PEC,CHARINDEX(@Delimiter,@IDS_ACTION_PEC,0)+1,DATALENGTH(@IDS_ACTION_PEC))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT cast(@Item as int)
         	END

         	IF DATALENGTH(@IDS_ACTION_PEC) > 0
         		begin
         			select @Item = cast(@IDS_ACTION_PEC as varchar(50))
         			INSERT INTO #List SELECT @Item 
         		end

         		SELECT	DISTINCT ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC
         		INTO	#TEMP_RELANCE_1
         		FROM    ARRIVEE_PIECE_PEC
         				INNER JOIN PIECE_PEC ON ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC 
         				INNER JOIN MODULE_PEC ON ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC 
         				INNER JOIN ACTION_PEC ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC

         		WHERE		(ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 IS NULL) 
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NULL) 
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL)
         				AND (ACTION_PEC.ID_ACTION_PEC  in (SELECT Item FROM #List))
         				AND (MODULE_PEC.BLN_OK_PIECE = 0)
         				AND (PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1)
         				AND (MODULE_PEC.BLN_ACTIF = 1)
         				AND (PIECE_PEC.BLN_ACTIF=1)

         				AND	(
         					(PIECE_PEC.BLN_ADHERENT =1)				
         					OR
         					(										
         						(PIECE_PEC.BLN_MODULE = 1)
         						AND (PIECE_PEC.BLN_ADHERENT = 0)
         						AND (PIECE_PEC.BLN_STAGIAIRE = 0)
         					)
         					)

         				AND ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 1) AND (EXISTS (select * from NR210 WHERE NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC))))
         		UNION

         		SELECT	DISTINCT ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC
         		FROM    ARRIVEE_PIECE_PEC
         				INNER JOIN PIECE_PEC ON ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC  
         				INNER JOIN STAGIAIRE_PEC ON ARRIVEE_PIECE_PEC.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC 
         				INNER JOIN ETABLISSEMENT ON STAGIAIRE_PEC.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT
         				INNER JOIN MODULE_PEC ON ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC 
         				INNER JOIN ACTION_PEC ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC

         		WHERE		(ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 IS NULL) 
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NULL) 
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL)
         				AND (ACTION_PEC.ID_ACTION_PEC  in (SELECT Item FROM #List))
         				AND (MODULE_PEC.BLN_OK_PIECE = 0)
         				AND (PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1)
         				AND (MODULE_PEC.BLN_ACTIF = 1)
         				AND (PIECE_PEC.BLN_ACTIF=1)
         				AND (PIECE_PEC.BLN_STAGIAIRE =1)
         				AND ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 1) AND ( EXISTS (select * from NR210 WHERE NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC))))

         		UPDATE ARRIVEE_PIECE_PEC
         		SET	   ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 = GETDATE()
         		WHERE  ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC IN (select #TEMP_RELANCE_1.ID_ARRIVEE_PIECE_PEC FROM #TEMP_RELANCE_1 )

         		SELECT	DISTINCT ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC
         		INTO	#TEMP_RELANCE_2
         		FROM    ARRIVEE_PIECE_PEC
         				INNER JOIN PIECE_PEC ON ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC 
         				INNER JOIN MODULE_PEC ON ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC 
         				INNER JOIN ACTION_PEC ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         				CROSS JOIN PARAMETRES

         		WHERE	    (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 IS NOT NULL)
         				AND (GETDATE()-ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 >= PARAMETRES.DELAI_RELANCE_PEC)
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NULL) 
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL)
         				AND (ACTION_PEC.ID_ACTION_PEC  in (SELECT Item FROM #List))
         				AND (MODULE_PEC.BLN_OK_PIECE = 0)
         				AND (PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1)
         				AND (MODULE_PEC.BLN_ACTIF = 1)
         				AND (PIECE_PEC.BLN_ACTIF=1)
         				AND	(
         					(PIECE_PEC.BLN_ADHERENT =1)				
         					OR
         					(										
         						(PIECE_PEC.BLN_MODULE = 1)
         						AND (PIECE_PEC.BLN_ADHERENT = 0)
         						AND (PIECE_PEC.BLN_STAGIAIRE = 0)
         					)
         					)
         				AND ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 1) AND (EXISTS (select * from NR210 WHERE NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC))))
         		UNION
         		SELECT	DISTINCT ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC
         		FROM    ARRIVEE_PIECE_PEC
         				INNER JOIN PIECE_PEC ON ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC  
         				INNER JOIN STAGIAIRE_PEC ON ARRIVEE_PIECE_PEC.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC 
         				INNER JOIN ETABLISSEMENT ON STAGIAIRE_PEC.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT
         				INNER JOIN MODULE_PEC ON ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC 
         				INNER JOIN ACTION_PEC ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         				CROSS JOIN PARAMETRES

         		WHERE		(ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 IS NOT NULL)
         				AND (GETDATE()-ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 >= PARAMETRES.DELAI_RELANCE_PEC)
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NULL) 
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL)
         				AND (ACTION_PEC.ID_ACTION_PEC  in (SELECT Item FROM #List))
         				AND (MODULE_PEC.BLN_OK_PIECE = 0)
         				AND (PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1)
         				AND (MODULE_PEC.BLN_ACTIF = 1)
         				AND (PIECE_PEC.BLN_ACTIF=1)
         				AND (PIECE_PEC.BLN_STAGIAIRE =1)
         				AND ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 1) AND ( EXISTS (select * from NR210 WHERE NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC))))

         		UPDATE ARRIVEE_PIECE_PEC
         		SET	   ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 = GETDATE()
         		WHERE  ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC IN (select #TEMP_RELANCE_2.ID_ARRIVEE_PIECE_PEC FROM #TEMP_RELANCE_2 )

         		SELECT	DISTINCT ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC
         		INTO	#TEMP_RELANCE_3
         		FROM    ARRIVEE_PIECE_PEC
         				INNER JOIN PIECE_PEC ON ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC 
         				INNER JOIN MODULE_PEC ON ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC 
         				INNER JOIN ACTION_PEC ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         				CROSS JOIN PARAMETRES

         		WHERE		(ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NOT NULL)
         				AND (GETDATE()-ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 >= PARAMETRES.DELAI_RELANCE_PEC)  
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL)
         				AND (ACTION_PEC.ID_ACTION_PEC  in (SELECT Item FROM #List))
         				AND (MODULE_PEC.BLN_OK_PIECE = 0)
         				AND (PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1)
         				AND (MODULE_PEC.BLN_ACTIF = 1)
         				AND (PIECE_PEC.BLN_ACTIF=1)
         				AND	(
         					(PIECE_PEC.BLN_ADHERENT =1)				
         					OR
         					(										
         						(PIECE_PEC.BLN_MODULE = 1)
         						AND (PIECE_PEC.BLN_ADHERENT = 0)
         						AND (PIECE_PEC.BLN_STAGIAIRE = 0)
         					)
         					)
         				AND ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 1) AND (EXISTS (select * from NR210 WHERE NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC))))
         		UNION
         		SELECT	DISTINCT ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC
         		FROM    ARRIVEE_PIECE_PEC
         				INNER JOIN PIECE_PEC ON ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC  
         				INNER JOIN STAGIAIRE_PEC ON ARRIVEE_PIECE_PEC.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC 
         				INNER JOIN ETABLISSEMENT ON STAGIAIRE_PEC.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT
         				INNER JOIN MODULE_PEC ON ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC 
         				INNER JOIN ACTION_PEC ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         				CROSS JOIN PARAMETRES

         		WHERE		(ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NOT NULL)
         				AND (GETDATE()-ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 >= PARAMETRES.DELAI_RELANCE_PEC)  
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL)
         				AND (ACTION_PEC.ID_ACTION_PEC  in (SELECT Item FROM #List))
         				AND (MODULE_PEC.BLN_OK_PIECE = 0)
         				AND (PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1)
         				AND (MODULE_PEC.BLN_ACTIF = 1)
         				AND (PIECE_PEC.BLN_ACTIF=1)
         				AND (PIECE_PEC.BLN_STAGIAIRE =1)
         				AND ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 1) AND ( EXISTS (select * from NR210 WHERE NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC))))

         		UPDATE ARRIVEE_PIECE_PEC
         		SET	   ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 = GETDATE()
         		WHERE  ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC IN (select #TEMP_RELANCE_3.ID_ARRIVEE_PIECE_PEC FROM #TEMP_RELANCE_3 )

         END

 CREATE PROCEDURE [dbo].[EDT_GENERATION_EDITION_AVIS_VV]
          @ID_ETABLISSEMENT  INT,
          @ID_BENEFICIAIRE  INT,
          @TYPE_BENEFICIAIRE  INT,
          @ID_ADRESSE    INT,
          @ID_CONTACT    INT,
          @ID_PERIODE    INT,
          @ID_TYPE_RECU   TINYINT = 3,
          @_Anomalie    VARCHAR(3)
         AS

         BEGIN
          DECLARE
           @LibelleFonction VARCHAR(50),
           @RaisonSociale  VARCHAR(64),
           @ID_ADHERENT  INT

          SET
           @ID_ADHERENT = (SELECT TOP 1 ID_ADHERENT FROM ETABLISSEMENT WHERE ID_ETABLISSEMENT = @ID_ETABLISSEMENT)

          DECLARE
           @NUM_SIRET VARCHAR(14)

          SET
           @NUM_SIRET = (SELECT NUM_SIRET FROM ETABLISSEMENT WHERE ID_ETABLISSEMENT = @ID_ETABLISSEMENT)

          DECLARE
           @annee_ms INT,
           @annee_col INT

          SELECT
           @annee_ms = PERIODE.NUM_ANNEE,
           @annee_col = PERIODE.NUM_ANNEE + 1
          FROM
           PERIODE
          WHERE
           PERIODE.BLN_ACTIF = 1
           AND ID_PERIODE = @ID_PERIODE

          IF (@_Anomalie = 'non')
          BEGIN

           IF (@ID_TYPE_RECU = 3)
           BEGIN 
            EXEC dbo.INS_RECU_VV
             @ID_ETABLISSEMENT,
             @ID_PERIODE
           END
          END

          DECLARE
           @ID_RECU INT

          SELECT
           @ID_RECU = MAX(RECU.ID_RECU)
          FROM
           RECU
           INNER JOIN POSTE_RECU
            ON RECU.ID_RECU = POSTE_RECU.ID_RECU
          WHERE
           POSTE_RECU.ID_ETABLISSEMENT_BENEFICIAIRE = @ID_ETABLISSEMENT
           AND RECU.ID_RECU_REMP IS NULL
           AND RECU.ID_PERIODE = @ID_PERIODE
           AND RECU.ID_TYPE_RECU = @ID_TYPE_RECU

          DECLARE
           @PRC_FRAIS_GESTION DECIMAL(5,2)

          SELECT
           @PRC_FRAIS_GESTION = t2.PRC_FRAIS_GESTION
          FROM
           ETABLISSEMENT t1
           INNER JOIN R20BIS t2
            ON t1.ID_GROUPE = t2.ID_GROUPE
          WHERE
           t1.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
           AND t2.ID_PERIODE = @ID_PERIODE

          IF (@TYPE_BENEFICIAIRE = 1)   
          BEGIN
           SELECT
            @RaisonSociale = ADHERENT.LIB_RAISON_SOCIALE
           FROM
            ETABLISSEMENT
            INNER JOIN ADHERENT
             ON ETABLISSEMENT.ID_ADHERENT = ADHERENT.ID_ADHERENT
           WHERE
            ETABLISSEMENT.ID_ETABLISSEMENT = @ID_BENEFICIAIRE

           SELECT
            @LibelleFonction = FONCTION.LIBL_FONCTION
           FROM
            NR31
            LEFT OUTER JOIN FONCTION
             ON FONCTION.ID_FONCTION = NR31.ID_FONCTION
           WHERE
            NR31.ID_CONTACT = @ID_CONTACT
            AND NR31.ID_ETABLISSEMENT = @ID_BENEFICIAIRE
          END
          ELSE IF (@TYPE_BENEFICIAIRE = 2) 
          BEGIN
           SELECT
            @RaisonSociale = ORGANISME_FORMATION.LIB_RAISON_SOCIALE
           FROM
            ETABLISSEMENT_OF
            INNER JOIN ORGANISME_FORMATION
             ON ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF
           WHERE
            ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_BENEFICIAIRE

           SELECT
            @LibelleFonction = FONCTION.LIBL_FONCTION
           FROM
            NR34
            LEFT OUTER JOIN FONCTION
             ON FONCTION.ID_FONCTION = NR34.ID_FONCTION
           WHERE
            NR34.ID_CONTACT = @ID_CONTACT
            AND NR34.ID_ETABLISSEMENT_OF = @ID_BENEFICIAIRE
          END
          ELSE IF (@TYPE_BENEFICIAIRE = 3) 
          BEGIN
           SELECT
            @RaisonSociale = TIERS.LIB_NOM
           FROM
            TIERS
           WHERE
            TIERS.ID_TIERS = @ID_BENEFICIAIRE

           SELECT
            @LibelleFonction = FONCTION.LIBL_FONCTION
           FROM
            NR33
            LEFT OUTER JOIN FONCTION
             ON FONCTION.ID_FONCTION = NR33.ID_FONCTION
           WHERE
            NR33.ID_CONTACT = @ID_CONTACT
            AND NR33.ID_TIERS = @ID_BENEFICIAIRE
          END;

          WITH XMLNAMESPACES (
           DEFAULT 'GENERATION_EDITION_AVIS_VV'
          )
          SELECT

           dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) AS BENEFICIAIRE,

           (
            SELECT

             isnull(COD_ADHERENT, '')    as COD_ADHERENT,
             dbo.GetFullDate(getDate()) as DATE,
             (
              SELECT top 1
               dbo.GetContactSalutation(@ID_CONTACT, 1)
              FROM
               CIVILITE
              FOR XML PATH('POLITESSE_HAUT'), TYPE
             )
            FROM
             ADHERENT
             INNER JOIN ETABLISSEMENT
              ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
            WHERE
             ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
            FOR XML PATH('ENTETE'), TYPE
           ),
           (
            SELECT
             (
              SELECT
               @annee_ms as ANNEE
              FOR XML PATH('PERIODE'), TYPE
             ),

             (
              SELECT
               (
                SELECT
                 TYPE_RECU.LIBL_TYPE_RECU
                FROM
                 TYPE_RECU
                WHERE
                 TYPE_RECU.ID_TYPE_RECU = @ID_TYPE_RECU
                FOR XML AUTO, ELEMENTS, TYPE
               ),
               (
                SELECT
                 INFO_RECU.COD_RECU,
                 (
                  SELECT
                   'Cet avis annule et remplace le Nø: ' + COD_RECU
                  FROM
                   RECU
                  WHERE
                   ID_RECU_REMP = INFO_RECU.ID_RECU
                 ) AS COD_RECU_REMP
                FROM
                 RECU as INFO_RECU
                WHERE
                 ID_RECU = @ID_RECU
                FOR XML AUTO, ELEMENTS, TYPE
               ),
               (
                SELECT
                 PERIODE.NUM_ANNEE as ANNEE
                FROM
                 PERIODE
                WHERE
                 PERIODE.BLN_ACTIF = 1
                 AND ID_PERIODE = @ID_PERIODE
                FOR XML AUTO, ELEMENTS, TYPE
               )
              FOR XML RAW('TITRE'), ELEMENTS, TYPE
             ),
             (
              SELECT
               dbo.GetFrenchCurrencyFormat(sum(CAST(POSTE_RECU.MNT_HT as money))) as TOTAL_HT,
               dbo.GetFrenchCurrencyFormat(sum(CAST(POSTE_RECU.MNT_HT as money)) - sum(CAST(POSTE_RECU.MNT_HT * (CAST(@PRC_FRAIS_GESTION as money)/100) as money)))  as DISPONIBLE,
               dbo.GetFrenchCurrencyFormat(CAST(dbo.GetCurrentTVA(ADHERENT.ID_TYPE_TVA) * 100 as money)) as TAU_TVA,
               dbo.GetFrenchCurrencyFormat(sum(CAST(POSTE_RECU.MNT_TVA as money))) as TOTAL_TVA,
               dbo.GetFrenchCurrencyFormat(sum(CAST(POSTE_RECU.MNT_TTC as money))) as TOTAL_TTC
              FROM
               RECU
               INNER JOIN ADHERENT
                ON RECU.ID_ADHERENT = ADHERENT.ID_ADHERENT
               INNER JOIN POSTE_RECU
                ON POSTE_RECU.ID_RECU = RECU.ID_RECU
              WHERE
               RECU.ID_RECU = @ID_RECU
              GROUP BY
               dbo.GetFrenchCurrencyFormat(CAST(dbo.GetCurrentTVA(ADHERENT.ID_TYPE_TVA) * 100 as money))
              FOR XML RAW('MONTANTS'), ELEMENTS, TYPE
             ),
             (
              SELECT
               (
                SELECT DISTINCT
                 CASE ID_MODE_VERSEMENT
                  WHEN  1
                   THEN 'ChŠque nø ' + NUM_CHEQUE + ' sur ' + LIB_BANQUE
                  WHEN  2
                   THEN 'Virement bancaire' + case when LIB_BANQUE <> 'Non D‚finie' then ' de ' + LIB_BANQUE else '' end
                  WHEN  4
                   THEN 'Pr‚lŠvement automatique'
                 END
                FROM
                 RECU_MODE_VERSEMENT
                WHERE
                 ID_RECU = @ID_RECU
                FOR XML RAW('REF_PAIEMENT'), ELEMENTS, TYPE
               )
              FOR XML PATH('RECU_MODE_VERSEMENT'), TYPE
             )
            FOR XML PATH('CORPS'), TYPE
           ),

           (
            SELECT
             @annee_ms AS ANNEE_MS,
             @annee_col AS ANNEE_COL
            FOR XML PATH('PERIODE_PIED'), TYPE
           ),

           (
            SELECT
             NOM_PNM_DIRECTEUR
            FROM
             PARAMETRES as SIGNATURE
            FOR XML AUTO, ELEMENTS, TYPE
           )
          FROM
           ADHERENT
           INNER JOIN ETABLISSEMENT
            ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
          WHERE
           ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
          FOR XML RAW('LETTRE'), ELEMENTS
         END

 CREATE PROCEDURE [BATCH_TRANSFERT_REGUL_FRAIS_GESTION_VV]
         
         @NUM_ANNEE_N       int,
         @ID_ADHERENT_TRAITE      int

         AS
         BEGIN

          IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
          BEGIN
           drop table #TMP_TRANSFERT
          END

          DECLARE 
          @DAT       DATETIME,
          @ID_TYPE_EVENEMENT_TRANSFERT INTEGER,
          @ID_GROUPE      INTEGER,
          @ID_ADHERENT     INTEGER,
          @ID_ETABLISSEMENT    INTEGER,
          @MNT_TRANSFERT     DECIMAL(15, 2),
          @ID_BRANCHE      INT,
          @ID_PERIODE_N     INTEGER, 
          @LIBL_MVT      VARCHAR(60),
          @LIBL_EVENEMENT     VARCHAR(50),
          @ID_ENVELOPPE     INTEGER,
          @LIBL_ENVELOPPE     VARCHAR(50),
          @ID_ACTIVITE_PLAN    INTEGER,
          @BLN_COMPTE_VERS_ENVELOPPE  TINYINT,
          @ID_TRANSFERT     INT,
          @ID_TYPE_FINANCEMENT   INT

          DECLARE @COD_TYPE_EVENEMENT_REGUL_FRAIS_GESTION_VV varchar(8)
          SET @COD_TYPE_EVENEMENT_REGUL_FRAIS_GESTION_VV = 'REGUFGVV'

          select @ID_TYPE_EVENEMENT_TRANSFERT = ID_TYPE_EVENEMENT FROM TYPE_EVENEMENT where COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT_REGUL_FRAIS_GESTION_VV 

          IF @ID_TYPE_EVENEMENT_TRANSFERT IS NULL 
          BEGIN
           SELECT 'Le type d''evenement associe au code evenement passe en parametre : ' + @COD_TYPE_EVENEMENT_REGUL_FRAIS_GESTION_VV + ' n''existe pas'
          END
          ELSE
          BEGIN

           SELECT @ID_TYPE_FINANCEMENT = 3 

           SELECT t.*, ADHERENT.ID_ETABLISSEMENT_PRINCIPAL
           INTO #TMP_TRANSFERT 
           FROM F_TRANSFERT_REGUL_FRAIS_GESTION_VV(@NUM_ANNEE_N, @ID_ADHERENT_TRAITE) t
           INNER JOIN ADHERENT ON ADHERENT.ID_ADHERENT = t.ID_ADHERENT

           SELECT @DAT = GETDATE()

           SELECT @ID_PERIODE_N = ID_PERIODE   
           from PERIODE     
           where NUM_ANNEE  = @NUM_ANNEE_N 
           AND  ID_TYPE_PERIODE = 1   

           SET @LIBL_EVENEMENT  = 'R‚gularisation Frais Gestion VV ' + CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
           SET @LIBL_MVT   = 'R‚gularisation Frais Gestion VV '  + CAST(@NUM_ANNEE_N AS VARCHAR(4)) 

           DECLARE cu_transfert CURSOR FOR
           SELECT ID_ADHERENT,  ID_GROUPE, ID_BRANCHE, MNT_TRANSFERT, ID_ETABLISSEMENT_PRINCIPAL
           FROM #TMP_TRANSFERT
           WHERE ABS(MNT_TRANSFERT) > 0

           OPEN cu_transfert

           FETCH cu_transfert INTO
           @ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @MNT_TRANSFERT, @ID_ETABLISSEMENT

           WHILE (@@FETCH_STATUS <> -1)
           BEGIN 

            SELECT  @ID_ENVELOPPE = ID_ENVELOPPE , @LIBL_ENVELOPPE = LIBL_ENVELOPPE 
            FROM  TYPE_ENVELOPPE 
            INNER JOIN ENVELOPPE ON ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE
            WHERE  TYPE_ENVELOPPE.BLN_FRAIS_GESTION_COLLECTE = 1 
            AND   TYPE_ENVELOPPE.ID_BRANCHE = @ID_BRANCHE

            IF @MNT_TRANSFERT >0
            BEGIN
             SET @BLN_COMPTE_VERS_ENVELOPPE  = 0
            END
            ELSE
            BEGIN
             SET @MNT_TRANSFERT = - @MNT_TRANSFERT
             SET @BLN_COMPTE_VERS_ENVELOPPE  = 1
            END

            exec @ID_TRANSFERT = INS_TRANSFERT 
             @LIBL_TRANSFERT    = @LIBL_EVENEMENT,
             @BLN_COMPTE_VERS_ENVELOPPE = @BLN_COMPTE_VERS_ENVELOPPE,  
             @ID_GROUPE     = @ID_GROUPE,
             @ID_ENVELOPPE    = @ID_ENVELOPPE,
             @DAT_TRANSFERT    = @DAT,
             @MNT_TRANSFERT    = @MNT_TRANSFERT, 
             @ID_TYPE_FINANCEMENT  = @ID_TYPE_FINANCEMENT,   
             @ID_UTILISATEUR    = 82, 
             @ID_PERIODE     = @ID_PERIODE_N,
             @COM_TRANSFERT    = @LIBL_MVT, 
             @LIBL_MVT_BUDGETAIRE  = @LIBL_MVT,
             @ID_TYPE_EVENEMENT   = @ID_TYPE_EVENEMENT_TRANSFERT,
             @ID_ETABLISSEMENT   = @ID_ETABLISSEMENT

            FETCH cu_transfert INTO
            @ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @MNT_TRANSFERT, @ID_ETABLISSEMENT

           END

           CLOSE cu_transfert
           DEALLOCATE cu_transfert
          END

          IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
          BEGIN
           drop table #TMP_TRANSFERT
          END

         END

         CREATE PROCEDURE [dbo].[UPD_DATE_REFUS_CONTRAT_PRO_ADH]
         	@CODS_CONTRAT_PRO TEXT	
         AS

         BEGIN

         	DECLARE @Item varchar(10);

         	CREATE TABLE #List(Item VARCHAR(10) collate French_CI_AI)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,1,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)-1))),
         			@CODS_CONTRAT_PRO=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)+1,DATALENGTH(@CODS_CONTRAT_PRO))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT cast(@Item as varchar)
         	END

         	IF DATALENGTH(@CODS_CONTRAT_PRO) > 0
         		begin
         			select @Item = cast(@CODS_CONTRAT_PRO as varchar(10))
         			INSERT INTO #List SELECT @Item 
         		end

         	UPDATE CONTRAT_PRO
         	SET	   CONTRAT_PRO.DAT_LETTRE_REFUS = GETDATE()
         	WHERE  CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)

         END

         CREATE PROCEDURE PORTAIL_UPD_LETTRAGE_MANUEL_FACTURE
           
         	@cod_facture	varchar(50),  
         	@bln_ok			tinyint   out,  
         	@message		varchar(7000) out  
         AS  

         BEGIN  

         	DECLARE
         	@ID_FACTURE						int,  
         	@ID_UTILISATEUR					int,  
         	@COM_EVENEMENT					varchar(7600),
         	@BLN_LETTRE						int, 
         	@TOTAL_FACTURE_NON_VALIDE_HT	decimal(18, 2)

         	SET @bln_ok = 1
         	SET @message = ''  

         	IF @cod_facture is null 
         	BEGIN   
         		SET @bln_ok = 0
         		SELECT @message = '!! ALERTE !! La valorisation du Nø Interne facture est obligatoire. Lettrage facture impossible'  
         	END  

         	IF @bln_ok = 1
         	BEGIN  
         		SELECT @id_facture = ID_FACTURE, @BLN_LETTRE = BLN_LETTRE, @TOTAL_FACTURE_NON_VALIDE_HT = TOTAL_FACTURE_NON_VALIDE_HT
         		FROM  VUE_LETTRAGE_FACTURE
         		WHERE COD_FACTURE = @cod_facture

         		IF @id_facture  IS NULL  
         		BEGIN  
         			SET @bln_ok = 0
         			SELECT @message = '!! ALERTE !! Facture avec Nø Interne facture= ' + CAST(@cod_facture AS VARCHAR)+ ' inexistante. Lettrage facture impossible.'  
         		END  
         	END

         	IF @bln_ok = 1
         	BEGIN  
         		SELECT @id_facture = ID_FACTURE, @BLN_LETTRE = ISNULL(BLN_LETTRE, 0), @TOTAL_FACTURE_NON_VALIDE_HT = TOTAL_FACTURE_NON_VALIDE_HT
         		FROM  VUE_LETTRAGE_FACTURE
         		WHERE COD_FACTURE = @cod_facture

         		IF @BLN_LETTRE = 1  
         		BEGIN  
         			SET @bln_ok = 0
         			SELECT @message = '!! ALERTE !! La Facture avec Nø Interne facture= ' + CAST(@cod_facture AS VARCHAR)+ ' est d‚j… lettr‚e. Lettrage facture impossible.'  
         		END  
         	END

         	IF @bln_ok = 1
         	BEGIN  

         		IF ABS(@TOTAL_FACTURE_NON_VALIDE_HT) >0
         		BEGIN  
         			SET @bln_ok = 0
         			SELECT @message = '!! ALERTE !! La Facture avec Nø Interne facture= ' + CAST(@cod_facture AS VARCHAR)+ ' est associ‚e … des Demandes de RŠglements PEC/Session PRO actives dont le rŠglement n''est pas valid‚. Lettrage facture impossible.'  
         		END  
         	END

         	IF @bln_ok = 1
         	BEGIN  

         		UPDATE FACTURE SET BLN_LETTRE = 1 WHERE ID_FACTURE = @ID_FACTURE
         		SELECT @message = 'Lettrage de la facture ayant pour Nø Interne '+ CAST(@cod_facture AS VARCHAR)+ ' effectu‚.'
         	END

         END

         CREATE PROCEDURE [dbo].[INS_SALARIE_STATS]
         	 @ID_SALARIE INT,
         	 @BLN_BENEFICIAIRE_ANI tinyint,
         	 @BLN_RQTH tinyint,
         	 @DAT_EFFET_CSP DATETIME,
         	 @DAT_FIN_CSP DATETIME,
         	 @ID_TYPE_CONTRAT_ANI INT,
         	 @NUM_IDE varchar(11)
         AS
         BEGIN
         	INSERT INTO SALARIE_STATS
         	(
         	 ID_SALARIE,
         	 BLN_BENEFICIAIRE_ANI,
         	 BLN_RQTH,
         	 DAT_EFFET_CSP,
         	 DAT_FIN_CSP,
         	 ID_TYPE_CONTRAT_ANI,
         	 NUM_IDE
         	 )
         	values
         	(
         	 @ID_SALARIE,
         	 @BLN_BENEFICIAIRE_ANI,
         	 @BLN_RQTH,
         	 @DAT_EFFET_CSP,
         	 @DAT_FIN_CSP,
         	 @ID_TYPE_CONTRAT_ANI,
         	 @NUM_IDE
         	)
         END

         CREATE PROCEDURE [dbo].[INS_UPD_FACTURE_DEMAT]
         	@ID_FACTURE INT,
         	@ID_EMETTEUR_ETABLISSEMENT_ADH INT,
         	@ID_EMETTEUR_ETABLISSEMENT_OF INT,
         	@NUM_FACTURE VARCHAR(50),
         	@DAT_RECEPTION DATETIME,
         	@DAT_EMISSION DATETIME,
         	@COD_D3R_TVA VARCHAR(7),
         	@MNT_HT FLOAT,
         	@MNT_TTC FLOAT,
         	@TAU_TVA FLOAT,
         	@IBAN VARCHAR(50),
         	@ID_UTILISATEUR INT,
         	@BLN_ACTIF TINYINT,
         	@ID_TYPE_FACTURE INT,	
         	@ID_FACTURE_ASSOCIEE_AVOIR INT,
         	@COD_DOSSIER VARCHAR(11),
         	@TYPE_DOSSIER VARCHAR(3),
         	@RESULT_FACTURE INT OUTPUT
         AS
         BEGIN
         	DECLARE
         		@ID_EMETTEUR_OF INT,
         		@ID_EMETTEUR_ADHERENT INT,
         		@ID_TRANSACTION INT,
         		@ID_TYPE_TVA INT,
         		@DUREE_CLOTURE INT,
         		@BLN_LETTRE INT,
         		@COD_FACTURE VARCHAR(50),
         		@TIMESTAMP TIMESTAMP = NULL

         	SET @BLN_ACTIF = 1

         	SELECT
         		@ID_UTILISATEUR = ID_UTILISATEUR
         	FROM
         		UTILISATEUR
         	WHERE
         		COD_UTIL  = 'ADMIN_D3R'

         	SELECT	@DUREE_CLOTURE = CAST(dbo.GET_PARAM_VALUE_ALERTES_PANIERS('DUREE_CLOTURE') AS INT)

         	SELECT
         		@ID_EMETTEUR_OF = ETABLISSEMENT_OF.ID_OF 
         	FROM
         		ETABLISSEMENT_OF 
         	WHERE
         		ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_EMETTEUR_ETABLISSEMENT_OF

         	SELECT
         		@ID_EMETTEUR_ADHERENT = ETABLISSEMENT.ID_ADHERENT 
         	FROM
         		ETABLISSEMENT 
         	WHERE
         		ETABLISSEMENT.ID_ETABLISSEMENT = @ID_EMETTEUR_ETABLISSEMENT_ADH

         	DECLARE @IBAN_PRINCIPAL VARCHAR(34)

         	SELECT
         		@ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION,
         		@IBAN_PRINCIPAL = NUM_IBAN
         	FROM
         		[TRANSACTION]
         	WHERE
         		[TRANSACTION].ID_SOUS_TYPE_TRANSACTION = 4 
         		AND [TRANSACTION].BLN_ACTIF = 1
         		AND
         		(
         			[TRANSACTION].ID_ETABLISSEMENT_OF_DEST = @ID_EMETTEUR_ETABLISSEMENT_OF
         			OR 
         			[TRANSACTION].ID_ETABLISSEMENT_DEST = @ID_EMETTEUR_ETABLISSEMENT_ADH
         		)
         		AND [TRANSACTION].BLN_TRANSACTION_REGLEMENT_PRINCIPAL = 1
         	ORDER BY
         		DAT_MODIF DESC

         	SET @ID_TYPE_TVA  = NULL
         	DECLARE @C INT
         	SET @C = CHARINDEX('/',@COD_D3R_TVA)
         	IF @C> 0
         	BEGIN
         		SELECT
         			@ID_TYPE_TVA = ID_TYPE_TVA 
         		FROM
         			TYPE_TVA
         		WHERE
         			COD_TYPE_TVA = SUBSTRING(@COD_D3R_TVA,@C+1,6)
         	END

         	IF @ID_TYPE_TVA IS NULL
         	BEGIN
         		SELECT
         			@ID_TYPE_TVA = ID_TYPE_TVA
         		FROM
         			TYPE_TVA
         		WHERE
         			COD_TYPE_TVA = '00' 
         	END

         	DECLARE
         		@ID_ETAT_IBAN_FACTURE INT,
         		@COD_ETAT_IBAN_FACTURE VARCHAR(8) = NULL
         	set @IBAN = left(REPLACE(@IBAN, ' ', ''),34)

         	IF (@IBAN IS NOT NULL)
         	BEGIN 
         		IF (@IBAN_PRINCIPAL = @IBAN) 
         		BEGIN
         			SET @COD_ETAT_IBAN_FACTURE = 'EGALPPAL'
         		END

         		ELSE IF (@IBAN_PRINCIPAL <> @IBAN AND @IBAN_PRINCIPAL IS NOT NULL)
         		BEGIN
         			SET @COD_ETAT_IBAN_FACTURE = 'DIFFPPAL'
         			SET @ID_TRANSACTION = NULL
         		END

         		ELSE IF (@IBAN_PRINCIPAL IS NULL)
         		BEGIN
         			SET @COD_ETAT_IBAN_FACTURE = 'PPALNULL' 
         		END
         	END	
         	ELSE 
         	BEGIN

         		IF ( @IBAN_PRINCIPAL IS NOT NULL)
         		BEGIN
         			SET @COD_ETAT_IBAN_FACTURE = 'FACTNULL' 
         		END

         		ELSE 
         		BEGIN
         			SET @COD_ETAT_IBAN_FACTURE = 'NULLNULL' 
         		END 
         	END	

         	IF NOT EXISTS (SELECT TOP 1 1 FROM FACTURE WHERE ID_FACTURE = @ID_FACTURE)
         	BEGIN
         		SET @ID_FACTURE = NULL
         		DECLARE
         			@YEAR_EMISSION INT = YEAR(@DAT_EMISSION),
         			@TMP_ID_FACT int = null

         		EXEC @TMP_ID_FACT = dbo.IS_NUM_FACTURE_EXISTE
         			@ID_EMETTEUR_ETABLISSEMENT_OF = @ID_EMETTEUR_ETABLISSEMENT_OF,
         			@ID_EMETTEUR_ETABLISSEMENT_ADH = @ID_EMETTEUR_ETABLISSEMENT_ADH ,
         			@ANNEE_EMISSION_FACTURE = @YEAR_EMISSION,
         			@NUM_FACTURE = @NUM_FACTURE,
         			@ID_FACTURE = NULL

         		IF coalesce(@TMP_ID_FACT,0) = 0
         		BEGIN
         			SET @ID_FACTURE = NULL
         		END
         		else
         			set @ID_FACTURE = @TMP_ID_FACT
         	END

         	DECLARE @cod INT, @annee INT, @n INT, @tmpCode VARCHAR(11)
         	IF @TYPE_DOSSIER = 'PEC'
         	BEGIN		
         		SET @tmpCode = REPLACE(@COD_DOSSIER, ' ', '')
         		SET @n = CHARINDEX('/', @tmpCode, 0)
         		IF (@n > 0)
         		BEGIN 
         			SET @cod = CAST(LEFT(@tmpCode, @n-1) AS INT)
         			SET @annee = CAST(SUBSTRING(@tmpCode, @n+1, 4) AS INT)
         		END
         	END

         	IF @ID_FACTURE IS NULL 
         	BEGIN		
         		DECLARE @MNT_TTC_NUMERISE DECIMAL(18,2)
         		SET @MNT_TTC_NUMERISE = @MNT_TTC

         		EXECUTE dbo.INS_FACTURE   
         			@ID_EMETTEUR_OF,
         			@ID_EMETTEUR_ADHERENT,
         			@ID_EMETTEUR_ETABLISSEMENT_ADH,
         			@ID_EMETTEUR_ETABLISSEMENT_OF,
         			@NUM_FACTURE,
         			@DAT_RECEPTION,
         			@DAT_EMISSION,
         			@ID_TYPE_TVA,
         			@MNT_HT,
         			@MNT_TTC,		  
         			@TAU_TVA,
         			@ID_TRANSACTION,
         			@ID_UTILISATEUR,
         			@BLN_ACTIF,
         			@ID_TYPE_FACTURE,
         			@ID_FACTURE_ASSOCIEE_AVOIR,
         			@COD_FACTURE OUTPUT,
         			@ID_FACTURE OUTPUT,
         			@MNT_TTC_NUMERISE

         		IF (@TYPE_DOSSIER = 'PEC' AND @cod IS NOT NULL AND @annee IS NOT NULL) 
         		BEGIN
         			DECLARE @ID_MODULE_PEC INT
         			DECLARE CUR_MODULE_PEC CURSOR FOR
         				SELECT	MODULE_PEC.ID_MODULE_PEC FROM MODULE_PEC 
         						INNER JOIN ACTION_PEC ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
         						INNER JOIN VUE_MODULES_VISIBLES_PANIERS MODULES_VISIBLES ON MODULES_VISIBLES.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         						AND ACTION_PEC.COD_ACTION_PEC = @cod 
         						and ACTION_PEC.ANNEE_ACTION_PEC= @annee 
         						AND NOT EXISTS(SELECT 1 FROM MODULE_FACTURE
         											WHERE MODULE_FACTURE.ID_FACTURE = @ID_FACTURE AND
         												MODULE_FACTURE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC)

         			OPEN CUR_MODULE_PEC
         			FETCH NEXT FROM CUR_MODULE_PEC INTO @ID_MODULE_PEC
         			WHILE @@FETCH_STATUS = 0   
         			BEGIN
         					EXECUTE dbo.INS_MODULE_FACTURE 
         					@ID_FACTURE,
         					@ID_MODULE_PEC,
         					NULL

         					FETCH NEXT FROM CUR_MODULE_PEC INTO @ID_MODULE_PEC
         			END
         			CLOSE	CUR_MODULE_PEC
         			DEALLOCATE CUR_MODULE_PEC
         		END
         		ELSE IF @TYPE_DOSSIER = 'PRO'
         		BEGIN
         			DECLARE @ID_MODULE_PRO INT
         			DECLARE CUR_MODULE_PRO CURSOR FOR
         				SELECT	MODULE_PRO.ID_MODULE_PRO FROM MODULE_PRO 
         						INNER JOIN CONTRAT_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         						INNER JOIN VUE_MODULES_VISIBLES_PANIERS MODULES_VISIBLES ON MODULES_VISIBLES.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO 
         						AND dbo.GETCONTRATPROCODE(CONTRAT_PRO.COD_CONTRAT_PRO,YEAR(CONTRAT_PRO.DAT_CREATION)) = @COD_DOSSIER
         						AND NOT EXISTS(SELECT 1 FROM MODULE_FACTURE
         										   WHERE MODULE_FACTURE.ID_FACTURE = @ID_FACTURE AND
         												MODULE_FACTURE.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO)

         			OPEN CUR_MODULE_PRO
         			FETCH NEXT FROM CUR_MODULE_PRO INTO @ID_MODULE_PRO
         			WHILE @@FETCH_STATUS = 0   
         			BEGIN
         					EXECUTE dbo.INS_MODULE_FACTURE
         					@ID_FACTURE,
         					NULL,
         					@ID_MODULE_PRO			

         					FETCH NEXT FROM CUR_MODULE_PRO INTO @ID_MODULE_PRO
         			END
         			CLOSE	CUR_MODULE_PRO
         			DEALLOCATE CUR_MODULE_PRO
         		END

         		UPDATE
         			FACTURE
         		SET
         			NUM_IBAN_NUMERISE = @IBAN
         		WHERE
         			FACTURE.ID_FACTURE = @ID_FACTURE

         		IF (@COD_ETAT_IBAN_FACTURE IS NOT NULL)
         		BEGIN
         			UPDATE
         				FACTURE 
         			SET 
         				ID_ETAT_IBAN_FACTURE = (SELECT TOP 1 ID_ETAT_IBAN_FACTURE FROM ETAT_IBAN_FACTURE 
         										WHERE COD_ETAT_IBAN_FACTURE = @COD_ETAT_IBAN_FACTURE
         										AND ETAT_IBAN_FACTURE.BLN_ACTIF = 1)
         			WHERE
         				FACTURE.ID_FACTURE = @ID_FACTURE
         		END

         		IF @ID_FACTURE IS NOT NULL
         		BEGIN
         			SET @RESULT_FACTURE = @ID_FACTURE
         		END

         	END	
         	ELSE 
         	BEGIN	

         		IF (@TYPE_DOSSIER = 'PEC' AND @cod IS NOT NULL AND @annee IS NOT NULL) 
         			BEGIN
         				DECLARE CUR_MODULE_PEC CURSOR FOR
         					SELECT	MODULE_PEC.ID_MODULE_PEC FROM MODULE_PEC 
         							INNER JOIN ACTION_PEC ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
         							INNER JOIN VUE_MODULES_VISIBLES_PANIERS MODULES_VISIBLES ON MODULES_VISIBLES.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         							AND ACTION_PEC.COD_ACTION_PEC = @cod 
         							and ACTION_PEC.ANNEE_ACTION_PEC= @annee 
         							AND NOT EXISTS(SELECT 1 FROM MODULE_FACTURE
         											   WHERE MODULE_FACTURE.ID_FACTURE = @ID_FACTURE AND
         													MODULE_FACTURE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC)

         				OPEN CUR_MODULE_PEC
         				FETCH NEXT FROM CUR_MODULE_PEC INTO @ID_MODULE_PEC
         				WHILE @@FETCH_STATUS = 0   
         				BEGIN
         					EXECUTE dbo.INS_MODULE_FACTURE
         					@ID_FACTURE,
         					@ID_MODULE_PEC,
         					NULL

         					FETCH NEXT FROM CUR_MODULE_PEC INTO @ID_MODULE_PEC
         				END
         				CLOSE	CUR_MODULE_PEC
         				DEALLOCATE CUR_MODULE_PEC
         			END		
         			ELSE IF @TYPE_DOSSIER = 'PRO'
         			BEGIN
         				DECLARE CUR_MODULE_PRO CURSOR FOR
         				SELECT	MODULE_PRO.ID_MODULE_PRO FROM MODULE_PRO 
         						INNER JOIN CONTRAT_PRO ON CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
         						INNER JOIN VUE_MODULES_VISIBLES_PANIERS MODULES_VISIBLES ON MODULES_VISIBLES.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO
         						AND dbo.GETCONTRATPROCODE(CONTRAT_PRO.COD_CONTRAT_PRO,YEAR(CONTRAT_PRO.DAT_CREATION)) = @COD_DOSSIER
         						AND NOT EXISTS(SELECT 1 FROM MODULE_FACTURE
         											   WHERE MODULE_FACTURE.ID_FACTURE = @ID_FACTURE AND
         													MODULE_FACTURE.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO)	
         				OPEN CUR_MODULE_PRO
         				FETCH NEXT FROM CUR_MODULE_PRO INTO @ID_MODULE_PRO
         				WHILE @@FETCH_STATUS = 0   
         				BEGIN

         					EXECUTE dbo.INS_MODULE_FACTURE 
         					@ID_FACTURE,
         					NULL,
         					@ID_MODULE_PRO

         					FETCH NEXT FROM CUR_MODULE_PRO INTO @ID_MODULE_PRO
         				END
         				CLOSE	CUR_MODULE_PRO
         				DEALLOCATE CUR_MODULE_PRO
         			END

         		SET @RESULT_FACTURE = @ID_FACTURE
         	END

         END

 CREATE PROCEDURE [BATCH_TRANSFERT_COMPENSATION_FRAIS_GESTION_VO_PLAN]
         
         @NUM_ANNEE_N       int,
         @ID_ADHERENT_TRAITE      int

         AS
         BEGIN

          IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
          BEGIN
           drop table #TMP_TRANSFERT
          END

          DECLARE 
          @DAT       DATETIME,
          @ID_TYPE_EVENEMENT_TRANSFERT INTEGER,
          @ID_GROUPE      INTEGER,
          @ID_ADHERENT     INTEGER,
          @ID_ETABLISSEMENT    INTEGER,
          @MNT_TRANSFERT     DECIMAL(15, 2),
          @ID_BRANCHE      INT,
          @ID_PERIODE_N     INTEGER, 
          @ID_PERIODE_N_PLUS1    INTEGER,
          @LIBL_MVT      VARCHAR(60),
          @LIBL_EVENEMENT     VARCHAR(50),
          @ID_ENVELOPPE     INTEGER,
          @LIBL_ENVELOPPE     VARCHAR(50),
          @ID_ACTIVITE_PLAN    INTEGER,
          @BLN_COMPTE_VERS_ENVELOPPE  TINYINT,
          @ID_TRANSFERT     INT,
          @ID_TYPE_FINANCEMENT   INT

          DECLARE @COD_TYPE_EVENEMENT_COMPENSATION_FRAIS_GESTION_VV varchar(8)
          SET @COD_TYPE_EVENEMENT_COMPENSATION_FRAIS_GESTION_VV = 'COMPFGVO'

          select @ID_TYPE_EVENEMENT_TRANSFERT = ID_TYPE_EVENEMENT FROM TYPE_EVENEMENT where COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT_COMPENSATION_FRAIS_GESTION_VV 

          IF @ID_TYPE_EVENEMENT_TRANSFERT IS NULL 
          BEGIN
           SELECT 'Le type d''evenement associe au code evenement passe en parametre : ' + @COD_TYPE_EVENEMENT_COMPENSATION_FRAIS_GESTION_VV + ' n''existe pas'
          END
          ELSE
          BEGIN

           SELECT @ID_TYPE_FINANCEMENT = 4 

           SELECT t.*, ADHERENT.ID_ETABLISSEMENT_PRINCIPAL
           INTO #TMP_TRANSFERT 
           FROM F_TRANSFERT_COMPENSATION_FRAIS_GESTION_VO_PLAN (@NUM_ANNEE_N, @ID_ADHERENT_TRAITE) t
           INNER JOIN ADHERENT ON ADHERENT.ID_ADHERENT = t.ID_ADHERENT

           SELECT @DAT = GETDATE()

           SELECT @ID_PERIODE_N = ID_PERIODE   
           from PERIODE     
           where NUM_ANNEE  = @NUM_ANNEE_N 
           AND  ID_TYPE_PERIODE = 1   

           SELECT @ID_PERIODE_N_PLUS1  = ID_PERIODE
           from PERIODE     
           where NUM_ANNEE    = @NUM_ANNEE_N + 1
           AND  ID_TYPE_PERIODE   = 1   

           SET @LIBL_EVENEMENT  = 'Dotation Taux Retour 100% ' + CAST(@NUM_ANNEE_N + 1 AS VARCHAR(4)) 
           SET @LIBL_MVT   = 'Dotation Taux Retour 100% '  + CAST(@NUM_ANNEE_N + 1 AS VARCHAR(4)) 

           DECLARE cu_transfert CURSOR FOR
           SELECT ID_ADHERENT,  ID_GROUPE, ID_BRANCHE, MNT_TRANSFERT, ID_ETABLISSEMENT_PRINCIPAL
           FROM #TMP_TRANSFERT
           WHERE ABS(MNT_TRANSFERT) > 0

           OPEN cu_transfert

           FETCH cu_transfert INTO
           @ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @MNT_TRANSFERT, @ID_ETABLISSEMENT

           WHILE (@@FETCH_STATUS <> -1)
           BEGIN 

            SELECT  @ID_ENVELOPPE = ID_ENVELOPPE , @LIBL_ENVELOPPE = LIBL_ENVELOPPE 
            FROM  TYPE_ENVELOPPE 
            INNER JOIN ENVELOPPE ON ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE
            WHERE  TYPE_ENVELOPPE.BLN_FRAIS_GESTION_COLLECTE = 1 
            AND   TYPE_ENVELOPPE.ID_BRANCHE = @ID_BRANCHE

            IF @MNT_TRANSFERT >0
            BEGIN
             SET @BLN_COMPTE_VERS_ENVELOPPE  = 0
            END
            ELSE
            BEGIN
             SET @MNT_TRANSFERT = - @MNT_TRANSFERT
             SET @BLN_COMPTE_VERS_ENVELOPPE  = 1
            END

            exec @ID_TRANSFERT = INS_TRANSFERT 
             @LIBL_TRANSFERT    = @LIBL_EVENEMENT,
             @BLN_COMPTE_VERS_ENVELOPPE = @BLN_COMPTE_VERS_ENVELOPPE,  
             @ID_GROUPE     = @ID_GROUPE,
             @ID_ENVELOPPE    = @ID_ENVELOPPE,
             @DAT_TRANSFERT    = @DAT,
             @MNT_TRANSFERT    = @MNT_TRANSFERT, 
             @ID_TYPE_FINANCEMENT  = @ID_TYPE_FINANCEMENT,   
             @ID_UTILISATEUR    = 82, 
             @ID_PERIODE     = @ID_PERIODE_N_PLUS1,
             @COM_TRANSFERT    = @LIBL_MVT, 
             @LIBL_MVT_BUDGETAIRE  = @LIBL_MVT,
             @ID_TYPE_EVENEMENT   = @ID_TYPE_EVENEMENT_TRANSFERT,
             @ID_ETABLISSEMENT   = @ID_ETABLISSEMENT

            FETCH cu_transfert INTO
            @ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @MNT_TRANSFERT, @ID_ETABLISSEMENT

           END

           CLOSE cu_transfert
           DEALLOCATE cu_transfert
          END

          IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
          BEGIN
           drop table #TMP_TRANSFERT
          END

         END

 CREATE PROCEDURE [dbo].[INS_RECU_VV]
          @ID_ETABLISSEMENT AS INT,
          @ID_PERIODE AS INT
         AS

         BEGIN

          DECLARE
           @COD_RECU_PREFIXE AS CHAR(1),
           @ID_TYPE_RECU INT,
           @ID_RECU INT,
           @ID_RECU_A_REMP INT,
           @ANNULER_RECU INT

          SET @ANNULER_RECU = 1
          SET @COD_RECU_PREFIXE = 'A'

          SET @ID_TYPE_RECU = 3
          SET @ID_RECU_A_REMP = NULL

          DECLARE
           @ID_ADHERENT INT
          SELECT
           @ID_ADHERENT = ID_ADHERENT
          FROM
           ETABLISSEMENT
          WHERE
           ID_ETABLISSEMENT = @ID_ETABLISSEMENT

          WHILE EXISTS (
           SELECT
            1 
           FROM
            RECU
            INNER JOIN TYPE_RECU
             ON RECU.ID_TYPE_RECU = TYPE_RECU.ID_TYPE_RECU
            INNER JOIN  COMPTEUR
             ON COMPTEUR.COD_CPT = TYPE_RECU.COD_TYPE_RECU
            INNER JOIN PERIODE
             ON PERIODE.ID_PERIODE = RECU.ID_PERIODE
           WHERE
            TYPE_RECU.ID_TYPE_RECU = @ID_TYPE_RECU
            AND  RECU.ID_PERIODE = @ID_PERIODE
            AND  COD_RECU = @COD_RECU_PREFIXE + CONVERT(VARCHAR,PERIODE.NUM_ANNEE) + CONVERT(VARCHAR,COMPTEUR.NUM_CPT_TMP + 1)
           )
          BEGIN
           UPDATE
            COMPTEUR
           SET
            NUM_CPT_TMP = COMPTEUR.NUM_CPT_TMP + 1
           FROM
            COMPTEUR
           WHERE
            COMPTEUR.COD_CPT = (SELECT TYPE_RECU.COD_TYPE_RECU FROM TYPE_RECU WHERE ID_TYPE_RECU = @ID_TYPE_RECU)
          END

          INSERT INTO RECU
          (
           COD_RECU,
           DAT_RECU,
           DAT_EDIT_RECU,
           ID_ADHERENT,
           BLN_ACTIF,
           ID_TYPE_RECU,
           MNT_RECU_HT,
           MNT_RECU_TTC,
           MNT_RECU_TVA,
           ID_PERIODE
          )
          SELECT 
           @COD_RECU_PREFIXE + CONVERT(VARCHAR,PERIODE.NUM_ANNEE) + CONVERT(VARCHAR,COMPTEUR.NUM_CPT_TMP + 1 ),
           GETDATE(),
           GETDATE(),
           @ID_ADHERENT,
           2,
           @ID_TYPE_RECU,
           0,
           0,
           0,
           @ID_PERIODE
          FROM 
           TYPE_RECU
           INNER JOIN COMPTEUR
            ON COMPTEUR.COD_CPT = TYPE_RECU.COD_TYPE_RECU
           INNER JOIN PERIODE
            ON PERIODE.ID_PERIODE = @ID_PERIODE
          WHERE
           TYPE_RECU.ID_TYPE_RECU = @ID_TYPE_RECU

          SELECT
           @ID_RECU = SCOPE_IDENTITY()

          UPDATE
           COMPTEUR
          SET
           NUM_CPT_TMP = COMPTEUR.NUM_CPT_TMP + 1 
          FROM 
           COMPTEUR
          WHERE 
           COMPTEUR.COD_CPT = (SELECT TYPE_RECU.COD_TYPE_RECU FROM TYPE_RECU WHERE ID_TYPE_RECU = @ID_TYPE_RECU)

          INSERT INTO POSTE_RECU
          (
           ID_RECU,
           MNT_HT,
           MNT_TVA,
           MNT_TTC,
           ID_PERIODE,
           ID_ACTIVITE,
           ID_ETABLISSEMENT_BENEFICIAIRE
          )
          SELECT
           @ID_RECU,
           SUM(POSTE_IMPUTATION.MNT_HT) AS MNT_HT,
           SUM(POSTE_IMPUTATION.MNT_TVA) AS MNT_TVA,
           SUM(POSTE_IMPUTATION.MNT_TTC) AS MNT_TTC,
           POSTE_VERSEMENT.ID_PERIODE,
           POSTE_IMPUTATION.ID_ACTIVITE,
           POSTE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE
          FROM 
           VERSEMENT
           INNER JOIN POSTE_VERSEMENT 
            ON POSTE_VERSEMENT.ID_VERSEMENT = VERSEMENT.ID_VERSEMENT
           INNER JOIN POSTE_IMPUTATION 
            ON POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT
          WHERE
           VERSEMENT.BLN_IMPAYE = 0
           AND VERSEMENT.BLN_ACTIF  = 1
           AND

           @ID_TYPE_RECU = 3
           AND POSTE_VERSEMENT.ID_TYPE_VERSEMENT = 3
           AND VERSEMENT.ID_MODE_VERSEMENT IN (1,2,4)
           AND VERSEMENT.ID_STATUT_VERSEMENT = 6
           AND NOT EXISTS (SELECT 1 
               FROM
                RECU_MODE_VERSEMENT
                INNER JOIN RECU
                 ON RECU_MODE_VERSEMENT.ID_RECU = RECU.ID_RECU
                INNER JOIN POSTE_RECU
                 ON POSTE_RECU.ID_RECU = RECU.ID_RECU
               WHERE
                RECU_MODE_VERSEMENT.ID_VERSEMENT = VERSEMENT.ID_VERSEMENT
                AND  RECU.ID_TYPE_RECU = 3
                AND  POSTE_RECU.ID_ETABLISSEMENT_BENEFICIAIRE = POSTE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE
                AND  RECU_MODE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE = POSTE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE
               )
           AND POSTE_VERSEMENT.ID_PERIODE = @ID_PERIODE
           AND POSTE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE = @ID_ETABLISSEMENT
          GROUP BY
           POSTE_VERSEMENT.ID_PERIODE, POSTE_IMPUTATION.ID_ACTIVITE, POSTE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE

          DELETE FROM
           RECU_MODE_VERSEMENT
          WHERE
           ID_RECU = @ID_RECU

          INSERT INTO RECU_MODE_VERSEMENT
          (
           ID_RECU,
           ID_VERSEMENT,
           ID_MODE_VERSEMENT,
           DAT_CREATION,
           LIB_BANQUE,
           MNT_VERSEMENT,
           NUM_CHEQUE,
           ID_STATUT_VERSEMENT,
           ID_ETABLISSEMENT_BENEFICIAIRE
          )
          SELECT DISTINCT
           @ID_RECU,
           v.ID_VERSEMENT,
           v.ID_MODE_VERSEMENT,
           v.DAT_CREATION,
           v.LIB_BANQUE,
           v.MNT_VERSEMENT,
           v.NUM_CHEQUE,
           v.ID_STATUT_VERSEMENT,
           @ID_ETABLISSEMENT
          FROM
           VERSEMENT v
           INNER JOIN POSTE_VERSEMENT pv
            ON v.ID_VERSEMENT = pv.ID_VERSEMENT
           INNER JOIN POSTE_IMPUTATION pimput
            ON pv.ID_POSTE_VERSEMENT = pimput.ID_POSTE_VERSEMENT
          WHERE
           pv.ID_ETABLISSEMENT_BENEFICIAIRE = @ID_ETABLISSEMENT
           AND v.BLN_IMPAYE = 0
           AND v.BLN_ACTIF = 1
           AND @ID_TYPE_RECU = 3
           AND pv.ID_TYPE_VERSEMENT = 3
           AND V.ID_MODE_VERSEMENT IN (1, 2,4)
           AND v.ID_STATUT_VERSEMENT = 6
           AND pv.ID_PERIODE = @ID_PERIODE
           AND NOT EXISTS (SELECT 1 
               FROM
                RECU_MODE_VERSEMENT
                INNER JOIN RECU
                 ON RECU_MODE_VERSEMENT.ID_RECU = RECU.ID_RECU
                INNER JOIN POSTE_RECU
                 ON POSTE_RECU.ID_RECU = RECU.ID_RECU
               WHERE
                RECU_MODE_VERSEMENT.ID_VERSEMENT = v.ID_VERSEMENT
                AND  RECU.ID_TYPE_RECU = 3
                AND  POSTE_RECU.ID_ETABLISSEMENT_BENEFICIAIRE = pv.ID_ETABLISSEMENT_BENEFICIAIRE
                AND  RECU_MODE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE = pv.ID_ETABLISSEMENT_BENEFICIAIRE
               )

          SELECT
           RECU.ID_RECU,
           SUM(POSTE_RECU.MNT_HT * dbo.ONEIF(ID_TYPE_RECU,3)) AS MNT_RECU_HT,
           SUM(POSTE_RECU.MNT_TTC * dbo.ONEIF(ID_TYPE_RECU,3)) AS MNT_RECU_TTC,
           SUM(POSTE_RECU.MNT_TVA * dbo.ONEIF(ID_TYPE_RECU,3)) AS MNT_RECU_TVA 
          INTO
           #RECU
          FROM 
           RECU
           INNER JOIN POSTE_RECU
            ON POSTE_RECU.ID_RECU = RECU.ID_RECU
          WHERE
           RECU.ID_RECU = @ID_RECU
           AND RECU.ID_PERIODE = @ID_PERIODE
           AND POSTE_RECU.ID_PERIODE = @ID_PERIODE
          GROUP BY
           RECU.ID_RECU,
           ID_TYPE_RECU

          UPDATE
           RECU
          SET
           MNT_RECU_TVA = #RECU.MNT_RECU_TVA,
           MNT_RECU_HT = #RECU.MNT_RECU_HT,
           MNT_RECU_TTC = #RECU.MNT_RECU_TTC
          FROM
           RECU
           INNER JOIN #RECU
            ON RECU.ID_RECU = #RECU.ID_RECU

          SET @ANNULER_RECU = 0
          IF EXISTS (SELECT ID_RECU FROM RECU WHERE MNT_RECU_HT = 0)
          BEGIN
           SET @ANNULER_RECU = 1
          END

          RETURN @ANNULER_RECU
         END

 CREATE PROCEDURE [BATCH_TRANSFERT_COMPENSATION_FRAIS_GESTION_VV]
         
         @NUM_ANNEE_N       int,
         @ID_ADHERENT_TRAITE      int

         AS
         BEGIN

          IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
          BEGIN
           drop table #TMP_TRANSFERT
          END

          DECLARE 
          @DAT       DATETIME,
          @ID_TYPE_EVENEMENT_TRANSFERT INTEGER,
          @ID_GROUPE      INTEGER,
          @ID_ADHERENT     INTEGER,
          @ID_ETABLISSEMENT    INTEGER,
          @MNT_TRANSFERT     DECIMAL(15, 2),
          @ID_BRANCHE      INT,
          @ID_PERIODE_N     INTEGER, 
          @LIBL_MVT      VARCHAR(60),
          @LIBL_EVENEMENT     VARCHAR(50),
          @ID_ENVELOPPE     INTEGER,
          @LIBL_ENVELOPPE     VARCHAR(50),
          @ID_ACTIVITE_PLAN    INTEGER,
          @BLN_COMPTE_VERS_ENVELOPPE  TINYINT,
          @ID_TRANSFERT     INT,
          @ID_TYPE_FINANCEMENT   INT

          DECLARE @COD_TYPE_EVENEMENT_COMPENSATION_FRAIS_GESTION_VV varchar(8)
          SET @COD_TYPE_EVENEMENT_COMPENSATION_FRAIS_GESTION_VV = 'COMPFGVV'

          select @ID_TYPE_EVENEMENT_TRANSFERT = ID_TYPE_EVENEMENT FROM TYPE_EVENEMENT where COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT_COMPENSATION_FRAIS_GESTION_VV 

          IF @ID_TYPE_EVENEMENT_TRANSFERT IS NULL 
          BEGIN
           SELECT 'Le type d''evenement associe au code evenement passe en parametre : ' + @COD_TYPE_EVENEMENT_COMPENSATION_FRAIS_GESTION_VV + ' n''existe pas'
          END
          ELSE
          BEGIN

           SELECT @ID_TYPE_FINANCEMENT = 3 

           SELECT t.*, ADHERENT.ID_ETABLISSEMENT_PRINCIPAL
           INTO #TMP_TRANSFERT 
           FROM F_TRANSFERT_COMPENSATION_FRAIS_GESTION_VV (@NUM_ANNEE_N, @ID_ADHERENT_TRAITE) t
           INNER JOIN ADHERENT ON ADHERENT.ID_ADHERENT = t.ID_ADHERENT

           SELECT @DAT = GETDATE()

           SELECT @ID_PERIODE_N = ID_PERIODE   
           from PERIODE     
           where NUM_ANNEE  = @NUM_ANNEE_N 
           AND  ID_TYPE_PERIODE = 1   

           SET @LIBL_EVENEMENT  = 'Compensation des Frais Gestion ' + CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
           SET @LIBL_MVT   = 'Compensation des Frais Gestion '  + CAST(@NUM_ANNEE_N AS VARCHAR(4)) 

           DECLARE cu_transfert CURSOR FOR
           SELECT ID_ADHERENT,  ID_GROUPE, ID_BRANCHE, MNT_TRANSFERT, ID_ETABLISSEMENT_PRINCIPAL
           FROM #TMP_TRANSFERT
           WHERE ABS(MNT_TRANSFERT) > 0

           OPEN cu_transfert

           FETCH cu_transfert INTO
           @ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @MNT_TRANSFERT, @ID_ETABLISSEMENT

           WHILE (@@FETCH_STATUS <> -1)
           BEGIN 

            SELECT  @ID_ENVELOPPE = ID_ENVELOPPE , @LIBL_ENVELOPPE = LIBL_ENVELOPPE 
            FROM  TYPE_ENVELOPPE 
            INNER JOIN ENVELOPPE ON ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE
            WHERE  TYPE_ENVELOPPE.BLN_FRAIS_GESTION_COLLECTE = 1 
            AND   TYPE_ENVELOPPE.ID_BRANCHE = @ID_BRANCHE

            IF @MNT_TRANSFERT >0
            BEGIN
             SET @BLN_COMPTE_VERS_ENVELOPPE  = 0
            END
            ELSE
            BEGIN
             SET @MNT_TRANSFERT = - @MNT_TRANSFERT
             SET @BLN_COMPTE_VERS_ENVELOPPE  = 1
            END

            exec @ID_TRANSFERT = INS_TRANSFERT 
             @LIBL_TRANSFERT    = @LIBL_EVENEMENT,
             @BLN_COMPTE_VERS_ENVELOPPE = @BLN_COMPTE_VERS_ENVELOPPE,  
             @ID_GROUPE     = @ID_GROUPE,
             @ID_ENVELOPPE    = @ID_ENVELOPPE,
             @DAT_TRANSFERT    = @DAT,
             @MNT_TRANSFERT    = @MNT_TRANSFERT, 
             @ID_TYPE_FINANCEMENT  = @ID_TYPE_FINANCEMENT,   
             @ID_UTILISATEUR    = 82, 
             @ID_PERIODE     = @ID_PERIODE_N,
             @COM_TRANSFERT    = @LIBL_MVT, 
             @LIBL_MVT_BUDGETAIRE  = @LIBL_MVT,
             @ID_TYPE_EVENEMENT   = @ID_TYPE_EVENEMENT_TRANSFERT,
             @ID_ETABLISSEMENT   = @ID_ETABLISSEMENT

            FETCH cu_transfert INTO
            @ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @MNT_TRANSFERT, @ID_ETABLISSEMENT

           END

           CLOSE cu_transfert
           DEALLOCATE cu_transfert
          END

          IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
          BEGIN
           drop table #TMP_TRANSFERT
          END

         END

         CREATE PROCEDURE [dbo].[UPD_DATE_REJET_CONTRAT_PRO_ADH]
         	@CODS_CONTRAT_PRO TEXT	
         AS

         BEGIN

         	DECLARE @Item varchar(10);

         	CREATE TABLE #List(Item VARCHAR(10) collate French_CI_AI)

         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,1,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)-1))),
         			@CODS_CONTRAT_PRO=RTRIM(LTRIM(SUBSTRING(@CODS_CONTRAT_PRO,CHARINDEX(@Delimiter,@CODS_CONTRAT_PRO,0)+1,DATALENGTH(@CODS_CONTRAT_PRO))))

         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT cast(@Item as varchar)
         	END

         	IF DATALENGTH(@CODS_CONTRAT_PRO) > 0
         		begin
         			select @Item = cast(@CODS_CONTRAT_PRO as varchar(10))
         			INSERT INTO #List SELECT @Item 
         		end

         	UPDATE CONTRAT_PRO
         	SET	   CONTRAT_PRO.DAT_LETTRE_REJET = GETDATE()
         	WHERE  CONTRAT_PRO.COD_CONTRAT_PRO IN (SELECT Item FROM #List)

         END

         CREATE PROCEDURE INS_VIREMENT_INTERNE_PRO
         AS
         	BEGIN
         		SELECT
         			GETDATE()						AS DAT_EDIT_VIREMENT_INTERNE_PRO,
         			NULL							AS DAT_COMPTA_VIREMENT_INTERNE_PRO,
         			ROUND(SUM(MNT_REGLE_TTC), 2) 	AS MNT_VIREMENT_INTERNE_PRO, 
         			NUM_IBAN,
         			NULL							AS DAT_GENERATION_SEPA,
         			ID_ACTIVITE
         		INTO
         			#TEMP_VIREMENT_INTERNE_PRO
         		FROM
         			VIEW_VIREMENT_INTERNE_PRO_DATA
         		WHERE
         			ID_ACTIVITE = 3
         		GROUP BY
         			ID_ACTIVITE, NUM_IBAN

         		INSERT INTO dbo.VIREMENT_INTERNE_PRO
         		(
         			DAT_EDIT_VIREMENT_INTERNE_PRO, 
         			DAT_COMPTA_VIREMENT_INTERNE_PRO, 
         			MNT_VIREMENT_INTERNE_PRO, 
         			NUM_IBAN, 
         			DAT_GENERATION_SEPA,
         			ID_ACTIVITE
         		)

         		SELECT
         			DAT_EDIT_VIREMENT_INTERNE_PRO,
         			DAT_COMPTA_VIREMENT_INTERNE_PRO,
         			MNT_VIREMENT_INTERNE_PRO, 
         			NUM_IBAN,
         			DAT_GENERATION_SEPA,
         			ID_ACTIVITE
         		FROM
         			#TEMP_VIREMENT_INTERNE_PRO
         		WHERE
         			MNT_VIREMENT_INTERNE_PRO <> 0
         	END

 CREATE PROCEDURE [dbo].[UPD_ORGANISME_FORMATION]
         	 @ID_OF INT,
         	 @ID_ADRESSE_PRINCIPALE INT,
         	 @ID_GROUPE_OF INT,
         	 @ID_NAF INT,
         	 @ID_DOMAINE_OF INT,
         	 @ID_ETABLISSEMENT_OF_PRINCIPAL INT,
         	 @ID_MODE_PAIEMENT INT,
         	 @ID_CONDITION_REGLEMENT INT,
         	 @ID_ETAT_SIRET INT,
         	 @ID_STATUT_JURIDIQUE INT,
         	 @ID_TYPE_ORGANISME INT,
         	 @COD_OF VARCHAR(8),
         	 @LIB_RAISON_SOCIALE VARCHAR(64),
         	 @LIB_SIGLE_OF VARCHAR(64),
         	 @LIB_NUM_DECLARATION VARCHAR(11),
         	 @BLN_ACTIF TINYINT,
         	 @BLN_PB_OF TINYINT,
         	 @LIB_CAUSE_PB_OF VARCHAR(255),
         	 @COM_OF VARCHAR(255),
         	 @NUM_SIRET VARCHAR(14),
         	 @ID_UTILISATEUR INT,
         	 @BLN_GESTION_GROUPE TINYINT,
         	 @TIME_STAMP TIMESTAMP,
         	 @LIB_TIERS_MANDATAIRE VARCHAR(50),
         	 @DAT_SITUATION_ECONOMIQUE DATETIME
         AS
         UPDATE ORGANISME_FORMATION SET
         	ID_ADRESSE_PRINCIPALE = @ID_ADRESSE_PRINCIPALE,
         	ID_GROUPE_OF = @ID_GROUPE_OF,
         	ID_NAF = @ID_NAF,
         	ID_DOMAINE_OF = @ID_DOMAINE_OF,
         	ID_ETABLISSEMENT_OF_PRINCIPAL = @ID_ETABLISSEMENT_OF_PRINCIPAL,
         	ID_MODE_PAIEMENT = @ID_MODE_PAIEMENT,
         	ID_CONDITION_REGLEMENT = @ID_CONDITION_REGLEMENT,
         	ID_ETAT_SIRET = @ID_ETAT_SIRET,
         	ID_STATUT_JURIDIQUE = @ID_STATUT_JURIDIQUE,
         	ID_TYPE_ORGANISME = @ID_TYPE_ORGANISME,
         	COD_OF = @COD_OF,
         	LIB_RAISON_SOCIALE = @LIB_RAISON_SOCIALE,
         	LIB_SIGLE_OF = @LIB_SIGLE_OF,
         	LIB_NUM_DECLARATION = @LIB_NUM_DECLARATION,
         	BLN_ACTIF = @BLN_ACTIF,
         	BLN_PB_OF = @BLN_PB_OF,
         	LIB_CAUSE_PB_OF = @LIB_CAUSE_PB_OF,
         	COM_OF = @COM_OF,
         	NUM_SIRET = @NUM_SIRET,
         	ID_UTILISATEUR = @ID_UTILISATEUR,
         	BLN_GESTION_GROUPE = @BLN_GESTION_GROUPE,
         	DAT_MODIF	= getDate(), 
         	DAT_SITUATION_ECONOMIQUE = @DAT_SITUATION_ECONOMIQUE,
         	LIB_TIERS_MANDATAIRE = @LIB_TIERS_MANDATAIRE
         FROM ORGANISME_FORMATION
         WHERE ID_OF = @ID_OF AND TIME_STAMP = @TIME_STAMP

         IF @@ROWCOUNT = 0   
         	BEGIN  
         	   IF EXISTS(SELECT * FROM ORGANISME_FORMATION WHERE ID_OF  = @ID_OF)      
         	   BEGIN  

         			IF EXISTS (SELECT * FROM ORGANISME_FORMATION WHERE ID_OF = @ID_OF AND TIME_STAMP = @TIME_STAMP)				
         				RAISERROR(50001, 16, 1)  
         			ELSE
         			RAISERROR ('Une modification de cet ‚l‚ment a eu lieu … la ext‚rieur de cet ‚cran, qui rend impossible la validation ici',
         						   16,1)			
         	   END  
         	   ELSE  
         	   BEGIN  

         	      RAISERROR(50002, 16, 1)  
         	   END  
         	END

         IF (coalesce(@NUM_SIRET,'') <> '')
         BEGIN
         	IF LEN(@NUM_SIRET) < 9
         		SET @NUM_SIRET = @NUM_SIRET + replicate('0',9-LEN(@NUM_SIRET))

         	UPDATE ETABLISSEMENT_OF
         	SET NUM_SIRET = @NUM_SIRET + SUBSTRING(NUM_SIRET,10,5)
         	WHERE ETABLISSEMENT_OF.ID_OF = @ID_OF
         END
         ELSE
         	UPDATE ETABLISSEMENT_OF
         	SET NUM_SIRET = NULL
         	WHERE ETABLISSEMENT_OF.ID_OF = @ID_OF

         if (@BLN_ACTIF = 1) and (@ID_ETABLISSEMENT_OF_PRINCIPAL is not null)
         	update ETABLISSEMENT_OF
         	set BLN_ACTIF = 1
         	where ID_ETABLISSEMENT_OF = @ID_ETABLISSEMENT_OF_PRINCIPAL

GO

 CREATE PROCEDURE [dbo].[GENERER_PREFACTURE] 
          @ID_MODULE_PEC INT,
          @ID_UTILISATEUR INT
         AS

         BEGIN
          SELECT
           ETABLISSEMENT.ID_ADHERENT,
           GEP.ID_SESSION_PEC,
           GEP.ID_ETABLISSEMENT,
           ETABLISSEMENT.BLN_PRINCIPAL
          INTO
           #ETABS
          FROM
           dbo.GET_ETABS_PREFACTURE(@ID_MODULE_PEC) AS GEP
           INNER JOIN ETABLISSEMENT
            ON ETABLISSEMENT.ID_ETABLISSEMENT = GEP.ID_ETABLISSEMENT

          DECLARE
           @ID_ADHERENT  INT,
           @ID_ETABLISSEMENT INT,
           @COD_FACTURE  VARCHAR(50),
           @ID_FACTURE   INT,
           @DATE    DATETIME

          SET @DATE = Getdate()

          BEGIN TRY
           BEGIN TRAN TRAN_GENERER_PREFACTURE

           DECLARE CURS_ETABS CURSOR FOR

           SELECT DISTINCT
            ISNULL(MAX(CASE #ETABS.BLN_PRINCIPAL WHEN 0 THEN NULL ELSE #ETABS.ID_ETABLISSEMENT END),MAX(#ETABS.ID_ETABLISSEMENT)) AS ID_ETABLISSEMENT,
            #ETABS.ID_ADHERENT
           FROM
            #ETABS
           GROUP BY
            #ETABS.ID_ADHERENT

           OPEN CURS_ETABS
           FETCH NEXT FROM CURS_ETABS
           INTO
            @ID_ETABLISSEMENT,
            @ID_ADHERENT

           WHILE (@@FETCH_STATUS = 0)
           BEGIN
            EXEC @ID_FACTURE = INS_FACTURE
             NULL,    
             @ID_ADHERENT,  
             @ID_ETABLISSEMENT, 
             NULL,    
             NULL,    
             NULL,    
             @DATE,   
             NULL,    
             NULL,    
             NULL,    
             NULL,    
             NULL,    
             @ID_UTILISATEUR, 
             1,     
             3,     
             NULL,    
             @COD_FACTURE OUTPUT, 
             @ID_FACTURE   

            SELECT
             ROUND(PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US * (US_SES.NB_HEURE_REM/US_MOD.NB_HEURE_REM),2) AS MNT_PLAN_FINANCEMENT_US,
             US_SES.ID_UNITE_STAGIAIRE,
             PLAN_FINANCEMENT_US.ID_TYPE_FINANCEMENT,
             PLAN_FINANCEMENT_US.ID_ENVELOPPE,
             PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE,
             0 as MNT_PLAN_FINANCEMENT_US_TVA,
             SESSION_PEC.ID_SESSION_PEC,
             #ETABS.ID_ETABLISSEMENT 
            INTO
             #PFUS
            FROM
             #ETABS
             INNER JOIN SESSION_PEC
              ON SESSION_PEC.ID_SESSION_PEC = #ETABS.ID_SESSION_PEC
             INNER JOIN STAGIAIRE_PEC STAG_SES
              ON STAG_SES.ID_SESSION_PEC =  SESSION_PEC.ID_SESSION_PEC
               AND STAG_SES.ID_ETABLISSEMENT = #ETABS.ID_ETABLISSEMENT
             INNER JOIN UNITE_STAGIAIRE US_SES
              ON US_SES.ID_STAGIAIRE_PEC = STAG_SES.ID_STAGIAIRE_PEC
             INNER JOIN STAGIAIRE_PEC STAG_MOD
              ON STAG_MOD.ID_SESSION_PEC IS NULL
               AND STAG_MOD.ID_INDIVIDU = STAG_SES.ID_INDIVIDU
               AND STAG_MOD.ID_ETABLISSEMENT = STAG_SES.ID_ETABLISSEMENT
               AND STAG_MOD.ID_MODULE_PEC = SESSION_PEC.ID_MODULE_PEC
             INNER JOIN UNITE_STAGIAIRE us_mod
              ON US_MOD.ID_STAGIAIRE_PEC = STAG_MOD.ID_STAGIAIRE_PEC
               AND US_MOD.ID_DISPOSITIF = US_SES.ID_DISPOSITIF
             INNER JOIN PLAN_FINANCEMENT_US
              ON PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE = US_MOD.ID_UNITE_STAGIAIRE
             INNER JOIN POSTE_COUT_ENGAGE
              ON PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE = POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE
              AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL
              AND POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT = 5
            WHERE
             #ETABS.ID_ADHERENT = @ID_ADHERENT
             AND PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US > 0
             AND US_SES.NB_HEURE_REM > 0
             AND
             (
              ID_ENVELOPPE IS NOT NULL
              OR ID_TYPE_FINANCEMENT IS NOT NULL
             )

            INSERT INTO POSTE_COUT_REGLE
            (
             COD_POSTE_COUT_REGLE,
             MNT_REGLE_HT,
             MNT_REGLE_TVA,
             MNT_REGLE_TTC,
             BLN_FACTURE_DIRECTE,
             TAU_TVA,
             BLN_ACTIF,
             ID_TRANSACTION,
             ID_SOUS_TYPE_COUT,
             ID_MODULE_PEC,
             MNT_DEMANDE_HT,
             BLN_A_CONTROLER,
             ID_ETABLISSEMENT,
             ID_TYPE_FACTURE,
             ID_ETAT_PEC,
             BLN_OK_PIECE,
             BLN_OK_FINANCEMENT,
             MNT_DEMANDE_TVA,
             MNT_DEMANDE_TTC,
             DAT_CREATION,
             ID_UTILISATEUR,
             ID_TYPE_VALIDATION,
             ID_POSTE_COUT_ENGAGE,
             ID_SESSION_PEC,
             ID_FACTURE
            )
            SELECT
             0,                
             SUM(MNT_PLAN_FINANCEMENT_US) ,         
             SUM(MNT_PLAN_FINANCEMENT_US_TVA) ,        
             SUM(MNT_PLAN_FINANCEMENT_US) + SUM(MNT_PLAN_FINANCEMENT_US_TVA), 
             0,                
             0,                
             1,       
             NULL,      
             5,       
             @ID_MODULE_PEC,    
             SUM(MNT_PLAN_FINANCEMENT_US),
             1,       
             #PFUS.ID_ETABLISSEMENT,  
             3,       
             5,       
             0,       
             0,       
             NULL,      
             SUM(MNT_PLAN_FINANCEMENT_US),
             GetDate(),     
             @ID_UTILISATEUR,
             8,       
             #PFUS.ID_POSTE_COUT_ENGAGE, 
             #PFUS.ID_SESSION_PEC,  
             @ID_FACTURE     
            FROM
             #PFUS
            GROUP BY 
             ID_SESSION_PEC, 
             ID_ETABLISSEMENT,
             ID_POSTE_COUT_ENGAGE 

            INSERT INTO PLAN_FINANCEMENT_US
            (
             DAT_PLAN_FINANCEMENT_US,
             MNT_PLAN_FINANCEMENT_US,
             BLN_ACTIF,
             COM_PLAN_FINANCEMENT_US,
             ID_UNITE_STAGIAIRE,
             ID_TYPE_FINANCEMENT,
             ID_ENVELOPPE,
             ID_POSTE_COUT_ENGAGE,
             ID_POSTE_COUT_REGLE,
             BLN_ATTENTE_FONDS,
             MNT_PLAN_FINANCEMENT_US_TVA
            )
            SELECT
             GETDATE(),
             #PFUS.MNT_PLAN_FINANCEMENT_US,
             1, 
             'Pr‚facture ID : ' +CAST(@ID_FACTURE as varchar(10)),
             #PFUS.ID_UNITE_STAGIAIRE,
             #PFUS.ID_TYPE_FINANCEMENT,
             #PFUS.ID_ENVELOPPE,
             null, 
             POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE,
             1, 
             #PFUS.MNT_PLAN_FINANCEMENT_US_TVA
            FROM
             #PFUS 
             INNER JOIN POSTE_COUT_REGLE
              ON POSTE_COUT_REGLE.ID_FACTURE = @ID_FACTURE
               AND POSTE_COUT_REGLE.ID_SESSION_PEC = #PFUS.ID_SESSION_PEC
               AND POSTE_COUT_REGLE.ID_ETABLISSEMENT = #PFUS.ID_ETABLISSEMENT 

            UPDATE
             FACTURE
            SET
             MNT_HT = (SELECT SUM(#PFUS.MNT_PLAN_FINANCEMENT_US) FROM #PFUS)
            FROM
             FACTURE
            WHERE
             ID_FACTURE = @ID_FACTURE

            DROP TABLE #PFUS

            FETCH NEXT FROM CURS_ETABS INTO
             @ID_ETABLISSEMENT,
             @ID_ADHERENT
           END

           CLOSE CURS_ETABS
           DEALLOCATE CURS_ETABS

           COMMIT TRAN TRAN_GENERER_PREFACTURE 
          END TRY
          BEGIN CATCH
           CLOSE CURS_ETABS
           DEALLOCATE CURS_ETABS
           SELECT
            ERROR_NUMBER() as ErrorNumber,
            ERROR_MESSAGE() as ErrorMessage,
            ERROR_LINE(),
            ERROR_PROCEDURE();

           IF ((XACT_STATE()) = -1)
           BEGIN
            PRINT
             N'The transaction TRAN_GENERER_PREFACTURE is in an uncommittable state. ' +
             'Rolling back transaction.'
            ROLLBACK TRANSACTION TRAN_GENERER_PREFACTURE;
           END

           IF (XACT_STATE()) = 1
           BEGIN
            PRINT
             N'The transaction TRAN_GENERER_PREFACTURE is committable. ' +
             'Committing transaction.'
            COMMIT TRANSACTION TRAN_GENERER_PREFACTURE;   
           END
          END CATCH 
         END

 CREATE PROCEDURE [dbo].[INS_PLAN_FINANCEMENT_US_CHIFFRAGE] 
          @ID_MODULE    INT, 
          @ID_DISPOSITIF   INT,
          @ID_UNITE_STAGIAIRE  INT,
          @ID_POSTE_COUT_ENGAGE INT,
          @MNT_CHIFFRE   DECIMAL(18,2),
          @COMMENTAIRE   VARCHAR(255),
          @MNT_DIFF    DECIMAL(18,2),
          @COMM_DIFF    VARCHAR(255),
          @POURC_AIC    DECIMAL(10,5)
         AS

         BEGIN
          SET NOCOUNT ON

          DECLARE
           @ID_PERIODE     INT, 
           @ID_OPERATION_FINANCIERE INT,
           @CIBLE_ACTION    INT,
           @ID_TYPE_FINANCEMENT  INT, 
           @ID_ENVELOPPE    INT, 
           @ID_AGENCE     INT,
           @ID_BRANCHE     INT, 
           @ID_ACTIVITE_PLAN_STAGIAIRE INT,
           @ID_STC INT, 
           @COD_DISPOSITIF VARCHAR(8), 
           @LIB_GROUPE     VARCHAR(35), 
           @DISP_IS_PLAN TINYINT 

          SELECT
           @ID_PERIODE = MODULE_PEC.ID_PERIODE,
           @ID_OPERATION_FINANCIERE = ACTION_PEC.ID_OPERATION_FINANCIERE,
           @CIBLE_ACTION = ACTION_PEC.CIBLE_ACTION,
           @ID_AGENCE = ACTION_PEC.ID_AGENCE
          FROM
           MODULE_PEC
           INNER JOIN ACTION_PEC
            ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
          AND MODULE_PEC.ID_MODULE_PEC = @ID_MODULE

         select  
            @LIB_GROUPE = GROUPE.LIB_GROUPE,
            @ID_BRANCHE = STAGIAIRE_PEC.ID_BRANCHE,
            @ID_ACTIVITE_plan_stagiaire = STAGIAIRE_PEC.ID_ACTIVITE
          from UNITE_STAGIAIRE
           inner join STAGIAIRE_PEC ON STAGIAIRE_PEC.ID_STAGIAIRE_PEC = UNITE_STAGIAIRE.ID_STAGIAIRE_PEC 
           inner join GROUPE ON STAGIAIRE_PEC.ID_GROUPE = GROUPE.ID_GROUPE
          where UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE

          SELECT
           @ID_STC = POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT
          FROM
           POSTE_COUT_ENGAGE
          WHERE
           ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE

          IF (@CIBLE_ACTION = 1)
          BEGIN
           SELECT
            @ID_OPERATION_FINANCIERE = OPERATION_FINANCIERE.ID_OPERATION_FINANCIERE
           FROM
            OPERATION_FINANCIERE
           WHERE
            ID_BRANCHE = @ID_BRANCHE AND
            ID_AGENCE = @ID_AGENCE AND
            ID_DISPOSITIF = @ID_DISPOSITIF AND
            ID_PERIODE = @ID_PERIODE AND
            BLN_DEFAUT = 1
          END

          SELECT
           @COD_DISPOSITIF = COD_DISPOSITIF , @Disp_is_Plan=BLN_PLAN
          FROM
           DISPOSITIF
          WHERE
           ID_DISPOSITIF = @ID_DISPOSITIF

          DECLARE
           @ID_PREMIER_TYPE_FINANCEMENT INT = null,
           @ID_ADHERENT INT,
           @ID_OPTION INT = null

           SELECT
            @ID_ADHERENT = ID_ADHERENT,
            @ID_OPTION = R20bis.ID_OPTION
           FROM
            UNITE_STAGIAIRE
            INNER JOIN STAGIAIRE_PEC
          ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
            INNER JOIN ETABLISSEMENT
          ON STAGIAIRE_PEC.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT
            left join R20bis ON (STAGIAIRE_PEC.ID_GROUPE = R20bis.ID_GROUPE 
                and R20bis.ID_PERIODE = @ID_PERIODE) 
           WHERE
            UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE

           SELECT TOP 1
            @ID_PREMIER_TYPE_FINANCEMENT = PLAN_FINANCEMENT_OPERATION.ID_TYPE_FINANCEMENT 
           FROM
            PLAN_FINANCEMENT_OPERATION
            INNER JOIN TYPE_FINANCEMENT
          ON TYPE_FINANCEMENT.ID_TYPE_FINANCEMENT = PLAN_FINANCEMENT_OPERATION.ID_TYPE_FINANCEMENT
            INNER JOIN ACTIVITE
          ON ACTIVITE.ID_ACTIVITE = PLAN_FINANCEMENT_OPERATION.ID_ACTIVITE
            INNER JOIN TYPE_ACTIVITE
          ON TYPE_ACTIVITE.ID_TYPE_ACTIVITE = ACTIVITE.ID_TYPE_ACTIVITE
          inner join dbo.OPTIONS_TYPE_FINANCEMENT on OPTIONS_TYPE_FINANCEMENT.ID_OPTION = @ID_OPTION
          and OPTIONS_TYPE_FINANCEMENT.ID_TYPE_FINANCEMENT = TYPE_FINANCEMENT.ID_TYPE_FINANCEMENT
            LEFT JOIN R19
          ON R19.ID_ADHERENT = @ID_ADHERENT
           AND R19.ID_PERIODE = @ID_PERIODE
           AND R19.ID_ACTIVITE = PLAN_FINANCEMENT_OPERATION.ID_ACTIVITE
           WHERE
            PLAN_FINANCEMENT_OPERATION.ID_SOUS_TYPE_COUT = @ID_STC
            AND ID_OPERATION_FINANCIERE = @ID_OPERATION_FINANCIERE
            AND
            (

          (

           TYPE_ACTIVITE.COD_TYPE_ACTIVITE <> 'PLAN'
           AND R19.ID_ACTIVITE IS NOT NULL
          )
          OR

          PLAN_FINANCEMENT_OPERATION.ID_ACTIVITE = @ID_ACTIVITE_PLAN_STAGIAIRE
            )
           ORDER BY
            TYPE_FINANCEMENT.NUM_ORDRE

          IF (@DISP_IS_PLAN = 1) 
          BEGIN

           SET @ID_TYPE_FINANCEMENT = @ID_PREMIER_TYPE_FINANCEMENT
           SET @ID_ENVELOPPE = NULL
          END
          ELSE  
          BEGIN 

           SET @ID_TYPE_FINANCEMENT = NULL

           SELECT TOP 1 
          @ID_ENVELOPPE = ENVELOPPE.ID_ENVELOPPE
           FROM
          PLAN_FINANCEMENT_OPERATION
          INNER JOIN ENVELOPPE
            ON ENVELOPPE.ID_ENVELOPPE = PLAN_FINANCEMENT_OPERATION.ID_ENVELOPPE
             AND ENVELOPPE.ID_PERIODE = @ID_PERIODE
          INNER JOIN TYPE_ENVELOPPE
            ON TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = ENVELOPPE.ID_TYPE_ENVELOPPE
             AND TYPE_ENVELOPPE.ID_ACTIVITE IS NOT NULL
             AND TYPE_ENVELOPPE.ID_AGENCE = @ID_AGENCE
             AND TYPE_ENVELOPPE.ID_BRANCHE = @ID_BRANCHE
             AND TYPE_ENVELOPPE.ID_DISPOSITIF = @ID_DISPOSITIF  
          INNER JOIN PARAM_COMPATIBILITE_TYPE_ENVELOPPE_DISPOSITIF compat on compat.ID_DISPOSITIF = @ID_DISPOSITIF 
            and compat.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE      
          INNER JOIN ACTIVITE ON TYPE_ENVELOPPE.ID_ACTIVITE = ACTIVITE.ID_ACTIVITE
          INNER JOIN TYPE_ACTIVITE ON ACTIVITE.ID_TYPE_ACTIVITE = TYPE_ACTIVITE.ID_TYPE_ACTIVITE
           AND TYPE_ACTIVITE.COD_TYPE_ACTIVITE <> 'PLAN'
          INNER JOIN R19
            ON R19.ID_ADHERENT = @ID_ADHERENT
             AND R19.ID_PERIODE = @ID_PERIODE
             AND R19.ID_ACTIVITE = TYPE_ENVELOPPE.ID_ACTIVITE
           WHERE
            PLAN_FINANCEMENT_OPERATION.ID_OPERATION_FINANCIERE = @ID_OPERATION_FINANCIERE
            AND PLAN_FINANCEMENT_OPERATION.ID_TYPE_FINANCEMENT IS NULL
            AND PLAN_FINANCEMENT_OPERATION.ID_ENVELOPPE IS NOT NULL
            AND PLAN_FINANCEMENT_OPERATION.ID_SOUS_TYPE_COUT = @ID_STC
           ORDER BY
          TYPE_ENVELOPPE.ID_ACTIVITE DESC

           IF (@ID_ENVELOPPE IS NULL)
           BEGIN
            IF (object_id('tempdb..#CHIFFRAGE_LOG') IS NOT NULL)
            BEGIN
           IF (@MNT_CHIFFRE <> 0)
           BEGIN
             DECLARE @BRANCHE VARCHAR(255)

             SELECT @BRANCHE = LIBC_BRANCHE
             FROM BRANCHE
             WHERE ID_BRANCHE = @ID_BRANCHE

             DECLARE @SOUS_TYPE_COUT VARCHAR(8)

             SELECT @SOUS_TYPE_COUT = COD_SOUS_TYPE_COUT 
             FROM SOUS_TYPE_COUT
             WHERE ID_SOUS_TYPE_COUT = @ID_STC 

             DECLARE @DECOMPTE VARCHAR(40)  = ''

             IF (@COMMENTAIRE LIKE 'Type de calcul PT%')
             BEGIN
               SET @DECOMPTE = 'd‚compt‚ du montant du plafond total et '
             END

             INSERT INTO #CHIFFRAGE_LOG (LOG_MSG)
             VALUES
             (
              @SOUS_TYPE_COUT + ' : pas d''enveloppe correspondant au dispositif ' + @COD_DISPOSITIF + ' et branche ' + @BRANCHE + '. Le montant chiffr‚ de ' + ltrim(str(@MNT_CHIFFRE,10,2)) + ' est ' + @DECOMPTE + 'pris en charge sur le plan s''il est disponible.'
             )
           END

           SET @ID_TYPE_FINANCEMENT = @ID_PREMIER_TYPE_FINANCEMENT
            END
           END 
          END 

          DELETE FROM PLAN_FINANCEMENT_US
          WHERE
           ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE
           AND ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE

          DECLARE
           @ID_ENVELOPPE_AIC INT,
           @MNT_AIC   DECIMAL(18,2)

          IF (@MNT_DIFF IS NOT NULL) AND (@MNT_DIFF <> 0)
          BEGIN

            IF (isnull(@POURC_AIC,0)>0) 
            BEGIN

              SELECT @ID_ENVELOPPE_AIC = ENVELOPPE.ID_ENVELOPPE
              FROM ENVELOPPE
            INNER JOIN TYPE_ENVELOPPE
             ON TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = ENVELOPPE.ID_TYPE_ENVELOPPE
             AND TYPE_ENVELOPPE.ID_ACTIVITE IN
                 (
               SELECT
                ID_ACTIVITE
               FROM
                GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')
                 )
              INNER JOIN PLAN_FINANCEMENT_OPERATION
               ON PLAN_FINANCEMENT_OPERATION.ID_ENVELOPPE = ENVELOPPE.ID_ENVELOPPE
              WHERE
             TYPE_ENVELOPPE.ID_DISPOSITIF IS NOT NULL
             AND TYPE_ENVELOPPE.ID_AGENCE = @ID_AGENCE
             AND TYPE_ENVELOPPE.ID_BRANCHE = @ID_BRANCHE
             AND TYPE_ENVELOPPE.ID_ACTIVITE =
             (
              SELECT
               ID_ACTIVITE
              FROM
               STAGIAIRE_PEC
               INNER JOIN UNITE_STAGIAIRE
             ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
              WHERE
               UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE
             )
             AND ENVELOPPE.ID_PERIODE = @ID_PERIODE
             AND PLAN_FINANCEMENT_OPERATION.ID_OPERATION_FINANCIERE = @ID_OPERATION_FINANCIERE
             AND PLAN_FINANCEMENT_OPERATION.ID_SOUS_TYPE_COUT = @ID_STC
             AND TYPE_ENVELOPPE.BLN_ACTIF = 1
               ORDER BY
             TYPE_ENVELOPPE.COD_TYPE_ENVELOPPE 

              IF (@ID_ENVELOPPE_AIC IS NOT NULL)
              BEGIN
             SET @MNT_AIC = CAST(@POURC_AIC / 100 * @MNT_DIFF AS DECIMAL(18,2))
             SET @MNT_DIFF = @MNT_DIFF - @MNT_AIC
              END
            END

             IF (@ID_PREMIER_TYPE_FINANCEMENT IS NULL)
             BEGIN

              INSERT INTO #CHIFFRAGE_LOG (LOG_MSG)
              VALUES
              (
            'La diff‚rence de ' + ltrim(str(@MNT_DIFF,10,2)) + ' ne peut pas ˆtre prise en charge sur le compte car il n est pas disponible. Voir le groupe ' + @LIB_GROUPE
              )
             END
             ELSE
             BEGIN

           IF (@ID_PREMIER_TYPE_FINANCEMENT IS NOT NULL AND @ID_TYPE_FINANCEMENT IS NULL)
           BEGIN
            IF NOT EXISTS(SELECT 1 FROM PLAN_FINANCEMENT_US WHERE ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE AND ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE)
            BEGIN
             INSERT INTO PLAN_FINANCEMENT_US
             (
           DAT_PLAN_FINANCEMENT_US,
           MNT_PLAN_FINANCEMENT_US,
           MNT_PLAN_FINANCEMENT_US_THEORIQUE,
           BLN_ACTIF,
           BLN_DECOUVERT,
           COM_PLAN_FINANCEMENT_US,
           ID_UNITE_STAGIAIRE,
           ID_TYPE_FINANCEMENT,
           ID_ENVELOPPE,
           ID_POSTE_COUT_ENGAGE,
           ID_POSTE_COUT_REGLE,
           BLN_ATTENTE_FONDS,
           ID_VIREMENT_INTERNE_PEC,
           MNT_PLAN_FINANCEMENT_US_TVA
             )
             VALUES
             (
           getdate(),
           cast(@MNT_DIFF AS DECIMAL(18,2)),
           @MNT_DIFF,
           1,
           NULL,
           @COMM_DIFF,
           @ID_UNITE_STAGIAIRE,
           @ID_PREMIER_TYPE_FINANCEMENT,
           NULL,
           @ID_POSTE_COUT_ENGAGE,
           NULL,
           1, 
           NULL,
           0
             )
            END
            ELSE
            BEGIN
              UPDATE
            PLAN_FINANCEMENT_US
              SET
            DAT_PLAN_FINANCEMENT_US = getdate(),
            MNT_PLAN_FINANCEMENT_US = cast(@MNT_DIFF AS DECIMAL(18,2)),
            MNT_PLAN_FINANCEMENT_US_THEORIQUE = @MNT_DIFF,
            BLN_ACTIF = 1,
            BLN_DECOUVERT = NULL,
            COM_PLAN_FINANCEMENT_US = @COMM_DIFF,
            ID_TYPE_FINANCEMENT = @ID_PREMIER_TYPE_FINANCEMENT,
            ID_ENVELOPPE = NULL,
            ID_POSTE_COUT_REGLE = NULL,
            BLN_ATTENTE_FONDS = 1,
            ID_VIREMENT_INTERNE_PEC = NULL,
            MNT_PLAN_FINANCEMENT_US_TVA = 0
              WHERE
            ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE
            AND ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
            END
           END
           ELSE
            BEGIN

              SET @MNT_CHIFFRE = @MNT_CHIFFRE + @MNT_DIFF
              SET @COMMENTAIRE = @COMMENTAIRE + '; ' + @COMM_DIFF
            END
          END
          END 

          IF (isnull(@MNT_AIC,0) > 0 AND @ID_ENVELOPPE_AIC IS NOT NULL)
          BEGIN
            IF (@ID_ENVELOPPE_AIC = @ID_ENVELOPPE)
            BEGIN

              SET @MNT_CHIFFRE = @MNT_CHIFFRE + @MNT_AIC
              SET @COMMENTAIRE = @COMMENTAIRE + '; dont AIC: ' + CAST(@MNT_AIC AS VARCHAR)
              SET @MNT_AIC = NULL
            END
           ELSE 
           BEGIN

             INSERT INTO PLAN_FINANCEMENT_US
             (
            DAT_PLAN_FINANCEMENT_US,
            MNT_PLAN_FINANCEMENT_US,
            MNT_PLAN_FINANCEMENT_US_THEORIQUE,
            BLN_ACTIF,
            BLN_DECOUVERT,
            COM_PLAN_FINANCEMENT_US,
            ID_UNITE_STAGIAIRE,
            ID_TYPE_FINANCEMENT,
            ID_ENVELOPPE,
            ID_POSTE_COUT_ENGAGE,
            BLN_ATTENTE_FONDS,
            MNT_PLAN_FINANCEMENT_US_TVA
             )
             VALUES
             (
            getdate(),
            cast(@MNT_AIC AS DECIMAL(18,2)),
            @MNT_AIC,
            1,
            NULL,
            'AIC de ' + cast(@POURC_AIC AS VARCHAR) + '%',
            @ID_UNITE_STAGIAIRE,
            NULL,
            @ID_ENVELOPPE_AIC,
            @ID_POSTE_COUT_ENGAGE,
            1, 
            0
           )
           END
          END 

          IF ((@ID_TYPE_FINANCEMENT IS NULL AND @ID_ENVELOPPE IS NULL)
          OR (@ID_TYPE_FINANCEMENT IS NOT NULL AND @ID_PREMIER_TYPE_FINANCEMENT IS NULL))
          BEGIN
           SET @MNT_CHIFFRE = 0
           SET @COMMENTAIRE = 'problŠme … remonter … l''administrateur'
          END

          IF (@ID_TYPE_FINANCEMENT IS NOT NULL OR @ID_ENVELOPPE IS NOT NULL)
          BEGIN
           IF(@ID_TYPE_FINANCEMENT IS NOT NULL)
           BEGIN
          SET @ID_ENVELOPPE = NULL
           END

           IF NOT EXISTS(SELECT 1 FROM PLAN_FINANCEMENT_US WHERE 
               ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE AND 
               ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
               and coalesce(ID_ENVELOPPE,0)= coalesce(@ID_ENVELOPPE,0)
               and coalesce(ID_TYPE_FINANCEMENT,0)= coalesce(@ID_TYPE_FINANCEMENT,0))
           BEGIN 
             INSERT INTO PLAN_FINANCEMENT_US
             (
            DAT_PLAN_FINANCEMENT_US,
            MNT_PLAN_FINANCEMENT_US,
            MNT_PLAN_FINANCEMENT_US_THEORIQUE,
            BLN_ACTIF,
            BLN_DECOUVERT,
            COM_PLAN_FINANCEMENT_US,
            ID_UNITE_STAGIAIRE,
            ID_TYPE_FINANCEMENT,
            ID_ENVELOPPE,
            ID_POSTE_COUT_ENGAGE,
            BLN_ATTENTE_FONDS,
            MNT_PLAN_FINANCEMENT_US_TVA
             )
             VALUES
             (
            getdate(),
            cast(@MNT_CHIFFRE AS DECIMAL(18,2)),
            @MNT_CHIFFRE,
            1,
            NULL,
            @COMMENTAIRE,
            @ID_UNITE_STAGIAIRE,
            @ID_TYPE_FINANCEMENT,
            @ID_ENVELOPPE,
            @ID_POSTE_COUT_ENGAGE,
            1, 
            0
             )
           END
           ELSE 
           BEGIN
           UPDATE
            PLAN_FINANCEMENT_US
           SET
             DAT_PLAN_FINANCEMENT_US = getdate(),
             MNT_PLAN_FINANCEMENT_US = cast(@MNT_CHIFFRE AS DECIMAL(18,2)),
             MNT_PLAN_FINANCEMENT_US_THEORIQUE = @MNT_CHIFFRE,
             BLN_ACTIF = 1,
             BLN_DECOUVERT = NULL,
             COM_PLAN_FINANCEMENT_US = @COMMENTAIRE,
             ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT,
             ID_ENVELOPPE = @ID_ENVELOPPE,
             BLN_ATTENTE_FONDS = 1,
             MNT_PLAN_FINANCEMENT_US_TVA = 0
           WHERE
             ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE
             AND ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
           END
          END
         END

         CREATE PROCEDURE INS_REGLEMENT_PRO
         (
         	@ID_TRANSACTION	INT,
         	@ID_AGENCE		INT
         )
         AS
         BEGIN

         	UPDATE
         		SESSION_PRO
         	SET
         		ID_REGLEMENT_PRO_ADH =	CASE
         									WHEN RP.ID_REGLEMENT_PRO =SESSION_PRO.ID_REGLEMENT_PRO_ADH THEN NULL
         									ELSE ID_REGLEMENT_PRO_ADH
         								END,
         		ID_REGLEMENT_PRO_OF =	CASE
         									WHEN RP.ID_REGLEMENT_PRO =SESSION_PRO.ID_REGLEMENT_PRO_OF THEN NULL
         									ELSE ID_REGLEMENT_PRO_OF
         								END
         	FROM
         		SESSION_PRO
         		INNER JOIN REGLEMENT_PRO RP ON	RP.ID_REGLEMENT_PRO =SESSION_PRO.ID_REGLEMENT_PRO_ADH
         									OR RP.ID_REGLEMENT_PRO =SESSION_PRO.ID_REGLEMENT_PRO_OF
         	WHERE
         		RP.BLN_ACTIF=0

         	SET NOCOUNT ON
         	DECLARE
         		@COMPTEUR_ORDRE_VIREMENT_PRO	AS INT,
         		@COMPTEUR						AS INT,
         		@COD_REGLEMENT_PRO_PREFIXE		AS VARCHAR(3),
         		@NBLIGNES						AS INT,
         		@ID_TYPE_DESTINATAIRE			AS INT,
         		@ID_TYPE_BENEFICIAIRE			AS INT,
         		@ID_BENEF						AS INT,
         		@ID_REGLEMENT_PRO				AS INT,
         		@ID_CONTRAT_PRO					AS INT

         	SET @COMPTEUR = 0
         	SET @COD_REGLEMENT_PRO_PREFIXE = ''
         	SET	@COMPTEUR_ORDRE_VIREMENT_PRO = coalesce((SELECT NUM_CPT_TMP FROM COMPTEUR WHERE COD_CPT = 'VIREMENT_PRO'), 0)

         	IF @COMPTEUR_ORDRE_VIREMENT_PRO = 0
         		BEGIN
         			SET @COMPTEUR_ORDRE_VIREMENT_PRO = coalesce((SELECT NUM_CPT FROM COMPTEUR WHERE COD_CPT = 'VIREMENT_PRO'), 0)
         		END

         	SELECT
         		-1													AS COD_REGLEMENT_PRO,
         		-1													AS NUM_VIREMENT,
         		getdate()											AS DAT_REGLEMENT,
         		NULL												AS DAT_EDITION,
         		sum(SESSION_PRO_POUR_REGLEMENT.MNT_REGLE_TTC)		AS MNT_REGLE_TTC,
         		sum(SESSION_PRO_POUR_REGLEMENT.MNT_REGLE_HT)		AS MNT_REGLE_HT,
         		1													AS BLN_ACTIF,
         		NULL												AS COM_REGLEMENT,
         		NULL												AS DAT_VALID_REGLEMENT,
         		NULL												AS DAT_COMPTA_REGLEMENT,
         		1													AS BLN_EN_COURS,
         		0													AS BLN_CRITERE,
         		SESSION_PRO_POUR_REGLEMENT.ID_TRANSACTION			AS ID_TRANSACTION,
         		NULL												AS DAT_GENERATION_SEPA,
         		@ID_AGENCE											AS ID_AGENCE,
         		0													AS TRAITE,
         		SESSION_PRO_POUR_REGLEMENT.ID_TYPE_DESTINATAIRE		AS ID_TYPE_DESTINATAIRE,
         		SESSION_PRO_POUR_REGLEMENT.ID_TYPE_BENEFICIAIRE		AS ID_TYPE_BENEFICIAIRE,
         		SESSION_PRO_POUR_REGLEMENT.ID_BENEFICIAIRE			AS ID_BENEFICIAIRE
         	INTO
         		#REGLEMENT_PRO
         	FROM
         		SESSION_PRO_POUR_REGLEMENT()
         	WHERE
         		SESSION_PRO_POUR_REGLEMENT.ID_TRANSACTION = @ID_TRANSACTION
         		AND SESSION_PRO_POUR_REGLEMENT.ID_AGENCE = @ID_AGENCE
         	GROUP BY
         		SESSION_PRO_POUR_REGLEMENT.ID_TRANSACTION,
         		SESSION_PRO_POUR_REGLEMENT.NUM_IBAN,
         		SESSION_PRO_POUR_REGLEMENT.ID_TYPE_DESTINATAIRE,
         		SESSION_PRO_POUR_REGLEMENT.ID_TYPE_BENEFICIAIRE,
         		SESSION_PRO_POUR_REGLEMENT.ID_BENEFICIAIRE

         	SET @NBLIGNES = @@ROWCOUNT
         	SET ROWCOUNT 1
         	SET NOCOUNT OFF
         	WHILE @NBLIGNES > 0
         		BEGIN
         			SET ROWCOUNT 0

         			DELETE FROM
         				#REGLEMENT_PRO
         			WHERE
         				TRAITE = 1

         			SET ROWCOUNT 1
         			SET @NBLIGNES = (SELECT count(*) FROM #REGLEMENT_PRO WHERE TRAITE = 1)

         			IF @NBLIGNES > 0
         				BEGIN
         					SET @COMPTEUR = @COMPTEUR + 1
         				END

         			UPDATE
         				#REGLEMENT_PRO
         			SET
         				NUM_VIREMENT = @COMPTEUR_ORDRE_VIREMENT_PRO + @COMPTEUR,
         				COD_REGLEMENT_PRO = @COD_REGLEMENT_PRO_PREFIXE + convert(VARCHAR, @COMPTEUR),
         				TRAITE = 1
         			WHERE
         				NUM_VIREMENT < 0

         			SET @NBLIGNES = (SELECT count(*) FROM #REGLEMENT_PRO WHERE TRAITE = 1)

         			IF @NBLIGNES > 0
         				BEGIN
         					SET @COMPTEUR = @COMPTEUR + 1
         				END

         			INSERT INTO REGLEMENT_PRO
         			(
         				COD_REGLEMENT_PRO,
         				NUM_VIREMENT,
         				DAT_REGLEMENT,
         				DAT_EDITION,
         				MNT_REGLE_TTC,
         				MNT_REGLE_HT,
         				BLN_ACTIF,
         				COM_REGLEMENT,
         				DAT_VALID_REGLEMENT,
         				DAT_COMPTA_REGLEMENT,
         				BLN_EN_COURS,
         				BLN_CRITERE,
         				ID_TRANSACTION,
         				DAT_GENERATION_SEPA,
         				ID_AGENCE,
         				ID_CONTRAT_PRO
         			)
         			SELECT
         				COD_REGLEMENT_PRO,
         				NUM_VIREMENT,
         				DAT_REGLEMENT,
         				DAT_EDITION,
         				MNT_REGLE_TTC,
         				MNT_REGLE_HT,
         				BLN_ACTIF,
         				COM_REGLEMENT,
         				DAT_VALID_REGLEMENT,
         				DAT_COMPTA_REGLEMENT,
         				BLN_EN_COURS,
         				BLN_CRITERE,
         				ID_TRANSACTION,
         				DAT_GENERATION_SEPA,
         				ID_AGENCE,
         				NULL 
         			FROM
         				#REGLEMENT_PRO
         			WHERE
         				TRAITE = 1

         			SET @NBLIGNES = @@ROWCOUNT

         			UPDATE
         				REGLEMENT_PRO
         			SET
         				COD_REGLEMENT_PRO = ID_REGLEMENT_PRO
         			WHERE
         				ID_REGLEMENT_PRO = scope_identity()

         			

         			SELECT	TOP 1
         				@ID_REGLEMENT_PRO		= scope_identity(),
         				@ID_TYPE_DESTINATAIRE	= ID_TYPE_DESTINATAIRE,
         				@ID_TYPE_BENEFICIAIRE	= ID_TYPE_BENEFICIAIRE,
         				@ID_BENEF = ID_BENEFICIAIRE
         			FROM
         				#REGLEMENT_PRO
         			SELECT
         				@ID_CONTRAT_PRO = ID_CONTRAT_PRO
         			FROM
         				REGLEMENT_PRO RP
         			WHERE
         				ID_REGLEMENT_PRO = @ID_REGLEMENT_PRO

         			SET ROWCOUNT 0
         			SET NOCOUNT ON

         			IF @ID_TYPE_DESTINATAIRE = 1 
         				BEGIN
         					UPDATE
         						SESSION_PRO
         					SET
         						SESSION_PRO.ID_REGLEMENT_PRO_ADH = @ID_REGLEMENT_PRO
         					FROM
         						SESSION_PRO
         						INNER JOIN REGLEMENT_PRO RP	ON RP.ID_TRANSACTION = SESSION_PRO.ID_TRANSACTION_ADH
         						INNER JOIN SESSION_BAP_PRO	ON SESSION_PRO.ID_SESSION_BAP_PRO_ADH = SESSION_BAP_PRO.ID_SESSION_BAP_PRO 
         						INNER JOIN [TRANSACTION] T	ON T.ID_TRANSACTION = SESSION_PRO.ID_TRANSACTION_ADH
         						INNER JOIN MODULE_PRO		ON MODULE_PRO.ID_MODULE_PRO		= SESSION_PRO.ID_MODULE_PRO
         						INNER JOIN CONTRAT_PRO		ON MODULE_PRO.ID_CONTRAT_PRO	= CONTRAT_PRO.ID_CONTRAT_PRO
         					WHERE
         						SESSION_PRO.BLN_ACTIF	= 1			
         						AND SESSION_PRO.DAT_BAP_ADH	IS NOT NULL 
         						AND SESSION_PRO.ID_REGLEMENT_PRO_ADH	IS NULL
         						AND SESSION_BAP_PRO.DAT_PAIEMENT	IS NULL
         						AND SESSION_BAP_PRO.DAT_RECEPTION IS NOT NULL
         						AND SESSION_PRO.ID_TRANSACTION_ADH= @ID_TRANSACTION
         						AND CONTRAT_PRO.ID_AGENCE			= @ID_AGENCE
         						AND RP.BLN_ACTIF		= 1
         						AND RP.BLN_EN_COURS	= 1
         						AND T.BLN_ACTIF			= 1
         						AND
         						(
         							@ID_TYPE_BENEFICIAIRE = 1
         							AND T.ID_ETABLISSEMENT_BENEF = @ID_BENEF

         							OR @ID_TYPE_BENEFICIAIRE = 2
         							AND T.ID_ETABLISSEMENT_OF_BENEF = @ID_BENEF

         							OR @ID_TYPE_BENEFICIAIRE = 3
         							AND T.ID_TIERS_BENEF = @ID_BENEF
         						)
         				END
         			ELSE 
         				BEGIN
         					UPDATE
         						SESSION_PRO
         					SET
         						SESSION_PRO.ID_REGLEMENT_PRO_OF = @ID_REGLEMENT_PRO
         					FROM
         						SESSION_PRO
         						INNER JOIN REGLEMENT_PRO RP	ON RP.ID_TRANSACTION = SESSION_PRO.ID_TRANSACTION_OF
         						INNER JOIN SESSION_BAP_PRO	ON SESSION_PRO.ID_SESSION_BAP_PRO_OF = SESSION_BAP_PRO.ID_SESSION_BAP_PRO 
         						INNER JOIN [TRANSACTION] T	ON T.ID_TRANSACTION = SESSION_PRO.ID_TRANSACTION_OF
         						INNER JOIN MODULE_PRO		ON MODULE_PRO.ID_MODULE_PRO		= SESSION_PRO.ID_MODULE_PRO
         						INNER JOIN CONTRAT_PRO		ON MODULE_PRO.ID_CONTRAT_PRO	= CONTRAT_PRO.ID_CONTRAT_PRO
         					WHERE
         						SESSION_PRO.BLN_ACTIF	= 1			
         						AND SESSION_PRO.DAT_BAP_OF IS NOT NULL 
         						AND SESSION_PRO.ID_REGLEMENT_PRO_OF	IS NULL
         						AND SESSION_BAP_PRO.DAT_PAIEMENT	IS NULL
         						AND SESSION_BAP_PRO.DAT_RECEPTION IS NOT NULL
         						AND SESSION_PRO.ID_TRANSACTION_OF = @ID_TRANSACTION
         						AND CONTRAT_PRO.ID_AGENCE			= @ID_AGENCE
         						AND RP.BLN_ACTIF		= 1
         						AND RP.BLN_EN_COURS	= 1
         						AND T.BLN_ACTIF			= 1
         						AND
         						(
         							@ID_TYPE_BENEFICIAIRE = 1
         							AND T.ID_ETABLISSEMENT_BENEF = @ID_BENEF

         							OR @ID_TYPE_BENEFICIAIRE = 2
         							AND T.ID_ETABLISSEMENT_OF_BENEF = @ID_BENEF

         							OR @ID_TYPE_BENEFICIAIRE = 3
         							AND T.ID_TIERS_BENEF = @ID_BENEF
         						)
         				END
         		END

         	UPDATE
         		COMPTEUR

         	SET
         		NUM_CPT_TMP = (SELECT isnull(max(NUM_VIREMENT), 0) + 1 FROM REGLEMENT_PRO RP)
         	WHERE
         		COD_CPT = 'VIREMENT_PRO'
         END

 CREATE PROCEDURE [dbo].[MVT_BUDGETAIRE_PEC_INS_EVENEMENT_PEC]
         
          @LIBL_EVENEMENT   varchar(50), 
          @DAT_EVENEMENT   datetime, 
          @ID_TYPE_EVENEMENT  int, 
          @ID_POSTE_COUT_ENGAGE int,
          @ID_POSTE_COUT_REGLE int,
          @ID_UTILISATEUR   int,
          @COM_EVENEMENT   varchar(7600),
          @ID_EVENEMENT   int = null out
         AS
         BEGIN
          DECLARE
           @COD_TYPE_EVENEMENT     varchar(8),
           @BLN_PB_EVENEMENT     tinyint,
           @TYPE_OBJET_COMMENTAIRE_EVENEMENT int,
           @LIB_PB_EVENEMENT     varchar(100),
           @ID_MODULE_PEC      int,
           @ID_EVENEMENT_MAX     int,
           @ID_EVENEMENT_MIN     int,
           @COD_DERNIER_TYPE_EVENEMENT   varchar(8),
           @MNT_SOLDE_ENGAGEMENT    float

          BEGIN

           
           SET @BLN_PB_EVENEMENT = 0
           SELECT @COD_TYPE_EVENEMENT = COD_TYPE_EVENEMENT
           FROM TYPE_EVENEMENT
           WHERE ID_TYPE_EVENEMENT = @ID_TYPE_EVENEMENT

           IF @ID_POSTE_COUT_ENGAGE IS NOT NULL 
           BEGIN
            IF EXISTS (
               SELECT 1
               FROM EVENEMENT
               INNER JOIN TYPE_EVENEMENT ON EVENEMENT.ID_TYPE_EVENEMENT = TYPE_EVENEMENT.ID_TYPE_EVENEMENT
               WHERE EVENEMENT.ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
               AND  COD_TYPE_EVENEMENT IN ('ENGAGMT', 'DESENG')
               AND  COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT
              )
            BEGIN
             SET @BLN_PB_EVENEMENT = 1
             SET @LIB_PB_EVENEMENT = 'Detection tentative d association de plusieurs evenements de meme type a un meme poste de cout'
            END

            IF @COD_TYPE_EVENEMENT = 'ANCLOPEC' 
            BEGIN   

             SELECT @ID_MODULE_PEC = ID_MODULE_PEC 
             FROM POSTE_COUT_ENGAGE
             WHERE ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE

             SELECT @ID_EVENEMENT_MAX = MAX(MVT_BUDGETAIRE.ID_EVENEMENT)
             from MVT_BUDGETAIRE
             INNER JOIN EVENEMENT ON EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT
             WHERE ID_MODULE_PEC = @ID_MODULE_PEC
             AND ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE

             SELECT @COD_DERNIER_TYPE_EVENEMENT = COD_TYPE_EVENEMENT
             FROM EVENEMENT  
             INNER JOIN TYPE_EVENEMENT ON EVENEMENT .ID_TYPE_EVENEMENT = TYPE_EVENEMENT.ID_TYPE_EVENEMENT
             WHERE ID_EVENEMENT = @ID_EVENEMENT_MAX

             IF @COD_DERNIER_TYPE_EVENEMENT NOT IN ('CLOMOPEC', 'CLOTACT') 
             BEGIN

              SELECT @MNT_SOLDE_ENGAGEMENT = CAST(SUM(MNT_MVT_BUDGETAIRE) AS DECIMAL(15,2))
              FROM MVT_BUDGETAIRE
              WHERE P_E_R IN ('E')
              AND  MVT_BUDGETAIRE.ID_MODULE_PEC = @ID_MODULE_PEC
              GROUP BY MVT_BUDGETAIRE.ID_MODULE_PEC

              IF ABS(@MNT_SOLDE_ENGAGEMENT)>0
              BEGIN
               SET @BLN_PB_EVENEMENT = 1
               SELECT @LIB_PB_EVENEMENT = 'Le dernier evenement associe a ce module n est pas une cloture et le solde d engagement du module > 0. Decloture impossible'
              END
             END

             IF NOT @BLN_PB_EVENEMENT = 1
             BEGIN
              

              SELECT @ID_EVENEMENT_MAX = MAX(MVT_BUDGETAIRE.ID_EVENEMENT)
              FROM MVT_BUDGETAIRE
              INNER JOIN EVENEMENT  ON EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT 
              INNER JOIN TYPE_EVENEMENT ON EVENEMENT .ID_TYPE_EVENEMENT = TYPE_EVENEMENT.ID_TYPE_EVENEMENT
              WHERE COD_TYPE_EVENEMENT IN ('CLOMOPEC', 'CLOTACT')
              AND ID_MODULE_PEC = @ID_MODULE_PEC
              AND ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE

              SELECT @ID_EVENEMENT_MIN = MIN(MVT_BUDGETAIRE.ID_EVENEMENT)
              FROM  MVT_BUDGETAIRE
              INNER JOIN EVENEMENT  ON EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT 
              INNER JOIN TYPE_EVENEMENT ON EVENEMENT .ID_TYPE_EVENEMENT = TYPE_EVENEMENT.ID_TYPE_EVENEMENT
              WHERE  MVT_BUDGETAIRE.ID_EVENEMENT <= @ID_EVENEMENT_MAX
              AND   ID_MODULE_PEC = @ID_MODULE_PEC
              AND   COD_TYPE_EVENEMENT IN ('CLOMOPEC', 'CLOTACT')
              AND   ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
              AND NOT EXISTS 
              (
               select ID_MODULE_PEC, M.ID_EVENEMENT, ID_POSTE_COUT_ENGAGE, COD_TYPE_EVENEMENT, MVT_BUDGETAIRE.* 
               from MVT_BUDGETAIRE M
               INNER JOIN EVENEMENT  ON EVENEMENT.ID_EVENEMENT = M.ID_EVENEMENT 
               INNER JOIN TYPE_EVENEMENT ON EVENEMENT .ID_TYPE_EVENEMENT = TYPE_EVENEMENT.ID_TYPE_EVENEMENT
               where M.ID_EVENEMENT <= @ID_EVENEMENT_MAX
               AND ID_MODULE_PEC = @ID_MODULE_PEC
               AND COD_TYPE_EVENEMENT NOT IN ('CLOMOPEC', 'CLOTACT')
               AND M.ID_EVENEMENT > MVT_BUDGETAIRE.ID_EVENEMENT
               AND ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
              )

             END
            END
           END
           IF @ID_POSTE_COUT_REGLE IS NOT NULL 
           BEGIN
            IF EXISTS (
               SELECT 1
               FROM EVENEMENT
               INNER JOIN TYPE_EVENEMENT ON EVENEMENT.ID_TYPE_EVENEMENT = TYPE_EVENEMENT.ID_TYPE_EVENEMENT
               WHERE EVENEMENT.ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE
               AND  COD_TYPE_EVENEMENT IN ('REGLEMT')
              )
            BEGIN
             SET @BLN_PB_EVENEMENT = 1
             SET @LIB_PB_EVENEMENT = 'Detection tentative d association de plusieurs evenements de meme type a un meme poste de cout'
            END
           END 

           IF @BLN_PB_EVENEMENT = 1
           BEGIN

            INSERT INTO PB_EVENEMENT_MVT_BUDGETAIRE_PEC
            (ID_EVENEMENT   ,
            LIB_PB_EVENEMENT  ,
            LIBL_EVENEMENT   ,
            DAT_EVENEMENT   ,
            ID_TYPE_EVENEMENT  ,
            ID_POSTE_COUT_ENGAGE ,
            ID_POSTE_COUT_REGLE  ,
            ID_UTILISATEUR   ,
            COM_EVENEMENT   
            )
            VALUES
            (
            @ID_EVENEMENT   ,
            @LIB_PB_EVENEMENT  ,
            @LIBL_EVENEMENT   ,
            @DAT_EVENEMENT   ,
            @ID_TYPE_EVENEMENT  ,
            @ID_POSTE_COUT_ENGAGE ,
            @ID_POSTE_COUT_REGLE ,
            @ID_UTILISATEUR   ,
            @COM_EVENEMENT   
            )
           END
           ELSE
           BEGIN

            INSERT INTO EVENEMENT 
            (LIBL_EVENEMENT, DAT_EVENEMENT, ID_TYPE_EVENEMENT, ID_POSTE_COUT_ENGAGE, ID_POSTE_COUT_REGLE)
            VALUES
            (@LIBL_EVENEMENT, @DAT_EVENEMENT, @ID_TYPE_EVENEMENT, @ID_POSTE_COUT_ENGAGE, @ID_POSTE_COUT_REGLE)
            SET @ID_EVENEMENT = @@IDENTITY   

            SET @TYPE_OBJET_COMMENTAIRE_EVENEMENT = 16
            IF @ID_UTILISATEUR IS NOT NULL AND LEN(LTRIM(RTRIM(@COM_EVENEMENT)))>0
            BEGIN
             INSERT INTO COMMENTAIRES 
             (TYPE_OBJET, ID_OBJET, COMMENTAIRE, ID_UTILISATEUR, [DATE])
             VALUES
             (@TYPE_OBJET_COMMENTAIRE_EVENEMENT, @ID_EVENEMENT, @COM_EVENEMENT, @ID_UTILISATEUR, @DAT_EVENEMENT)  
            END

            EXEC dbo.MVT_BUDGETAIRE_PEC_GENERATION @ID_EVENEMENT, @ID_EVENEMENT_MIN, @ID_EVENEMENT_MAX

           END 

          END
         END

 CREATE PROCEDURE [dbo].[CHIFFRAGE_VALIDATION] 
          @ID_MODULE INT,
          @RETURN_BLN_MODIFIED INT OUTPUT
         AS

         BEGIN
          SET NOCOUNT ON;

          DECLARE
           @ID_OPE_FIN INT,
           @CIBLE_ACTION INT

          SET @RETURN_BLN_MODIFIED = 0

          SELECT
           @ID_OPE_FIN = ID_OPERATION_FINANCIERE,
           @CIBLE_ACTION = CIBLE_ACTION
          FROM
           ACTION_PEC
           INNER JOIN MODULE_PEC
            ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
          WHERE
           ID_MODULE_PEC = @ID_MODULE

          IF (@CIBLE_ACTION = 1)
          BEGIN
           RETURN   
          END

          SELECT
           PLAN_FINANCEMENT_OPERATION.ID_ENVELOPPE,
           ID_SOUS_TYPE_COUT
          INTO
           #ENVELOPPES
          FROM
           PLAN_FINANCEMENT_OPERATION
           INNER JOIN ENVELOPPE
            ON PLAN_FINANCEMENT_OPERATION.ID_ENVELOPPE IS NOT NULL
             AND PLAN_FINANCEMENT_OPERATION.ID_ENVELOPPE = ENVELOPPE.ID_ENVELOPPE
           INNER JOIN TYPE_ENVELOPPE
            ON TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE  = ENVELOPPE.ID_TYPE_ENVELOPPE
          WHERE
           ID_OPERATION_FINANCIERE = @ID_OPE_FIN
           AND ID_TYPE_FINANCEMENT IS NULL
           AND PLAN_FINANCEMENT_OPERATION.ID_ENVELOPPE IS NOT NULL  

          DECLARE
           @ID_PCR    INT,
           @ID_STC    INT,
           @ID_PCE    INT,
           @TAU_TVA   DECIMAL(10,5),
           @ID_UNITE_STAGIAIRE INT,
           @HR_ENGAGE   DECIMAL(18,2),
           @HR_REGLE   DECIMAL(18,2)

          DECLARE CURS_PCR CURSOR FOR 
          SELECT DISTINCT
           POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE,
           ID_SOUS_TYPE_COUT,
           POSTE_COUT_REGLE.TAU_TVA,
           PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE,
           UNITE_STAGIAIRE.NB_HEURE_ENGAGE,
           UNITE_STAGIAIRE.NB_HEURE_REGLE
          FROM
           PLAN_FINANCEMENT_US
           INNER JOIN POSTE_COUT_REGLE
            ON POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE = PLAN_FINANCEMENT_US.ID_POSTE_COUT_REGLE
           INNER JOIN UNITE_STAGIAIRE
            ON UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE = PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE
          WHERE
           POSTE_COUT_REGLE.ID_MODULE_PEC = @ID_MODULE
           AND POSTE_COUT_REGLE.DAT_BAP IS NULL
           AND POSTE_COUT_REGLE.BLN_ACTIF = 1
           AND PLAN_FINANCEMENT_US.ID_ENVELOPPE IS NOT NULL
           AND PLAN_FINANCEMENT_US.ID_ENVELOPPE  NOT IN
           (
            SELECT
             ID_ENVELOPPE
            FROM
             #ENVELOPPES
            WHERE
             #ENVELOPPES.ID_SOUS_TYPE_COUT = POSTE_COUT_REGLE.ID_SOUS_TYPE_COUT
           )

          OPEN CURS_PCR
          FETCH NEXT FROM CURS_PCR INTO
           @ID_PCR,
           @ID_STC,
           @TAU_TVA,
           @ID_UNITE_STAGIAIRE,
           @HR_ENGAGE,
           @HR_REGLE

          WHILE (@@FETCH_STATUS = 0)
          BEGIN

           SELECT
            @ID_PCE = ID_POSTE_COUT_ENGAGE
           FROM
            POSTE_COUT_ENGAGE
           WHERE
            ID_SOUS_TYPE_COUT = @ID_STC
            AND ID_MODULE_PEC = @ID_MODULE
            AND DAT_DESENGAGEMENT IS NULL

           DELETE FROM
            PLAN_FINANCEMENT_US
           WHERE 
            ID_POSTE_COUT_REGLE = @ID_PCR
            AND ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE

           INSERT INTO dbo.PLAN_FINANCEMENT_US
           (
            DAT_PLAN_FINANCEMENT_US,
            MNT_PLAN_FINANCEMENT_US,
            BLN_ACTIF,
            BLN_DECOUVERT,
            COM_PLAN_FINANCEMENT_US,
            ID_UNITE_STAGIAIRE,
            ID_TYPE_FINANCEMENT,
            ID_ENVELOPPE,
            ID_POSTE_COUT_ENGAGE,
            ID_POSTE_COUT_REGLE,
            BLN_ATTENTE_FONDS,
            ID_VIREMENT_INTERNE_PEC,
            MNT_PLAN_FINANCEMENT_US_TVA
           )
           SELECT
            GetDate(),
            round([MNT_PLAN_FINANCEMENT_US]* (@HR_REGLE / @HR_ENGAGE),2),
            BLN_ACTIF,
            BLN_DECOUVERT,
            'Recr‚‚ par ChiffrageValidation le ' + convert(varchar(10),Getdate(), 104),
            @ID_UNITE_STAGIAIRE,
            ID_TYPE_FINANCEMENT,
            ID_ENVELOPPE,
            NULL,
            @ID_PCR,
            0,
            NULL,
            round([MNT_PLAN_FINANCEMENT_US] * (@HR_REGLE / @HR_ENGAGE) * @TAU_TVA, 2)
           FROM
            PLAN_FINANCEMENT_US
           WHERE
            ID_POSTE_COUT_ENGAGE = @ID_PCE
            AND ID_POSTE_COUT_REGLE IS NULL

           UPDATE
            POSTE_COUT_REGLE
           SET
            BLN_OK_FINANCEMENT = 0,
            ID_ETAT_PEC = 4
           WHERE
            ID_POSTE_COUT_REGLE = @ID_PCR

           SET @RETURN_BLN_MODIFIED = 1

           FETCH NEXT FROM CURS_PCR INTO
            @ID_PCR,
            @ID_STC,
            @TAU_TVA,
            @ID_UNITE_STAGIAIRE,
            @HR_ENGAGE,
            @HR_REGLE
          END
          CLOSE CURS_PCR
          DEALLOCATE CURS_PCR
         END

         CREATE PROCEDURE [dbo].[SP_SUIVI_DELAI_FACTURE]
         	@annee_ref int = NULL 
         AS
         BEGIN
         SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

         if object_id('tempdb..#TMP_FACT') is not null
         	drop table #TMP_FACT
         if object_id('TMP_DELAI_FACT') is not null
         	drop table TMP_DELAI_FACT

         SELECT *
         INTO #TMP_FACT
         FROM
         (
         SELECT 
         [ID Facture] = FACTURE.ID_FACTURE,
         [Type Facture] = TYPE_FACTURE.LIBC_TYPE_FACTURE,
         [Code Interne Facture] = FACTURE.COD_FACTURE,
         [Code Externe Facture] = FACTURE.NUM_FACTURE,

         SOURCE = CASE WHEN FACTURE.ID_UTILISATEUR_CREATEUR = 79 THEN 'D3R' ELSE 'OPTIFORM' END,

  Type_Emetteur_Facture = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL		THEN 'OF'
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL		THEN 'ADH'
       ELSE '?'
       END,
  COD_Emetteur_Facture = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL		THEN CAST(OF_EMET_FAC.COD_OF AS VARCHAR(10))
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL		THEN CAST(ADH_EMET_FAC.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Emetteur_Facture
      = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL  THEN OF_EMET_FAC.LIB_RAISON_SOCIALE 
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL THEN ADH_EMET_FAC.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_Emetteur_Facture
      = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF  IS NOT NULL THEN ET_OF_EMET_FAC.NUM_SIRET
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT  IS NOT NULL THEN ET_ADH_EMET_FAC.NUM_SIRET
       ELSE '?'
       END,  
         [Mnt Facture HT]		= FACTURE.MNT_HT,
         [Date Emission Facture]	= FACTURE.DAT_EMISSION,
         [Date Reception Facture] = FACTURE.DAT_RECEPTION,

         [Date Saisie Facture]	= FACTURE.DAT_CREATION, 
         [Facture Lettree]	= FACTURE.BLN_LETTRE,

         ID_PCR=POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE,
         [Date Creation PCR] = CONVERT(VARCHAR, POSTE_COUT_REGLE.DAT_CREATION, 103),
         ID_MODULE = MODULE_PEC.ID_MODULE_PEC,
         ID_SESSION = SESSION_PEC.ID_SESSION_PEC,
         PERIMETRE = 'PEC', 
         [Date BAP] = POSTE_COUT_REGLE.DAT_BAP,

         [Date Creation SESSION BAP]		= S_BAP.DAT_CREATION, 
         [Date Edition SESSION BAP]		= S_BAP.DAT_EDITION, 
         [Date Reception SESSION BAP]	= S_BAP.DAT_RECEPTION, 
         [NOM UTILISATEUR SESSION BAP]	= U_BAP .LIB_NOM,
         [DT UTILISATEUR SESSION BAP]	= DT_U_BAP.LIBC_AGENCE,

         [Code Reglement] =	CAST(
         					CASE 
         					WHEN REGLEMENT.NUM_CHEQUE IS NOT NULL THEN 'Cheque Nø ' + REGLEMENT.NUM_CHEQUE
         					WHEN REGLEMENT.NUM_VIREMENT IS NOT NULL THEN 'Virement Nø ' + CAST(REGLEMENT.NUM_VIREMENT AS VARCHAR)
         					ELSE '?'
         					END
         					AS VARCHAR (30)),

         IBAN_REGLEMENT = TRANS_REGL.NUM_IBAN,
         [Date Creation Reglement] = REGLEMENT.DAT_REGLEMENT,
         [Date Edition Reglement] = REGLEMENT.DAT_EDITION,
         [Date Validation Reglement] = REGLEMENT.DAT_VALID_REGLEMENT,

         [Code DOSSIER]			= dbo.GetActionPECCode(ACTION_PEC.COD_ACTION_PEC, ACTION_PEC.ANNEE_ACTION_PEC),
         [Code Module]			= MODULE_PEC.COD_MODULE_PEC  ,
         [Code Session]			= SESSION_PEC.COD_SESSION_PEC  ,
         [Date Debut Session ]	= CONVERT(VARCHAR, SESSION_PEC.DAT_DEBUT, 103),
         [Date Fin Session ]		= CONVERT(VARCHAR, SESSION_PEC.DAT_FIN, 103),
         [Code OF Module]			= ISNULL(OF_MODULE.COD_OF, ''),
         [Raison Sociale OF Module]	= ISNULL(OF_MODULE.LIB_RAISON_SOCIALE, ''),
         [Interne/Externe]		= CASE WHEN MODULE_PEC.BLN_EXTERNE = 1 THEN 'Externe' ELSE 'Interne' END,
         [Subroge]				= CASE WHEN POSTE_COUT_REGLE.ID_ETABLISSEMENT IS NULL THEN 'Oui' ELSE 'Non' END,
         [Sous Type Cout]		= SOUS_TYPE_COUT.LIBC_SOUS_TYPE_COUT,
         [Nb Source Financement]	= SOURCE_FINANCEMENT.NB_SOURCE_FINANCEMENT,
         [Dispositif Financement]= 
         						CASE 
         						WHEN SOURCE_FINANCEMENT.NB_SOURCE_FINANCEMENT > 1 
         							THEN 'Multi Financement'
         						WHEN (SOURCE_FINANCEMENT.NB_SOURCE_FINANCEMENT = 1 AND SOURCE_FINANCEMENT.ID_ENVELOPPE IS NOT NULL)
         							THEN DISPOSITIF.LIBL_DISPOSITIF
         						WHEN (SOURCE_FINANCEMENT.NB_SOURCE_FINANCEMENT = 1 AND SOURCE_FINANCEMENT.ID_ENVELOPPE IS NULL)
         							THEN 'Compte Groupe'					
         						END,

         [Montant Impute HT]  =POSTE_COUT_REGLE.MNT_REGLE_HT,
         [Montant BAPE HT]	 = CASE WHEN POSTE_COUT_REGLE.DAT_BAP IS NOT NULL THEN POSTE_COUT_REGLE.MNT_REGLE_HT ELSE 0 END,
         [Montant Regle HT]	 = CASE WHEN REGLEMENT.DAT_VALID_REGLEMENT IS NOT NULL THEN POSTE_COUT_REGLE.MNT_REGLE_HT ELSE 0 END,

         [Code Adherent Financeur] = ADHERENT.COD_ADHERENT,
         [Raison Sociale Adherent Financeur] = ADHERENT.LIB_RAISON_SOCIALE,
         [SIRET Financeur] = ETABLISSEMENT.NUM_SIRET,
         [SIREN Financeur] = ADHERENT.NUM_SIREN,
         [Branche Etablissement Financeur] = BRANCHE.LIBC_BRANCHE,
         [Agence Etablissement Financeur] = DT_ETAB.LIBC_AGENCE,
         [Code Groupe Etablissement Financeur] = GROUPE.COD_GROUPE,
         [AGF Etablissement Financeur] = AGF_ETAB.LIB_NOM,
         [Code Departement Etablissement Financeur] = ISNULL(DEPARTEMENT.COD_DEPARTEMENT, '?'),
         [Region Admin Etablissement Financeur] = ISNULL(REGION_ADMINISTRATIVE.LIBC_REGION_ADMINISTRATIVE, '?'),
         [Montant Finance] = FINANCE_ETABLISSEMENT.MNT_FINANCE,

TRANS_REGL	.ID_TRANSACTION,

  Type_Destinataire = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST		IS NOT NULL		THEN 'OF'
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST		IS NOT NULL		THEN 'ADH'
       ELSE '?'
       END,
  COD_Destinataire = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST		IS NOT NULL		THEN CAST(OF_DEST.COD_OF AS VARCHAR(10))
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST		IS NOT NULL		THEN CAST(ADH_DEST.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Destinataire
      = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST		IS NOT NULL  THEN OF_DEST.LIB_RAISON_SOCIALE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST		IS NOT NULL THEN ADH_DEST.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_Destinataire
      = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST  IS NOT NULL THEN ET_OF_DEST.NUM_SIRET
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST  IS NOT NULL THEN ET_ADH_DEST.NUM_SIRET
       ELSE '?'
       END,  

  Type_Beneficiaire = CASE 
       WHEN  TRANS_REGL.ID_TIERS_BENEF    IS NOT NULL THEN 'Tiers'
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF IS NOT NULL THEN 'OF'
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF  IS NOT NULL THEN 'ADH'
       ELSE '?'
       END,
  COD_Beneficiaire = CASE 
       WHEN TRANS_REGL.ID_TIERS_BENEF				IS NOT NULL THEN CAST(COD_TIERS AS VARCHAR(10))
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF	IS NOT NULL THEN CAST(OF_BENEF.COD_OF AS VARCHAR(10))
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF		IS NOT NULL THEN CAST(ADH_BENEF.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Beneficiaire 
      = CASE 
       WHEN TRANS_REGL.ID_TIERS_BENEF    IS NOT NULL THEN TIERS.LIB_NOM 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF IS NOT NULL THEN OF_BENEF.LIB_RAISON_SOCIALE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF  IS NOT NULL THEN ADH_BENEF.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_BENEFICIAIRE
      = CASE 
       WHEN TRANS_REGL.ID_TIERS_BENEF    IS NOT NULL THEN TIERS.NUM_SIRET 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF IS NOT NULL THEN ET_OF_BENEF.NUM_SIRET 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF  IS NOT NULL THEN ET_ADH_BENEF.NUM_SIRET 
       ELSE '?'
       END

         FROM POSTE_COUT_REGLE
         INNER JOIN SESSION_PEC		ON SESSION_PEC.ID_SESSION_PEC			= POSTE_COUT_REGLE.ID_SESSION_PEC
         INNER JOIN MODULE_PEC		ON MODULE_PEC.ID_MODULE_PEC				= SESSION_PEC.ID_MODULE_PEC
         INNER JOIN ACTION_PEC		ON ACTION_PEC.ID_ACTION_PEC				= MODULE_PEC.ID_ACTION_PEC 
         INNER JOIN SOUS_TYPE_COUT	ON POSTE_COUT_REGLE.ID_SOUS_TYPE_COUT	= SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT
         LEFT JOIN [SESSION] S_BAP	ON POSTE_COUT_REGLE.ID_SESSION			= S_BAP.ID_SESSION
         LEFT JOIN UTILISATEUR U_BAP ON U_BAP .ID_UTILISATEUR				= S_BAP.ID_UTILISATEUR
         LEFT JOIN REGLEMENT			ON POSTE_COUT_REGLE.ID_REGLEMENT		= REGLEMENT .ID_REGLEMENT 

         LEFT JOIN ETABLISSEMENT_OF		ETOF_MODULE ON MODULE_PEC.ID_ETABLISSEMENT_OF	= ETOF_MODULE.ID_ETABLISSEMENT_OF
         LEFT JOIN ORGANISME_FORMATION	OF_MODULE	ON OF_MODULE.ID_OF					= ETOF_MODULE.ID_OF

         LEFT JOIN AGENCE DT_U_BAP	ON U_BAP.ID_AGENCE						= DT_U_BAP.ID_AGENCE
         INNER JOIN FACTURE			ON POSTE_COUT_REGLE.ID_FACTURE			= FACTURE.ID_FACTURE		
         INNER JOIN TYPE_FACTURE		ON TYPE_FACTURE.ID_TYPE_FACTURE			= FACTURE.ID_TYPE_FACTURE

left join ETABLISSEMENT_OF		ET_OF_EMET_FAC		ON FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF = ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_EMET_FAC			ON ET_OF_EMET_FAC.ID_OF					= OF_EMET_FAC.ID_OF
left join ETABLISSEMENT			ET_ADH_EMET_FAC		ON FACTURE.ID_EMETTEUR_ETABLISSEMENT_ADH= ET_ADH_EMET_FAC.ID_ETABLISSEMENT
left join ADHERENT				ADH_EMET_FAC		ON ADH_EMET_FAC.ID_ADHERENT				= ET_ADH_EMET_FAC.ID_ADHERENT

         INNER JOIN
         	(
         	SELECT ID_POSTE_COUT_REGLE , NB_SOURCE_FINANCEMENT = COUNT( DISTINCT ID_TYPE_FINANCEMENT) + COUNT( DISTINCT ID_ENVELOPPE), ID_TYPE_FINANCEMENT = MIN( ID_TYPE_FINANCEMENT), ID_ENVELOPPE = MIN( ID_ENVELOPPE)
         	FROM PLAN_FINANCEMENT_US 
         	WHERE ABS(MNT_PLAN_FINANCEMENT_US) > 0
         	GROUP BY ID_POSTE_COUT_REGLE 
         	)	SOURCE_FINANCEMENT	ON SOURCE_FINANCEMENT. ID_POSTE_COUT_REGLE  = POSTE_COUT_REGLE .ID_POSTE_COUT_REGLE 
         LEFT JOIN ENVELOPPE			ON ENVELOPPE.ID_ENVELOPPE = SOURCE_FINANCEMENT	.ID_ENVELOPPE 
         LEFT JOIN TYPE_ENVELOPPE	ON ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE
         LEFT JOIN DISPOSITIF		ON DISPOSITIF.ID_DISPOSITIF = TYPE_ENVELOPPE.ID_DISPOSITIF

         INNER JOIN
         	(

         	SELECT ID_POSTE_COUT_REGLE, ID_ETABLISSEMENT, MNT_FINANCE = SUM(MNT_PLAN_FINANCEMENT_US)
         	FROM STAGIAIRE_PEC 
         	INNER JOIN UNITE_STAGIAIRE		ON UNITE_STAGIAIRE .ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
         	INNER JOIN PLAN_FINANCEMENT_US	ON UNITE_STAGIAIRE .ID_UNITE_STAGIAIRE = PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE 
         	GROUP BY ID_POSTE_COUT_REGLE, ID_ETABLISSEMENT
         	) FINANCE_ETABLISSEMENT
         	ON FINANCE_ETABLISSEMENT.ID_POSTE_COUT_REGLE = POSTE_COUT_REGLE .ID_POSTE_COUT_REGLE 
         INNER JOIN ETABLISSEMENT			ON  FINANCE_ETABLISSEMENT.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT 
         INNER JOIN ADHERENT					ON  ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         INNER JOIN GROUPE					ON  GROUPE.ID_GROUPE = ETABLISSEMENT.ID_GROUPE

         INNER JOIN ADRESSE					ON	ADRESSE.ID_ADRESSE = ETABLISSEMENT.ID_ADRESSE_PRINCIPALE
         INNER JOIN BRANCHE					ON  ETABLISSEMENT.ID_BRANCHE = BRANCHE .ID_BRANCHE 
         INNER JOIN AGENCE DT_ETAB			ON  ETABLISSEMENT.ID_AGENCE = DT_ETAB.ID_AGENCE
         LEFT JOIN DEPARTEMENT				ON	DEPARTEMENT.ID_DEPARTEMENT = LEFT(ADRESSE.LIB_CP_CEDEX, 2)
         LEFT JOIN REGION_ADMINISTRATIVE		ON	REGION_ADMINISTRATIVE.ID_REGION_ADMINISTRATIVE = DEPARTEMENT.ID_REGION_ADMINISTRATIVE
         LEFT JOIN UTILISATEUR AGF_ETAB		ON	AGF_ETAB.ID_UTILISATEUR = ETABLISSEMENT.ID_CHARGEE_RELATION

         LEFT JOIN [TRANSACTION]	TRANS_REGL	ON  POSTE_COUT_REGLE.ID_TRANSACTION = TRANS_REGL.ID_TRANSACTION

left join ETABLISSEMENT_OF		ET_OF_DEST		ON TRANS_REGL.ID_ETABLISSEMENT_OF_DEST   = ET_OF_DEST.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_DEST			ON OF_DEST.ID_OF      = ET_OF_DEST.ID_OF
left join ETABLISSEMENT			ET_ADH_DEST		ON TRANS_REGL.ID_ETABLISSEMENT_DEST    = ET_ADH_DEST.ID_ETABLISSEMENT
left join ADHERENT				ADH_DEST		ON ADH_DEST.ID_ADHERENT     = ET_ADH_DEST.ID_ADHERENT

left join ETABLISSEMENT_OF		ET_OF_BENEF		ON TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF = ET_OF_BENEF.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_BENEF		ON OF_BENEF.ID_OF						= ET_OF_BENEF.ID_OF
left join ETABLISSEMENT			ET_ADH_BENEF	ON TRANS_REGL.ID_ETABLISSEMENT_BENEF    = ET_ADH_BENEF.ID_ETABLISSEMENT
left join ADHERENT				ADH_BENEF		ON ADH_BENEF.ID_ADHERENT				= ET_ADH_BENEF.ID_ADHERENT
left join TIERS					TIERS			ON TIERS.ID_TIERS						= TRANS_REGL.ID_TIERS_BENEF

UNION

         SELECT 
         [ID Facture]				= FACTURE.ID_FACTURE,
         [Type Facture] = TYPE_FACTURE.LIBC_TYPE_FACTURE,
         [Code Interne Facture]		= FACTURE.COD_FACTURE,
         [Code Externe Facture]		= FACTURE.NUM_FACTURE,

         SOURCE = CASE WHEN FACTURE.ID_UTILISATEUR_CREATEUR = 79 THEN 'D3R' ELSE 'OPTIFORM' END,

  Type_Emetteur_Facture = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL		THEN 'OF'
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL		THEN 'ADH'
       ELSE '?'
       END,
  COD_Emetteur_Facture = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL		THEN CAST(OF_EMET_FAC.COD_OF AS VARCHAR(10))
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL		THEN CAST(ADH_EMET_FAC.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Emetteur_Facture
      = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL  THEN OF_EMET_FAC.LIB_RAISON_SOCIALE 
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL THEN ADH_EMET_FAC.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_Emetteur_Facture
      = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF  IS NOT NULL THEN ET_OF_EMET_FAC.NUM_SIRET
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT  IS NOT NULL THEN ET_ADH_EMET_FAC.NUM_SIRET
       ELSE '?'
       END,  
         [Mnt Facture HT]			= FACTURE.MNT_HT,
         [Date Emission Facture]	= FACTURE.DAT_EMISSION,
         [Date Reception Facture] = FACTURE.DAT_RECEPTION,
         [Date Saisie Facture]	= ISNULL(FACTURE.DAT_MODIF, FACTURE.DAT_CREATION),
         [Facture Lettree]	= FACTURE.BLN_LETTRE,

         ID_PCR						= SESSION_PRO.ID_SESSION_PRO,
         [Date Creation PCR]			= SESSION_PRO.DAT_CREATION,
         ID_MODULE					= MODULE_PRO.ID_MODULE_PRO,
         ID_SESSION					= SESSION_PRO.ID_SESSION_PRO,
         PERIMETRE					= 'CPRO_OF', 
         [Date BAP]					= SESSION_PRO.DAT_BAP_OF,

         [Date Creation SESSION BAP]		= S_BAP.DAT_CREATION, 
         [Date Edition SESSION BAP]		= S_BAP.DAT_EDITION, 
         [Date Reception SESSION BAP]	= S_BAP.DAT_RECEPTION, 
         [NOM UTILISATEUR SESSION BAP]	= U_BAP .LIB_NOM,
         [DT UTILISATEUR SESSION BAP]	= DT_U_BAP.LIBC_AGENCE,

         [Code Reglement] =	CAST(
         					CASE 
         					WHEN REGLEMENT_PRO.NUM_CHEQUE IS NOT NULL THEN 'Cheque Nø ' + REGLEMENT_PRO.NUM_CHEQUE
         					WHEN REGLEMENT_PRO.NUM_VIREMENT IS NOT NULL THEN 'Virement Nø ' + CAST(REGLEMENT_PRO.NUM_VIREMENT AS VARCHAR)
         					ELSE '?'
         					END
         					AS VARCHAR (30)),

         IBAN_REGLEMENT = TRANS_REGL.NUM_IBAN,
         [Date Creation Reglement]	= REGLEMENT_PRO.DAT_REGLEMENT, 
         [Date Edition Reglement]	= REGLEMENT_PRO.DAT_EDITION, 
         [Date Validation Reglement] = REGLEMENT_PRO.DAT_VALID_REGLEMENT, 

         [Code DOSSIER]				= CONTRAT_PRO.COD_CONTRAT_PRO,
         [Code Module]				= MODULE_PRO.COD_MODULE_PRO  ,
         [Code Session]				= SESSION_PRO.COD_SESSION_PRO  ,
         [Date Debut Session ]		= CONVERT(VARCHAR, SESSION_PRO.DAT_DEBUT, 103),
         [Date Fin Session ]			= CONVERT(VARCHAR, SESSION_PRO.DAT_FIN, 103),
         [Code OF Module]			= ISNULL(OF_MODULE.COD_OF, ''),
         [Raison Sociale OF Module]	= ISNULL(OF_MODULE.LIB_RAISON_SOCIALE, ''),
         [Interne/Externe]			= CASE WHEN MODULE_PRO.ID_ETABLISSEMENT_OF IS NOT NULL THEN 'Externe' ELSE 'Interne' END,
         [Subroge]					= CASE WHEN ISNULL(SESSION_PRO.BLN_SUBROGE, 0) = 1  THEN 'Oui' ELSE 'Non' END,
         [Sous Type Cout]			= DISPOSITIF.LIBC_DISPOSITIF + ' '+ TYPE_FORMATION.LIBC_TYPE_FORMATION,
         [Nb Source Financement]		= 1,
         [Dispositif Financement]	= DISPOSITIF.LIBL_DISPOSITIF,

         [Montant Impute HT]		 =	SESSION_PRO.MNT_VERSE_HT_OF,
         [Montant BAPE HT]	 =		CASE WHEN SESSION_PRO.DAT_BAP_OF IS NOT NULL THEN SESSION_PRO.MNT_VERSE_HT_OF ELSE 0 END,
         [Montant Regle HT]	 =		CASE WHEN REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NOT NULL THEN SESSION_PRO.MNT_VERSE_HT_OF ELSE 0 END,

         [Code Adherent Financeur] = ADHERENT.COD_ADHERENT,
         [Raison Sociale Adherent Financeur] = ADHERENT.LIB_RAISON_SOCIALE,
         [SIRET Financeur] = ETABLISSEMENT.NUM_SIRET,
         [SIREN Financeur] = ADHERENT.NUM_SIREN,
         [Branche Etablissement Financeur] = BRANCHE.LIBC_BRANCHE,
         [Agence Etablissement Financeur] = DT_ETAB.LIBC_AGENCE,
         [Code Groupe Etablissement Financeur] = GROUPE.COD_GROUPE,
         [AGF Etablissement Financeur] = AGF_ETAB.LIB_NOM,
         [Code Departement Etablissement Financeur] = ISNULL(DEPARTEMENT.COD_DEPARTEMENT, '?'),
         [Region Admin Etablissement Financeur] = ISNULL(REGION_ADMINISTRATIVE.LIBC_REGION_ADMINISTRATIVE, '?'),
         [Montant Finance] = SESSION_PRO.MNT_VERSE_HT_OF,

TRANS_REGL	.ID_TRANSACTION,

  Type_Destinataire = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST		IS NOT NULL		THEN 'OF'
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST		IS NOT NULL		THEN 'ADH'
       ELSE '?'
       END,
  COD_Destinataire = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST		IS NOT NULL		THEN CAST(OF_DEST.COD_OF AS VARCHAR(10))
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST		IS NOT NULL		THEN CAST(ADH_DEST.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Destinataire
      = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST		IS NOT NULL  THEN OF_DEST.LIB_RAISON_SOCIALE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST		IS NOT NULL THEN ADH_DEST.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_Destinataire
      = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST  IS NOT NULL THEN ET_OF_DEST.NUM_SIRET
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST  IS NOT NULL THEN ET_ADH_DEST.NUM_SIRET
       ELSE '?'
       END,  

  Type_Beneficiaire = CASE 
       WHEN  TRANS_REGL.ID_TIERS_BENEF    IS NOT NULL THEN 'Tiers'
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF IS NOT NULL THEN 'OF'
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF  IS NOT NULL THEN 'ADH'
       ELSE '?'
       END,
  COD_Beneficiaire = CASE 
       WHEN TRANS_REGL.ID_TIERS_BENEF				IS NOT NULL THEN CAST(COD_TIERS AS VARCHAR(10))
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF	IS NOT NULL THEN CAST(OF_BENEF.COD_OF AS VARCHAR(10))
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF		IS NOT NULL THEN CAST(ADH_BENEF.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Beneficiaire 
      = CASE 
       WHEN TRANS_REGL.ID_TIERS_BENEF    IS NOT NULL THEN TIERS.LIB_NOM 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF IS NOT NULL THEN OF_BENEF.LIB_RAISON_SOCIALE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF  IS NOT NULL THEN ADH_BENEF.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_BENEFICIAIRE
      = CASE 
       WHEN TRANS_REGL.ID_TIERS_BENEF    IS NOT NULL THEN TIERS.NUM_SIRET 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF IS NOT NULL THEN ET_OF_BENEF.NUM_SIRET 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF  IS NOT NULL THEN ET_ADH_BENEF.NUM_SIRET 
       ELSE '?'
       END

         FROM	   SESSION_PRO		
         INNER JOIN MODULE_PRO							ON MODULE_PRO.ID_MODULE_PRO				= SESSION_PRO.ID_MODULE_PRO
         INNER JOIN CONTRAT_PRO							ON CONTRAT_PRO.ID_CONTRAT_PRO			= MODULE_PRO.ID_CONTRAT_PRO
         INNER JOIN TYPE_FORMATION						ON MODULE_PRO.ID_TYPE_FORMATION			= TYPE_FORMATION.ID_TYPE_FORMATION
         LEFT JOIN SESSION_BAP_PRO S_BAP					ON SESSION_PRO.ID_SESSION_BAP_PRO_OF	= S_BAP.ID_SESSION_BAP_PRO
         LEFT JOIN UTILISATEUR U_BAP						ON U_BAP .ID_UTILISATEUR				= S_BAP.ID_UTILISATEUR
         LEFT JOIN REGLEMENT_PRO							ON SESSION_PRO.ID_REGLEMENT_PRO_OF		= REGLEMENT_PRO .ID_REGLEMENT_PRO

         LEFT JOIN ETABLISSEMENT_OF	ETOF_MODULE 		ON MODULE_PRO.ID_ETABLISSEMENT_OF		= ETOF_MODULE.ID_ETABLISSEMENT_OF
         LEFT JOIN ORGANISME_FORMATION	OF_MODULE		ON OF_MODULE.ID_OF						= ETOF_MODULE.ID_OF

         LEFT JOIN AGENCE	DT_U_BAP					ON U_BAP.ID_AGENCE						= DT_U_BAP.ID_AGENCE
         INNER JOIN FACTURE								ON SESSION_PRO.ID_FACTURE_OF			= FACTURE.ID_FACTURE		
         INNER JOIN TYPE_FACTURE							ON TYPE_FACTURE.ID_TYPE_FACTURE			= FACTURE.ID_TYPE_FACTURE

left join ETABLISSEMENT_OF		ET_OF_EMET_FAC		ON FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF = ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_EMET_FAC			ON ET_OF_EMET_FAC.ID_OF					= OF_EMET_FAC.ID_OF
left join ETABLISSEMENT			ET_ADH_EMET_FAC		ON FACTURE.ID_EMETTEUR_ETABLISSEMENT_ADH= ET_ADH_EMET_FAC.ID_ETABLISSEMENT
left join ADHERENT				ADH_EMET_FAC		ON ADH_EMET_FAC.ID_ADHERENT				= ET_ADH_EMET_FAC.ID_ADHERENT

         INNER JOIN DISPOSITIF							ON DISPOSITIF.ID_DISPOSITIF = TYPE_FORMATION.ID_DISPOSITIF

         INNER JOIN ETABLISSEMENT						ON  CONTRAT_PRO.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT 
         INNER JOIN ADHERENT								ON  ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         INNER JOIN GROUPE								ON  GROUPE.ID_GROUPE = ETABLISSEMENT.ID_GROUPE

         INNER JOIN ADRESSE								ON	ADRESSE.ID_ADRESSE = ETABLISSEMENT.ID_ADRESSE_PRINCIPALE
         INNER JOIN BRANCHE								ON  ETABLISSEMENT.ID_BRANCHE = BRANCHE .ID_BRANCHE 
         INNER JOIN AGENCE DT_ETAB						ON  ETABLISSEMENT.ID_AGENCE = DT_ETAB.ID_AGENCE

         LEFT JOIN DEPARTEMENT							ON	DEPARTEMENT.ID_DEPARTEMENT = LEFT(ADRESSE.LIB_CP_CEDEX, 2)
         LEFT JOIN REGION_ADMINISTRATIVE					ON	REGION_ADMINISTRATIVE.ID_REGION_ADMINISTRATIVE = DEPARTEMENT.ID_REGION_ADMINISTRATIVE
         LEFT JOIN UTILISATEUR AGF_ETAB					ON	AGF_ETAB.ID_UTILISATEUR = ETABLISSEMENT.ID_CHARGEE_RELATION

         LEFT JOIN [TRANSACTION]	TRANS_REGL				ON  SESSION_PRO.ID_TRANSACTION_OF = TRANS_REGL.ID_TRANSACTION

left join ETABLISSEMENT_OF		ET_OF_DEST		ON TRANS_REGL.ID_ETABLISSEMENT_OF_DEST   = ET_OF_DEST.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_DEST			ON OF_DEST.ID_OF      = ET_OF_DEST.ID_OF
left join ETABLISSEMENT			ET_ADH_DEST		ON TRANS_REGL.ID_ETABLISSEMENT_DEST    = ET_ADH_DEST.ID_ETABLISSEMENT
left join ADHERENT				ADH_DEST		ON ADH_DEST.ID_ADHERENT     = ET_ADH_DEST.ID_ADHERENT

left join ETABLISSEMENT_OF		ET_OF_BENEF		ON TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF = ET_OF_BENEF.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_BENEF		ON OF_BENEF.ID_OF						= ET_OF_BENEF.ID_OF
left join ETABLISSEMENT			ET_ADH_BENEF	ON TRANS_REGL.ID_ETABLISSEMENT_BENEF    = ET_ADH_BENEF.ID_ETABLISSEMENT
left join ADHERENT				ADH_BENEF		ON ADH_BENEF.ID_ADHERENT				= ET_ADH_BENEF.ID_ADHERENT
left join TIERS					TIERS			ON TIERS.ID_TIERS						= TRANS_REGL.ID_TIERS_BENEF

UNION

         SELECT
         [ID Facture]				= FACTURE.ID_FACTURE,
         [Type Facture]			= TYPE_FACTURE.LIBC_TYPE_FACTURE,
         [Code Interne Facture]		= FACTURE.COD_FACTURE,
         [Code Externe Facture]		= FACTURE.NUM_FACTURE,

         SOURCE = CASE WHEN FACTURE.ID_UTILISATEUR_CREATEUR = 79 THEN 'D3R' ELSE 'OPTIFORM' END,

  Type_Emetteur_Facture = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL		THEN 'OF'
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL		THEN 'ADH'
       ELSE '?'
       END,
  COD_Emetteur_Facture = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL		THEN CAST(OF_EMET_FAC.COD_OF AS VARCHAR(10))
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL		THEN CAST(ADH_EMET_FAC.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Emetteur_Facture
      = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL  THEN OF_EMET_FAC.LIB_RAISON_SOCIALE 
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL THEN ADH_EMET_FAC.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_Emetteur_Facture
      = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF  IS NOT NULL THEN ET_OF_EMET_FAC.NUM_SIRET
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT  IS NOT NULL THEN ET_ADH_EMET_FAC.NUM_SIRET
       ELSE '?'
       END,  
         [Mnt Facture HT]			= FACTURE.MNT_HT,
         [Date Emission Facture]	= FACTURE.DAT_EMISSION,
         [Date Reception Facture] = FACTURE.DAT_RECEPTION,
         [Date Saisie Facture]	= ISNULL(FACTURE.DAT_MODIF, FACTURE.DAT_CREATION),
         [Facture Lettree]		= FACTURE.BLN_LETTRE,

         ID_PCR						= SESSION_PRO.ID_SESSION_PRO,
         [Date Creation PCR]			= SESSION_PRO.DAT_CREATION, 
         ID_MODULE					= MODULE_PRO.ID_MODULE_PRO,
         ID_SESSION					= SESSION_PRO.ID_SESSION_PRO,
         PERIMETRE					= 'CPRO_ADH', 
         [Date BAP]					= SESSION_PRO.DAT_BAP_ADH,

         [Date Creation SESSION BAP]		= S_BAP.DAT_CREATION, 
         [Date Edition SESSION BAP]		= S_BAP.DAT_EDITION, 
         [Date Reception SESSION BAP]	= S_BAP.DAT_RECEPTION, 
         [NOM UTILISATEUR SESSION BAP]	= U_BAP .LIB_NOM,
         [DT UTILISATEUR SESSION BAP]	= DT_U_BAP.LIBC_AGENCE,

         [Code Reglement] =	CAST(
         					CASE 
         					WHEN REGLEMENT_PRO.NUM_CHEQUE IS NOT NULL THEN 'Cheque Nø ' + REGLEMENT_PRO.NUM_CHEQUE
         					WHEN REGLEMENT_PRO.NUM_VIREMENT IS NOT NULL THEN 'Virement Nø ' + CAST(REGLEMENT_PRO.NUM_VIREMENT AS VARCHAR)
         					ELSE '?'
         					END
         					AS VARCHAR (30)),

         IBAN_REGLEMENT = TRANS_REGL.NUM_IBAN,
         [Date Creation Reglement] = REGLEMENT_PRO.DAT_REGLEMENT,
         [Date Edition Reglement] = REGLEMENT_PRO.DAT_EDITION, 
         [Date Validation Reglement] = REGLEMENT_PRO.DAT_VALID_REGLEMENT, 

         [Code DOSSIER]				= CONTRAT_PRO.COD_CONTRAT_PRO,
         [Code Module]				= MODULE_PRO.COD_MODULE_PRO  ,
         [Code Session]				= SESSION_PRO.COD_SESSION_PRO  ,
         [Date Debut Session ]		= CONVERT(VARCHAR, SESSION_PRO.DAT_DEBUT, 103),
         [Date Fin Session ]			= CONVERT(VARCHAR, SESSION_PRO.DAT_FIN, 103),
         [Code OF Module]			= ISNULL(OF_MODULE.COD_OF, ''),
         [Raison Sociale OF Module]	= ISNULL(OF_MODULE.LIB_RAISON_SOCIALE, ''),
         [Interne/Externe]			= CASE WHEN MODULE_PRO.ID_ETABLISSEMENT_OF IS NOT NULL THEN 'Externe' ELSE 'Interne' END,
         [Subroge]					= CASE WHEN ISNULL(SESSION_PRO.BLN_SUBROGE, 0) = 1  THEN 'Oui' ELSE 'Non' END,
         [Sous Type Cout]			= DISPOSITIF.LIBC_DISPOSITIF + ' '+ TYPE_FORMATION.LIBC_TYPE_FORMATION,
         [Nb Source Financement]		= 1,
         [Dispositif Financement]	= DISPOSITIF.LIBL_DISPOSITIF,
         [Montant Impute HT]		 =	SESSION_PRO.MNT_VERSE_HT_ADH,
         [Montant BAPE HT]	 =		CASE WHEN SESSION_PRO.DAT_BAP_ADH IS NOT NULL THEN SESSION_PRO.MNT_VERSE_HT_ADH ELSE 0 END,
         [Montant Regle HT]	 =		CASE WHEN REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NOT NULL THEN SESSION_PRO.MNT_VERSE_HT_ADH ELSE 0 END,

         [Code Adherent Financeur] = ADHERENT.COD_ADHERENT,
         [Raison Sociale Adherent Financeur] = ADHERENT.LIB_RAISON_SOCIALE,
         [SIRET Financeur] = ETABLISSEMENT.NUM_SIRET,
         [SIREN Financeur] = ADHERENT.NUM_SIREN,
         [Branche Etablissement Financeur] = BRANCHE.LIBC_BRANCHE,
         [Agence Etablissement Financeur] = DT_ETAB.LIBC_AGENCE,
         [Code Groupe Etablissement Financeur] = GROUPE.COD_GROUPE,
         [AGF Etablissement Financeur] = AGF_ETAB.LIB_NOM,
         [Code Departement Etablissement Financeur] = ISNULL(DEPARTEMENT.COD_DEPARTEMENT, '?'),
         [Region Admin Etablissement Financeur] = ISNULL(REGION_ADMINISTRATIVE.LIBC_REGION_ADMINISTRATIVE, '?'),
         [Montant Finance] = SESSION_PRO.MNT_VERSE_HT_ADH,

TRANS_REGL	.ID_TRANSACTION,

  Type_Destinataire = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST		IS NOT NULL		THEN 'OF'
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST		IS NOT NULL		THEN 'ADH'
       ELSE '?'
       END,
  COD_Destinataire = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST		IS NOT NULL		THEN CAST(OF_DEST.COD_OF AS VARCHAR(10))
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST		IS NOT NULL		THEN CAST(ADH_DEST.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Destinataire
      = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST		IS NOT NULL  THEN OF_DEST.LIB_RAISON_SOCIALE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST		IS NOT NULL THEN ADH_DEST.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_Destinataire
      = CASE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_DEST  IS NOT NULL THEN ET_OF_DEST.NUM_SIRET
       WHEN TRANS_REGL.ID_ETABLISSEMENT_DEST  IS NOT NULL THEN ET_ADH_DEST.NUM_SIRET
       ELSE '?'
       END,  

  Type_Beneficiaire = CASE 
       WHEN  TRANS_REGL.ID_TIERS_BENEF    IS NOT NULL THEN 'Tiers'
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF IS NOT NULL THEN 'OF'
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF  IS NOT NULL THEN 'ADH'
       ELSE '?'
       END,
  COD_Beneficiaire = CASE 
       WHEN TRANS_REGL.ID_TIERS_BENEF				IS NOT NULL THEN CAST(COD_TIERS AS VARCHAR(10))
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF	IS NOT NULL THEN CAST(OF_BENEF.COD_OF AS VARCHAR(10))
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF		IS NOT NULL THEN CAST(ADH_BENEF.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Beneficiaire 
      = CASE 
       WHEN TRANS_REGL.ID_TIERS_BENEF    IS NOT NULL THEN TIERS.LIB_NOM 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF IS NOT NULL THEN OF_BENEF.LIB_RAISON_SOCIALE 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF  IS NOT NULL THEN ADH_BENEF.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_BENEFICIAIRE
      = CASE 
       WHEN TRANS_REGL.ID_TIERS_BENEF    IS NOT NULL THEN TIERS.NUM_SIRET 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF IS NOT NULL THEN ET_OF_BENEF.NUM_SIRET 
       WHEN TRANS_REGL.ID_ETABLISSEMENT_BENEF  IS NOT NULL THEN ET_ADH_BENEF.NUM_SIRET 
       ELSE '?'
       END

         FROM	   SESSION_PRO		
         INNER JOIN MODULE_PRO							ON MODULE_PRO.ID_MODULE_PRO				= SESSION_PRO.ID_MODULE_PRO
         INNER JOIN CONTRAT_PRO							ON CONTRAT_PRO.ID_CONTRAT_PRO			= MODULE_PRO.ID_CONTRAT_PRO
         INNER JOIN TYPE_FORMATION						ON MODULE_PRO.ID_TYPE_FORMATION			= TYPE_FORMATION.ID_TYPE_FORMATION
         LEFT JOIN SESSION_BAP_PRO S_BAP				    ON SESSION_PRO.ID_SESSION_BAP_PRO_ADH	= S_BAP.ID_SESSION_BAP_PRO
         LEFT JOIN UTILISATEUR U_BAP						ON U_BAP .ID_UTILISATEUR				= S_BAP.ID_UTILISATEUR
         LEFT JOIN REGLEMENT_PRO							ON SESSION_PRO.ID_REGLEMENT_PRO_ADH		= REGLEMENT_PRO .ID_REGLEMENT_PRO

         LEFT JOIN ETABLISSEMENT_OF	ETOF_MODULE 		ON MODULE_PRO.ID_ETABLISSEMENT_OF		= ETOF_MODULE.ID_ETABLISSEMENT_OF
         LEFT JOIN ORGANISME_FORMATION	OF_MODULE		ON OF_MODULE.ID_OF						= ETOF_MODULE.ID_OF

         LEFT JOIN AGENCE	DT_U_BAP					ON U_BAP.ID_AGENCE						= DT_U_BAP.ID_AGENCE
         INNER JOIN FACTURE								ON SESSION_PRO.ID_FACTURE_ADHERENT		= FACTURE.ID_FACTURE		
         INNER JOIN TYPE_FACTURE							ON TYPE_FACTURE.ID_TYPE_FACTURE			= FACTURE.ID_TYPE_FACTURE

left join ETABLISSEMENT_OF		ET_OF_EMET_FAC		ON FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF = ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_EMET_FAC			ON ET_OF_EMET_FAC.ID_OF					= OF_EMET_FAC.ID_OF
left join ETABLISSEMENT			ET_ADH_EMET_FAC		ON FACTURE.ID_EMETTEUR_ETABLISSEMENT_ADH= ET_ADH_EMET_FAC.ID_ETABLISSEMENT
left join ADHERENT				ADH_EMET_FAC		ON ADH_EMET_FAC.ID_ADHERENT				= ET_ADH_EMET_FAC.ID_ADHERENT

         INNER JOIN DISPOSITIF							ON DISPOSITIF.ID_DISPOSITIF = TYPE_FORMATION.ID_DISPOSITIF

         INNER JOIN ETABLISSEMENT						ON  CONTRAT_PRO.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT 
         INNER JOIN ADHERENT								ON  ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         INNER JOIN GROUPE								ON  GROUPE.ID_GROUPE = ETABLISSEMENT.ID_GROUPE

         INNER JOIN ADRESSE								ON	ADRESSE.ID_ADRESSE = ETABLISSEMENT.ID_ADRESSE_PRINCIPALE
         INNER JOIN BRANCHE								ON  ETABLISSEMENT.ID_BRANCHE = BRANCHE .ID_BRANCHE 
         INNER JOIN AGENCE DT_ETAB						ON  ETABLISSEMENT.ID_AGENCE = DT_ETAB.ID_AGENCE

         LEFT JOIN DEPARTEMENT							ON	DEPARTEMENT.ID_DEPARTEMENT = LEFT(ADRESSE.LIB_CP_CEDEX, 2)
         LEFT JOIN REGION_ADMINISTRATIVE					ON	REGION_ADMINISTRATIVE.ID_REGION_ADMINISTRATIVE = DEPARTEMENT.ID_REGION_ADMINISTRATIVE
         LEFT JOIN UTILISATEUR AGF_ETAB					ON	AGF_ETAB.ID_UTILISATEUR = ETABLISSEMENT.ID_CHARGEE_RELATION

         LEFT JOIN [TRANSACTION]	TRANS_REGL				ON  SESSION_PRO.ID_TRANSACTION_ADH = TRANS_REGL.ID_TRANSACTION

left join ETABLISSEMENT_OF		ET_OF_DEST		ON TRANS_REGL.ID_ETABLISSEMENT_OF_DEST  = ET_OF_DEST.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_DEST			ON OF_DEST.ID_OF						= ET_OF_DEST.ID_OF
left join ETABLISSEMENT			ET_ADH_DEST		ON TRANS_REGL.ID_ETABLISSEMENT_DEST		= ET_ADH_DEST.ID_ETABLISSEMENT
left join ADHERENT				ADH_DEST		ON ADH_DEST.ID_ADHERENT					= ET_ADH_DEST.ID_ADHERENT

left join ETABLISSEMENT_OF		ET_OF_BENEF		ON TRANS_REGL.ID_ETABLISSEMENT_OF_BENEF = ET_OF_BENEF.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_BENEF		ON OF_BENEF.ID_OF						= ET_OF_BENEF.ID_OF
left join ETABLISSEMENT			ET_ADH_BENEF	ON TRANS_REGL.ID_ETABLISSEMENT_BENEF    = ET_ADH_BENEF.ID_ETABLISSEMENT
left join ADHERENT				ADH_BENEF		ON ADH_BENEF.ID_ADHERENT				= ET_ADH_BENEF.ID_ADHERENT
left join TIERS					TIERS			ON TIERS.ID_TIERS						= TRANS_REGL.ID_TIERS_BENEF

UNION

         SELECT
         [ID Facture]				= FACTURE.ID_FACTURE,
         [Type Facture]				= TYPE_FACTURE.LIBC_TYPE_FACTURE,
         [Code Interne Facture]		= FACTURE.COD_FACTURE,
         [Code Externe Facture]		= FACTURE.NUM_FACTURE,

         SOURCE = CASE WHEN FACTURE.ID_UTILISATEUR_CREATEUR = 79 THEN 'D3R' ELSE 'OPTIFORM' END,

  Type_Emetteur_Facture = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL		THEN 'OF'
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL		THEN 'ADH'
       ELSE '?'
       END,
  COD_Emetteur_Facture = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL		THEN CAST(OF_EMET_FAC.COD_OF AS VARCHAR(10))
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL		THEN CAST(ADH_EMET_FAC.COD_ADHERENT AS VARCHAR(10))
       ELSE '?'
       END,
  RAISON_SOCIALE_Emetteur_Facture
      = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF		IS NOT NULL  THEN OF_EMET_FAC.LIB_RAISON_SOCIALE 
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT		IS NOT NULL THEN ADH_EMET_FAC.LIB_RAISON_SOCIALE  
       ELSE '?'
       END,
  SIRET_Emetteur_Facture
      = CASE 
       WHEN ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF  IS NOT NULL THEN ET_OF_EMET_FAC.NUM_SIRET
       WHEN ET_ADH_EMET_FAC.ID_ETABLISSEMENT  IS NOT NULL THEN ET_ADH_EMET_FAC.NUM_SIRET
       ELSE '?'
       END,  
         [Mnt Facture HT]			= FACTURE.MNT_HT,
         [Date Emission Facture]	= FACTURE.DAT_EMISSION,
         [Date Reception Facture] = FACTURE.DAT_RECEPTION,
         [Date Saisie Facture]	= ISNULL(FACTURE.DAT_MODIF, FACTURE.DAT_CREATION),
         [Facture Lettree]		= FACTURE.BLN_LETTRE,

         ID_PCR						= NULL,
         [Date Creation PCR]			= NULL,
         ID_MODULE					= NULL,
         ID_SESSION					= NULL,
         PERIMETRE					= NULL,
         [Date BAP]					= NULL,

         [Date Creation SESSION BAP]		= NULL, 
         [Date Edition SESSION BAP]		= NULL,
         [Date Reception SESSION BAP]	= NULL,
         [NOM UTILISATEUR SESSION BAP]	= U_FAC.LIB_NOM,
         [DT UTILISATEUR]				= AGENCE.LIBC_AGENCE,

         [Code Reglement] =	NULL,

         IBAN_REGLEMENT = NULL,
         [Date Creation Reglement] = NULL,
         [Date Edition Reglement] = NULL,
         [Date Validation Reglement] = NULL,

         [Code DOSSIER]				= NULL,
         [Code Module]				= NULL,
         [Code Session]				= NULL,
         [Date Debut Session ]		= NULL,
         [Date Fin Session ]			= NULL,
         [Code OF Module]			= NULL,
         [Raison Sociale OF Module]	= NULL,
         [Interne/Externe]			= NULL,
         [Subroge]					= NULL,
         [Sous Type Cout]			= NULL,
         [Nb Source Financement]		= 0,
         [Dispositif Financement]	= NULL,
         [Montant Impute HT]			= 0,
         [Montant BAPE HT]			= 0,
         [Montant Regle HT]			= 0,

         [Code Adherent Financeur] = NULL,
         [Raison Sociale Adherent Financeur] = NULL,
         [SIRET Financeur] = NULL,
         [SIREN Financeur] = NULL,
         [Branche Etablissement Financeur] = NULL,
         [Agence Etablissement Financeur] = NULL,
         [Code Groupe Etablissement Financeur] = NULL,
         [AGF Etablissement Financeur] = NULL,
         [Code Departement Etablissement Financeur] = NULL,
         [Region Admin Etablissement Financeur] = NULL,
         [Montant Finance] = 0,

ID_TRANSACTION = NULL,

  Type_Destinataire = NULL,
  COD_Destinataire = NULL,
  RAISON_SOCIALE_Destinataire      = NULL,
  SIRET_Destinataire = NULL,

  Type_Beneficiaire = NULL,
  COD_Beneficiaire = NULL,
  RAISON_SOCIALE_Beneficiaire = NULL,
  SIRET_BENEFICIAIRE = NULL

         FROM	   

         FACTURE								
         INNER JOIN TYPE_FACTURE							ON TYPE_FACTURE.ID_TYPE_FACTURE			= FACTURE.ID_TYPE_FACTURE

left join ETABLISSEMENT_OF		ET_OF_EMET_FAC		ON FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF = ET_OF_EMET_FAC.ID_ETABLISSEMENT_OF
left join ORGANISME_FORMATION	OF_EMET_FAC			ON ET_OF_EMET_FAC.ID_OF					= OF_EMET_FAC.ID_OF
left join ETABLISSEMENT			ET_ADH_EMET_FAC		ON FACTURE.ID_EMETTEUR_ETABLISSEMENT_ADH= ET_ADH_EMET_FAC.ID_ETABLISSEMENT
left join ADHERENT				ADH_EMET_FAC		ON ADH_EMET_FAC.ID_ADHERENT				= ET_ADH_EMET_FAC.ID_ADHERENT
LEFT JOIN UTILISATEUR			U_FAC				ON U_FAC .ID_UTILISATEUR				= FACTURE.ID_UTILISATEUR_CREATEUR
         LEFT JOIN AGENCE									ON U_FAC.ID_AGENCE						= AGENCE.ID_AGENCE

         WHERE 
         FACTURE.BLN_ACTIF = 1
         AND NOT EXISTS
         (SELECT 1 FROM POSTE_COUT_REGLE WHERE BLN_ACTIF = 1 AND POSTE_COUT_REGLE.ID_FACTURE  = FACTURE.ID_FACTURE )
         AND NOT EXISTS
         (SELECT 1 FROM SESSION_PRO WHERE BLN_ACTIF = 1 AND SESSION_PRO .ID_FACTURE_ADHERENT  = FACTURE.ID_FACTURE )
         AND NOT EXISTS
         (SELECT 1 FROM SESSION_PRO WHERE BLN_ACTIF = 1 AND SESSION_PRO .ID_FACTURE_OF  = FACTURE.ID_FACTURE)

) VUE_BAP_UTILISATEUR
         WHERE [Type Facture] != 'Pr‚-facture'

         AND 

         ( 
         	( 

         		YEAR ([Date Saisie Facture] ) >= coalesce(@annee_ref,YEAR(GETDATE())) - 1
         	) 

         )

         ORDER BY [Code Interne Facture] , ID_PCR

         SELECT 
         [Code Interne Facture] ,
         [Code Externe Facture] ,
         Type_Emetteur_Facture = MAX (Type_Emetteur_Facture ),
COD_Emetteur_Facture = MAX(COD_Emetteur_Facture ),
RAISON_SOCIALE_Emetteur_Facture = MAX(RAISON_SOCIALE_Emetteur_Facture ),
SIRET_Emetteur_Facture= MAX(  SIRET_Emetteur_Facture),
         [Type Facture]			= MIN([Type Facture]),
         [Mnt Facture HT]		= MAX ([Mnt Facture HT]	),
         [Mnt Impute HT]			= ISNULL(SUM ([Montant Impute HT] ), 0),
         [Mnt BAPE HT]			= ISNULL(SUM ([Montant BAPE HT] ), 0),
         [Mnt Regle HT]			= ISNULL(SUM ([Montant Regle HT] ), 0),
         [Facture Lettree]		=		CASE WHEN ISNULL(MAX ([Facture Lettree]), 0) =1 THEN 'Oui' ELSE 'Non' END,
         [Date Emission Facture]	= CONVERT(VARCHAR, MIN([Date Emission Facture]), 103),
         [Date Reception Facture] = CONVERT(VARCHAR, MIN([Date Reception Facture]) , 103),

         [Delai Reception] = CONVERT(int, MIN([Date Reception Facture]) - MIN([Date Emission Facture])) ,

         [Date Saisie Facture]	= CONVERT(VARCHAR, MIN([Date Saisie Facture]), 103),

         [Delai Saisie Facture] = CONVERT(int, MIN([Date Saisie Facture]) - MIN([Date Reception Facture])) ,
         [Date Creation DR] = CONVERT(VARCHAR, MAX([Date Creation PCR] ), 103),
         [Date BAP MIN]= CONVERT(VARCHAR, MIN([Date BAP]), 103),
         [Date BAP MAX]= CONVERT(VARCHAR, MAX([Date BAP]), 103),
         [Delai BAP] = CONVERT(int, ISNULL(MAX([Date BAP]), GETDATE()) - MIN([Date Reception Facture])) ,
         [NB_PCR] = COUNT (distinct ID_PCR),
         [NB_MODULE] = COUNT (distinct ID_MODULE),
         [NB_SESSION] = COUNT (distinct ID_SESSION),

         [Date Creation SESSION BAP]		= CONVERT(VARCHAR, MAX([Date Creation SESSION BAP]	), 103),
         [Date Edition SESSION BAP]		=  CONVERT(VARCHAR, MAX([Date Edition SESSION BAP]), 103),
         [Date Reception SESSION BAP]	=  CONVERT(VARCHAR, MAX([Date Reception SESSION BAP]), 103),
         [Delai Reception SESSION BAP] = CONVERT(int, ISNULL(MAX([Date Reception SESSION BAP]), GETDATE()) - MIN([Date Reception Facture])) ,
         [NOM UTILISATEUR]	= MAX([NOM UTILISATEUR SESSION BAP]),
         [NB UTILISATEUR SESSION BAP]	= COUNT (DISTINCT [NOM UTILISATEUR SESSION BAP]),
         [DT UTILISATEUR SESSION BAP]	= MAX([DT UTILISATEUR SESSION BAP]),
         [Branche Etablissement Finance] = MAX([Branche Etablissement Financeur]),
         [Nb Branche Etablissement Finance] = COUNT ( DISTINCT [Branche Etablissement Financeur]),
         [DT Etablissement Financeur] = MAX([Agence Etablissement Financeur]),
         [Nb DT Etablissement Financeur] = COUNT ( DISTINCT [Agence Etablissement Financeur]),
         [Nb Reglement] = COUNT(distinct [Code Reglement]),
         [Date Creation Reglement] = CONVERT(VARCHAR, MAX([Date Creation Reglement] ),103),
         [Date Edition Reglement] = CONVERT(VARCHAR, MAX([Date Edition Reglement] ), 103),
         [Date Validation Reglement MIN] = CONVERT(VARCHAR, MIN([Date Validation Reglement] ), 103),
         [Date Validation Reglement MAX] = CONVERT(VARCHAR, MAX([Date Validation Reglement] ), 103),
         [Delai Validation Reglement] = CONVERT(int, ISNULL(MAX([Date Validation Reglement] ), GETDATE()) - MIN([Date Reception Facture])) ,
         [Date Lettrage Facture] = CASE WHEN ISNULL(MAX ([Facture Lettree]), 0) =1 THEN CONVERT(VARCHAR, MAX([Date Validation Reglement] ) , 103) ELSE NULL END,
         SOURCE=MAX(SOURCE)
         INTO TMP_DELAI_FACT
         FROM #TMP_FACT
         GROUP BY 
         [Code Interne Facture] ,
         [Code Externe Facture] 
         ORDER BY MIN([Date Saisie Facture]), [Code Interne Facture]

         SELECT * FROM TMP_DELAI_FACT

         END

CREATE PROCEDURE [dbo].[MVT_BUDGETAIRE_PEC_GENERATION]  
         
          @ID_EVENEMENT  INT   ,
          @ID_EVENEMENT_MIN INT = NULL , 
          @ID_EVENEMENT_MAX INT = NULL 
         AS
         DECLARE @COD_TYPE_EVENEMENT varchar(8) ,
           @ID_TYPE_MOUVEMENT int   ,
           @P_E_R    varchar(1) ,
           @LIB_PB_EVENEMENT varchar(100)

         BEGIN

          IF EXISTS (SELECT 1 FROM MVT_BUDGETAIRE WHERE ID_EVENEMENT = @ID_EVENEMENT)
          BEGIN
           select 'Pb : Mvt Budgetaires deja generes pour cet evenement ', *
           FROM EVENEMENT
           WHERE ID_EVENEMENT = @ID_EVENEMENT  

           SET @LIB_PB_EVENEMENT = 'Detection tentative de generation de Mvt Budgetaires deja generes pour cet evenement'

           INSERT INTO PB_EVENEMENT_MVT_BUDGETAIRE_PEC
           (ID_EVENEMENT   ,
           LIB_PB_EVENEMENT  ,
           LIBL_EVENEMENT   ,
           DAT_EVENEMENT   ,
           ID_TYPE_EVENEMENT  ,
           ID_POSTE_COUT_ENGAGE ,
           ID_POSTE_COUT_REGLE  
           )
           SELECT
           ID_EVENEMENT   ,
           @LIB_PB_EVENEMENT  ,
           LIBL_EVENEMENT   ,
           DAT_EVENEMENT   ,
           ID_TYPE_EVENEMENT  ,
           ID_POSTE_COUT_ENGAGE ,
           ID_POSTE_COUT_REGLE  
           FROM EVENEMENT
           WHERE ID_EVENEMENT = @ID_EVENEMENT 
          END
          ELSE
          BEGIN
           SELECT @COD_TYPE_EVENEMENT = TYPE_EVENEMENT .COD_TYPE_EVENEMENT 
           FROM EVENEMENT 
           INNER JOIN TYPE_EVENEMENT ON TYPE_EVENEMENT.ID_TYPE_EVENEMENT = EVENEMENT.ID_TYPE_EVENEMENT 
           WHERE ID_EVENEMENT = @ID_EVENEMENT

           IF @COD_TYPE_EVENEMENT = 'CHIF_PEC'
           BEGIN

            SET @ID_TYPE_MOUVEMENT = 19 
            SET @P_E_R = 'P'
            EXEC MVT_BUDGETAIRE_PEC_ANNULATION @ID_EVENEMENT, @ID_TYPE_MOUVEMENT, @P_E_R 

            SET @ID_TYPE_MOUVEMENT = 18 
            SET @P_E_R = 'P'
            EXEC MVT_BUDGETAIRE_PEC_INS_PLAN_FINANCEMENT @ID_EVENEMENT, @ID_TYPE_MOUVEMENT, @P_E_R 
           END

           IF @COD_TYPE_EVENEMENT = 'DESENG'
           BEGIN

            SET @ID_TYPE_MOUVEMENT = 21 
            SET @P_E_R = 'E'
            EXEC MVT_BUDGETAIRE_PEC_ANNULATION @ID_EVENEMENT, @ID_TYPE_MOUVEMENT, @P_E_R 

           END

           IF @COD_TYPE_EVENEMENT = 'ENGAGMT'
           BEGIN

            SET @ID_TYPE_MOUVEMENT = 19 
            SET @P_E_R = 'P'
            EXEC MVT_BUDGETAIRE_PEC_ANNULATION @ID_EVENEMENT , @ID_TYPE_MOUVEMENT, @P_E_R 

            SET @ID_TYPE_MOUVEMENT = 20 
            SET @P_E_R = 'E'
            EXEC MVT_BUDGETAIRE_PEC_INS_PLAN_FINANCEMENT @ID_EVENEMENT, @ID_TYPE_MOUVEMENT, @P_E_R  
           END 

           IF @COD_TYPE_EVENEMENT = 'ANNULATI'
           BEGIN   

            SET @ID_TYPE_MOUVEMENT = 19 
            SET @P_E_R = 'P'
            EXEC MVT_BUDGETAIRE_PEC_ANNULATION @ID_EVENEMENT , @ID_TYPE_MOUVEMENT, @P_E_R 

            SET @ID_TYPE_MOUVEMENT = 21 
            SET @P_E_R = 'E'
            EXEC MVT_BUDGETAIRE_PEC_ANNULATION @ID_EVENEMENT , @ID_TYPE_MOUVEMENT, @P_E_R 
           END   

           IF @COD_TYPE_EVENEMENT = 'CLOMOPEC'
           BEGIN

            SET @ID_TYPE_MOUVEMENT = 19 
            SET @P_E_R = 'P'
            EXEC MVT_BUDGETAIRE_PEC_ANNULATION @ID_EVENEMENT , @ID_TYPE_MOUVEMENT, @P_E_R 

            SET @ID_TYPE_MOUVEMENT = 21 
            SET @P_E_R = 'E'
            EXEC MVT_BUDGETAIRE_PEC_ANNULATION @ID_EVENEMENT , @ID_TYPE_MOUVEMENT, @P_E_R 
           END   

           IF @COD_TYPE_EVENEMENT = 'REGLEMT'
           BEGIN

            SET @ID_TYPE_MOUVEMENT = 21 
            SET @P_E_R = 'E'
            EXEC MVT_BUDGETAIRE_PEC_INS_PLAN_FINANCEMENT @ID_EVENEMENT , @ID_TYPE_MOUVEMENT, @P_E_R 

            SET @ID_TYPE_MOUVEMENT = 22 
            SET @P_E_R = 'R'
            EXEC MVT_BUDGETAIRE_PEC_INS_PLAN_FINANCEMENT @ID_EVENEMENT, @ID_TYPE_MOUVEMENT, @P_E_R  
           END  

           IF @COD_TYPE_EVENEMENT = 'ANCLOPEC' 
           BEGIN

            SET @ID_TYPE_MOUVEMENT = 20 
            SET @P_E_R = 'E'         
            EXEC MVT_BUDGETAIRE_PEC_ANNULATION @ID_EVENEMENT , @ID_TYPE_MOUVEMENT, @P_E_R, @ID_EVENEMENT_MIN, @ID_EVENEMENT_MAX

            SET @ID_TYPE_MOUVEMENT = 18 
            SET @P_E_R = 'P'         
            EXEC MVT_BUDGETAIRE_PEC_ANNULATION @ID_EVENEMENT , @ID_TYPE_MOUVEMENT, @P_E_R, @ID_EVENEMENT_MIN, @ID_EVENEMENT_MAX
           END  

          END
         END

GO

 CREATE PROCEDURE [dbo].[UPD_DEMANDES_MNT_CHIFFRAGE]
          @ID_UNITE_STAGIAIRE int,
          @MONTANT   decimal(18,2),
          @MONTANT_TVA  decimal(18,2),
          @ID_POSTE_COUT_REGLE int,
          @ID_TYPE int, 

          @ID_POSTE_COUT_ENGAGE int
         AS

         BEGIN

          DECLARE @COUNT INT
          DECLARE @ID_TEMP INT
          DECLARE @MNT_TEMP decimal(18,2)

          IF (@ID_TYPE < 0)
          BEGIN

           SELECT @COUNT = COUNT(*)
           FROM PLAN_FINANCEMENT_US
           WHERE ID_TYPE_FINANCEMENT = - @ID_TYPE
            AND ID_ENVELOPPE IS NULL
            AND ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE
            AND ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE

           IF @COUNT = 0 OR @COUNT IS NULL
           BEGIN
            INSERT INTO PLAN_FINANCEMENT_US
             (DAT_PLAN_FINANCEMENT_US, MNT_PLAN_FINANCEMENT_US,
             BLN_ACTIF, ID_UNITE_STAGIAIRE,  ID_TYPE_FINANCEMENT,
             ID_ENVELOPPE, ID_POSTE_COUT_REGLE, BLN_ATTENTE_FONDS, MNT_PLAN_FINANCEMENT_US_TVA)
            VALUES
             (GETDATE(), @MONTANT, 
             1, @ID_UNITE_STAGIAIRE, - @ID_TYPE, 
             NULL, @ID_POSTE_COUT_REGLE, 1, @MONTANT_TVA)
           END
           ELSE
           BEGIN
            UPDATE PLAN_FINANCEMENT_US
            SET MNT_PLAN_FINANCEMENT_US  = @MONTANT,
             MNT_PLAN_FINANCEMENT_US_TVA = @MONTANT_TVA
            WHERE ID_TYPE_FINANCEMENT = - @ID_TYPE
             AND ID_ENVELOPPE IS NULL
             AND ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE
             AND ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE
           END

          END
          ELSE 
          BEGIN

           SELECT @COUNT = COUNT(*)
           FROM PLAN_FINANCEMENT_US
           WHERE ID_TYPE_FINANCEMENT IS NULL
            AND ID_ENVELOPPE = @ID_TYPE
            AND ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE
            AND ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE

           IF @COUNT = 0 OR @COUNT IS NULL
           BEGIN
            INSERT INTO PLAN_FINANCEMENT_US
             (DAT_PLAN_FINANCEMENT_US, MNT_PLAN_FINANCEMENT_US,
             BLN_ACTIF, ID_UNITE_STAGIAIRE, ID_TYPE_FINANCEMENT,
             ID_ENVELOPPE, ID_POSTE_COUT_REGLE, BLN_ATTENTE_FONDS, MNT_PLAN_FINANCEMENT_US_TVA)
            VALUES
             (GETDATE(), @MONTANT, 
             1, @ID_UNITE_STAGIAIRE, NULL, 
             @ID_TYPE, @ID_POSTE_COUT_REGLE, 1, @MONTANT_TVA)

           END
           ELSE
           BEGIN
            UPDATE PLAN_FINANCEMENT_US
            SET MNT_PLAN_FINANCEMENT_US  = @MONTANT,
             MNT_PLAN_FINANCEMENT_US_TVA = @MONTANT_TVA
            WHERE ID_TYPE_FINANCEMENT IS NULL 
             AND ID_ENVELOPPE = @ID_TYPE
             AND ID_UNITE_STAGIAIRE = @ID_UNITE_STAGIAIRE
             AND ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE
           END 

          END
         END

create procedure CKParser.TheEnd 
as 
begin         
	print 'Everything worked';
end
