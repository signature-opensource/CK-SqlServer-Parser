
         
         CREATE PROCEDURE [BATCH_TRANSFERT_DOTATION_EXCEPTIONNELLE_PME_2015]
         /*
         =============================================  
         Author  : MBL
         Create date : 22/09/2015
         Description : Proc‚dure permettant de lancer des tranferts des dotations exceptionnelles 2015 pour les adh‚rents de type PME (champ application P10-49)
         sur le compte Selection DEFI	(@COD_TYPE_EVENEMENT_DOTATION = 'DOTPME15')
         Le traitement fait appel a la fonction de table F_TRANSFERT_DOTATION_EXCEPTIONNELLE_PME_2015 constituant un outil d'aide … la d‚cision 
         afin de g‚n‚rer ces transferts.
         
         -- CONDITION DE LANCEMENT
         Parametre : la valorisation du parametre @ID_ADHERENT_TRAITE est optionnelle. 
         			S'il est valorise, le traitement n'est declenche que pour l'adherent de mˆme ID
         			S'il n'est pas valorise, le traitement est declenche pour tous les adherents
         -- =============================================
         */
         @ID_ADHERENT_TRAITE						int
         AS
         BEGIN
         
         	IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
         	BEGIN
         		drop table #TMP_TRANSFERT
         	END
         
         
         	DECLARE 
         	@DAT							DATETIME,
         	@ID_TYPE_EVENEMENT_TRANSFERT	INTEGER,
         	@COD_TYPE_EVENEMENT_DOTATION	VARCHAR(08),	
         	@ID_GROUPE						INTEGER,
         	@ID_ADHERENT					INTEGER,
         	@ID_ETABLISSEMENT				INTEGER,
         	@NUM_ANNEE_N					INTEGER, 
         	@MNT_TRANSFERT					DECIMAL(15, 2),
         	@ID_BRANCHE						INT,
         	@ID_PERIODE_N					INTEGER, 
         	@ID_PERIODE_N_PLUS1				INTEGER, 
         	@LIBL_MVT						VARCHAR(60),
         	@LIBL_EVENEMENT					VARCHAR(50),
         	@ID_ENVELOPPE					INTEGER,
         	@LIBL_ENVELOPPE					VARCHAR(50),
         	@ID_ACTIVITE					INTEGER,
         	@BLN_COMPTE_VERS_ENVELOPPE		TINYINT,
         	@ID_TRANSFERT					INT,
         	@ID_TYPE_FINANCEMENT			INT
         
         	SELECT @NUM_ANNEE_N = 2015
         
         	SET @COD_TYPE_EVENEMENT_DOTATION = 'DOTPME15'
         	SELECT @ID_TYPE_EVENEMENT_TRANSFERT = ID_TYPE_EVENEMENT
         	FROM TYPE_EVENEMENT
         	WHERE COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT_DOTATION 
         
         		
         	SELECT t.*, ADHERENT.ID_ETABLISSEMENT_PRINCIPAL
         	INTO #TMP_TRANSFERT 
         	FROM F_TRANSFERT_DOTATION_EXCEPTIONNELLE_PME_2015(@NUM_ANNEE_N, @ID_ADHERENT_TRAITE, @COD_TYPE_EVENEMENT_DOTATION ) t
         	INNER JOIN ADHERENT ON ADHERENT.ID_ADHERENT = t.ID_ADHERENT
         	INNER JOIN ETABLISSEMENT ON ETABLISSEMENT.ID_ETABLISSEMENT = ADHERENT.ID_ETABLISSEMENT_PRINCIPAL
         
         
         	SELECT @DAT = GETDATE()
         
         	SELECT @ID_TYPE_FINANCEMENT = 5 -- Compte selection DEFI
         
         	SELECT	@ID_PERIODE_N	= ID_PERIODE   
         	from	PERIODE     
         	where	NUM_ANNEE		= @NUM_ANNEE_N -1
         	AND		ID_TYPE_PERIODE = 1   
         
         	SELECT	@ID_PERIODE_N_PLUS1		= ID_PERIODE
         	from	PERIODE     
         	where	NUM_ANNEE				= @NUM_ANNEE_N 
         	AND		ID_TYPE_PERIODE			= 1   
         
         	SET @LIBL_EVENEMENT		= 'Transfert Dotation Exceptionnelle PME '	+ CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
         	SET @LIBL_MVT			= 'Doublement exceptionnel dotation PME '		+ CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
         
         
         	DECLARE cu_transfert CURSOR FOR
         	SELECT ID_ADHERENT, ID_GROUPE = ID_GROUPE_DOTATION, ID_BRANCHE, [ID_ACTIVITE_PLAN_N+1], MNT_TRANSFERT, ID_ETABLISSEMENT_PRINCIPAL
         	FROM #TMP_TRANSFERT
         	WHERE ABS(MNT_TRANSFERT) > 0
         
         	OPEN cu_transfert
         
         	FETCH cu_transfert INTO
         	@ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @ID_ACTIVITE, @MNT_TRANSFERT, @ID_ETABLISSEMENT
         
         
         	WHILE (@@FETCH_STATUS <> -1)
         	BEGIN	
         		--Recherche de l'enveloppe de collecte PIVOT
         		SELECT		@ID_ENVELOPPE = ID_ENVELOPPE , @LIBL_ENVELOPPE = LIBL_ENVELOPPE 
         		FROM		TYPE_ENVELOPPE 
         		INNER JOIN	ENVELOPPE ON ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE
         		WHERE		TYPE_ENVELOPPE.BLN_COLLECTE = 1 
         		AND			TYPE_ENVELOPPE.ID_ACTIVITE	= @ID_ACTIVITE 
         		AND			ENVELOPPE.ID_PERIODE		= @ID_PERIODE_N
         		AND			TYPE_ENVELOPPE.ID_BRANCHE	= @ID_BRANCHE
         
         		--SELECT LIBL_ENVELOPPE = @LIBL_ENVELOPPE 
         		IF @MNT_TRANSFERT >0
         		BEGIN
         			SET @BLN_COMPTE_VERS_ENVELOPPE  = 0
         		END
         		ELSE
         		BEGIN
         			SET @MNT_TRANSFERT = - @MNT_TRANSFERT
         			SET @BLN_COMPTE_VERS_ENVELOPPE  = 1
         		END
         
         		--SELECT '@ID_TRANSFERT = INS_TRANSFERT ',
         		--	LIBL_TRANSFERT				= @LIBL_EVENEMENT,
         		--	BLN_COMPTE_VERS_ENVELOPPE	= @BLN_COMPTE_VERS_ENVELOPPE,  
         		--	ID_GROUPE					= @ID_GROUPE,
         		--	ID_ENVELOPPE				= @ID_ENVELOPPE,
         		--	DAT_TRANSFERT				= @DAT,
         		--	MNT_TRANSFERT				= @MNT_TRANSFERT, 
         		--	ID_TYPE_FINANCEMENT			= @ID_TYPE_FINANCEMENT,   
         		--	ID_UTILISATEUR				= 82, 
         		--	ID_PERIODE					= @ID_PERIODE_N_PLUS1,
         		--	COM_TRANSFERT				= @LIBL_MVT, 
         		--	LIBL_MVT_BUDGETAIRE			= @LIBL_MVT,
         		--	ID_TYPE_EVENEMENT			= @ID_TYPE_EVENEMENT_TRANSFERT,
         		--	ID_ETABLISSEMENT			= @ID_ETABLISSEMENT
         				
         		exec @ID_TRANSFERT = INS_TRANSFERT 
         			@LIBL_TRANSFERT				= @LIBL_EVENEMENT,
         			@BLN_COMPTE_VERS_ENVELOPPE	= @BLN_COMPTE_VERS_ENVELOPPE,  
         			@ID_GROUPE					= @ID_GROUPE,
         			@ID_ENVELOPPE				= @ID_ENVELOPPE,
         			@DAT_TRANSFERT				= @DAT,
         			@MNT_TRANSFERT				= @MNT_TRANSFERT, 
         			@ID_TYPE_FINANCEMENT		= @ID_TYPE_FINANCEMENT,   -- Type de financement sur Compte Historique
         			@ID_UTILISATEUR				= 82, 
         			@ID_PERIODE					= @ID_PERIODE_N_PLUS1,
         			@COM_TRANSFERT				= @LIBL_MVT, 
         			@LIBL_MVT_BUDGETAIRE		= @LIBL_MVT,
         			@ID_TYPE_EVENEMENT			= @ID_TYPE_EVENEMENT_TRANSFERT,
         			@ID_ETABLISSEMENT			= @ID_ETABLISSEMENT
         
         		FETCH cu_transfert INTO
         		@ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @ID_ACTIVITE, @MNT_TRANSFERT, @ID_ETABLISSEMENT
         
         
         	END
         
         	CLOSE cu_transfert
         	DEALLOCATE cu_transfert
         
         	IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
         	BEGIN
         		drop table #TMP_TRANSFERT
         	END
         
         END		
         
         -- =================================================  
         -- Author  : KW  
         -- Create date : 17 avril 2008  
         -- Description : Edition de la lettre de stat 2483  
         -- =================================================  
         -- Author  : KW  
         -- Modif date : 23 avril 2008  
         -- Description : Correction bugs
         			-- Transco des ouvriers 
         			-- Suppression filtre sur id_action_pec en dur
         -- ================================================  
         CREATE PROCEDURE [dbo].[EDT_LETTRE_ETAT_2483]  
           @ID_ETABLISSEMENT INT,  
           @ID_BENEFICIAIRE INT,  
           @TYPE_BENEFICIAIRE INT,  
           @ID_ADRESSE INT,  
           @ID_CONTACT INT,  
           @ID_PERIODE INT  
         AS  
           
         BEGIN  
           
         --[LEC_EDT_2483] 21,2008  
           
          DECLARE @NUM_ANNEE SMALLINT  
          DECLARE @ID_ADHERENT SMALLINT  
           
          -- Recuperation de l'annee  
          SELECT @NUM_ANNEE = NUM_ANNEE   
          FROM PERIODE   
          WHERE PERIODE.ID_PERIODE = @ID_PERIODE  
           
          SELECT AGENCE.ID_AGENCE,  
            ADHERENT.ID_ADHERENT,  
            ADHERENT.COD_ADHERENT,  
            LIB_PNM_CONSEILLER,  
            LIB_NOM_CONSEILLER  
          INTO #TEMP_INFOS     
          FROM ADHERENT   
            INNER JOIN ETABLISSEMENT ON ADHERENT.ID_ETABLISSEMENT_PRINCIPAL = ETABLISSEMENT.ID_ETABLISSEMENT  
            INNER JOIN AGENCE ON AGENCE.ID_AGENCE = ADHERENT.ID_AGENCE  
          WHERE ADHERENT.ID_ETABLISSEMENT_PRINCIPAL = @ID_ETABLISSEMENT   
           
          SELECT @ID_ADHERENT = #TEMP_INFOS.ID_ADHERENT  
          FROM #TEMP_INFOS  
            
           
          -- Tableau des donn‚es  
          CREATE TABLE #TMP_CSP  
          (  
           ID_ADHERENT   INT,  
           ID_CSP    INT,  
           NB_SAL_HOMME  INT DEFAULT 0,  
           NB_SAL_FEMME  INT DEFAULT 0,  
           NB_HEURE_TOT  INT DEFAULT 0,  
           NB_SAL_DIF   INT DEFAULT 0,  
           NB_HEURE_DIF  INT DEFAULT 0  
          )  
           
          -- Tableau des totaux  
          CREATE TABLE #TMP_TOTAL  
          (  
           ID_ADHERENT   INT,  
           NB_SAL_HOMME  INT DEFAULT 0,  
           NB_SAL_FEMME  INT DEFAULT 0,  
           NB_HEURE_TOT  INT DEFAULT 0,  
           NB_SAL_DIF   INT DEFAULT 0,  
           NB_HEURE_DIF  INT DEFAULT 0,  
           NB_SAL_PP   INT DEFAULT 0,  
           NB_HEURE_PP   INT DEFAULT 0,  
           NB_SAL_ALLOC   INT DEFAULT 0,  
           NB_HEURE_ALLOC  INT DEFAULT 0  
          )  
           
          /* Calcul du nombre de stagiaire et du nombre d'heure par CSP et par sexe */  
          SELECT ID_CSP, BLN_MASCULIN, INDIVIDU.ID_INDIVIDU, SUM(NB_HEURE_REGLE) NB_HEURE_REGLE  
          INTO #TMP1  
          FROm STAGIAIRE_PEC, INDIVIDU, SESSION_PEC, ETABLISSEMENT, MODULE_PEC,ACTION_PEC  
          WHERE STAGIAIRE_PEC.ID_INDIVIDU  = INDIVIDU  .ID_INDIVIDU    
          AND STAGIAIRE_PEC.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT   
          AND ETABLISSEMENT.ID_ADHERENT = @ID_ADHERENT   
          AND STAGIAIRE_PEC.ID_SESSION_PEC = SESSION_PEC.ID_SESSION_PEC  
          AND YEAR (SESSION_PEC.DAT_DEBUT) = @NUM_ANNEE  
          AND SESSION_PEC.ID_SESSION_PEC IS NOT NULL  
          AND MODULE_PEC.ID_MODULE_PEC = SESSION_PEC.ID_MODULE_PEC  
          AND MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC  
          AND MODULE_PEC.BLN_ACTIF = 1  
          AND ACTION_PEC.BLN_ACTIF = 1  
          AND SESSION_PEC.BLN_ACTIF = 1  
          GROUP BY ID_CSP, BLN_MASCULIN, INDIVIDU.ID_INDIVIDU  
          HAVING SUM(NB_HEURE_REGLE) > 0  
          ORDER BY 1, 2  
           
          UPDATE #TMP1 SET ID_CSP = 2 WHERE ID_CSP = 1  
           
          INSERT INTO #TMP_CSP (ID_ADHERENT, ID_CSP) SELECT @ID_ADHERENT, ID_CSP FROM CSP WHERE ID_CSP > 1  
           
          UPDATE #TMP_CSP  
          SET NB_SAL_HOMME = TOT.NB_INDIVIDU  
          FROM (SELECT ID_CSP, NB_INDIVIDU = COUNT(DISTINCT ID_INDIVIDU) FROm #TMP1  WHERE BLN_MASCULIN = 1 GROUP BY ID_CSP) TOT  
          WHERE #TMP_CSP.ID_CSP = TOT.ID_CSP   
           
          UPDATE #TMP_CSP  
          SET NB_SAL_FEMME = TOT.NB_INDIVIDU  
          FROM (SELECT ID_CSP, NB_INDIVIDU = COUNT(DISTINCT ID_INDIVIDU) FROm #TMP1  WHERE BLN_MASCULIN = 0 GROUP BY ID_CSP) TOT  
          WHERE #TMP_CSP.ID_CSP = TOT.ID_CSP   
           
          UPDATE #TMP_CSP  
          SET NB_HEURE_TOT = TOT.NB_HEURE_REGLE  
          FROM (SELECT ID_CSP, NB_HEURE_REGLE = SUM(NB_HEURE_REGLE) FROm #TMP1 GROUP BY ID_CSP) TOT  
          WHERE #TMP_CSP.ID_CSP = TOT.ID_CSP   
           
          /* Calcul du nombre de stagiaire et du nb heure par CSP pour le DIF */  
          SELECT ID_CSP, INDIVIDU.ID_INDIVIDU, SUM(UNITE_STAGIAIRE.NB_HEURE_REGLE) NB_HEURE_REGLE  
          INTO #TMP2  
          FROm STAGIAIRE_PEC, INDIVIDU, SESSION_PEC, ETABLISSEMENT, MODULE_PEC,ACTION_PEC, UNITE_STAGIAIRE  
          WHERE STAGIAIRE_PEC.ID_INDIVIDU  = INDIVIDU  .ID_INDIVIDU    
          AND STAGIAIRE_PEC.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT   
          AND ETABLISSEMENT.ID_ADHERENT = @ID_ADHERENT   
          AND STAGIAIRE_PEC.ID_SESSION_PEC = SESSION_PEC.ID_SESSION_PEC  
          AND YEAR (SESSION_PEC.DAT_DEBUT) = @NUM_ANNEE  
          AND SESSION_PEC.ID_SESSION_PEC IS NOT NULL  
          AND MODULE_PEC.ID_MODULE_PEC = SESSION_PEC.ID_MODULE_PEC  
          AND STAGIAIRE_PEC .ID_STAGIAIRE_PEC = UNITE_STAGIAIRE .ID_STAGIAIRE_PEC   
          AND ID_DISPOSITIF IN (3, 4)  
          AND MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC  
          AND MODULE_PEC.BLN_ACTIF = 1  
          AND ACTION_PEC.BLN_ACTIF = 1  
          AND SESSION_PEC.BLN_ACTIF = 1  
          GROUP BY ID_CSP, BLN_MASCULIN, INDIVIDU.ID_INDIVIDU  
          HAVING SUM(UNITE_STAGIAIRE.NB_HEURE_REGLE) > 0  
          ORDER BY 1, 2  
         
          UPDATE #TMP2 SET ID_CSP = 2 WHERE ID_CSP = 1  
         
          UPDATE #TMP_CSP  
          SET NB_SAL_DIF = TOT.NB_SAL  
          FROM (SELECT ID_CSP, NB_SAL =  COUNT(DISTINCT ID_INDIVIDU) FROm #TMP2 GROUP BY ID_CSP) TOT  
          WHERE #TMP_CSP.ID_CSP = TOT.ID_CSP   
         
          UPDATE #TMP_CSP  
          SET NB_HEURE_DIF = TOT.NB_HEURE_REGLE  
          FROM (SELECT ID_CSP, NB_HEURE_REGLE = SUM(NB_HEURE_REGLE) FROm #TMP2 GROUP BY ID_CSP) TOT  
          WHERE #TMP_CSP.ID_CSP = TOT.ID_CSP   
             
          INSERT INTO #TMP_TOTAL  
          (  
           ID_ADHERENT,  
           NB_SAL_HOMME,  
           NB_SAL_FEMME,  
           NB_HEURE_TOT,  
           NB_SAL_DIF ,  
           NB_HEURE_DIF  
          )  
          SELECT @ID_ADHERENT,  
            SUM(NB_SAL_HOMME),  
            SUM(NB_SAL_FEMME),  
            SUM(NB_HEURE_TOT),  
            SUM(NB_SAL_DIF) ,  
            SUM(NB_HEURE_DIF)  
          FROM #TMP_CSP  
           
          /* Calcul du nombre de stagiaire et du nombre heures pour la PP*/  
          SELECT INDIVIDU.ID_INDIVIDU, SUM(UNITE_STAGIAIRE.NB_HEURE_REGLE) NB_HEURE_REGLE  
          INTO #TMP3  
          FROm STAGIAIRE_PEC, INDIVIDU, SESSION_PEC, ETABLISSEMENT, MODULE_PEC, UNITE_STAGIAIRE, ACTION_PEC
          WHERE STAGIAIRE_PEC.ID_INDIVIDU  = INDIVIDU  .ID_INDIVIDU    
          AND STAGIAIRE_PEC.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT   
          AND ETABLISSEMENT.ID_ADHERENT = @ID_ADHERENT   
          AND STAGIAIRE_PEC.ID_SESSION_PEC = SESSION_PEC.ID_SESSION_PEC
          AND MODULE_PEC.ID_ACTION_PEC  = ACTION_PEC.ID_ACTION_PEC  
          AND YEAR (SESSION_PEC.DAT_DEBUT) = @NUM_ANNEE  
          AND SESSION_PEC.ID_SESSION_PEC IS NOT NULL  
          AND MODULE_PEC.ID_MODULE_PEC = SESSION_PEC.ID_MODULE_PEC  
          AND STAGIAIRE_PEC .ID_STAGIAIRE_PEC = UNITE_STAGIAIRE .ID_STAGIAIRE_PEC   
          AND ID_DISPOSITIF IN (8, 9)  
          AND MODULE_PEC.BLN_ACTIF = 1  
          AND ACTION_PEC.BLN_ACTIF = 1  
          AND SESSION_PEC.BLN_ACTIF = 1  
          GROUP BY INDIVIDU.ID_INDIVIDU  
          HAVING SUM(UNITE_STAGIAIRE.NB_HEURE_REGLE) > 0  
          ORDER BY 1, 2  
         
           
          UPDATE #TMP_TOTAL  
          SET NB_SAL_PP= TOT.NB_SAL  
          FROM (SELECT NB_SAL =  COUNT(DISTINCT ID_INDIVIDU) FROm #TMP3) TOT  
           
          UPDATE #TMP_TOTAL  
          SET NB_HEURE_PP  = TOT.NB_HEURE_REGLE  
          FROM (SELECT NB_HEURE_REGLE = ISNULL(SUM(NB_HEURE_REGLE),0) FROm #TMP3 ) TOT  
           
           
          /* Calcul du nombre de stagiaire et du nombre heures pour l'allocation de formation*/  
          SELECT INDIVIDU.ID_INDIVIDU, SUM(NB_HEURES_HORS_TT) NB_HEURE_REGLE  
          INTO #TMP4  
          FROm STAGIAIRE_PEC, INDIVIDU, SESSION_PEC, ETABLISSEMENT, MODULE_PEC, ACTION_PEC  
          WHERE STAGIAIRE_PEC.ID_INDIVIDU  = INDIVIDU  .ID_INDIVIDU    
          AND STAGIAIRE_PEC.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT   
          AND ETABLISSEMENT.ID_ADHERENT = @ID_ADHERENT   
          AND STAGIAIRE_PEC.ID_SESSION_PEC = SESSION_PEC.ID_SESSION_PEC  
          AND MODULE_PEC.ID_ACTION_PEC  = ACTION_PEC.ID_ACTION_PEC  
          AND YEAR (SESSION_PEC.DAT_DEBUT) = @NUM_ANNEE  
          AND SESSION_PEC.ID_SESSION_PEC IS NOT NULL  
          AND MODULE_PEC.ID_MODULE_PEC = SESSION_PEC.ID_MODULE_PEC  
          AND MODULE_PEC.BLN_ACTIF = 1  
          AND ACTION_PEC.BLN_ACTIF = 1  
          AND SESSION_PEC.BLN_ACTIF = 1  
          AND MODULE_PEC.BLN_ACTIF = 1  
          AND ACTION_PEC.BLN_ACTIF = 1  
          AND SESSION_PEC.BLN_ACTIF = 1  
          GROUP BY INDIVIDU.ID_INDIVIDU  
          HAVING SUM(NB_HEURES_HORS_TT) > 0  
          ORDER BY 1, 2  
           
          UPDATE #TMP_TOTAL  
          SET NB_SAL_ALLOC= TOT.NB_SAL  
          FROM (SELECT NB_SAL =  COUNT(DISTINCT ID_INDIVIDU) FROm #TMP4) TOT  
           
          UPDATE #TMP_TOTAL  
          SET NB_HEURE_ALLOC  = TOT.NB_HEURE_REGLE  
          FROM (SELECT NB_HEURE_REGLE = ISNULL(SUM(NB_HEURE_REGLE),0) FROm #TMP4 ) TOT;  
           
           
          /*SELECT * FROM #TMP_CSP  
          ORDER BY 1;  
          SELECT * FROM #TMP_TOTAL;*/  
           
          --- XML Generation -------------------------------------------------------------------------------------------------------  
          WITH XMLNAMESPACES (  
           DEFAULT 'EDT_LETTRE_ETAT_2483'  
          )  
           
          SELECT   
           (  
            SELECT   
             -- Recuperation des informations sur l'agence  
             dbo.GetXmlAgenceContact(#TEMP_INFOS.ID_AGENCE) AS EMETTEUR,  
               
             -- Recuperation des informations sur le contact  
             dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) AS BENEFICIAIRE,  
           
             #TEMP_INFOS.COD_ADHERENT     AS COD_ADHERENT,  
             dbo.GetFullDate(GETDATE())     AS DATE,  
             @NUM_ANNEE         AS NUM_ANNEE  
           
            FOR XML RAW('ENTETE'), ELEMENTS, TYPE  
           ),  
           (   
            SELECT  
             (  
              SELECT  
               (  
                SELECT  
                 (  
                  SELECT #TMP_CSP.NB_SAL_HOMME AS NB_HOMMES_OUVRIERS   
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 2  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_FEMME AS NB_FEMMES_OUVRIERS   
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 2  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_HOMME AS NB_HOMMES_EMPLOYES   
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 3  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_FEMME AS NB_FEMMES_EMPLOYES   
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 3  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_HOMME AS NB_HOMMES_TECH   
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 4  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_FEMME AS NB_FEMMES_TECH  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 4  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_HOMME AS NB_HOMMES_CADRES   
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 5  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_FEMME AS NB_FEMMES_CADRES  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 5  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
           
                 -- Colonne 2  
                 (  
                  SELECT #TMP_CSP.NB_HEURE_TOT AS NB_HEURE_TOT_OUVRIERS  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 2  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_HEURE_TOT AS NB_HEURE_TOT_EMPLOYES  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 3  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_HEURE_TOT AS NB_HEURE_TOT_TECH  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 4  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_HEURE_TOT AS NB_HEURE_TOT_CADRES  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 5  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                   
                 -- Colonne 3  
                 (  
                  SELECT #TMP_CSP.NB_SAL_DIF AS NB_SAL_DIF_OUVRIERS  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 2  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_DIF AS NB_SAL_DIF_EMPLOYES  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 3  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_DIF AS NB_SAL_DIF_TECH  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 4  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_SAL_DIF AS NB_SAL_DIF_CADRES  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 5  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
           
                 -- Colonne 4  
                 (  
                  SELECT #TMP_CSP.NB_HEURE_DIF AS NB_HEURE_DIF_OUVRIERS  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 2  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_HEURE_DIF AS NB_HEURE_DIF_EMPLOYES  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 3  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_HEURE_DIF AS NB_HEURE_DIF_TECH  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 4  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 ),  
                 (  
                  SELECT #TMP_CSP.NB_HEURE_DIF AS NB_HEURE_DIF_CADRES  
                  FROM #TMP_CSP  
                  WHERE #TMP_CSP.ID_CSP = 5  
                  FOR XML RAW(''), ELEMENTS, TYPE  
                 )  
           
                FOR XML RAW('DETAIL'), ELEMENTS, TYPE  
               ),  
               (  
                SELECT #TMP_TOTAL.NB_SAL_HOMME,  
                  #TMP_TOTAL.NB_SAL_FEMME,  
                  #TMP_TOTAL.NB_HEURE_TOT,  
                  #TMP_TOTAL.NB_SAL_DIF,  
                  #TMP_TOTAL.NB_HEURE_DIF   
                FROM #TMP_TOTAL  
                FOR XML RAW('TOTAL'), ELEMENTS, TYPE  
               ),  
               (  
                SELECT #TMP_TOTAL.NB_SAL_PP,  
                  #TMP_TOTAL.NB_HEURE_PP,  
                  #TMP_TOTAL.NB_SAL_ALLOC,  
                  #TMP_TOTAL.NB_HEURE_ALLOC  
                FROM #TMP_TOTAL  
                FOR XML RAW('RESUME'), ELEMENTS, TYPE  
               )  
              FOR XML RAW('TABLEAU'), ELEMENTS, TYPE  
             )      
            FOR XML RAW('CORPS'), ELEMENTS, TYPE  
           ),  
           (  
            SELECT #TEMP_INFOS.LIB_PNM_CONSEILLER     AS LIB_PNM_CONSEILLER,  
              #TEMP_INFOS.LIB_NOM_CONSEILLER     AS LIB_NOM_CONSEILLER  
            FOR XML RAW('SIGNATURE'), ELEMENTS, TYPE  
           )  
          FROM #TEMP_INFOS  
          FOR XML RAW('LETTRE'), ELEMENTS  
           
         END  
           

         CREATE PROCEDURE [dbo].[EDT_LETTRE_PEC_ENGAGEMENT_ADH]
          @ID_ETABLISSEMENT INT,
          @ID_BENEFICIAIRE INT,
          @TYPE_BENEFICIAIRE INT,
          @ID_ADRESSE   INT,
          @ID_CONTACT   INT,
          @CODE_ACTION_PEC VARCHAR(11)
         AS
         -- ===========================================================================================================================================================================================
         -- Author  : KW
         -- Date   : XX XXXX 2007
         -- Description : Cr‚ation
         -- ===========================================================================================================================================================================================
         -- Author  : SV
         -- Date   : 23 ao–t 2007
         -- Description : Modification du flux X.M.L.
         --      --> Suppression du tag TABLEAU_TYPE_COUT dans la partie INDIVIDU
         --      --> Remplacement de la chaŒne "Financements Professionalisation" par "Financements Pro."
         -- ===========================================================================================================================================================================================
         -- Author  : SV
         -- Date   : 24 ao–t 2007
         -- Description : Utilisation de l'activit‚ du type d'enveloppe et non pas du dispositif dans la cr‚ation de la table temporaire #PLAN_FINANCEMENTS_INDIVIDUS
         --      Ajout d'un filtre sur la pr‚sence de l'individu dans le dispositif lors de la cr‚ation du tag TABLEAU_MODULE
         -- ===========================================================================================================================================================================================
         -- Author  : SV
         -- Date   : 07 septembre 2007
         -- Description : Utilisation des montants calcul‚s pour la d‚finition de la pr‚sence des colonnes CC, FM et ENV des individus
         --      Suppression du champ ID_ACTIVITE qui ne sert plus … rien dans la table temporaire #PLAN_FINANCEMENTS_INDIVIDUS
         -- ===========================================================================================================================================================================================
         -- Author  : SV
         -- Date   : 27 novembre 2007
         -- Description : Ajout d'une v‚rification sur le nombre d'heure engag‚ d'une unit‚ stagiaire (des stagiaires avec 0 ‚taient remont‚s)
         --    - #TMP_MODULES_INDIVIDUS
         --    - #TMP_NB_HEURES_INDIVIDUS
         --    - #TMP_DISPOSITIFS_INDIVIDUS 
         --    - #PLAN_FINANCEMENTS_INDIVIDUS
         -- ===========================================================================================================================================================================================
         -- Author  : AMA
         -- Date   : 06 Mai 2008
         -- Description : Ajout des noms et pr‚noms du charg‚ de relation.
         -- ===========================================================================================================================================================================================
         -- Author  : SBR
         -- Date   : 27/05/2008
         -- Description : La partie Emetteur est renseign‚e … partir de la fonction GetXmlAdrChargeRelation (au lieu de GetXmlAgenceContact)
         --      Le paramŠtre en entr‚e @ID_ACTION_PEC est remplac‚ par @CODE_ACTION_PEC, concat‚nation de COD_ACTION_PEC et ANNEE_ACTION_PEC
         -- ===========================================================================================================================================================================================
         -- Author  : SBR
         -- Date   : 30/06/08
         -- Description : On ‚carte les modules non imputables
         -- ===========================================================================================================================================================================================
         -- Author  : SBR
         -- Date   : 23/07/08
         -- Description : R‚‚criture de la proc‚dure sur la base d'une nouvelle matrice d'‚dition
         -- ===========================================================================================================================================================================================
         -- Author  : SBR
         -- Date   : 08/09/08
         -- Description : Impact nouvelle charte graphique sur ‚dition et ajout num interne action et module
         -- ===========================================================================================================================================================================================
         -- Le 29/12/08 par SBR -  Correction bug, pour les actions collectives, le tableau de financement renvoyait le financement 
         --        global de l'action et pas de l'‚tablissement concern‚.
         --        Ajout du filtre sur l'‚tablissement au moment du calcul du financement (#TMP_FINANCEMENTS)
         -- ===========================================================================================================================================================================================
         -- Le 23/02/09 par SBR -  Adaptation suite au groupe de travail sur les ‚ditions
         --        Correction bug mauvaise gestion des engagements successifs ==> mauvaise gestion conditions rŠglement CP
         -- ===========================================================================================================================================================================================
         -- Le 24/02/09 par SBR -  Correction mauvaise gestion de la source financement P-10. Apparaissait dans colonne … la charge de l'entreprise, 
         --        apparait d‚sormais dans la colonne Autres
         -- ===========================================================================================================================================================================================
         -- Le 25/02/09 par SBR -  Ajout d'un filtre suppl‚mentaire lors de la r‚cup‚ration des modules: on ne r‚cupŠre que les modules auxquels des stagiaires de l'‚tablissement concern‚ participent
         -- ===========================================================================================================================================================================================
         -- Le 11/03/09 par SBR -  Calcul du montant … la charge de l'entreprise uniquement dans le cas des actions individuelles. Pour les actions co, affichage du libell‚ Action Co.
         -- ===========================================================================================================================================================================================
         -- Le 28/07/09 par SBR -  Correction caclul des montants demand‚s par module, type de co–t qui prennaient en compte les postes de cout d‚sengag‚s
         -- ===========================================================================================================================================================================================
         -- Le 20/10/09 par SBR -  Suppression de la condition PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US > 0 afin d'afficher les modules qui ne sont pas financ‚s par C2P
         -- ===========================================================================================================================================================================================
         -- Le 24/11/09 par SBR -  Correction alimentation colonnes Fonds mutualis‚s et Autres qui n'‚taient pas correctement g‚r‚es dans le cadre des actions CO
         -- ===========================================================================================================================================================================================
         -- Le 02/12/09 par SBR -  Ajout du Nø d'action sur 1Šre page du courrier (partie entˆte, champ REFERENCE)
         -- ===========================================================================================================================================================================================
         -- Le 21/12/09 par SBR -  Pour les actions CO, si un ‚tablissement n'a aucun stagiaire dans un module donn‚, les informations relatives … ce module sont ‚cart‚es
         -- ===========================================================================================================================================================================================
         -- Le 09/07/10 par SBR -  Ajout du dispositif DIF portable
         -- ===========================================================================================================================================================================================
         -- Le 07/03/11 par SBR - Ajout nveau STC subrog‚ "Repas factur‚s/OF"
         -- ===========================================================================================================================================================================================
         -- Le 02/12/11 par BBL - Ajout supplement
         -- ===========================================================================================================================================================================================
         -- Le 01/06/12 par SLAH : 13531 BBL-Modification libelle OPCA 
         -- ===========================================================================================================================================================================================
         -- Le 18/09/12 par DSZ - 13697 Ajout num tel de l'emetteur; politesse_bas recuper‚ de 2.3 (13761)
         -- ===========================================================================================================================================================================================
         -- Le 08/11/12 par LDE #14239: SUPPRESSION DES SALARIES SUR LES ACCORDS DE PRISE EN CHARGE
         -- ===========================================================================================================================================================================================
         -- Le 12/11/12 par DSZ : 13910 la condition dur dispositf actif ne concerne PLPPDIF
         -- ===========================================================================================================================================================================================
         -- Le 28/11/12 par EOU : 14346 
         -- ===========================================================================================================================================================================================
         -- Le 18/02/13 par DSZ - 14832 : suppression Signature
         -- ===========================================================================================================================================================================================
         -- Le 15/04/13 par TLE - 14978 : R‚cup‚ration du Charg‚ de Relation au lieu du Charg‚ de Mission pour les accords d'actions collectives
         -- ===========================================================================================================================================================================================
         -- Le 14/05/13 par EOU - 15161 : [Demandeur d'emploi] - (3) - R‚duire les cas de g‚n‚ration de la lettre d'engagement PEC
         -- ===========================================================================================================================================================================================
         -- Le 23/07/13 par DSZ - 15613 (selon la spec dans 14987) : pour les actions collectives : le gestionnaire (charg‚ de relation) du premier ‚tablissement saisi de l'action 
         -- ===========================================================================================================================================================================================
         -- Le 27/08/13 par DSZ - 15818 traiter  le cas ou COD_PUBLIC_PRIORITAIRE est null
         -- ===========================================================================================================================================================================================
         -- LDE 22/05/2014 #213
         -- ===========================================================================================================================================================================================
         -- LDE/OPA 23/05/2014 : #213 En tant qu'utilisateur OPTIFORM, 
         -- lorsque j'‚dite un courrier (PEC, PRO) … destination d'un Adh‚rent ou d'un OF, je veux que, 
         -- l'adresse … afficher dans la zone "correspondance" soit l'adresse TSA propre … ce type de courrier 
         -- si le mode "TSA" s'applique sinon qu'elle soit l'adresse actuelle
         -- ===========================================================================================================================================================================================
         -- DSZ 19/12/2014 US#627 reforme 2015 : modif MNT_FIN_xxx  : nommage et calcul
         -- ===========================================================================================================================================================================================
         -- LDE 22/12/2014 #627: retrait de ID_PUBLIC_FAF et modification du calcul de "Autres"
         -- ===========================================================================================================================================================================================
         -- DSZ 21/04/2015 #826: utilisation des balises conditionnelles dans CK
         -- ===========================================================================================================================================================================================
         -- DSZ 23/04/2015 #826: ajout du code pour cpf-sup
         -- ===========================================================================================================================================================================================
         BEGIN
          -- r‚cup‚ration de l'ID_ACTION_PEC en fonction de @CODE_ACTION_PEC
          DECLARE  @ID_ACTION_PEC INT
           SELECT  @ID_ACTION_PEC = dbo.GetActionPECId(@CODE_ACTION_PEC)
          
          -- action individuelle ou collective?
          DECLARE  @ACTION_CO BIT
          
          SELECT
           @ACTION_CO =
            CASE
             WHEN  (CIBLE_ACTION = 1 OR BLN_REPRISE_ADHOC = 1)
              THEN 0
             ELSE  1
            END
          FROM
           ACTION_PEC
          WHERE
           ID_ACTION_PEC = @ID_ACTION_PEC
          
          --pour les actions collectives on selectionne le charge de relation
          --de l'‚tablissement du  premier stagiaire saisi, dont les heures sont valoris‚es >0.  (14978)
          DECLARE  @ID_CR INT
          IF (@ACTION_CO = 1)
          BEGIN
           SELECT
            @ID_CR = ETABLISSEMENT.ID_CHARGEE_RELATION
           FROM
            ETABLISSEMENT
            INNER JOIN STAGIAIRE_PEC
             ON ETABLISSEMENT.ID_ETABLISSEMENT = STAGIAIRE_PEC.ID_ETABLISSEMENT
           WHERE
            STAGIAIRE_PEC.ID_STAGIAIRE_PEC = 
            (
             SELECT
              MIN(ID_STAGIAIRE_PEC)
             FROM
              STAGIAIRE_PEC
              INNER JOIN MODULE_PEC
               ON STAGIAIRE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
             WHERE
              ID_ACTION_PEC = @ID_ACTION_PEC
              AND STAGIAIRE_PEC.NB_HEURE_ENGAGE > 0
              AND MODULE_PEC.BLN_ACTIF = 1
            )
          END
          
          SELECT
           ETABLISSEMENT.ID_ETABLISSEMENT,
           ADHERENT.ID_ADHERENT,
           ADHERENT.COD_ADHERENT,
           ETABLISSEMENT.NUM_SIRET,
           ADRESSE.LIB_CP_CEDEX,
           RTRIM(CASE WHEN PATINDEX('%CEDEX%', ADRESSE.LIB_VIL_CEDEX) <> 0 THEN LEFT(ADRESSE.LIB_VIL_CEDEX, PATINDEX('%CEDEX%', ADRESSE.LIB_VIL_CEDEX)-1) ELSE ADRESSE.LIB_VIL_CEDEX END) AS LIB_VIL_CEDEX, -- RETRAITEMENT DE LA VILLE AU CAS Oë DE LA FORME COMMUNE CEDEX 999
           COALESCE(dbo.IS_EMPTY(ETABLISSEMENT.LIB_ENSEIGNE), ADHERENT.LIB_RAISON_SOCIALE) AS NOM_ADH,
           CR.ID_UTILISATEUR AS ID_UTIL,
           CR.LIB_PNM   AS LIB_PRENOM_CHARGE_RELATION,
           CR.LIB_NOM   AS LIB_NOM_CHARGE_RELATION,
           CR.LIB_VILLE,
           CR.EMAIL   AS EMAIL_CHARGE_RELATION
          INTO
           #TMP_GENERAL
          FROM
           ADHERENT
           INNER JOIN ETABLISSEMENT
            ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
             AND  ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
           INNER JOIN ADRESSE
            ON ETABLISSEMENT.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE
           INNER JOIN NR140
            ON ETABLISSEMENT.ID_ETABLISSEMENT = NR140.ID_ETABLISSEMENT
             AND  NR140.ID_ACTION_PEC = @ID_ACTION_PEC
           INNER JOIN ACTION_PEC
            ON ACTION_PEC.ID_ACTION_PEC = NR140.ID_ACTION_PEC
           LEFT JOIN UTILISATEUR CR -- charg‚ de relation (14978)
            ON CR.ID_UTILISATEUR =
             (CASE
              WHEN  @ACTION_CO = 1 THEN @ID_CR
              ELSE  ETABLISSEMENT.ID_CHARGEE_RELATION
             END)
          
          /** R‚cup‚ration les informations sur l'action PEC choisie pour l'‚tablissement choisi **/
          SELECT
           ACTION_PEC.ID_ACTION_PEC,
           ACTION_PEC.LIBL_ACTION_PEC,
           ACTION_PEC.DAT_DEB_ACTION_PEC,
           ACTION_PEC.DAT_FIN_ACTION_PEC,
           NR140.ID_ETABLISSEMENT
          INTO
           #TMP_ACTION_PEC
          FROM
           ACTION_PEC
           INNER JOIN NR140  ON ACTION_PEC.ID_ACTION_PEC = NR140.ID_ACTION_PEC
          WHERE
           ACTION_PEC.ID_ACTION_PEC = @ID_ACTION_PEC
           AND  NR140.ID_ETABLISSEMENT = @ID_ETABLISSEMENT;
          
          /** R‚cup‚ration des infos sur les modules ***************************************************************/
          SELECT DISTINCT
           MODULE_PEC.ID_ACTION_PEC,
           MODULE_PEC.ID_MODULE_PEC,
           MODULE_PEC.DAT_DEBUT,
           MODULE_PEC.DAT_FIN,
           MODULE_PEC.NUM_INTERNE,
           MODULE_PEC.NUM_DUREE_HEURE,
           MODULE_PEC.NUM_DUREE_JOUR,
           MODULE_PEC.COD_MODULE_PEC,
           MODULE_PEC.LIBL_MODULE_PEC,
           COALESCE(ORGANISME_FORMATION.LIB_SIGLE_OF, NOM_ADH) AS LIB_SIGLE_OF,
           MODULE_PEC.BLN_DELEGATION_PAIEMENT,
           MODULE_PEC.BLN_EXTERNE,
           MAX(ENGAGEMENT.DAT_BAE) AS DAT_BAE,
           0 as CONTIENT_CP,
           0 as CONTIENT_REPOF,
           cast(0 as MONEY) as LIMITE_REPOF,
           cast(0 as MONEY) as LIMITE_CP
          INTO
           #TMP_MODULE
          FROM
           #TMP_ACTION_PEC
           INNER JOIN #TMP_GENERAL
            ON #TMP_ACTION_PEC.ID_ETABLISSEMENT = #TMP_GENERAL.ID_ETABLISSEMENT
           INNER JOIN MODULE_PEC
            ON #TMP_ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
           LEFT JOIN ETABLISSEMENT_OF
            ON MODULE_PEC.ID_ETABLISSEMENT_OF = ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF
           LEFT JOIN ORGANISME_FORMATION
            ON ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF
           INNER JOIN ENGAGEMENT
            ON MODULE_PEC.ID_ACTION_PEC = ENGAGEMENT.ID_ACTION_PEC
           INNER JOIN POSTE_COUT_ENGAGE
            ON POSTE_COUT_ENGAGE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
             AND  POSTE_COUT_ENGAGE.ID_ENGAGEMENT = ENGAGEMENT.ID_ENGAGEMENT
          WHERE
           MODULE_PEC.BLN_ACTIF = 1 -- module actif
           AND MODULE_PEC.BLN_IMPUTABLE = 1 -- module imputable
           AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL -- pas d‚sengagement
           AND POSTE_COUT_ENGAGE.ID_ENGAGEMENT IS NOT NULL -- poste de cout engag‚
           AND #TMP_ACTION_PEC.ID_ETABLISSEMENT = @ID_ETABLISSEMENT -- filtre sur l'‚tablissmeent concern‚
           -- modif du 25/02/09 par SBR - on ne r‚cupŠre que les modules auxquels des stagiaires de l'‚tablissement concern‚ participent
           AND EXISTS 
           (SELECT 1 FROM STAGIAIRE_PEC  WHERE STAGIAIRE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC AND STAGIAIRE_PEC.ID_ETABLISSEMENT = @ID_ETABLISSEMENT)
          GROUP BY
           MODULE_PEC.ID_ACTION_PEC,
           MODULE_PEC.ID_MODULE_PEC,
           MODULE_PEC.DAT_DEBUT,
           MODULE_PEC.DAT_FIN,
           MODULE_PEC.NUM_INTERNE,
           MODULE_PEC.NUM_DUREE_HEURE,
           MODULE_PEC.NUM_DUREE_JOUR,
           MODULE_PEC.COD_MODULE_PEC,
           MODULE_PEC.LIBL_MODULE_PEC,
           COALESCE(ORGANISME_FORMATION.LIB_SIGLE_OF, NOM_ADH),
           MODULE_PEC.BLN_DELEGATION_PAIEMENT,
           MODULE_PEC.BLN_EXTERNE
          ORDER BY
           MODULE_PEC.COD_MODULE_PEC;
          
          /** R‚cup‚ration de la liste des individus concern‚s *******************************************************/
          SELECT DISTINCT
           #TMP_MODULE.ID_MODULE_PEC,
           INDIVIDU.ID_INDIVIDU,
           INDIVIDU.NOM_INDIVIDU,
           INDIVIDU.PRENOM_INDIVIDU
          INTO
           #TMP_INDIVIDUS
          FROM
           #TMP_MODULE
           INNER JOIN POSTE_COUT_ENGAGE
            ON #TMP_MODULE.ID_MODULE_PEC = POSTE_COUT_ENGAGE.ID_MODULE_PEC
           INNER JOIN PLAN_FINANCEMENT_US
            ON PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE = POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE
           INNER JOIN UNITE_STAGIAIRE
            ON PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE = UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE
           INNER JOIN STAGIAIRE_PEC
            ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
             AND STAGIAIRE_PEC.ID_ETABLISSEMENT = @ID_ETABLISSEMENT -- filtre sur l'‚tablissement concern‚ (SBR le 29/12/08)
           INNER JOIN INDIVIDU
            ON INDIVIDU.ID_INDIVIDU = STAGIAIRE_PEC.ID_INDIVIDU
           INNER JOIN DISPOSITIF
            ON UNITE_STAGIAIRE.ID_DISPOSITIF = DISPOSITIF.ID_DISPOSITIF
           LEFT JOIN PUBLIC_PRIORITAIRE
            ON PUBLIC_PRIORITAIRE.ID_PUBLIC_PRIORITAIRE = UNITE_STAGIAIRE.ID_PUBLIC_PRIORITAIRE
          WHERE
           STAGIAIRE_PEC.NB_HEURE_ENGAGE > 0    -- LDE 08/11/2012 #14239
           AND ((DISPOSITIF.COD_DISPOSITIF <> 'CSP' AND (DISPOSITIF.COD_DISPOSITIF <> 'DIFPORT' OR COALESCE(PUBLIC_PRIORITAIRE.COD_PUBLIC_PRIORITAIRE,'') <> 'X97')) OR UNITE_STAGIAIRE.NB_HEURE_ENGAGE = 0)
          ORDER BY
           INDIVIDU.NOM_INDIVIDU,
           INDIVIDU.PRENOM_INDIVIDU;
          
          /** R‚cup‚ration des modules o— les individus participent ****************************************************************/
          SELECT DISTINCT
           #TMP_INDIVIDUS.ID_INDIVIDU,
           #TMP_MODULE.ID_MODULE_PEC,
           #TMP_MODULE.LIBL_MODULE_PEC
          INTO
           #TMP_MODULES_INDIVIDUS
          FROM
           #TMP_INDIVIDUS
           INNER JOIN #TMP_MODULE
            ON #TMP_INDIVIDUS.ID_MODULE_PEC = #TMP_MODULE.ID_MODULE_PEC
           INNER JOIN POSTE_COUT_ENGAGE
            ON #TMP_MODULE.ID_MODULE_PEC = POSTE_COUT_ENGAGE.ID_MODULE_PEC
           INNER JOIN PLAN_FINANCEMENT_US
            ON PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE = POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE
           INNER JOIN UNITE_STAGIAIRE
            ON PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE = UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE
           INNER JOIN STAGIAIRE_PEC
            ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
             AND  STAGIAIRE_PEC.ID_ETABLISSEMENT = @ID_ETABLISSEMENT -- filtre sur l'‚tablissement concern‚ (SBR le 29/12/08)
           INNER JOIN INDIVIDU
            ON INDIVIDU.ID_INDIVIDU = STAGIAIRE_PEC.ID_INDIVIDU
          ORDER BY
           #TMP_INDIVIDUS.ID_INDIVIDU;
          
          /** R‚cup‚ration des heures engag‚es pour chaque individus *************************************************/
          SELECT DISTINCT
           DISPOSITIF.ID_DISPOSITIF,
           DISPOSITIF.LIBC_DISPOSITIF,
           DISPOSITIF.LIBL_DISPOSITIF,
           #TMP_MODULES_INDIVIDUS.ID_MODULE_PEC,
           #TMP_INDIVIDUS.ID_INDIVIDU,
           CAST(UNITE_STAGIAIRE.NB_HEURE_ENGAGE AS DECIMAL(18,2)) AS NB_HEURE_ENGAGE
          INTO
           #TMP_NB_HEURES_INDIVIDUS
          FROM
           #TMP_MODULES_INDIVIDUS
           INNER JOIN POSTE_COUT_ENGAGE
            ON #TMP_MODULES_INDIVIDUS.ID_MODULE_PEC = POSTE_COUT_ENGAGE.ID_MODULE_PEC
           INNER JOIN PLAN_FINANCEMENT_US
            ON PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE = POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE
           INNER JOIN UNITE_STAGIAIRE
            ON PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE = UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE
           INNER JOIN STAGIAIRE_PEC
            ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
             AND  STAGIAIRE_PEC.ID_ETABLISSEMENT = @ID_ETABLISSEMENT -- filtre sur l'‚tablissement concern‚
           INNER JOIN #TMP_INDIVIDUS
            ON STAGIAIRE_PEC.ID_INDIVIDU = #TMP_INDIVIDUS.ID_INDIVIDU
           INNER JOIN DISPOSITIF
            ON DISPOSITIF.ID_DISPOSITIF = UNITE_STAGIAIRE.ID_DISPOSITIF
           LEFT JOIN PUBLIC_PRIORITAIRE
            ON PUBLIC_PRIORITAIRE.ID_PUBLIC_PRIORITAIRE = UNITE_STAGIAIRE.ID_PUBLIC_PRIORITAIRE
          WHERE
           (DISPOSITIF.BLN_ACTIF = 1 OR DISPOSITIF.COD_DISPOSITIF = 'PLPPDIF' )
           AND (STAGIAIRE_PEC.NB_HEURE_ENGAGE > 0 )  -- LDE 08/11/2012 #14239
           AND (DISPOSITIF.COD_DISPOSITIF <> 'CSP' and (DISPOSITIF.COD_DISPOSITIF <> 'DIFPORT' or coalesce(PUBLIC_PRIORITAIRE.COD_PUBLIC_PRIORITAIRE,'') <> 'X97'))
         
          /** R‚cup‚ration des financements *********************************************/
          CREATE TABLE #TMP_FINANCEMENTS
          (
           ID_MODULE_PEC INT,
           ID_TYPE_COUT INT,
           COD_TYPE_COUT VARCHAR(8),
           LIBL_TYPE_COUT VARCHAR(50),
           MNT_FIN_COMPTE DECIMAL(18,2),
           MNT_FIN_CPF DECIMAL(18,2),
           MNT_FIN_DIF_PPRO DECIMAL(18,2),
           MNT_FIN_FDS_MUT DECIMAL(18,2),
           MNT_FIN_AUTRES DECIMAL(18,2),
           MNT_TOTAL DECIMAL(18,2),
           MNT_DEMANDE DECIMAL(18,2),
           MNT_CHARGE_ENT DECIMAL(18,2)
          );
          
          -- calcul des montants financ‚s par type de co–t
          INSERT INTO
           #TMP_FINANCEMENTS
          (
           ID_MODULE_PEC,
           ID_TYPE_COUT,
           COD_TYPE_COUT,
           LIBL_TYPE_COUT,
           MNT_FIN_COMPTE,
           MNT_FIN_CPF,
           MNT_FIN_DIF_PPRO,
           MNT_FIN_FDS_MUT,
           MNT_FIN_AUTRES
          )
          SELECT
           #TMP_MODULE.ID_MODULE_PEC,
           TYPE_COUT.ID_TYPE_COUT,
           TYPE_COUT.COD_TYPE_COUT,
           TYPE_COUT.LIBL_TYPE_COUT,
           SUM
           (
            CASE
             WHEN  PLAN_FINANCEMENT_US.ID_TYPE_FINANCEMENT IS NOT NULL
              THEN PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US
             ELSE  0
            END
           ) AS MNT_FIN_COMPTE, --compte adh‚rent selon r‚forme 2015
           SUM
           (
            CASE
             WHEN  DISP_ENV.COD_DISPOSITIF  IN ('CPF')
              THEN PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US
             ELSE  0
            END
           ) AS MNT_FIN_CPF, -- CPF selon r‚forme 2015
           SUM
           (
            CASE
             WHEN  DISP_ENV.COD_DISPOSITIF  IN ('DIFPRIO', 'DIFPORT', 'PPPRIO')
              THEN PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US
             ELSE  0
            END
           ) AS MNT_FIN_DIF_PPRO, -- DIF PRIO, DIF Portable et PERIODE PRO : ensemble selon la r‚forme 2015
           SUM
           (
            CASE
             WHEN  (TYPE_ENVELOPPE.ID_MODE_FINANCEMENT = 1 AND DISP_ENV.COD_DISPOSITIF NOT IN ('P-10', 'DIFPRIO', 'FORMTUT', 'FONCTUT', 'PPPRIO', 'DIFPORT', 'CPF'))
              THEN PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US
             ELSE  0
            END
           ) AS MNT_FIN_FDS_MUT, --laiss‚ inchang‚ pour le moment
           SUM
           (
            CASE
            -- /!\ ATTENTION: si vous modifiez les conditions ci-dessus, merci de reporter la modification ci-dessous /!\ --
             WHEN  PLAN_FINANCEMENT_US.ID_TYPE_FINANCEMENT IS NULL
                AND NOT (DISP_ENV.COD_DISPOSITIF IS NOT NULL AND DISP_ENV.COD_DISPOSITIF IN ('CPF'))
                AND NOT (DISP_ENV.COD_DISPOSITIF IS NOT NULL AND DISP_ENV.COD_DISPOSITIF IN ('DIFPRIO', 'DIFPORT', 'PPPRIO'))
                AND NOT ((TYPE_ENVELOPPE.ID_MODE_FINANCEMENT = 1 AND DISP_ENV.COD_DISPOSITIF IS NOT NULL AND DISP_ENV.COD_DISPOSITIF NOT IN ('P-10', 'DIFPRIO', 'FORMTUT', 'FONCTUT', 'PPPRIO', 'DIFPORT', 'CPF')))
              THEN PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US
             ELSE  0
            END
           ) AS MNT_FIN_AUTRES --fonction tutorale, formation tuteur, financements publics: selon la r‚forme 2015
          FROM
           #TMP_MODULE
           INNER JOIN POSTE_COUT_ENGAGE
            ON #TMP_MODULE.ID_MODULE_PEC = POSTE_COUT_ENGAGE.ID_MODULE_PEC
           INNER JOIN PLAN_FINANCEMENT_US
            ON PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE = POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE
           INNER JOIN UNITE_STAGIAIRE
            ON PLAN_FINANCEMENT_US.ID_UNITE_STAGIAIRE = UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE
           INNER JOIN STAGIAIRE_PEC
            ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
             AND  STAGIAIRE_PEC.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
           INNER JOIN SOUS_TYPE_COUT
            ON POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT = SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT
           INNER JOIN TYPE_COUT
            ON SOUS_TYPE_COUT.ID_TYPE_COUT = TYPE_COUT.ID_TYPE_COUT
           LEFT JOIN ENVELOPPE
            ON PLAN_FINANCEMENT_US.ID_ENVELOPPE = ENVELOPPE.ID_ENVELOPPE
           LEFT JOIN TYPE_ENVELOPPE
            ON ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE
           LEFT JOIN DISPOSITIF DISP_ENV
            ON DISP_ENV.ID_DISPOSITIF = TYPE_ENVELOPPE.ID_DISPOSITIF
          WHERE
           PLAN_FINANCEMENT_US.BLN_ACTIF = 1
           AND  TYPE_COUT.BLN_ACTIF = 1
           AND  SOUS_TYPE_COUT.BLN_ACTIF = 1
           AND  POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL -- pas d‚sengagement
           AND  POSTE_COUT_ENGAGE.ID_ENGAGEMENT IS NOT NULL -- poste de cout engag‚
          GROUP BY
           #TMP_MODULE.ID_MODULE_PEC,
           STAGIAIRE_PEC.ID_ETABLISSEMENT,
           TYPE_COUT.ID_TYPE_COUT,
           TYPE_COUT.COD_TYPE_COUT,
           TYPE_COUT.LIBL_TYPE_COUT;
         
          -- calcul du total des montants financ‚s par type de co–t
          UPDATE
           #TMP_FINANCEMENTS
          SET
           MNT_TOTAL = MNT_FIN_COMPTE + MNT_FIN_DIF_PPRO + MNT_FIN_CPF + MNT_FIN_FDS_MUT + MNT_FIN_AUTRES;
          
          -- calcul des montants demand‚s par module, type de co–t. Pour les actions co, les montants … la charge de l'entreprise ne peuvent pas ˆtre calcul‚s
          IF (@ACTION_CO = 0) -- action individuelle
          BEGIN
           UPDATE
            T1
           SET
            T1.MNT_DEMANDE = T2.MNT_DEMANDE
           FROM
            #TMP_FINANCEMENTS T1
            INNER JOIN
            (
             SELECT
              #TMP_MODULE.ID_MODULE_PEC,
              SOUS_TYPE_COUT.ID_TYPE_COUT,
              SUM(POSTE_COUT_ENGAGE.MNT_PREVISIONNEL_HT) AS MNT_DEMANDE
             FROM
              POSTE_COUT_ENGAGE
              INNER JOIN SOUS_TYPE_COUT
               ON POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT = SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT
              INNER JOIN #TMP_MODULE
               ON #TMP_MODULE.ID_MODULE_PEC = POSTE_COUT_ENGAGE.ID_MODULE_PEC
             WHERE
              POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL -- pas d‚sengagement
              AND POSTE_COUT_ENGAGE.ID_ENGAGEMENT IS NOT NULL -- poste de cout engag‚
             GROUP BY
              #TMP_MODULE.ID_MODULE_PEC,
              SOUS_TYPE_COUT.ID_TYPE_COUT
            ) T2
             ON T1.ID_MODULE_PEC = T2.ID_MODULE_PEC
              AND T1.ID_TYPE_COUT = T2.ID_TYPE_COUT;
          END
          
          -- calcul du montant … la charge de l'entreprise. Pour les actions co le co–t total de l'action n'est pas calcul‚
          UPDATE
           #TMP_FINANCEMENTS
          SET
           MNT_CHARGE_ENT =
            CASE @ACTION_CO
             WHEN  0
              THEN (MNT_DEMANDE - MNT_TOTAL)
             ELSE  -1
            END;
            
            --y a-t-il le dispositif CPF dans un des modules? # 826
          declare @CONTIENT_CPF int = 0
           if exists(select 1 from #TMP_MODULE
           INNER JOIN POSTE_COUT_ENGAGE
            ON #TMP_MODULE.ID_MODULE_PEC = POSTE_COUT_ENGAGE.ID_MODULE_PEC
           INNER JOIN PLAN_FINANCEMENT_US
            ON PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE = POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE
           INNER JOIN ENVELOPPE
            ON PLAN_FINANCEMENT_US.ID_ENVELOPPE = ENVELOPPE.ID_ENVELOPPE
           INNER JOIN TYPE_ENVELOPPE
            ON ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE
           INNER JOIN DISPOSITIF DISP_ENV
            ON DISP_ENV.ID_DISPOSITIF = TYPE_ENVELOPPE.ID_DISPOSITIF
            WHERE DISP_ENV.COD_DISPOSITIF in ('CPF', 'CPF-SUP'))
           set @CONTIENT_CPF = 1
          
          -- modif du 11/03/09 par SBR: si montant demand‚ < montant engag‚ alors montant … la charge de l'entreprise = 0
          UPDATE
           #TMP_FINANCEMENTS
          SET
           MNT_CHARGE_ENT = 0
          WHERE
           MNT_CHARGE_ENT < 0
          
          /** RŠglement des co–ts p‚dagogiques *********************************************/ 
          -- module avec sous type de cout CP
          UPDATE
           #TMP_MODULE
          SET
           CONTIENT_CP = 1, --sinon 0
           LIMITE_CP = #TMP_FINANCEMENTS.MNT_TOTAL
          FROM
           #TMP_MODULE
           INNER JOIN #TMP_FINANCEMENTS
            ON #TMP_MODULE.ID_MODULE_PEC = #TMP_FINANCEMENTS.ID_MODULE_PEC
             AND #TMP_FINANCEMENTS.COD_TYPE_COUT = 'CP'; -- co–ts p‚dagogiques
         
          
         
          -- module avec sous type de cout REPOF
          with repof as
          (
          select 
          
          #TMP_MODULE.ID_MODULE_PEC,
          CAST(SUM(COALESCE(POSTE_COUT_ENGAGE.MNT_ENGAGE_HT,0)) AS MONEY) as limite_repof
           FROM
            #TMP_MODULE
            INNER JOIN POSTE_COUT_ENGAGE
             ON POSTE_COUT_ENGAGE.ID_MODULE_PEC = #TMP_MODULE.ID_MODULE_PEC
            INNER JOIN SOUS_TYPE_COUT 
             ON POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT = SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT 
            INNER JOIN ENGAGEMENT 
             ON ENGAGEMENT.ID_ACTION_PEC = #TMP_MODULE.ID_ACTION_PEC
           WHERE
            POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL
            AND  SOUS_TYPE_COUT.COD_SOUS_TYPE_COUT = 'REPOF'
            AND  ENGAGEMENT.DAT_BAE IS NOT NULL
            AND  ENGAGEMENT.ID_TYPE_ENGAGEMENT <> 2 -- pas d‚sengagement
            AND  ENGAGEMENT.ID_ENGAGEMENT = POSTE_COUT_ENGAGE.ID_ENGAGEMENT
            group by #TMP_MODULE.ID_MODULE_PEC
          )
          UPDATE
           #TMP_MODULE
          SET
           CONTIENT_REPOF = 1,
           #TMP_MODULE.LIMITE_REPOF = repof.limite_repof 
          FROM repof
          WHERE repof.ID_MODULE_PEC = #TMP_MODULE.ID_MODULE_PEC;
          
         
          /*****  Creation du flux XML *******************************************************************************/
          WITH XMLNAMESPACES (
           DEFAULT 'LETTRE_PEC_ENGAGEMENT_ADH'
          )
          /************************************************************************************************************/
         
          SELECT
           -- R‚cup‚ration des informations sur le contact et le b‚n‚ficiaire
           dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) AS BENEFICIAIRE,
         
           /**********************************************************************************************/
           /***************** ENTETE DE LA LETTRE *******************************************************/
           /**********************************************************************************************/
           (
            SELECT
             /******************* Recuperation des informations de l'entete ****************************/
             ISNULL(COD_ADHERENT, '')    AS COD_ADHERENT,
             ISNULL(LIB_PRENOM_CHARGE_RELATION, '') AS LIB_PRENOM_CHARGE_RELATION,
             ISNULL(LIB_NOM_CHARGE_RELATION, '')  AS LIB_NOM_CHARGE_RELATION,
             ISNULL(EMAIL_CHARGE_RELATION, '')  AS EMAIL,
         
             -- R‚cup‚ration des informations sur l'‚metteur
             dbo.GetXmlAdrUtilAvecTel(ENTETE.ID_UTIL, (SELECT TOP 1 BLN_TSA FROM ETABLISSEMENT WHERE ID_ETABLISSEMENT = @ID_BENEFICIAIRE), 1) as EMETTEUR, -- LDE 22/05/2014 #213
         
             -- ajout du 02/12/09 par SBR
             @CODE_ACTION_PEC as REFERENCE,
             RTRIM(CASE WHEN PATINDEX('%CEDEX%', LIB_VILLE) <> 0 THEN LEFT(LIB_VILLE, PATINDEX('%CEDEX%', LIB_VILLE)-1) ELSE LIB_VILLE END) AS LIB_VILLE, -- retraitement de la ville au cas o— de la forme COMMUNE CEDEX 999
             dbo.GetFullDate(GETDATE()) AS DATE,
             (
              SELECT TOP 1
               dbo.GetContactSalutation(@ID_CONTACT, 1)
              FROM
               CIVILITE
              FOR XML PATH('POLITESSE_HAUT'), TYPE
             )
            FROM
             #TMP_GENERAL ENTETE     
            FOR XML AUTO, ELEMENTS, TYPE
           ),
          @CONTIENT_CPF as CONTIENT_CPF,
           (
            SELECT top 1
             dbo.GetContactSalutation(@ID_CONTACT, 1)
            FROM
             CIVILITE
            FOR XML PATH('POLITESSE_BAS'), TYPE
           ),
           /**********************************************************************************************/
           /***************** CODE ADHERENT ****************************************************/
           /**********************************************************************************************/
           ISNULL(COD_ADHERENT, '')    AS COD_ADHERENT,
           /**********************************************************************************************/
           /***************** ANNEXES (Les tableaux) ****************************************************/
           /**********************************************************************************************/
           (
            SELECT
             ISNULL(LETTRE.NUM_SIRET, '')  AS NUM_SIRET,
             ISNULL(LETTRE.NOM_ADH, '')   AS RAISON_SOCIALE,
             ISNULL(LIB_VIL_CEDEX, '')   AS VILLE_ETABLISSEMENT,
             @CODE_ACTION_PEC     AS COD_ACTION_PEC,
             ISNULL(ANNEXES.LIBL_ACTION_PEC, '') AS LIBL_ACTION_PEC,
             ISNULL(CONVERT(VARCHAR(10),ANNEXES.DAT_DEB_ACTION_PEC, 103), '')  AS DAT_DEB_ACTION_PEC,
             ISNULL(CONVERT(VARCHAR(10),ANNEXES.DAT_FIN_ACTION_PEC, 103), '')  AS DAT_FIN_ACTION_PEC,
             (
              SELECT
              (
               SELECT
                ISNULL(MODULE.COD_MODULE_PEC, '') AS COD_MODULE,
                ISNULL(MODULE.LIBL_MODULE_PEC, '') AS LIBL_MODULE_PEC,
                ISNULL('Nø interne module : ' + dbo.IS_EMPTY(MODULE.NUM_INTERNE), '') AS NUM_INTERNE_MODULE,
                ISNULL(CONVERT(VARCHAR(10), MODULE.DAT_BAE, 103), '') AS DAT_ENGAGEMENT,
                ISNULL(MODULE.LIB_SIGLE_OF, '') AS NOM_OF, 
                ISNULL(CONVERT(VARCHAR(10),MODULE.DAT_DEBUT, 103), '') AS DAT_DEBUT,
                ISNULL(CONVERT(VARCHAR(10),MODULE.DAT_FIN, 103), '') AS DAT_FIN,
                dbo.GetFrenchCurrencyFormat(CAST((MODULE.NUM_DUREE_HEURE) AS DECIMAL(10,2))) AS DUREE_HEURE,
                dbo.GetFrenchCurrencyFormat(CAST((MODULE.NUM_DUREE_JOUR) AS DECIMAL(10,2))) AS DUREE_JOUR,
                (
                 SELECT
                  #TMP_FINANCEMENTS.LIBL_TYPE_COUT,
                  dbo.GetFrenchCurrencyFormat(CAST(#TMP_FINANCEMENTS.MNT_FIN_COMPTE AS MONEY)) AS MNT_FIN_COMPTE,
                  dbo.GetFrenchCurrencyFormat(CAST(#TMP_FINANCEMENTS.MNT_FIN_cpf AS MONEY)) AS MNT_FIN_CPF,
                  dbo.GetFrenchCurrencyFormat(CAST(#TMP_FINANCEMENTS.MNT_FIN_DIF_PPRO AS MONEY)) AS MNT_FIN_DIF_PPRO,
                  dbo.GetFrenchCurrencyFormat(CAST(#TMP_FINANCEMENTS.MNT_FIN_FDS_MUT AS MONEY)) AS MNT_FIN_FDS_MUT,
                  dbo.GetFrenchCurrencyFormat(CAST(#TMP_FINANCEMENTS.MNT_FIN_AUTRES AS MONEY)) AS MNT_FIN_AUTRES,
                  dbo.GetFrenchCurrencyFormat(CAST(#TMP_FINANCEMENTS.MNT_TOTAL AS MONEY)) AS MNT_TOTAL,
                  CASE @ACTION_CO 
                   WHEN  0
                    THEN dbo.GETFRENCHCURRENCYFORMAT(CAST(#TMP_FINANCEMENTS.MNT_CHARGE_ENT AS MONEY))
                   ELSE  'Action Co.'
                  END AS MNT_CHARGE_ENT,
                  CASE @ACTION_CO
                   WHEN  0
                    THEN dbo.GetFrenchCurrencyFormat(CAST(#TMP_FINANCEMENTS.MNT_DEMANDE AS MONEY))
                   ELSE  'Action Co.'
                  END AS MNT_COUT_TOT
                 FROM
                  #TMP_FINANCEMENTS
                 WHERE
                  MODULE.ID_MODULE_PEC = #TMP_FINANCEMENTS.ID_MODULE_PEC
                 FOR XML RAW('DETAIL_FINANCEMENT'), ELEMENTS, TYPE
                ),
                (
                 SELECT
                  dbo.GetFrenchCurrencyFormat(CAST(SUM(#TMP_FINANCEMENTS.MNT_FIN_COMPTE) AS MONEY)) AS TOT_MNT_FIN_COMPTE,
                  dbo.GetFrenchCurrencyFormat(CAST(SUM(#TMP_FINANCEMENTS.MNT_FIN_CPF) AS MONEY)) AS TOT_MNT_FIN_CPF,
                  dbo.GetFrenchCurrencyFormat(CAST(SUM(#TMP_FINANCEMENTS.MNT_FIN_DIF_PPRO) AS MONEY)) AS TOT_MNT_FIN_DIF_PPRO,
                  dbo.GetFrenchCurrencyFormat(CAST(SUM(#TMP_FINANCEMENTS.MNT_FIN_FDS_MUT) AS MONEY)) AS TOT_MNT_FIN_FDS_MUT,
                  dbo.GetFrenchCurrencyFormat(CAST(SUM(#TMP_FINANCEMENTS.MNT_FIN_AUTRES) AS MONEY)) AS TOT_MNT_FIN_AUTRES,
                  dbo.GetFrenchCurrencyFormat(CAST(SUM(#TMP_FINANCEMENTS.MNT_TOTAL) AS MONEY)) AS TOT_MNT_TOTAL,
                  CASE @ACTION_CO 
                   WHEN  0
                    THEN dbo.GetFrenchCurrencyFormat(CAST(SUM(#TMP_FINANCEMENTS.MNT_CHARGE_ENT) AS MONEY))
                   ELSE  'Action Co.'
                  END AS TOT_MNT_CHARGE_ENT,
                  CASE @ACTION_CO
                   WHEN  0
                    THEN dbo.GetFrenchCurrencyFormat(CAST(SUM(#TMP_FINANCEMENTS.MNT_DEMANDE) AS MONEY))
                   ELSE  'Action Co.'
                  END AS TOT_MNT_COUT_TOT
                 FROM
                  #TMP_FINANCEMENTS
                 WHERE
                  MODULE.ID_MODULE_PEC = #TMP_FINANCEMENTS.ID_MODULE_PEC
                 GROUP BY
                  #TMP_FINANCEMENTS.ID_MODULE_PEC
                 FOR XML RAW('TOTAL_FINANCEMENT'), ELEMENTS, TYPE
                ),
                
                BLN_DELEGATION_PAIEMENT AS SUBROGE,
                CONTIENT_REPOF, 
                CONTIENT_CP ,
                dbo.GETFRENCHCURRENCYFORMAT(CAST(LIMITE_REPOF AS MONEY)) as LIMITE_REPOF,
           dbo.GETFRENCHCURRENCYFORMAT(CAST(LIMITE_CP AS MONEY)) as LIMITE_CP,
                (
                 SELECT
                  dbo.GetFrenchCurrencyFormat(SUM(#TMP_NB_HEURES_INDIVIDUS.NB_HEURE_ENGAGE)) AS NB_HEURES_STAG_ENG
                 FROM
                  #TMP_NB_HEURES_INDIVIDUS
                 WHERE
                  MODULE.ID_MODULE_PEC = #TMP_NB_HEURES_INDIVIDUS.ID_MODULE_PEC
                 GROUP
                  BY #TMP_NB_HEURES_INDIVIDUS.ID_MODULE_PEC
                 FOR XML RAW('NB_HEURES_STAGIAIRES'), ELEMENTS, TYPE
                ),
                (
                 SELECT
                  cast(count(distinct #TMP_NB_HEURES_INDIVIDUS.ID_INDIVIDU) AS INT) AS NB_STAG
                 FROM
                  #TMP_NB_HEURES_INDIVIDUS
                 WHERE
                  MODULE.ID_MODULE_PEC = #TMP_NB_HEURES_INDIVIDUS.ID_MODULE_PEC
                 GROUP BY
                  #TMP_NB_HEURES_INDIVIDUS.ID_MODULE_PEC
                 FOR XML RAW('NB_STAGIAIRES'), ELEMENTS, TYPE
                ),
                (
                 SELECT
                  'Dont ' + #TMP_NB_HEURES_INDIVIDUS.LIBC_DISPOSITIF AS LIB_DISPOSITIF,
                  dbo.GetFrenchCurrencyFormat(sum(#TMP_NB_HEURES_INDIVIDUS.NB_HEURE_ENGAGE)) AS NB_HEURES_STAG_DISP_ENG
                 FROM
                  #TMP_NB_HEURES_INDIVIDUS
                 WHERE
                  MODULE.ID_MODULE_PEC = #TMP_NB_HEURES_INDIVIDUS.ID_MODULE_PEC
                 GROUP BY
                  #TMP_NB_HEURES_INDIVIDUS.ID_MODULE_PEC,
                  #TMP_NB_HEURES_INDIVIDUS.LIBC_DISPOSITIF
                 FOR XML RAW('NB_HEURES_STAGIAIRES_DISP'), ELEMENTS, TYPE
                ),
                (
                 SELECT
                  NOM_INDIVIDU,
                  PRENOM_INDIVIDU
                 FROM
                  #TMP_INDIVIDUS
                 WHERE
                  MODULE.ID_MODULE_PEC = #TMP_INDIVIDUS.ID_MODULE_PEC
                 FOR XML RAW('STAGIAIRES'), ELEMENTS, TYPE
                )
               FROM
                #TMP_MODULE AS MODULE
               WHERE
                EXISTS
                (
                 SELECT 1
                 FROM
                  #TMP_MODULES_INDIVIDUS
                 WHERE
                  #TMP_MODULES_INDIVIDUS.ID_MODULE_PEC = MODULE.ID_MODULE_PEC
                )
               FOR XML AUTO, ELEMENTS, TYPE
              )
             )
            FROM
             #TMP_ACTION_PEC  AS ANNEXES
            FOR XML AUTO, ELEMENTS, TYPE
           )
          FROM
           #TMP_GENERAL as LETTRE
          FOR XML AUTO, ELEMENTS
         END
         
		CREATE PROCEDURE [dbo].[INS_R19_SIMPLE]
          @ID_ADHERENT INT,
          @ID_ACTIVITE INT,
          @ID_PERIODE INT
         AS
         -- =============================================  
         -- Author:  SFEIR - DSZ  
         -- Create date: 04/11/2014 
         -- Description: Insertion simple (i.e. sans v‚rification de contraintes m‚tier) d'une ligne dans R19.
         -- Remarques: en vue de la reforme 2015 insertion ligne simple
         --    sans verification si le type d'activite existe deja
         --    pour ne pas retoucher INS_R19 car impact possible
         --    non ‚tudi‚ (s–r : Synchro Extranet).
         -- =============================================  
         BEGIN
          if not exists(select 1 from R19 
          where ID_ACTIVITE = @ID_ACTIVITE
          and ID_ADHERENT = @ID_ADHERENT 
          and ID_PERIODE = @ID_PERIODE)
           -- <--fin ajout DSZ
           INSERT INTO R19
           (
            ID_ADHERENT,
            ID_ACTIVITE,
            ID_PERIODE
           )
           VALUES
           (
            @ID_ADHERENT,
            @ID_ACTIVITE,
            @ID_PERIODE
           )
         END
		          
create procedure CKParser.TheEnd 
as 
begin         
	print 'Everything worked';
end