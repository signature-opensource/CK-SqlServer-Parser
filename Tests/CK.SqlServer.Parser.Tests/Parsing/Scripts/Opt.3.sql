	CREATE PROCEDURE [dbo].[LEC_GRP_MVT_COMPTE_PER]
          @ID_GROUPE INT,
          @DAT_DEBUT DATETIME,  
          @DAT_FIN DATETIME
         AS

         BEGIN
          SET NOCOUNT ON

          DECLARE
           @LIB_RAISON_SOCIALE VARCHAR(50),
           @ID     INT,
           @ID_COMPTE   INT,
           @ID_COMPTE_SAV  INT,
           @MNT_MVT   DECIMAL(18,2),
           @MNT_SOLDE   DECIMAL(18,2),
           @MNT_SOLDE_SAV  DECIMAL(18,2),
           @P_E_R    VARCHAR(1),
           @P_E_R_SAV   VARCHAR(1),
           @COD_GROUPE   VARCHAR(8)

          IF (@ID_GROUPE IS NOT NULL)
          BEGIN
           SELECT
            @LIB_RAISON_SOCIALE = LIB_GROUPE,
            @COD_GROUPE = COD_GROUPE
           FROM
            GROUPE
           WHERE
            ID_GROUPE = @ID_GROUPE

           SELECT DISTINCT
            COMPTE.ID_GROUPE,
            LIB_RAISON_SOCIALE = @LIB_RAISON_SOCIALE,
            COMPTE.ID_COMPTE,
            COMPTE.ID_TYPE_FINANCEMENT,
            PERIODE.ID_PERIODE,
            PERIODE.NUM_ANNEE,
            CAST(0 AS decimal(18,2)) MNT_INITIAL_P,
            CAST(0 AS decimal(18,2)) MNT_FINAL_P,
            CAST(0 AS decimal(18,2)) MNT_INITIAL_E,
            CAST(0 AS decimal(18,2)) MNT_FINAL_E,
            CAST(0 AS decimal(18,2)) MNT_INITIAL_R,
            CAST(0 AS decimal(18,2)) MNT_FINAL_R
           INTO
            #TMP_CPT_INIT
           FROM
            COMPTE
            INNER JOIN MVT_BUDGETAIRE
             ON COMPTE.ID_COMPTE = MVT_BUDGETAIRE.ID_COMPTE
            INNER JOIN PERIODE
             ON COMPTE.ID_PERIODE = PERIODE.ID_PERIODE
           WHERE
            COMPTE.ID_GROUPE = @ID_GROUPE

            AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112)<= convert(varchar(8), @DAT_FIN, 112)
           OPTION(MAXDOP 1)

           SELECT
            MVT.ID_TYPE_FINANCEMENT,
            SUM
            (
             CASE
              WHEN  P_E_R IN ('R')
                 AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112) < convert(varchar(8), @DAT_DEBUT, 112)
               THEN MNT_MVT_BUDGETAIRE * (CASE PB.SENS WHEN 'D' THEN -1 ELSE 1 END)
              ELSE  0
             END
            ) MNT_INITIAL_R,
            SUM
            (
             CASE
              WHEN  P_E_R IN ('R','E') AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112) < convert(varchar(8), @DAT_DEBUT, 112)
               THEN MNT_MVT_BUDGETAIRE * (CASE PB.SENS WHEN 'D' THEN -1 ELSE 1 END)
              ELSE  0
             END
            ) MNT_INITIAL_E,
            SUM
            (
             CASE
              WHEN  P_E_R IN ('R','E','P') AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112) < convert(varchar(8), @DAT_DEBUT, 112)
               THEN MNT_MVT_BUDGETAIRE * (CASE PB.SENS WHEN 'D' THEN -1 ELSE 1 END)
              ELSE  0
             END
            ) MNT_INITIAL_P,
            SUM
            (
             CASE
              WHEN  P_E_R IN ('R')
               THEN MNT_MVT_BUDGETAIRE * (CASE PB.SENS WHEN 'D' THEN -1 ELSE 1 END)
              ELSE  0
             END
            ) MNT_FINAL_R,
            SUM
            (
             CASE
              WHEN  P_E_R IN ('R','E')
               THEN MNT_MVT_BUDGETAIRE * (CASE PB.SENS WHEN 'D' THEN -1 ELSE 1 END)
              ELSE  0
             END
            ) MNT_FINAL_E,
            SUM
            (
             CASE
              WHEN  P_E_R IN ('R','E','P')
               THEN MNT_MVT_BUDGETAIRE * (CASE PB.SENS WHEN 'D' THEN -1 ELSE 1 END)
              ELSE  0
             END
            ) MNT_FINAL_P
           INTO 
            #TMP_SOLDES
           FROM
            MVT_BUDGETAIRE MVT
            INNER JOIN COMPTE CPT
             ON CPT.ID_COMPTE = MVT.ID_COMPTE
            INNER JOIN PARAMETRAGE_BUDGETAIRE PB
             ON PB.ID_TYPE_MOUVEMENT = MVT.ID_TYPE_MOUVEMENT
              AND PB.ID_TYPE_COMPTE = CPT.ID_TYPE_COMPTE
           WHERE
            CPT.ID_GROUPE = @ID_GROUPE
            AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112)<= convert(varchar(8), @DAT_FIN, 112)
           GROUP BY
            MVT.ID_TYPE_FINANCEMENT

           UPDATE
            #TMP_CPT_INIT
           SET
            MNT_INITIAL_R = #TMP_SOLDES.MNT_INITIAL_R,
            MNT_INITIAL_E = #TMP_SOLDES.MNT_INITIAL_E,
            MNT_INITIAL_P = #TMP_SOLDES.MNT_INITIAL_P,
            MNT_FINAL_R  = #TMP_SOLDES.MNT_FINAL_R,
            MNT_FINAL_E  = #TMP_SOLDES.MNT_FINAL_E,
            MNT_FINAL_P  = #TMP_SOLDES.MNT_FINAL_P
           FROM
            #TMP_SOLDES
           WHERE
            #TMP_CPT_INIT.ID_TYPE_FINANCEMENT = #TMP_SOLDES.ID_TYPE_FINANCEMENT

           CREATE TABLE #TMP_MVT
           (
            ID      INT NOT NULL IDENTITY,
            COD_MODULE_PEC   VARCHAR(15),
            ID_COMPTE    INT,
            ID_ADHERENT    INT,
            DAT_MVT_BUDGETAIRE  DATETIME,
            LIBL_TYPE_FINANCEMENT VARCHAR(50),
            LIBC_PUBLIC_FAF   VARCHAR(50),
            NUM_ANNEE    INT,
            MNT_MVT     DECIMAL(18,2),
            MNT_DEBIT    DECIMAL(18,2),
            MNT_CREDIT    DECIMAL(18,2),
            LIBL_MVT_BUDGETAIRE  VARCHAR(60),
            LIBL_PERIODE   VARCHAR(50),
            P_E_R     VARCHAR(1),
            NUM_SIRET    VARCHAR(14),
            LIB_VIL_CEDEX   VARCHAR(50),
            ID_TYPE_FINANCEMENT  INT
           )

           INSERT INTO #TMP_MVT
           (
            COD_MODULE_PEC,
            ID_COMPTE,
            ID_ADHERENT,
            DAT_MVT_BUDGETAIRE,
            LIBL_TYPE_FINANCEMENT,
            LIBC_PUBLIC_FAF,
            NUM_ANNEE,
            MNT_MVT,
            MNT_DEBIT,
            MNT_CREDIT,
            LIBL_MVT_BUDGETAIRE,
            LIBL_PERIODE,
            P_E_R,
            NUM_SIRET,
            LIB_VIL_CEDEX,
            ID_TYPE_FINANCEMENT
           )
           SELECT
            COD_MODULE_PEC,
            MVT.ID_COMPTE,
            MVT.ID_ADHERENT,
            MVT.DAT_MVT_BUDGETAIRE,
            LIBL_TYPE_FINANCEMENT,
            CASE COMPTE.ID_TYPE_FINANCEMENT
             WHEN  2
              THEN 'Fonds mutualis‚s'
             ELSE  'Tout Public'
            END,
            PERIODE.NUM_ANNEE,
            MNT_MVT_SIRIUS = MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 1 ELSE -1 END),
            MNT_DEBIT = MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 1 ELSE 0 END),
            MNT_CREDIT = MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 0 ELSE 1 END),
            LIBL_MVT_BUDGETAIRE,
            LIBL_PERIODE = 'Ann‚e Rbt. P' + COD_ACTIVITE + ' ' + CAST(NUM_ANNEE AS VARCHAR),
            P_E_R,
            ETABLISSEMENT.NUM_SIRET,
            CASE
             WHEN  PATINDEX('%CEDEX%', ADRESSE.LIB_VIL_CEDEX) <> 0
              THEN LEFT(ADRESSE.LIB_VIL_CEDEX, PATINDEX('%CEDEX%', ADRESSE.LIB_VIL_CEDEX) - 1)
             ELSE  ADRESSE.LIB_VIL_CEDEX
            END AS LIB_VIL_CEDEX,
            TYPE_FINANCEMENT.ID_TYPE_FINANCEMENT
           FROM
            MVT_BUDGETAIRE MVT
            INNER JOIN EVENEMENT
             ON MVT.ID_EVENEMENT = EVENEMENT.ID_EVENEMENT
            LEFT JOIN MODULE_PEC
             ON MODULE_PEC.ID_MODULE_PEC = MVT.ID_MODULE_PEC
            INNER JOIN COMPTE
             ON MVT.ID_COMPTE = COMPTE.ID_COMPTE
            INNER JOIN TYPE_COMPTE
             ON COMPTE.ID_TYPE_COMPTE = TYPE_COMPTE.ID_TYPE_COMPTE
            INNER JOIN TYPE_MOUVEMENT
             ON MVT.ID_TYPE_MOUVEMENT = TYPE_MOUVEMENT.ID_TYPE_MOUVEMENT
            INNER JOIN PARAMETRAGE_BUDGETAIRE
             ON PARAMETRAGE_BUDGETAIRE.ID_TYPE_COMPTE = TYPE_COMPTE.ID_TYPE_COMPTE
              AND PARAMETRAGE_BUDGETAIRE.ID_TYPE_MOUVEMENT = TYPE_MOUVEMENT.ID_TYPE_MOUVEMENT
            INNER JOIN PERIODE
             ON COMPTE.ID_PERIODE = PERIODE.ID_PERIODE
            INNER JOIN TYPE_FINANCEMENT
             ON COMPTE.ID_TYPE_FINANCEMENT = TYPE_FINANCEMENT.ID_TYPE_FINANCEMENT
            INNER JOIN ACTIVITE
             ON COMPTE.ID_ACTIVITE = ACTIVITE.ID_ACTIVITE
            LEFT JOIN ETABLISSEMENT
             ON MVT.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT
            LEFT JOIN ADRESSE
             ON ADRESSE.ID_ADRESSE = ETABLISSEMENT.ID_ADRESSE_PRINCIPALE
           WHERE
            COMPTE.ID_GROUPE = @ID_GROUPE
            AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112)>= convert(varchar(8), @DAT_DEBUT, 112)
            AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112)<= convert(varchar(8), @DAT_FIN, 112)
           OPTION(MAXDOP 1)

           SELECT DISTINCT
            (SELECT MAX(ID_COMPTE) FROM #TMP_CPT_INIT TMP WHERE #TMP_CPT_INIT.ID_TYPE_FINANCEMENT = TMP.ID_TYPE_FINANCEMENT GROUP BY TMP.ID_TYPE_FINANCEMENT)AS ID_COMPTE,
            #TMP_CPT_INIT.ID_TYPE_FINANCEMENT
           INTO
            #TMP_COMPTES
           FROM
            #TMP_CPT_INIT
           WHERE
            #TMP_CPT_INIT.ID_TYPE_FINANCEMENT NOT IN (SELECT DISTINCT ID_TYPE_FINANCEMENT FROM #TMP_MVT)
           GROUP BY
            ID_COMPTE,
            ID_TYPE_FINANCEMENT

           INSERT INTO #TMP_MVT
           (
            ID_COMPTE,
            LIBC_PUBLIC_FAF,
            MNT_MVT,
            MNT_DEBIT,
            MNT_CREDIT,
            ID_TYPE_FINANCEMENT,
            LIBL_TYPE_FINANCEMENT
           )
           SELECT DISTINCT
            ID_COMPTE,
            CASE #TMP_COMPTES.ID_TYPE_FINANCEMENT
             WHEN  2
              THEN 'Fonds mutualis‚s'
             ELSE  'Tout Public'
            END AS LIBC_PUBLIC_FAF,
            0,
            0,
            0,
            #TMP_COMPTES.ID_TYPE_FINANCEMENT,
            LIBL_TYPE_FINANCEMENT
           FROM
            #TMP_COMPTES
            INNER JOIN TYPE_FINANCEMENT
             ON TYPE_FINANCEMENT.ID_TYPE_FINANCEMENT = #TMP_COMPTES.ID_TYPE_FINANCEMENT
           GROUP BY
            ID_COMPTE,
            #TMP_COMPTES.ID_TYPE_FINANCEMENT,
            LIBL_TYPE_FINANCEMENT

           SELECT
            ID_GROUPE = #TMP_CPT_INIT.ID_GROUPE,
            ID_ADHERENT = #TMP_MVT.ID_ADHERENT,
            LIB_RAISON_SOCIALE = #TMP_CPT_INIT.LIB_RAISON_SOCIALE,
            LIBL_TYPE_FINANCEMENT = UPPER(#TMP_MVT.LIBL_TYPE_FINANCEMENT),
            LIBC_PUBLIC_FAF = #TMP_MVT.LIBC_PUBLIC_FAF,
            LIBL_PERIODE = #TMP_MVT.LIBL_PERIODE,
            MNT_INITIAL_P = #TMP_CPT_INIT.MNT_INITIAL_P,
            MNT_INITIAL_E = #TMP_CPT_INIT.MNT_INITIAL_E,
            MNT_INITIAL_R = #TMP_CPT_INIT.MNT_INITIAL_R,
            MNT_FINAL_P = #TMP_CPT_INIT.MNT_FINAL_P,
            MNT_FINAL_E = #TMP_CPT_INIT.MNT_FINAL_E,
            MNT_FINAL_R = #TMP_CPT_INIT.MNT_FINAL_R,
            DAT_MVT_BUDGETAIRE = #TMP_MVT.DAT_MVT_BUDGETAIRE,
            LIBL_MVT_BUDGETAIRE = #TMP_MVT.LIBL_MVT_BUDGETAIRE,
            P_E_R,
            #TMP_MVT.NUM_SIRET,
            #TMP_MVT.LIB_VIL_CEDEX,
            MNT_DEBIT,
            MNT_CREDIT,
            ID_COMPTE = #TMP_MVT.ID_COMPTE,
            @COD_GROUPE AS COD_GROUPE,
            (SELECT SUM(MNT_DEBIT) FROM #TMP_MVT T  WHERE P_E_R = 'P' AND #TMP_MVT.LIBL_TYPE_FINANCEMENT = T.LIBL_TYPE_FINANCEMENT) AS SOLD_MNT_DEBIT_P,
            (SELECT SUM(MNT_DEBIT) FROM #TMP_MVT T  WHERE P_E_R = 'E' AND #TMP_MVT.LIBL_TYPE_FINANCEMENT = T.LIBL_TYPE_FINANCEMENT) AS SOLD_MNT_DEBIT_E,
            (SELECT SUM(MNT_DEBIT) FROM #TMP_MVT T  WHERE P_E_R = 'R' AND #TMP_MVT.LIBL_TYPE_FINANCEMENT = T.LIBL_TYPE_FINANCEMENT) AS SOLD_MNT_DEBIT_R,
            (SELECT SUM(MNT_CREDIT) FROM #TMP_MVT T  WHERE P_E_R = 'P' AND #TMP_MVT.LIBL_TYPE_FINANCEMENT = T.LIBL_TYPE_FINANCEMENT) AS SOLD_MNT_CREDIT_P,
            (SELECT SUM(MNT_CREDIT) FROM #TMP_MVT T  WHERE P_E_R = 'E' AND #TMP_MVT.LIBL_TYPE_FINANCEMENT = T.LIBL_TYPE_FINANCEMENT) AS SOLD_MNT_CREDIT_E,
            (SELECT SUM(MNT_CREDIT) FROM #TMP_MVT T  WHERE P_E_R = 'R' AND #TMP_MVT.LIBL_TYPE_FINANCEMENT = T.LIBL_TYPE_FINANCEMENT) AS SOLD_MNT_CREDIT_R
           FROM
            #TMP_MVT
            INNER JOIN #TMP_CPT_INIT
             ON #TMP_MVT.ID_COMPTE = #TMP_CPT_INIT.ID_COMPTE
           ORDER BY
            #TMP_MVT.ID_COMPTE,
            #TMP_MVT.LIBL_TYPE_FINANCEMENT,
            #TMP_MVT.LIBC_PUBLIC_FAF DESC,
            #TMP_MVT.LIBL_PERIODE,
            #TMP_MVT.COD_MODULE_PEC,
            DAT_MVT_BUDGETAIRE
          END
         END

         CREATE PROCEDURE INS_VIREMENT_INTERNE_COLLECTE__ET__UPD_POSTE_IMPUTATION
         	@MODE_NON_VERSEMENT		INT

         AS
         BEGIN
         	DECLARE
         		@LAST_ID_OVIC	INT

         	SELECT
         		POSTE_IMPUTATION.ID_POSTE_IMPUTATION,
         		POSTE_IMPUTATION.ID_ACTIVITE,
         		POSTE_IMPUTATION.ID_POSTE_VERSEMENT,
         		POSTE_IMPUTATION.ID_VIREMENT_INTERNE_COLLECTE,
         		POSTE_IMPUTATION.MNT_HT,
         		POSTE_IMPUTATION.MNT_TVA,
         		POSTE_IMPUTATION.DAT_MAJ_CPT_ADH ,
         		POSTE_IMPUTATION.TIME_STAMP,
         		POSTE_IMPUTATION.ID_VIREMENT_INTERNE_COLLECTE_IMPAYE,
         		CASE

         			WHEN ID_VIREMENT_INTERNE_COLLECTE	IS NOT NULL
         				THEN	POSTE_IMPUTATION.MNT_TTC*(-1)
         			ELSE													POSTE_IMPUTATION.MNT_TTC
         		END															AS MNT_TTC,

         		

         		CASE

         			WHEN ID_VIREMENT_INTERNE_COLLECTE	IS NOT NULL
         				THEN	NULL
         			WHEN TYPE_VERSEMENT.BLN_VERSEMENT_INITIAL = 1
         				THEN 1 
         			WHEN TYPE_VERSEMENT.BLN_VERSEMENT_INITIAL = 0 AND TYPE_VERSEMENT.BLN_VERSEMENT = 1
         				THEN 2 
         			ELSE  3 
         		END															AS TYPE_VERS
         	INTO
         		#TMP_PI
         	FROM
         		POSTE_IMPUTATION
         		INNER JOIN POSTE_VERSEMENT ON POSTE_IMPUTATION.ID_POSTE_VERSEMENT=POSTE_VERSEMENT.ID_POSTE_VERSEMENT 
         		INNER JOIN TYPE_VERSEMENT ON POSTE_VERSEMENT.ID_TYPE_VERSEMENT=TYPE_VERSEMENT.ID_TYPE_VERSEMENT
         		INNER JOIN VERSEMENT ON VERSEMENT.ID_VERSEMENT=POSTE_VERSEMENT.ID_VERSEMENT
         		LEFT JOIN BORDEREAU ON VERSEMENT.ID_BORDEREAU=BORDEREAU.ID_BORDEREAU
         	WHERE
         		POSTE_VERSEMENT.BLN_ACTIF						= 1
         		AND VERSEMENT.BLN_ACTIF								= 1
         		AND POSTE_IMPUTATION.MNT_TTC						<> 0
         		AND
         		(
         			@MODE_NON_VERSEMENT							= 0

         			AND TYPE_VERSEMENT.BLN_VERSEMENT_INITIAL		= 1
         			AND ID_VIREMENT_INTERNE_COLLECTE				IS NULL
         			AND BORDEREAU.ID_LOT_REMISE_BANCAIRE			IS NOT NULL

         			OR @MODE_NON_VERSEMENT							= 1

         			AND TYPE_VERSEMENT.BLN_VERSEMENT_INITIAL		= 0

         			AND ID_VIREMENT_INTERNE_COLLECTE				IS NULL

         			OR @MODE_NON_VERSEMENT							= 1
         			AND VERSEMENT.ID_STATUT_VERSEMENT				= 7
         			AND ID_VIREMENT_INTERNE_COLLECTE_IMPAYE			IS NULL
         			AND ID_VIREMENT_INTERNE_COLLECTE				IS NOT NULL
         		)
         	ORDER BY
         		
         		ID_ACTIVITE, ID_POSTE_IMPUTATION

         	IF EXISTS (SELECT ID_POSTE_IMPUTATION FROM #TMP_PI)
         	BEGIN

         		SELECT TOP 1
         			@LAST_ID_OVIC = ID_VIREMENT_INTERNE_COLLECTE
         		FROM
         			VIREMENT_INTERNE_COLLECTE
         		ORDER BY
         			ID_VIREMENT_INTERNE_COLLECTE DESC

         		SELECT
         			
         			#TMP_PI.ID_ACTIVITE,
         			NULL							AS DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         			NULL							AS DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         			round(sum(MNT_TTC), 2)			AS MNT_VIREMENT_INTERNE_COLLECTE,
         			NULL							AS DAT_GENERATION_SEPA,
         			TYPE_VERS
         		INTO
         			#TMP_OVIC
         		FROM
         			#TMP_PI
         		GROUP BY
         			
         			ID_ACTIVITE,
         			TYPE_VERS
         		ORDER BY
         			
         			ID_ACTIVITE, TYPE_VERS

         		IF @MODE_NON_VERSEMENT = 1
         		BEGIN

         			INSERT INTO
         				VIREMENT_INTERNE_COLLECTE
         				(
         					ID_ACTIVITE,
         					DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         					DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         					MNT_VIREMENT_INTERNE_COLLECTE,
         					DAT_GENERATION_SEPA
         				)
         			SELECT
         				ID_ACTIVITE,
         				DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         				DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         				MNT_VIREMENT_INTERNE_COLLECTE,
         				DAT_GENERATION_SEPA
         			FROM
         				#TMP_OVIC
         			WHERE

         				TYPE_VERS IS NULL

         			UPDATE
         				POSTE_IMPUTATION
         			SET
         					POSTE_IMPUTATION.ID_VIREMENT_INTERNE_COLLECTE_IMPAYE	= VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE
         			FROM
         				#TMP_PI
         				INNER JOIN VIREMENT_INTERNE_COLLECTE	ON #TMP_PI.ID_ACTIVITE	= VIREMENT_INTERNE_COLLECTE.ID_ACTIVITE
         			WHERE
         				POSTE_IMPUTATION.ID_POSTE_IMPUTATION					= #TMP_PI.ID_POSTE_IMPUTATION
         				AND POSTE_IMPUTATION.TIME_STAMP								= #TMP_PI.TIME_STAMP

         				AND	#TMP_PI.TYPE_VERS											IS NULL
         				AND VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE	> @LAST_ID_OVIC

         			

         			SELECT TOP 1
         				@LAST_ID_OVIC = ID_VIREMENT_INTERNE_COLLECTE
         			FROM
         				VIREMENT_INTERNE_COLLECTE
         			ORDER BY
         				ID_VIREMENT_INTERNE_COLLECTE DESC

         			INSERT INTO
         				VIREMENT_INTERNE_COLLECTE
         				(
         					ID_ACTIVITE,
         					DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         					DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         					MNT_VIREMENT_INTERNE_COLLECTE,
         					DAT_GENERATION_SEPA
         				)
         			SELECT
         				ID_ACTIVITE,
         				DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         				DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         				MNT_VIREMENT_INTERNE_COLLECTE,
         				DAT_GENERATION_SEPA
         			FROM
         				#TMP_OVIC
         			WHERE

         				TYPE_VERS = 2 

         			UPDATE
         				POSTE_IMPUTATION
         			SET
         				POSTE_IMPUTATION.ID_VIREMENT_INTERNE_COLLECTE	= VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE
         			FROM
         				#TMP_PI
         				INNER JOIN VIREMENT_INTERNE_COLLECTE	ON #TMP_PI.ID_ACTIVITE	= VIREMENT_INTERNE_COLLECTE.ID_ACTIVITE
         			WHERE
         				POSTE_IMPUTATION.ID_POSTE_IMPUTATION					= #TMP_PI.ID_POSTE_IMPUTATION
         				AND POSTE_IMPUTATION.TIME_STAMP								= #TMP_PI.TIME_STAMP

         				AND TYPE_VERS = 2
         				AND VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE	> @LAST_ID_OVIC

         			

         			SELECT TOP 1
         				@LAST_ID_OVIC = ID_VIREMENT_INTERNE_COLLECTE
         			FROM 
         				VIREMENT_INTERNE_COLLECTE
         			ORDER BY
         				ID_VIREMENT_INTERNE_COLLECTE DESC

         			INSERT INTO
         				VIREMENT_INTERNE_COLLECTE
         				(
         					ID_ACTIVITE,
         					DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         					DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         					MNT_VIREMENT_INTERNE_COLLECTE,
         					DAT_GENERATION_SEPA
         				)
         			SELECT
         				ID_ACTIVITE,
         				DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         				DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         				MNT_VIREMENT_INTERNE_COLLECTE,
         				DAT_GENERATION_SEPA
         			FROM
         				#TMP_OVIC
         			WHERE

         				TYPE_VERS = 3 

         			UPDATE
         				POSTE_IMPUTATION
         			SET
         				POSTE_IMPUTATION.ID_VIREMENT_INTERNE_COLLECTE	= VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE
         			FROM
         				#TMP_PI
         				INNER JOIN VIREMENT_INTERNE_COLLECTE	ON #TMP_PI.ID_ACTIVITE	= VIREMENT_INTERNE_COLLECTE.ID_ACTIVITE
         			WHERE
         				POSTE_IMPUTATION.ID_POSTE_IMPUTATION					= #TMP_PI.ID_POSTE_IMPUTATION
         				AND POSTE_IMPUTATION.TIME_STAMP								= #TMP_PI.TIME_STAMP

         				AND TYPE_VERS												= 3

         				AND	VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE	> @LAST_ID_OVIC

         			

         		END
         		ELSE
         		BEGIN

         			SELECT TOP 1
         				@LAST_ID_OVIC = ID_VIREMENT_INTERNE_COLLECTE
         			FROM
         				VIREMENT_INTERNE_COLLECTE
         			ORDER BY
         				ID_VIREMENT_INTERNE_COLLECTE DESC
         			INSERT INTO VIREMENT_INTERNE_COLLECTE
         			(
         				ID_ACTIVITE,
         				DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         				DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         				MNT_VIREMENT_INTERNE_COLLECTE,
         				DAT_GENERATION_SEPA
         			)
         			SELECT
         				ID_ACTIVITE,
         				DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         				DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         				sum(MNT_VIREMENT_INTERNE_COLLECTE),
         				DAT_GENERATION_SEPA
         			FROM
         				#TMP_OVIC
         			GROUP BY
         				ID_ACTIVITE,
         				DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         				DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         				DAT_GENERATION_SEPA

         			UPDATE
         				POSTE_IMPUTATION
         			SET
         				POSTE_IMPUTATION.ID_VIREMENT_INTERNE_COLLECTE	= VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE
         			FROM
         				#TMP_PI
         				INNER JOIN VIREMENT_INTERNE_COLLECTE	ON #TMP_PI.ID_ACTIVITE	= VIREMENT_INTERNE_COLLECTE.ID_ACTIVITE
         			WHERE
         				POSTE_IMPUTATION.ID_POSTE_IMPUTATION					= #TMP_PI.ID_POSTE_IMPUTATION
         				AND POSTE_IMPUTATION.TIME_STAMP								= #TMP_PI.TIME_STAMP
         				AND VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE	> @LAST_ID_OVIC
         		END

         		

         		SELECT
         			'Return Value' = 1
         	END
         	ELSE
         	BEGIN
         		SELECT
         			'Return Value' = 0
         	END

         END

CREATE PROCEDURE [dbo].[LEC_GRP_COMPTE_GROUPE_ADHERENT]
         @ID_GROUPE integer,
         @ID_ADHERENT integer,
         @DAT_DEBUT datetime,
         @DAT_FIN datetime
         AS

         BEGIN
          SET NOCOUNT ON

          DECLARE
           @LIB_RAISON_SOCIALE VARCHAR(50),
           @ID     INT, 
           @ID_COMPTE   INT,
           @ID_COMPTE_SAV  INT,
           @MNT_MVT   DECIMAL(18,2),
           @MNT_SOLDE   DECIMAL(18,2),
           @BLN_OK    TINYINT,
           @MNT_SOLDE_SAV  DECIMAL(18,2)

          SET @BLN_OK = 0

          IF (@ID_GROUPE IS NOT NULL)
          BEGIN
           IF EXISTS (SELECT 1 FROM ADHERENT WHERE ID_GROUPE = @ID_GROUPE AND BLN_GESTION_GROUPE = 1)
           BEGIN
            SET @BLN_OK = 1
            SELECT
             @LIB_RAISON_SOCIALE = LIB_GROUPE 
            FROM
             GROUPE
            WHERE
             ID_GROUPE = @ID_GROUPE
           END
          END 

          IF @BLN_OK = 1
          BEGIN

           SELECT DISTINCT
            COMPTE.ID_GROUPE,
            LIB_RAISON_SOCIALE = @LIB_RAISON_SOCIALE, 
            COMPTE.ID_COMPTE,
            COMPTE.ID_TYPE_FINANCEMENT,
            PERIODE.ID_PERIODE,
            PERIODE.NUM_ANNEE,
            CAST(0 AS DECIMAL(18,2)) MNT_INITIAL,
            CAST(0 AS DECIMAL(18,2)) MNT_FINAL
           INTO
            #TMP_CPT_INIT
           FROM
            COMPTE,
            MVT_BUDGETAIRE,
            PERIODE
           WHERE 
            @ID_GROUPE IS NOT NULL
            AND COMPTE.ID_GROUPE = @ID_GROUPE
            AND COMPTE.ID_COMPTE = MVT_BUDGETAIRE.ID_COMPTE
            AND COMPTE.ID_PERIODE = PERIODE.ID_PERIODE
            AND MVT_BUDGETAIRE.P_E_R = 'R'
            AND CONVERT(VARCHAR(8), DAT_MVT_BUDGETAIRE, 112)>= CONVERT(VARCHAR(8), @DAT_DEBUT, 112)
            AND CONVERT(VARCHAR(8), DAT_MVT_BUDGETAIRE, 112)<= CONVERT(VARCHAR(8), @DAT_FIN, 112)

           CREATE TABLE #TMP_MVT
           (
            ID integer not null identity,
            ID_COMPTE integer,
            ID_ADHERENT integer,
            DAT_MVT_BUDGETAIRE datetime, 
            LIBL_TYPE_FINANCEMENT varchar(50), 
            LIBC_PUBLIC_FAF varchar(50), 
            NUM_ANNEE integer,
            MNT_MVT decimal(18,2), 
            MNT_DEBIT decimal(18,2),
            MNT_CREDIT decimal(18,2),
            LIBL_MVT_BUDGETAIRE varchar(50),
            LIBL_PERIODE varchar(50),
            MNT_SOLDE decimal(18,2),
            MNT_SOLDE_FINAL decimal(18,2)
           )

           INSERT INTO #TMP_MVT
           ( ID_COMPTE,  
            ID_ADHERENT, 
            DAT_MVT_BUDGETAIRE, 
            LIBL_TYPE_FINANCEMENT, 
            LIBC_PUBLIC_FAF, 
            NUM_ANNEE , 
            MNT_MVT , 
            MNT_DEBIT ,
            MNT_CREDIT ,
            LIBL_MVT_BUDGETAIRE, 
            LIBL_PERIODE,  
            MNT_SOLDE,
            MNT_SOLDE_FINAL)
           SELECT 
            MVT.ID_COMPTE, 
            MVT.ID_ADHERENT, 
            MVT.DAT_MVT_BUDGETAIRE, 
            LIBL_TYPE_FINANCEMENT, 
               CASE COMPTE.ID_TYPE_FINANCEMENT
          WHEN 2
           THEN 'Fonds mutualis‚s'
          ELSE 'Tout Public'
          END,
            PERIODE.NUM_ANNEE, 
            MNT_MVT_SIRIUS = MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 1 ELSE -1 END), 
            MNT_DEBIT = MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 1 ELSE 0 END),
            MNT_CREDIT = MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 0 ELSE 1 END),
            LIBL_MVT_BUDGETAIRE,
            LIBL_PERIODE = 'Ann‚e Rbt. P10+ ' + CAST(NUM_ANNEE AS VARCHAR), 
            MNT_SOLDE = CAST(0 AS decimal(18,2)), 
            MNT_SOLDE_FINAL = CAST(0 AS decimal(18,2)) 
           FROM MVT_BUDGETAIRE MVT, EVENEMENT, COMPTE, TYPE_COMPTE, TYPE_MOUVEMENT, PARAMETRAGE_BUDGETAIRE, PERIODE,
            TYPE_FINANCEMENT
           WHERE
            MVT.ID_EVENEMENT = EVENEMENT.ID_EVENEMENT
            AND MVT.ID_COMPTE = COMPTE.ID_COMPTE
            AND @ID_GROUPE IS NOT NULL  
            AND
            (
             (MVT.ID_ADHERENT IS NOT NULL AND COMPTE.ID_GROUPE = @ID_GROUPE)
             OR
             (MVT.ID_ADHERENT IS NULL AND MVT.ID_GROUPE = @ID_GROUPE)
            )
            AND COMPTE.ID_TYPE_COMPTE = TYPE_COMPTE.ID_TYPE_COMPTE 
            AND MVT.ID_TYPE_MOUVEMENT = TYPE_MOUVEMENT.ID_TYPE_MOUVEMENT
            AND PARAMETRAGE_BUDGETAIRE.ID_TYPE_MOUVEMENT = TYPE_MOUVEMENT.ID_TYPE_MOUVEMENT
            AND PARAMETRAGE_BUDGETAIRE.ID_TYPE_COMPTE = TYPE_COMPTE.ID_TYPE_COMPTE
            AND COMPTE.ID_PERIODE = PERIODE.ID_PERIODE
            AND COMPTE.ID_TYPE_FINANCEMENT = TYPE_FINANCEMENT.ID_TYPE_FINANCEMENT
            AND P_E_R = 'R'
            and convert(varchar(8), DAT_MVT_BUDGETAIRE, 112)>= convert(varchar(8), @DAT_DEBUT, 112)
            and convert(varchar(8), DAT_MVT_BUDGETAIRE, 112)<= convert(varchar(8), @DAT_FIN, 112)
           ORDER BY
            COMPTE.ID_TYPE_FINANCEMENT,
            NUM_ANNEE,
            DAT_MVT_BUDGETAIRE

           UPDATE
            #TMP_CPT_INIT 
           SET MNT_INITIAL = dbo.GetSoldeDeGroupe(@DAT_DEBUT,@ID_GROUPE)

           UPDATE  #TMP_MVT 
           SET #TMP_MVT.MNT_SOLDE_FINAL = dbo.GetSoldeDeGroupe(@DAT_FIN,@ID_GROUPE)

           DECLARE cu_solde CURSOR FOR 
           SELECT ID, ID_COMPTE, MNT_MVT
           FROM #TMP_MVT
           ORDER BY ID

           OPEN cu_solde

           SET @ID_COMPTE_SAV = 0
           FETCH cu_solde INTO
           @ID, @ID_COMPTE, @MNT_MVT

           WHILE (@@fetch_status <> -1)
           BEGIN

            IF @ID_COMPTE_SAV <> @ID_COMPTE
            BEGIN

             SELECT @MNT_SOLDE = MNT_INITIAL - @MNT_MVT
             FROM #TMP_CPT_INIT
             WHERE ID_COMPTE = @ID_COMPTE
            END
            ELSE
            BEGIN
             SELECT @MNT_SOLDE = MNT_SOLDE - @MNT_MVT
             FROM #TMP_MVT
             WHERE ID = @ID - 1
            END

            UPDATE #TMP_MVT SET MNT_SOLDE = @MNT_SOLDE WHERE ID = @ID

            SET @ID_COMPTE_SAV = @ID_COMPTE
            SET @MNT_SOLDE_SAV = @MNT_SOLDE

            FETCH cu_solde INTO
            @ID, @ID_COMPTE, @MNT_MVT

            IF @ID_COMPTE_SAV <> @ID_COMPTE OR (@@fetch_status = -1)
            BEGIN

             UPDATE #TMP_CPT_INIT
             SET MNT_FINAL = @MNT_SOLDE_SAV
             WHERE ID_COMPTE = @ID_COMPTE_SAV
            END
           END

           CLOSE cu_solde 
           DEALLOCATE cu_solde 

           

           UPDATE #TMP_MVT 
           SET MNT_CREDIT = 0, MNT_DEBIT = -1 * MNT_CREDIT
           WHERE MNT_CREDIT < 0

           UPDATE #TMP_MVT 
           SET MNT_DEBIT = 0, MNT_CREDIT = -1 * MNT_DEBIT
           WHERE MNT_DEBIT < 0

           SELECT
            ID_GROUPE = #TMP_CPT_INIT.ID_GROUPE, 
            ID_ADHERENT = #TMP_MVT.ID_ADHERENT, 
            LIB_RAISON_SOCIALE = #TMP_CPT_INIT.LIB_RAISON_SOCIALE, 
            LIBL_TYPE_FINANCEMENT = UPPER(#TMP_MVT.LIBL_TYPE_FINANCEMENT), 
            LIBC_PUBLIC_FAF = #TMP_MVT.LIBC_PUBLIC_FAF, 
            LIBL_PERIODE = #TMP_MVT.LIBL_PERIODE, 
            MNT_INITIAL = #TMP_CPT_INIT.MNT_INITIAL, 
            DAT_MVT_BUDGETAIRE = #TMP_MVT.DAT_MVT_BUDGETAIRE, 
            LIBL_MVT_BUDGETAIRE = #TMP_MVT.LIBL_MVT_BUDGETAIRE, 
            MNT_DEBIT, 
            MNT_CREDIT, 
            MNT_SOLDE, 
            MNT_FINAL, 
            ID_COMPTE = #TMP_MVT.ID_COMPTE,
            MNT_SOLDE_FINAL = #TMP_MVT.MNT_SOLDE_FINAL
           FROM #TMP_MVT, #TMP_CPT_INIT
           WHERE #TMP_MVT .ID_COMPTE = #TMP_CPT_INIT.ID_COMPTE 
          END
          ELSE
          BEGIN

           SELECT
            ID_GROUPE = NULL,
            ID_ADHERENT = NULL,
            LIB_RAISON_SOCIALE = NULL,
            LIBL_TYPE_FINANCEMENT = NULL,
            LIBC_PUBLIC_FAF = NULL,
            LIBL_PERIODE = NULL,
            MNT_INITIAL = NULL,
            DAT_MVT_BUDGETAIRE = NULL,
            LIBL_MVT_BUDGETAIRE = NULL,
            MNT_DEBIT = NULL,
            MNT_CREDIT= NULL,
            MNT_SOLDE= NULL,
            MNT_FINAL= NULL,
            ID_COMPTE = NULL,
            MNT_SOLDE_FINAL = NULL
          END
         END

         CREATE PROCEDURE INS_R22_BIS_EXTRANET 
         	@ID_ETABLISSEMENT int, 
         	@COD_EFFECTIF varchar(8) ,
         	@ANNEE int,
         	@EFFECTIF int
         AS
         BEGIN
         	DECLARE @id_type_effectif int
         	SELECT @id_type_effectif = id_type_effectif from type_effectif where COD_TYPE_EFFECTIF = @COD_EFFECTIF
         	IF @id_type_effectif is not null
         		INSERT INTO R22_BIS
         			   ([ID_TYPE_EFFECTIF]
         			   ,[ID_PERIODE]
         			   ,[ID_ETABLISSEMENT]
         			   ,[NUM_EFFECTIF])
         		 VALUES
         	           (@id_type_effectif
         			   ,(SELECT id_periode from PERIODE where COD_PERIODE = @ANNEE and ID_TYPE_PERIODE = 1)
         			   ,@ID_ETABLISSEMENT
         			   ,@EFFECTIF)

         END

         CREATE PROCEDURE LEC_GRP_ORDRE_VIREMENT_VERSEMENT
         	@TYPE_ETAT INTEGER, 
         	@DATE_DEBUT datetime,
         	@DATE_FIN datetime
         AS  
         	BEGIN
         		CREATE TABLE #TEMP
         		(
         			ID_VIREMENT_INTERNE_COLLECTE INTEGER,
         			ID_ACTIVITE INTEGER,
         			DAT_EDIT_VIREMENT_INTERNE_COLLECTE Datetime,
         			DAT_COMPTA_VIREMENT_INTERNE_COLLECTE Datetime,
         			MNT_VIREMENT_INTERNE_COLLECTE decimal(18,2)			
         		)

         		IF (@TYPE_ETAT = 1)
         			BEGIN
         				INSERT INTO #TEMP
         				SELECT DISTINCT
         					VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE,
         					VIREMENT_INTERNE_COLLECTE.ID_ACTIVITE,
         					VIREMENT_INTERNE_COLLECTE.DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         					VIREMENT_INTERNE_COLLECTE.DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         					VIREMENT_INTERNE_COLLECTE.MNT_VIREMENT_INTERNE_COLLECTE
         				FROM
         					VIREMENT_INTERNE_COLLECTE 
         					LEFT OUTER JOIN POSTE_IMPUTATION ON VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE = POSTE_IMPUTATION.ID_VIREMENT_INTERNE_COLLECTE 
         					LEFT OUTER JOIN POSTE_VERSEMENT ON POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT
         				WHERE
         					POSTE_VERSEMENT.ID_TYPE_VERSEMENT IN (1,2,3,4)
         					AND VIREMENT_INTERNE_COLLECTE.DAT_EDIT_VIREMENT_INTERNE_COLLECTE IS NULL

         				UPDATE VIREMENT_INTERNE_COLLECTE
         				SET VIREMENT_INTERNE_COLLECTE.DAT_EDIT_VIREMENT_INTERNE_COLLECTE = getDate()
         				where VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE in (select ID_VIREMENT_INTERNE_COLLECTE from #TEMP)

         				UPDATE #TEMP
         				SET DAT_EDIT_VIREMENT_INTERNE_COLLECTE = getDate()
         			END
         		ELSE
         			BEGIN
         				INSERT INTO #TEMP
         				SELECT DISTINCT
         					VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE,
         					VIREMENT_INTERNE_COLLECTE.ID_ACTIVITE,
         					VIREMENT_INTERNE_COLLECTE.DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         					VIREMENT_INTERNE_COLLECTE.DAT_COMPTA_VIREMENT_INTERNE_COLLECTE,
         					VIREMENT_INTERNE_COLLECTE.MNT_VIREMENT_INTERNE_COLLECTE
         				FROM
         					VIREMENT_INTERNE_COLLECTE
         					LEFT OUTER JOIN POSTE_IMPUTATION ON VIREMENT_INTERNE_COLLECTE.ID_VIREMENT_INTERNE_COLLECTE = POSTE_IMPUTATION.ID_VIREMENT_INTERNE_COLLECTE
         					LEFT OUTER JOIN POSTE_VERSEMENT ON POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT
         				WHERE
         					POSTE_VERSEMENT.ID_TYPE_VERSEMENT IN (1,2,3,4)

         					AND (@DATE_DEBUT IS NULL OR DATEDIFF(DAY,DAT_EDIT_VIREMENT_INTERNE_COLLECTE,@DATE_DEBUT) <= 0) 
         					AND	(@DATE_FIN  IS NULL OR DATEDIFF(DAY,DAT_EDIT_VIREMENT_INTERNE_COLLECTE,@DATE_FIN ) >= 0)
         			END

         		SELECT
         			#TEMP.ID_VIREMENT_INTERNE_COLLECTE,
         			#TEMP.MNT_VIREMENT_INTERNE_COLLECTE,
         			#TEMP.DAT_EDIT_VIREMENT_INTERNE_COLLECTE,
         			TRANSIT.NUM_IBAN_TRANSIT,
         			TRANSIT.LIB_COMPTE_BANQUE AS LIB_COMPTE_BANQUE_TRANSIT,
         			ACTIVITE.LIB_COMPTE_BANQUE AS LIB_COMPTE_BANQUE_ACTIVITE,
         			ACTIVITE.NUM_IBAN_ACTIVITE,
         			ACTIVITE.LIB_BANQUE,
         			PARAMETRES.NOM_PNM_SECRETAIRE_GENERAL,
         			PARAMETRES.NOM_PNM_DAF,
         			PARAMETRES.NOM_PNM_DIRECTEUR,
         			PARAMETRES.PIED_PAGE_C2P,
         			PARAMETRES.BANQUE_VIREMENT,			
         			(SELECT RTRIM(
         					CASE 
         						WHEN PATINDEX('%CEDEX%', LIB_VILLE) <> 0 THEN LEFT(LIB_VILLE, PATINDEX('%CEDEX%', LIB_VILLE)-1) 
         						ELSE LIB_VILLE 
         					END) 
         			FROM AGENCE 
         			WHERE COD_AGENCE = 'NE') AGENCE_LIB_VILLE
         		FROM
         			#TEMP
         			INNER JOIN ACTIVITE ON  #TEMP.ID_ACTIVITE = ACTIVITE.ID_ACTIVITE
         			CROSS JOIN PARAMETRES
         			CROSS JOIN TRANSIT
         		WHERE
         			#TEMP.MNT_VIREMENT_INTERNE_COLLECTE > 0
         		order by
         			#TEMP.ID_VIREMENT_INTERNE_COLLECTE
         	END

	CREATE PROCEDURE [dbo].[BATCH_MVT_BUDGETAIRE_VERSEMENT_DIFF__SS_DIFF__REGUL_POSITIF]
          @TYPE_EVENEMENT  INT,
          @SANS_DIFF   INT
         AS

         BEGIN

          DECLARE
            @ID_TYPE_ENVELOPPE    INT,
            @ID_TYPE_MVT_COLLECTE   INT,
            @ID_TYPE_MVT_FRAIS    INT,
            @ID_TYPE_MVT_COMPTE    INT,
            @ID_TYPE_MVT_MUT    INT,
            @ID_TYPE_FINANCEMENT_COLLECTE INT,
            @ID_TYPE_FINANCEMENT_FRAIS  INT,
            @ID_TYPE_FINANCEMENT_COMPTE  INT,
            @ID_TYPE_FINANCEMENT_MUT  INT,
            @P_E_R       varchar(1), 
            @ID_TYPE_MVT_MUT_FPSPP   INT   

          IF @SANS_DIFF not in (0, 1) OR @TYPE_EVENEMENT not in (1, 5, 13)
          BEGIN
           PRINT N'
         [BATCH_MVT_BUDGETAIRE_VERSEMENT_DIFF__SS_DIFF__REGUL_POSITIF] 
         Les parametres doivent avoir les valeurs suivantes : 
           - TYPE_VERSEMENT  1 evenement versement ou
                 5 evenement r‚gularisation montant positif ou
                13 evenement versement differe

           >> cette option n est valable que pour TYPE_VERSEMENT = 1
           - DIFF     0 pour versement differe ou
                 1 pour non differe

         >> Fin de la transaction.'
           RETURN (SELECT 'Return Value' = -1)
          END

          BEGIN TRY
           BEGIN TRAN TRANSACTION_MVT_BUDGETAIRE

           SET  @P_E_R = 'R' 

           IF @TYPE_EVENEMENT = 13
           BEGIN
            SET @P_E_R = 'P' 
           END

           print 'Cr‚ation des comptes'
           exec BATCH_MVT_BUDGETAIRE_CREATION_COMPTE  @TYPE_EVENEMENT, @SANS_DIFF, 1
           print 'Cr‚ation des comptes - fin'

           SET  @ID_TYPE_ENVELOPPE    = 6 
           SET  @ID_TYPE_MVT_COLLECTE   = 6  
           SET  @ID_TYPE_MVT_FRAIS    = 9  
           SET  @ID_TYPE_MVT_COMPTE    = 10 
           SET     @ID_TYPE_MVT_MUT    = 11 
           SET  @ID_TYPE_FINANCEMENT_COLLECTE = NULL
           SET  @ID_TYPE_FINANCEMENT_FRAIS  = NULL
           SET  @ID_TYPE_FINANCEMENT_COMPTE  = 1
           SET     @ID_TYPE_FINANCEMENT_MUT  = NULL
           SET  @ID_TYPE_MVT_MUT_FPSPP   = 23 

           SELECT
            *
           INTO
            #TMP_DONNEES_OBTENUES_NORMAL
           FROM 
            BATCH_MVT_BUDGETAIRE_OBTENTION_DONNEES(@TYPE_EVENEMENT, 1, @SANS_DIFF)
           WHERE
            @TYPE_EVENEMENT <> 5 OR MNT_HT > 0

           SELECT  
            M.*
           INTO 
            #TMP_DONNEES_OBTENUES_NORMAL_COL
           FROM 
            #TMP_DONNEES_OBTENUES_NORMAL as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND 
            TYPE_ENVELOPPE.ID_ACTIVITE = M.ID_ACTIVITE
            AND
            TYPE_ENVELOPPE.BLN_COLLECTE = 1

           print 'R‚cup‚ration des informations'

           SELECT
            ID_ACTIVITE,
            ID_PERIODE                    as ID_PERIODE_FISC,
            ID_PERIODE                       as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            ID_GROUPE,
            @ID_TYPE_FINANCEMENT_COLLECTE   as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_COLLECTE     as ID_TYPE_MOUVEMENT,
            cast(ROUND(ABS(MNT_HT), 2) as float) as MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@TYPE_EVENEMENT = 5)       
              then 'V'+ VERSEMENT_ORIGINE +' R‚gul.positif.etablis ' + convert(char(6), ID_GROUPE) + ' ' + COD_ACTIVITE + ' ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 3) 
              then 'V'+ VERSEMENT_ORIGINE +' Versement volontaire ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 4) 
                 then 'V'+ VERSEMENT_ORIGINE +' Reliquat Normal ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT <> 3)        
              then 'V'+ VERSEMENT_ORIGINE +' Encaissement diff‚r‚ '  + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT = 3)        
              then 'V'+ VERSEMENT_ORIGINE +' Ech‚ancier pr‚v. ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
            end     as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,
            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE
           INTO
            #TMP_MVT_BUDGETAIRE_NORMAL
           FROM
            #TMP_DONNEES_OBTENUES_NORMAL_COL

           UNION ALL

           SELECT
            M.ID_ACTIVITE,
            M.ID_PERIODE       as ID_PERIODE_FISC,
            M.ID_PERIODE       as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            M.ID_GROUPE,
            @ID_TYPE_FINANCEMENT_FRAIS    as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_FRAIS      as ID_TYPE_MOUVEMENT,
            cast(ROUND(ABS(MNT_HT - DONT_REGLE_FPSPP)*TAU_FRAIS_GESTION / 100, 2) as decimal(18,2)) 
                      as MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@TYPE_EVENEMENT = 5) then 'V'+ VERSEMENT_ORIGINE +' R‚gul.positif.etablis. ' + convert(char(6), M.ID_GROUPE) + ' Frais ' +convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1)  then 'V'+ VERSEMENT_ORIGINE +' Frais gestion ' + convert(char(4), NUM_ANNEE)
              when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT <> 3 )  
              then 'V'+ VERSEMENT_ORIGINE +' Frais de gestion diff‚r‚ ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
              when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT = 3 )  
              then 'V'+ VERSEMENT_ORIGINE +' Frais de gestion Ech‚ancier' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
            end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,
            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE
           FROM
            #TMP_DONNEES_OBTENUES_NORMAL as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND 
            TYPE_ENVELOPPE.BLN_FRAIS_GESTION_COLLECTE = 1        
           where
           M.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 
            AND
            VERSEMENT_ORIGINE = 'V'  

           UNION ALL

           SELECT
            #TMP_DONNEES_OBTENUES_NORMAL.ID_ACTIVITE,
            ID_PERIODE        as ID_PERIODE_FISC,
            ID_PERIODE        as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            ID_GROUPE,
            @ID_TYPE_FINANCEMENT_COMPTE    as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_COMPTE      as ID_TYPE_MOUVEMENT,  
            cast(ROUND(ABS(MNT_HT - DONT_REGLE_FPSPP)- (
                  (ABS(MNT_HT - DONT_REGLE_FPSPP) * TAU_FRAIS_GESTION / 100) 
                  + (ABS(MNT_HT - DONT_REGLE_FPSPP) * PRC_MUTUALISATION_VERSEMENT_RELIQUAT)
                  ), 2) as float)
                      as MNT_MVT_BUDGETAIRE,
            null         as ID_ENVELOPPE,
            CPT_GRP_ID_COMPTE      as ID_COMPTE,
            case
             when (@TYPE_EVENEMENT = 5)       then 'V'+ VERSEMENT_ORIGINE +' R‚gul.positif.etablis. ' + convert(char(6), ID_GROUPE) + ' ' + COD_ACTIVITE + ' ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 3) then 'V'+ VERSEMENT_ORIGINE +' Versement volontaire ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 4) then 'V'+ VERSEMENT_ORIGINE +' Reliquat normal ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT <> 3) then 'V'+ VERSEMENT_ORIGINE +' Encaissement diff‚r‚ '  + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)    
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT = 3)  then 'V'+ VERSEMENT_ORIGINE +' Ech‚ancier pr‚v.' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
             end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,
            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE
           FROM
            #TMP_DONNEES_OBTENUES_NORMAL
            inner join TYPE_ENVELOPPE on (TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = #TMP_DONNEES_OBTENUES_NORMAL.ID_TYPE_ENVELOPPE)
            where
            #TMP_DONNEES_OBTENUES_NORMAL.ID_ACTIVITE   in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 
            and TYPE_ENVELOPPE.BLN_FRAIS_GESTION_COLLECTE = 1

           UNION ALL

           SELECT
            M.ID_ACTIVITE,
            ID_PERIODE        as ID_PERIODE_FISC,
            ID_PERIODE        as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            M.ID_GROUPE,
            @ID_TYPE_FINANCEMENT_MUT    as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_MUT      as ID_TYPE_MOUVEMENT,   
            cast(ROUND(((ABS(MNT_HT- DONT_REGLE_FPSPP) * PRC_MUTUALISATION_VERSEMENT_RELIQUAT)), 2) as float) 
                      as MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@TYPE_EVENEMENT = 5) then 'V'+ VERSEMENT_ORIGINE +' R‚gul.positif.etablis. ' + rtrim(ltrim(convert(char(6), M.ID_GROUPE))) + ' Alim. Pot Mut ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1)  then 'V'+ VERSEMENT_ORIGINE +' Alim. Pot Mut ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT <> 3)  then 'V'+ VERSEMENT_ORIGINE +' Alim. Pot Mut diff‚r‚ ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT = 3)   then 'V'+ VERSEMENT_ORIGINE +' Alim. Pot Mut Ech‚ancier pr‚v. ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)

            end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,
            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE
           FROM
            #TMP_DONNEES_OBTENUES_NORMAL as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND 
            TYPE_ENVELOPPE.BLN_POT_MUTUALISATION_COLLECTE = 1
            and
            TYPE_ENVELOPPE.ID_ACTIVITE is null
           where
           M.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 
            AND
            VERSEMENT_ORIGINE = 'O'

           UNION ALL

           SELECT
            M.ID_ACTIVITE,
            ID_PERIODE        as ID_PERIODE_FISC,
            ID_PERIODE        as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            M.ID_GROUPE,
            @ID_TYPE_FINANCEMENT_MUT    as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_MUT_FPSPP     as ID_TYPE_MOUVEMENT,   
            CAST(ROUND(ABS(DONT_REGLE_FPSPP), 2) as float) as MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@TYPE_EVENEMENT = 5) then 'V'+ VERSEMENT_ORIGINE +' R‚gul.positif.etablis. ' + convert(char(6), M.ID_GROUPE) + ' Alim. Pot Mut FPSPP' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1)  then 'V'+ VERSEMENT_ORIGINE +' Alim. Pot Mut FPSPP' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT <> 3)  then 'V'+ VERSEMENT_ORIGINE +' Alim. Pot Mut diff‚r‚ FPSPP' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT = 3)   then 'V'+ VERSEMENT_ORIGINE +' Alim. Pot Mut Ech‚ancier pr‚. FPSPP' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)

            end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,
            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE
           FROM
            #TMP_DONNEES_OBTENUES_NORMAL as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND 
            TYPE_ENVELOPPE.BLN_POT_MUTUALISATION_COLLECTE = 1
            and
            TYPE_ENVELOPPE.ID_ACTIVITE is null
           where
            M.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 

           print 'fin versement'

           SET  @ID_TYPE_ENVELOPPE    = 6
           SET  @ID_TYPE_MVT_COLLECTE   = 7
           SET  @ID_TYPE_MVT_FRAIS    = 9
           SET  @ID_TYPE_MVT_COMPTE    = 10
           SET  @ID_TYPE_MVT_MUT    = 11
           SET  @ID_TYPE_FINANCEMENT_COLLECTE = NULL
           SET  @ID_TYPE_FINANCEMENT_FRAIS  = NULL
           SET  @ID_TYPE_FINANCEMENT_COMPTE  = 1
           SET  @ID_TYPE_FINANCEMENT_MUT  = NULL
           SET  @ID_TYPE_MVT_MUT_FPSPP   = 23

           SELECT
            *
           INTO 
            #TMP_DONNEES_OBTENUES_TARDIF
           FROM
            BATCH_MVT_BUDGETAIRE_OBTENTION_DONNEES(@TYPE_EVENEMENT, 2, @SANS_DIFF)
           WHERE
            @TYPE_EVENEMENT <> 5 OR MNT_HT > 0

           SELECT
            M.*
           INTO 
            #TMP_DONNEES_OBTENUES_TARDIF_COL
           FROM 
            #TMP_DONNEES_OBTENUES_TARDIF as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND 
            TYPE_ENVELOPPE.ID_ACTIVITE = M.ID_ACTIVITE
            AND
            TYPE_ENVELOPPE.BLN_COLLECTE = 1      

           print 'R‚cup‚ration des informations - fin'

           SELECT
            ID_ACTIVITE,
            ID_PERIODE        as ID_PERIODE_FISC,
            ID_PERIODE        as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE           as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            ID_GROUPE,
            @ID_TYPE_FINANCEMENT_COLLECTE   as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_COLLECTE     as ID_TYPE_MOUVEMENT,  
            cast(ROUND(ABS(MNT_HT), 2) as float) as MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@TYPE_EVENEMENT = 5)       
              then 'V'+ VERSEMENT_ORIGINE +' R‚gul.positif.etablis ' + convert(char(6), ID_GROUPE) + ' ' + COD_ACTIVITE + ' ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 3 ) 
              then 'V'+ VERSEMENT_ORIGINE +' Versement volontaire Tardif' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 4 ) 
              then 'V'+ VERSEMENT_ORIGINE +' Reliquat Tardif'+convert(char(4), NUM_ANNEE)
            end     as LIBL_MVT_BUDGETAIRE,  
            NUM_ANNEE,
            NUM_CHEQUE,

            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE

           INTO
            #TMP_MVT_BUDGETAIRE_TARDIF
           FROM
            #TMP_DONNEES_OBTENUES_TARDIF_COL

           UNION ALL

           SELECT
            M.ID_ACTIVITE,
            M.ID_PERIODE       as ID_PERIODE_FISC,
            M.ID_PERIODE       as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            M.ID_GROUPE,
            @ID_TYPE_FINANCEMENT_FRAIS    as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_FRAIS      as ID_TYPE_MOUVEMENT,  

            cast(ROUND(ABS(MNT_HT - DONT_REGLE_FPSPP)*TAU_FRAIS_GESTION / 100, 2) as decimal(18,2)) 
                      as MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@SANS_DIFF = 1)  then 'V'+ VERSEMENT_ORIGINE +' Frais gestion ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 0)  then 'V'+ VERSEMENT_ORIGINE +' Frais de gestion diff‚r‚ ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
            end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,

            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE

           FROM
            #TMP_DONNEES_OBTENUES_TARDIF as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND
            TYPE_ENVELOPPE.BLN_FRAIS_GESTION_COLLECTE = 1        
           where
           M.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 
            AND
            VERSEMENT_ORIGINE = 'V'

           UNION ALL

           SELECT
            #TMP_DONNEES_OBTENUES_TARDIF.      ID_ACTIVITE,
            ID_PERIODE        as ID_PERIODE_FISC,
            ID_PERIODE        as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            ID_GROUPE,
            @ID_TYPE_FINANCEMENT_COMPTE    as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_COMPTE      as ID_TYPE_MOUVEMENT,  
            cast(ROUND(ABS(MNT_HT - DONT_REGLE_FPSPP)- (
                  (ABS(MNT_HT - DONT_REGLE_FPSPP) * TAU_FRAIS_GESTION / 100) 
                  + (ABS(MNT_HT - DONT_REGLE_FPSPP) * PRC_MUTUALISATION_VERSEMENT_RELIQUAT)
                  ), 2) as decimal(18,2))
                      as MNT_MVT_BUDGETAIRE,
            null         as ID_ENVELOPPE,
            CPT_GRP_ID_COMPTE      as ID_COMPTE,
            case
             when (@TYPE_EVENEMENT = 5)       then 'V'+ VERSEMENT_ORIGINE +' R‚gul.positif.etablis. ' + convert(char(6), ID_GROUPE) + ' ' + COD_ACTIVITE + ' ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 3) then 'V'+ VERSEMENT_ORIGINE +' Versement volontaire tardif' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 4) then 'V'+ VERSEMENT_ORIGINE +' Reliquat tardif ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT <> 3)  then 'V'+ VERSEMENT_ORIGINE +' Encaissement diff‚r‚ ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE) 
             when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT = 3)  then 'V'+ VERSEMENT_ORIGINE +' Ech‚ancier pr‚v. ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE) 
            end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,
            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE
           FROM
            #TMP_DONNEES_OBTENUES_TARDIF
            inner join TYPE_ENVELOPPE on (TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = #TMP_DONNEES_OBTENUES_TARDIF.ID_TYPE_ENVELOPPE)
            where
             #TMP_DONNEES_OBTENUES_TARDIF.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 
            AND
            TYPE_ENVELOPPE.BLN_FRAIS_GESTION_COLLECTE = 1

           UNION ALL

           SELECT
            M.ID_ACTIVITE,
            ID_PERIODE        as ID_PERIODE_FISC,
            ID_PERIODE        as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            M.ID_GROUPE,
            @ID_TYPE_FINANCEMENT_MUT     as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_MUT      as ID_TYPE_MOUVEMENT,   
            cast(ROUND(((ABS(MNT_HT-DONT_REGLE_FPSPP)* PRC_MUTUALISATION_VERSEMENT_RELIQUAT) ), 2) as decimal(18,2)) 
                      as MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@TYPE_EVENEMENT = 5)       
              then 'V'+ VERSEMENT_ORIGINE +' R‚gul.positif.etablis ' + convert(char(6), M.ID_GROUPE) + ' ' + COD_ACTIVITE + ' ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 3)
              then 'V'+ VERSEMENT_ORIGINE +' Versement volontaire Tardif' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 4) 
              then 'V'+ VERSEMENT_ORIGINE +' Reliquat Tardif' + convert(char(4), NUM_ANNEE)
            end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,
            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE
           FROM
            #TMP_DONNEES_OBTENUES_TARDIF as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND
            TYPE_ENVELOPPE.BLN_POT_MUTUALISATION_COLLECTE = 1
            and
            TYPE_ENVELOPPE.ID_ACTIVITE is null
            where
            M.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 
            AND
            VERSEMENT_ORIGINE = 'O'

           UNION ALL

           SELECT
            M.ID_ACTIVITE,
            ID_PERIODE        as ID_PERIODE_FISC,
            ID_PERIODE        as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            M.ID_GROUPE,
            @ID_TYPE_FINANCEMENT_MUT     as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_MUT_FPSPP     as ID_TYPE_MOUVEMENT,   
            cast(ROUND(DONT_REGLE_FPSPP, 2) as float) as MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@TYPE_EVENEMENT = 5)       
              then 'V'+ VERSEMENT_ORIGINE +' R‚gul.positif.etablis.FPSPP ' + convert(char(6), M.ID_GROUPE) + ' ' + COD_ACTIVITE + ' ' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 3)
              then 'V'+ VERSEMENT_ORIGINE +' Versement volontaire Tardif FPSPP' + convert(char(4), NUM_ANNEE)
             when (@SANS_DIFF = 1 AND ID_TYPE_VERSEMENT = 4) 
              then 'V'+ VERSEMENT_ORIGINE +' Reliquat Tardif FPSPP' + convert(char(4), NUM_ANNEE)
            end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,

            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE

           FROM
            #TMP_DONNEES_OBTENUES_TARDIF as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND
            TYPE_ENVELOPPE.BLN_POT_MUTUALISATION_COLLECTE = 1
            and
            TYPE_ENVELOPPE.ID_ACTIVITE is null
            where
            M.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 
            AND
            VERSEMENT_ORIGINE = 'O'

           print 'Versement tardif - fin'

           SET  @ID_TYPE_ENVELOPPE    = 5  
           SET  @ID_TYPE_MVT_COLLECTE   = 8
           SET  @ID_TYPE_MVT_FRAIS    = 11
           SET     @ID_TYPE_MVT_COMPTE    = 10
           SET  @ID_TYPE_FINANCEMENT_COLLECTE = NULL
           SET  @ID_TYPE_FINANCEMENT_FRAIS  = NULL
           SET     @ID_TYPE_FINANCEMENT_COMPTE  = NULL
           SET  @ID_TYPE_MVT_MUT_FPSPP   = 23 

           SELECT
            *
           INTO 
            #TMP_DONNEES_OBTENUES_ANT
           FROM
            BATCH_MVT_BUDGETAIRE_OBTENTION_DONNEES(@TYPE_EVENEMENT, 3, @SANS_DIFF) 
           WHERE
            @TYPE_EVENEMENT <> 5 OR MNT_HT > 0

           SELECT
            M.*
           INTO 
            #TMP_DONNEES_OBTENUES_ANT_COL
           FROM 
            #TMP_DONNEES_OBTENUES_ANT as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND 
            TYPE_ENVELOPPE.ID_ACTIVITE = M.ID_ACTIVITE
            AND
            TYPE_ENVELOPPE.BLN_COLLECTE = 1      

           print 'R‚cup‚ration des informations - fin'

           SELECT
            M.ID_ACTIVITE,
            M.ID_PERIODE       as ID_PERIODE_FISC,
            M.ID_PERIODE       as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            M.ID_GROUPE,
            @ID_TYPE_FINANCEMENT_COLLECTE   as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_COLLECTE     as ID_TYPE_MOUVEMENT,
            cast(ROUND(ABS(MNT_HT), 2) as float) as MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            null         as ID_COMPTE,
            'Reliquat ant‚rieur Collecte ' + convert(char(4), NUM_ANNEE)
                      as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,
            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE
           INTO
            #TMP_MVT_BUDGETAIRE_ANT
           FROM
            #TMP_DONNEES_OBTENUES_ANT_COL AS M

           UNION ALL

           SELECT
            M.ID_ACTIVITE,
            M.ID_PERIODE       as ID_PERIODE_FISC,
            M.ID_PERIODE       as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            M.ID_GROUPE,
            @ID_TYPE_FINANCEMENT_FRAIS    as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_FRAIS      as ID_TYPE_MOUVEMENT,
            cast(ROUND(ABS(MNT_HT - DONT_REGLE_FPSPP) , 2) as float) as MNT_MVT_BUDGETAIRE,  
            ID_ENVELOPPE_PERIODE_EN_COURS   as ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@SANS_DIFF = 1)  then 'V'+ VERSEMENT_ORIGINE +' Alim. Pot Mut ant‚rieure Collecte ' + convert(char(4), NUM_ANNEE)

               when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT <> 3)  then 'V'+ VERSEMENT_ORIGINE +' Encaissement diff‚r‚ ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
               when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT = 3)  then 'V'+ VERSEMENT_ORIGINE +' Ech‚ancier pr‚v. ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE) 

            end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,

            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE

           FROM
            #TMP_DONNEES_OBTENUES_ANT as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND 
            TYPE_ENVELOPPE.BLN_POT_MUTUALISATION_COLLECTE = 1
            and
            TYPE_ENVELOPPE.ID_ACTIVITE is null
            where
            M.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 

           UNION ALL

           SELECT
            M.ID_ACTIVITE,
            M.ID_PERIODE       as ID_PERIODE_FISC,
            M.ID_PERIODE       as ID_PERIODE_CPT,
            ID_ETABLISSEMENT_BENEFICIAIRE   as ID_ETABLISSEMENT,
            ID_ADHERENT_BENEFICIAIRE    as ID_ADHERENT,
            @P_E_R         as P_E_R,
            M.ID_GROUPE,
            @ID_TYPE_FINANCEMENT_FRAIS    as ID_TYPE_FINANCEMENT,
            null         as ID_DISPOSITIF,
            'N'          as ID_N_R,
            DAT_EVENEMENT       as DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            @ID_TYPE_MVT_MUT_FPSPP     as ID_TYPE_MOUVEMENT,   
            cast(ROUND(ABS(DONT_REGLE_FPSPP) , 2) as float) as MNT_MVT_BUDGETAIRE,  
            ID_ENVELOPPE_PERIODE_EN_COURS   as ID_ENVELOPPE,
            null         as ID_COMPTE,
            case
             when (@SANS_DIFF = 1)  then 'V'+ VERSEMENT_ORIGINE +' Alim. Pot Mut FPSPP ant‚rieure Collecte ' + convert(char(4), NUM_ANNEE)

               when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT <> 3)  then 'V'+ VERSEMENT_ORIGINE +' Encaissement diff‚r‚ FPSPP' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
               when (@SANS_DIFF = 0 AND ID_MODE_VERSEMENT = 3)  then 'V'+ VERSEMENT_ORIGINE +' Ech‚ancier pr‚v. FPSPP ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE) 

            end          as LIBL_MVT_BUDGETAIRE,
            NUM_ANNEE,
            NUM_CHEQUE,

            VERSEMENT_ORIGINE as VERSEMENT_ORIGINE

           FROM
            #TMP_DONNEES_OBTENUES_ANT as M
           INNER JOIN TYPE_ENVELOPPE on
            TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE = M.ID_TYPE_ENVELOPPE
            AND 
            TYPE_ENVELOPPE.BLN_POT_MUTUALISATION_COLLECTE = 1
            and
            TYPE_ENVELOPPE.ID_ACTIVITE is null
           where
           M.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) 

           print 'Versement ant‚rieur - fin'

           IF (@TYPE_EVENEMENT = 1 AND @SANS_DIFF = 0)
           BEGIN

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
             ID_DISPOSITIF,
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
             ID_ACTIVITE,
             ID_PERIODE_FISC,
             ID_PERIODE_CPT,
             ID_ETABLISSEMENT,
             ID_ADHERENT,
             'P' as P_E_R,
             ID_GROUPE,
             ID_TYPE_FINANCEMENT,
             ID_DISPOSITIF,
             ID_N_R,
             DAT_MVT_BUDGETAIRE,
             ID_EVENEMENT,
             case
              when (ID_TYPE_MOUVEMENT =  6) then 12
              when (ID_TYPE_MOUVEMENT =  9) then 15
              when (ID_TYPE_MOUVEMENT = 10) then 16
              when (ID_TYPE_MOUVEMENT = 11) then 17
             end          as ID_TYPE_MOUVEMENT,
             MNT_MVT_BUDGETAIRE * (-1)    as MNT_MVT_BUDGETAIRE, 
             ID_ENVELOPPE,
             ID_COMPTE,
             case

              when (ID_TYPE_MOUVEMENT in (6, 10, 11)) then 'V'+ VERSEMENT_ORIGINE +' Diff‚r‚ remis en banque ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)

              when (ID_TYPE_MOUVEMENT =  9)  then 'V'+ VERSEMENT_ORIGINE +' Frais de gestion diff‚r‚ ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
             end          as LIBL_MVT_BUDGETAIRE
            FROM 
             #TMP_MVT_BUDGETAIRE_NORMAL

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
             ID_DISPOSITIF,
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
             ID_ACTIVITE,
             ID_PERIODE_FISC,
             ID_PERIODE_CPT,
             ID_ETABLISSEMENT,
             ID_ADHERENT,
             'P' as P_E_R,
             ID_GROUPE,
             ID_TYPE_FINANCEMENT,
             ID_DISPOSITIF,
             ID_N_R,
             DAT_MVT_BUDGETAIRE,
             ID_EVENEMENT,
             case
              when (ID_TYPE_MOUVEMENT =  7) then 12
              when (ID_TYPE_MOUVEMENT =  9) then 15
              when (ID_TYPE_MOUVEMENT = 10) then 16
              when (ID_TYPE_MOUVEMENT = 11) then 17
             end          as ID_TYPE_MOUVEMENT,
             MNT_MVT_BUDGETAIRE * (-1)    as MNT_MVT_BUDGETAIRE, 
             ID_ENVELOPPE,
             ID_COMPTE,
             case

              when (ID_TYPE_MOUVEMENT in (7, 10, 11)) then 'V'+ VERSEMENT_ORIGINE +' Diff‚r‚ remis en banque ' +NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)

              when (ID_TYPE_MOUVEMENT =  9)  then 'V'+ VERSEMENT_ORIGINE +' Frais de gestion diff‚r‚ ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
             end          as LIBL_MVT_BUDGETAIRE
            FROM 
             #TMP_MVT_BUDGETAIRE_TARDIF

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
             ID_DISPOSITIF,
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
             ID_ACTIVITE,
             ID_PERIODE_FISC,
             ID_PERIODE_CPT,
             ID_ETABLISSEMENT,
             ID_ADHERENT,
             'P' as P_E_R,
             ID_GROUPE,
             ID_TYPE_FINANCEMENT,
             ID_DISPOSITIF,
             ID_N_R,
             DAT_MVT_BUDGETAIRE,
             ID_EVENEMENT,
             case
              when (ID_TYPE_MOUVEMENT =  8) then 12
              when (ID_TYPE_MOUVEMENT = 11) then 16
             end          as ID_TYPE_MOUVEMENT,
             MNT_MVT_BUDGETAIRE*(-1)     as MNT_MVT_BUDGETAIRE, 
             ID_ENVELOPPE,
             ID_COMPTE,
             case

              when (ID_TYPE_MOUVEMENT in (8, 11)) then 'V'+ VERSEMENT_ORIGINE +' Diff‚r‚ remis en banque ' + NUM_CHEQUE + ' Collecte ' + convert(char(4), NUM_ANNEE)
             end          as LIBL_MVT_BUDGETAIRE
            FROM 
             #TMP_MVT_BUDGETAIRE_ANT

           END

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
             ID_DISPOSITIF,
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
            ID_ACTIVITE,
            ID_PERIODE_FISC,
            ID_PERIODE_CPT,
            ID_ETABLISSEMENT,
            ID_ADHERENT,
            P_E_R,
            ID_GROUPE,
            ID_TYPE_FINANCEMENT,
            ID_DISPOSITIF,
            ID_N_R,
            DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            ID_TYPE_MOUVEMENT,
            MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            ID_COMPTE,
            LIBL_MVT_BUDGETAIRE
           FROM 
            #TMP_MVT_BUDGETAIRE_NORMAL

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
             ID_DISPOSITIF,
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
            ID_ACTIVITE,
            ID_PERIODE_FISC,
            ID_PERIODE_CPT,
            ID_ETABLISSEMENT,
            ID_ADHERENT,
            P_E_R,
            ID_GROUPE,
            ID_TYPE_FINANCEMENT,
            ID_DISPOSITIF,
            ID_N_R,
            DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            ID_TYPE_MOUVEMENT,
            MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            ID_COMPTE,
            LIBL_MVT_BUDGETAIRE
           FROM 
            #TMP_MVT_BUDGETAIRE_TARDIF

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
             ID_DISPOSITIF,
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
            ID_ACTIVITE,
            ID_PERIODE_FISC,
            ID_PERIODE_CPT,
            ID_ETABLISSEMENT,
            ID_ADHERENT,
            P_E_R,
            ID_GROUPE,
            ID_TYPE_FINANCEMENT,
            ID_DISPOSITIF,
            ID_N_R,
            DAT_MVT_BUDGETAIRE,
            ID_EVENEMENT,
            ID_TYPE_MOUVEMENT,
            MNT_MVT_BUDGETAIRE,
            ID_ENVELOPPE,
            ID_COMPTE,
            LIBL_MVT_BUDGETAIRE
           FROM 
            #TMP_MVT_BUDGETAIRE_ANT

           print 'Transaction commit'
           COMMIT TRAN TRANSACTION_MVT_BUDGETAIRE

           print 'end'

          END TRY

          BEGIN CATCH
           SELECT
            ERROR_NUMBER() as ErrorNumber,
            ERROR_MESSAGE() as ErrorMessage;

           IF (XACT_STATE()) = -1
           BEGIN
            PRINT
             N'The transaction is in an uncommittable state. ' +
             'Rolling back transaction.'
            ROLLBACK TRANSACTION TRANSACTION_MVT_BUDGETAIRE;
           END;

           IF (XACT_STATE()) = 1
           BEGIN
            PRINT
             N'The transaction is committable. ' +
             'Committing transaction.'
            COMMIT TRANSACTION TRANSACTION_MVT_BUDGETAIRE;   
           END;

          END CATCH

         END

         CREATE PROCEDURE [dbo].[GET_TAUX_TVA_GROUPE] 
         	@ID_GROUPE int
         AS
         BEGIN
         	DECLARE @ID_PERIODE_TVA AS INTEGER

         	select @ID_PERIODE_TVA = ID_PERIODE
         	from dbo.PERIODE
         	where ID_TYPE_PERIODE = 2
         	and GetDate() between DAT_DEB_PERIODE and DAT_FIN_PERIODE

         	declare @ID_ETABLISSEMENT int
         	set @ID_ETABLISSEMENT = dbo.GetEtabRepresentatifDuGroupe(@ID_GROUPE)

         	SELECT  
         		R26.TAU_TVA, 
         		ETABLISSEMENT.ID_GROUPE
         	FROM ETABLISSEMENT  
         		inner join R26 on (ETABLISSEMENT.ID_TYPE_TVA = R26.ID_TYPE_TVA and
         							R26.ID_PERIODE = @ID_PERIODE_TVA)
         	where ETABLISSEMENT.ID_ETABLISSEMENT  = @ID_ETABLISSEMENT 

         END

create procedure CKParser.TheEnd 
as 
begin         
	print 'Everything worked';
end
