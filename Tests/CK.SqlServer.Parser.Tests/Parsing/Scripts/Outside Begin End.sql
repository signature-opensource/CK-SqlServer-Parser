CREATE PROCEDURE [dbo].[CN_GESTION_ARRIVEE_PIECE] 
          @ID_MODULE    int,
          @TYPE_DOSSIER   varchar(3),
          @COD_PIECE varchar(8),
          @ID_UTILISATEUR   int,
          @NOM_FICHIER   varchar(100)
         AS
         -- =============================================
         -- Author:  BBL
         -- Create date: 11/05/2016
         -- MISE A jour des pieces 
         -- =============================================
         BEGIN
         	DECLARE
         		@ID_PIECE INT,
         		@ID_ARRIVEE_PIECE INT,
         		@NUM_FILE INT
         	
         	IF ( @ID_MODULE IS NULL )
         	BEGIN
         		RAISERROR('ID Module is NULL ', 16, 1)
         	END
         
         		
         	IF( @TYPE_DOSSIER = 'PEC')
         	BEGIN
         		select @ID_PIECE = ID_PIECE_PEC from PIECE_PEC where COD_PIECE_PEC = @COD_PIECE
         		
         		IF ( @ID_PIECE IS NULL )
         		BEGIN
         			RAISERROR('Code piece pec erreur  [CN_GESTION_ARRIVEE_PIECE]', 16, 1)
         		END
         		
         		select @ID_ARRIVEE_PIECE= ID_ARRIVEE_PIECE_PEC, @NUM_FILE = NUM_FILE from ARRIVEE_PIECE_PEC where  ID_MODULE_PEC = @ID_MODULE and ID_PIECE_PEC =@ID_PIECE 
         		IF( @ID_ARRIVEE_PIECE IS NULL )
         		BEGIN
         		   INSERT INTO ARRIVEE_PIECE_PEC 
         		   (
         			COD_ARRIVEE_PIECE_PEC, 
         			DAT_ARRIVEE_PIECE_PEC,
         			BLN_ACTIF,
         			BLN_CONFORME,
         			ID_MODULE_PEC,
         			ID_PIECE_PEC,
         			ID_STAGIAIRE_PEC, 
         			ID_ADHERENT,
         			ID_SESSION_PEC,
         			ID_DISPOSITIF,
         			ID_POSTE_COUT_REGLE,
         			NUM_FILE,
         			ID_UTILISATEUR,
         			NOM_FICHIER,
         			NUM_LOT
         		   )
         		   VALUES 
         		   (
         			0,
         			GETDATE(),
         			1,
         			1,
         			@ID_MODULE,
         			@ID_PIECE,
         			NULL, 
         			NULL,
         			NULL,
         			NULL,
         			NULL,
         			1,
         			@ID_UTILISATEUR,
         			@NOM_FICHIER,
         			NULL
         		   )
         
         		   UPDATE
         			ARRIVEE_PIECE_PEC
         		   SET
         			COD_ARRIVEE_PIECE_PEC = SCOPE_IDENTITY()
         		   WHERE
         			ID_ARRIVEE_PIECE_PEC = SCOPE_IDENTITY()
         			
         		END
         		ELSE
         			UPDATE
         				ARRIVEE_PIECE_PEC
         		   SET
         			DAT_ARRIVEE_PIECE_PEC = GETDATE(),
         			ID_UTILISATEUR = @ID_UTILISATEUR,
         			NOM_FICHIER = @NOM_FICHIER,
         			NUM_FILE = @NUM_FILE +1
         		   WHERE
         			ID_ARRIVEE_PIECE_PEC = @ID_ARRIVEE_PIECE
         
         		END
         		
         	END
         	
         	select @ID_PIECE = null
         	
         	IF( @TYPE_DOSSIER = 'PRO' )
         	BEGIN
         		select @ID_PIECE = ID_PIECE_PRO from PIECE_PRO where COD_PIECE_PRO = @COD_PIECE
         		
         		IF ( @ID_PIECE IS NULL )
         		BEGIN
         			RAISERROR('Code piece pro erreur  [CN_GESTION_ARRIVEE_PIECE]', 16, 1)
         		END
         	
         	
         		select @ID_ARRIVEE_PIECE= ID_ARRIVEE_PIECE_PRO, @NUM_FILE = NUM_FILE from ARRIVEE_PIECE_PRO where  ID_MODULE_PRO = @ID_MODULE and ID_PIECE_PRO =@ID_PIECE 
         		IF( @ID_ARRIVEE_PIECE IS NULL )
         		BEGIN
         
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
         					GETDATE(), 
         					1, 
         					1, 
         					NULL,
         					@ID_PIECE, 
         					NULL, 
         					@ID_MODULE, 
         					NULL, 
         					NULL, 
         					NULL, 
         					NULL, 
         					NULL, 
         					NULL, 
         					NULL,
         					@NUM_FILE,
         					@ID_UTILISATEUR,
         					@NOM_FICHIER,
         					NULL
         				)
         
         
         			UPDATE	ARRIVEE_PIECE_PRO
         			SET		COD_ARRIVEE_PIECE_PRO = SCOPE_IDENTITY() 
         			WHERE	ID_ARRIVEE_PIECE_PRO = SCOPE_IDENTITY() 
         	
         		END
         		ELSE
         		BEGIN
         			UPDATE
         				ARRIVEE_PIECE_PRO
         		   SET
         			DAT_ARRIVEE_PIECE_PRO = GETDATE(),
         			ID_UTILISATEUR = @ID_UTILISATEUR,
         			NOM_FICHIER = @NOM_FICHIER,
         			NUM_FILE = @NUM_FILE +1
         		   WHERE
         			ID_ARRIVEE_PIECE_PRO = @ID_ARRIVEE_PIECE
         
         		END
         
         END

GO

         
         
         -- =============================================
         -- Author:		SV
         -- Create date: 14 ao–t 2007
         -- Description:	Ajout d'un contrainte sur le BLN_ACTIF du poste co–t r‚gl‚
         -- =============================================
         -- Author:		KS
         -- Modif. date: 14 sept 2007
         -- Description:	Ajout de l'ID AGENCE
         -- ---------------------------------------------
         -- Modif. date: 17 sept 2007
         -- Description:	bln actif = 2 + null en date ‚dition
         -- =============================================
         -- Author:		SV
         -- Modif. date: 31 octobre 2007
         -- Description:	Ajout de la prise en compte de l'agence
         -- =============================================
         -- Author:		KS
         -- Modif. date: 29 nov 2007
         -- Description:	MANTIS : 0006221 >> MaJ du PCR selon le num iban
         -- ---------------------------------------------
         -- Modif. date: 06 d‚c 2007
         -- Description:	MANTIS : 0006304 >> MaJ des PCR selon mode b‚n‚f. (adh ou ‚tab)
         -- =============================================
         
         
         CREATE PROCEDURE [dbo].[INS_REGLEMENT_20080625]
         (
         	@MODE_EDITION AS TINYINT = 0, -- 0 = CREATION / 1 = REGEN
         	@ID_TRANSACTION AS INT,
         	@ID_AGENCE		AS INT
         )
         
         AS
         BEGIN
         	SET NOCOUNT ON
         	DECLARE @COMPTEUR_ORDRE_VIREMENT AS INT
         	DECLARE @COMPTEUR				AS INT
         	DECLARE @COD_REGLEMENT_PREFIXE	AS VARCHAR(3)
         	DECLARE @NBLIGNES				AS INT
         	DECLARE @ID_TYPE_DESTINATAIRE	AS INT
         	DECLARE @ID_TYPE_BENEFICIAIRE	AS INT
         	DECLARE @ID_BENEF				AS INT
         
         	SET @COMPTEUR = 0
         	SET @COD_REGLEMENT_PREFIXE = ''
         
         
         	SELECT @COMPTEUR_ORDRE_VIREMENT = COALESCE((SELECT NUM_CPT_TMP FROM COMPTEUR WHERE COD_CPT = 'VIREMENT'),0)
         	IF @COMPTEUR_ORDRE_VIREMENT = 0
         	BEGIN
         		SET @COMPTEUR_ORDRE_VIREMENT = COALESCE((SELECT NUM_CPT FROM COMPTEUR WHERE COD_CPT = 'VIREMENT'),0)
         	END
         
         	---------------------------------------------------------------------------------------------------------------------------
         	--								Creation des lignes REGLEMENT dans une table temporaire #REGLEMENT
         	---------------------------------------------------------------------------------------------------------------------------
         	-- [POSTE_COUT_REGLE_POUR_REGLEMENT] peut renvoyer potentiellement 2 lignes si il y a 2 types de b‚n‚f.
         	-- le filtrage sur ID transaction va alors prendre une seule ligne
         	SELECT 
         		PCRPR.ID_TYPE_BENEFICIAIRE	as ID_TYPE_DESTINATAIRE,	-- [erreur dans le script] - ADH ou ETA OF
         		PCRPR.ID_TYPE_DESTINATAIRE	as ID_TYPE_BENEFICIAIRE,	-- erreur dans le script
         		PCRPR.ID_DESTINATAIRE		as ID_BENEF,				-- erreur dans le script
         		-1 AS COD_REGLEMENT,
         		GETDATE() AS DAT_REGLEMENT,
         		-1 AS NUM_VIREMENT,
         		GETDATE() AS DAT_EDITION,
         --		PCRPR.NUM_IBAN,
         		SUM(PCRPR.MNT_REGLE_TTC) AS MNT_REGLE_TTC,
         		SUM(PCRPR.MNT_REGLE_HT) AS MNT_REGLE_HT,
         		PCRPR.ID_TRANSACTION,
         		1 AS BLN_ACTIF,
         		1 AS BLN_EN_COURS,
         
         		CASE
         			WHEN ( EXISTS(SELECT ID_POSTE_COUT_REGLE FROM POSTE_COUT_REGLE WHERE BLN_CRITERE = 1 AND NUM_IBAN = PCRPR.NUM_IBAN AND PCRPR.ID_TRANSACTION = ID_TRANSACTION) ) THEN 1
         			ELSE 0
         		END	AS BLN_CRITERE,
         		0 AS TRAITE
         
         	INTO #REGLEMENT
         	FROM
         		[dbo].[POSTE_COUT_REGLE_POUR_REGLEMENT](@MODE_EDITION) AS PCRPR
         	WHERE 
         		PCRPR.ID_TRANSACTION = @ID_TRANSACTION
         		and PCRPR.ID_AGENCE = @ID_AGENCE
         	GROUP BY 
         		PCRPR.ID_TRANSACTION,
         		PCRPR.NUM_IBAN,
         		PCRPR.ID_TYPE_DESTINATAIRE,
         		PCRPR.ID_TYPE_BENEFICIAIRE,
         		PCRPR.ID_DESTINATAIRE
         	
         	-- en principe il n'y a qu'une ligne, mais mieux vaut pr‚venir ...
         	SELECT	TOP 1 @ID_TYPE_DESTINATAIRE = ID_TYPE_DESTINATAIRE, @ID_TYPE_BENEFICIAIRE = ID_TYPE_BENEFICIAIRE, @ID_BENEF = ID_BENEF 
         		from #REGLEMENT 
         
         	---------------------------------------------------------------------------------------------------------------------------
         	--								MAJ des lignes REGLEMENT et insertion dans la table REGLEMENT
         	---------------------------------------------------------------------------------------------------------------------------
         	SET @NBLIGNES = @@ROWCOUNT
         	SET ROWCOUNT 1	
         	SET NOCOUNT OFF
         
         	WHILE (@NBLIGNES > 0)
         	BEGIN
         		SET ROWCOUNT 0
         		DELETE FROM #REGLEMENT WHERE TRAITE = 1
         		SET ROWCOUNT 1
         
         		SET @NBLIGNES = (SELECT COUNT(*) FROM #REGLEMENT WHERE TRAITE = 1)
         
         		IF @NBLIGNES > 0
         		BEGIN
         			SET @COMPTEUR = @COMPTEUR + 1
         		END
         		
         		UPDATE #REGLEMENT
         		SET 
         			NUM_VIREMENT = @COMPTEUR_ORDRE_VIREMENT + @COMPTEUR + 1,
         			COD_REGLEMENT = @COD_REGLEMENT_PREFIXE + CONVERT(VARCHAR,@COMPTEUR),
         			TRAITE = 1
         		WHERE NUM_VIREMENT < 0
         
         		SET @NBLIGNES = (SELECT COUNT(*) FROM #REGLEMENT WHERE TRAITE = 1)
         
         		IF @NBLIGNES > 0
         		BEGIN
         			SET @COMPTEUR = @COMPTEUR + 1
         		END
         
         		INSERT INTO REGLEMENT
         		(
         			COD_REGLEMENT,
         			DAT_REGLEMENT,
         			NUM_VIREMENT,
         			DAT_EDITION,
         			MNT_REGLE_TTC,
         			MNT_REGLE_HT,
         			ID_TRANSACTION,
         			BLN_ACTIF,
         			BLN_EN_COURS,
         			BLN_CRITERE,
         			ID_AGENCE
         		)
         		SELECT 
         			COD_REGLEMENT,
         			DAT_REGLEMENT,
         			NUM_VIREMENT,
         			null,
         			MNT_REGLE_TTC,
         			MNT_REGLE_HT,
         			ID_TRANSACTION,
         			BLN_ACTIF,
         			BLN_EN_COURS,
         			BLN_CRITERE,
         			@ID_AGENCE	
         		FROM #REGLEMENT
         		WHERE TRAITE = 1
         		SET @NBLIGNES = @@ROWCOUNT
         
         		UPDATE REGLEMENT 
         		SET COD_REGLEMENT = ID_REGLEMENT 
         		WHERE ID_REGLEMENT = SCOPE_IDENTITY()
         
         
         	END
         	SET ROWCOUNT 0
         	SET NOCOUNT ON
         
         	---------------------------------------------------------------------------------------------------------------------------
         	--											MAJ de la table POSTE_COUT_REGLE
         	---------------------------------------------------------------------------------------------------------------------------
         	IF (@ID_TYPE_DESTINATAIRE = 2)
         		BEGIN
         			UPDATE POSTE_COUT_REGLE
         			SET 
         				POSTE_COUT_REGLE.ID_REGLEMENT = REGLEMENT.ID_REGLEMENT
         			FROM
         				POSTE_COUT_REGLE
         					INNER JOIN MODULE_PEC			ON	MODULE_PEC.ID_MODULE_PEC	= POSTE_COUT_REGLE.ID_MODULE_PEC
         					INNER JOIN [TRANSACTION]	T	ON	T.ID_TRANSACTION			= POSTE_COUT_REGLE.ID_TRANSACTION AND
         															T.ID_ETABLISSEMENT_OF_DEST	= MODULE_PEC.ID_ETABLISSEMENT_OF
         					INNER JOIN ETABLISSEMENT_OF		ON	ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = MODULE_PEC.ID_ETABLISSEMENT_OF
         					INNER JOIN ACTION_PEC			ON	MODULE_PEC.ID_ACTION_PEC	= ACTION_PEC.ID_ACTION_PEC 
         					INNER JOIN SESSION				ON	SESSION.ID_SESSION			= POSTE_COUT_REGLE.ID_SESSION 
         
         					INNER JOIN [TRANSACTION]	T1	ON	T1.NUM_IBAN					= T.NUM_IBAN
         					INNER JOIN REGLEMENT			ON	REGLEMENT.ID_TRANSACTION	= T1.ID_TRANSACTION
         			WHERE 
         				REGLEMENT.BLN_ACTIF = 1 
         				AND REGLEMENT.BLN_EN_COURS = 1 
         				AND POSTE_COUT_REGLE.BLN_ACTIF = 1			-- Ne pas modifier les inactifs
         				AND POSTE_COUT_REGLE.DAT_BAP IS NOT NULL
         				AND POSTE_COUT_REGLE.ID_REGLEMENT IS NULL 
         				AND SESSION.DAT_PAIEMENT IS NULL 
         				AND SESSION.DAT_RECEPTION IS NOT NULL
         
         				AND T.BLN_ACTIF = 1
         				AND SESSION.ID_SESSION			IS NOT NULL AND SESSION.DAT_PAIEMENT IS NULL AND SESSION.DAT_RECEPTION IS NOT NULL
         				AND POSTE_COUT_REGLE.BLN_FACTURE_DIRECTE = 1
         
         				AND 
         				(
         					@ID_TYPE_BENEFICIAIRE = 1 AND T.ID_ETABLISSEMENT_BENEF = @ID_BENEF
         					OR
         					@ID_TYPE_BENEFICIAIRE = 2 AND T.ID_ETABLISSEMENT_OF_BENEF = @ID_BENEF
         					OR
         					@ID_TYPE_BENEFICIAIRE = 3 AND T.ID_TIERS_BENEF = @ID_BENEF
         				)
         				AND REGLEMENT.id_agence		= @ID_AGENCE
         				AND ACTION_PEC.id_agence	= @ID_AGENCE
         				AND REGLEMENT.ID_TRANSACTION	= @ID_TRANSACTION
         		END
         	ELSE
         		BEGIN
         			UPDATE POSTE_COUT_REGLE
         			SET 
         				POSTE_COUT_REGLE.ID_REGLEMENT = REGLEMENT.ID_REGLEMENT
         			FROM
         				POSTE_COUT_REGLE
         					INNER JOIN MODULE_PEC			ON	MODULE_PEC.ID_MODULE_PEC	= POSTE_COUT_REGLE.ID_MODULE_PEC
         					INNER JOIN [TRANSACTION]	T	ON	T.ID_TRANSACTION			= POSTE_COUT_REGLE.ID_TRANSACTION AND
         															T.ID_ETABLISSEMENT_DEST = POSTE_COUT_REGLE.ID_ETABLISSEMENT
         					INNER JOIN ADHERENT				ON ADHERENT.ID_ETABLISSEMENT_PRINCIPAL = POSTE_COUT_REGLE.ID_ETABLISSEMENT
         
         					INNER JOIN ACTION_PEC			ON	MODULE_PEC.ID_ACTION_PEC	= ACTION_PEC.ID_ACTION_PEC 
         					INNER JOIN SESSION				ON	SESSION.ID_SESSION			= POSTE_COUT_REGLE.ID_SESSION 
         
         					INNER JOIN [TRANSACTION]	T1	ON	T1.NUM_IBAN					= T.NUM_IBAN
         					INNER JOIN REGLEMENT			ON	REGLEMENT.ID_TRANSACTION	= T1.ID_TRANSACTION
         			WHERE 
         				REGLEMENT.BLN_ACTIF = 1 
         				AND REGLEMENT.BLN_EN_COURS = 1 
         				AND POSTE_COUT_REGLE.BLN_ACTIF = 1			-- Ne pas modifier les inactifs
         				AND POSTE_COUT_REGLE.DAT_BAP IS NOT NULL
         				AND POSTE_COUT_REGLE.ID_REGLEMENT IS NULL 
         				AND SESSION.DAT_PAIEMENT IS NULL 
         				AND SESSION.DAT_RECEPTION IS NOT NULL
         
         				AND T.BLN_ACTIF = 1
         				AND SESSION.ID_SESSION			IS NOT NULL AND SESSION.DAT_PAIEMENT IS NULL AND SESSION.DAT_RECEPTION IS NOT NULL
         				AND POSTE_COUT_REGLE.BLN_FACTURE_DIRECTE = 0
         
         				AND 
         				(
         					@ID_TYPE_BENEFICIAIRE = 1 AND T.ID_ETABLISSEMENT_BENEF = @ID_BENEF
         					OR
         					@ID_TYPE_BENEFICIAIRE = 2 AND T.ID_ETABLISSEMENT_OF_BENEF = @ID_BENEF
         					OR
         					@ID_TYPE_BENEFICIAIRE = 3 AND T.ID_TIERS_BENEF = @ID_BENEF
         				)
         				AND REGLEMENT.id_agence		= @ID_AGENCE
         				AND ACTION_PEC.id_agence	= @ID_AGENCE
         				AND POSTE_COUT_REGLE.ID_TRANSACTION = @ID_TRANSACTION
         		END
         END
         
         
         UPDATE COMPTEUR
         SET NUM_CPT_TMP = @COMPTEUR_ORDRE_VIREMENT + @COMPTEUR
         WHERE 
         	COD_CPT = 'VIREMENT'
         ---------------------------------------------------------------------------------------------------------------------------------
         
         