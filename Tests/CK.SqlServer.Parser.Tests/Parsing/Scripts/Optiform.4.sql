
	CREATE PROCEDURE [dbo].[LEC_DET_VIREMENT]
         	@ID_VIREMENT int
         AS
         BEGIN
         
         	select 
         		V_E.ID_VIREMENT_ENVELOPPE, 
         		V_E.COD_VIREMENT_ENVELOPPE, 
         		V_E.LIBL_VIREMENT_ENVELOPPE AS LIBELLE,
         		V_E.DAT_CREATION, 
         		E1.ID_ENVELOPPE AS ID_ENVELOPPE_SOURCE,
         		E1.COD_ENVELOPPE AS COD_ENVELOPPE_SOURCE,
         		E1.LIBL_ENVELOPPE AS LIBL_ENVELOPPE_SOURCE, 
         		E2.ID_ENVELOPPE AS ID_ENVELOPPE_DESTINATION,
         		E2.COD_ENVELOPPE AS COD_ENVELOPPE_DESTINATION,
         		E2.LIBL_ENVELOPPE AS LIBL_ENVELOPPE_DESTINATION,
         		V_E.MNT_PREVISIONNEL,V_E.MNT_ENGAGE,V_E.MNT_REEL, 
         		V_E.DAT_VIREMENT_ENVELOPPE,
         		UTILISATEUR.COD_UTIL AS PAR,
         		UTILISATEUR.ID_UTILISATEUR,
         		V_E.COM_VIREMENT_ENVELOPPE,
         		V_E.TIME_STAMP
         	from VIREMENT_ENVELOPPE V_E 
         		INNER JOIN UTILISATEUR ON UTILISATEUR.ID_UTILISATEUR = V_E.ID_UTILISATEUR
         		INNER JOIN ENVELOPPE E1 ON E1.ID_ENVELOPPE = V_E.ID_ENVELOPPE_SOURCE
         		INNER JOIN ENVELOPPE E2 ON E2.ID_ENVELOPPE = V_E.ID_ENVELOPPE_DESTINATION
         	WHERE V_E.ID_VIREMENT_ENVELOPPE = @ID_VIREMENT
         
         	IF @@ROWCOUNT = 0   
         	BEGIN  
         	   IF EXISTS(SELECT * FROM VIREMENT_ENVELOPPE WHERE ID_VIREMENT_ENVELOPPE = @ID_VIREMENT)      
         	   BEGIN  
         		  /* Problme de Concurrence d'accs */  
         		  RAISERROR('Problme de Concurrende d''accs', 16, 1)
         		  RETURN -1  
         	   END     
         	END
         
         
         END
         
         
		-- =============================================
         -- Author		: GiO
         -- Create date	: 25 Mars 2008
         -- Description	: proc‚dure pour l'‚dition des ordres de virements recap contrat pro
         -- =============================================
         -- Author		: DCH
         -- Create date	: 03 Avril 2008
         -- Description	: correction jointure sur les ‚tablissements OF et adherents
         -- =============================================
         -- Author		: SAFI
         -- Create date	: 30-10-2008
         -- Description	: Adding Extra Coloumns in the Select 				
         -- =============================================
         -- Author		: SAFI
         -- Create date	: 19-12-2008
         -- Description	: Adding Extra relations 
         				  -- OF		vers ADH
         				  -- ADH	vers OF	
         -- =============================================
         -- Author		:  SAFI
         -- Create date	:  12-02-2009
         -- Description	:  Adding Extra COLOUMN  
         				-- PARAMETRES.BANQUE_VIREMENT				  
         -- =============================================
         -- DSZ 03/12/12 : 14425 : renommage agence IDF en NE
         -- =============================================
         -- HBO - 141113 - M16371: Lot 1 - Modification structure de donn‚es / proc‚dures stock‚es
         -- =============================================
         -- HBO - 201113 - M16378: Lot 1 - Editions
         -- =============================================
         CREATE PROCEDURE EDT_ORDRES_VIREMENT_RECAP_CONTRAT_PRO
         	@DATE_DEBUT AS DATETIME,
         	@DATE_FIN AS DATETIME
         AS
         	BEGIN
         		-- SAFI AJOUTER 
         		-- Declare Constants variables 
         		DECLARE
         			@NUM_IBAN_ACTIVITE VARCHAR(34),
         			@BIC_ACTIVITE VARCHAR(11),
         			@LIB_COMPTE_BANQUE VARCHAR (50),
         			@AGENCE_LIB_VILLE VARCHAR(50),
         			@NOM_PNM_DAF VARCHAR(100),
         			@NOM_PNM_SECRETAIRE_GENERAL VARCHAR(100),
         			@PIED_PAGE_C2P VARCHAR(2000),
         			@BANQUE_VIREMENT VARCHAR(150),
         			@NOM_PNM_DIRECTEUR VARCHAR(100)
         		-- End Declare constant variables
         	
         		--Filling up the Constants
         
         		--Filling @NUM_IBAN_ACTIVITE & @LIB_COMPTE_BANQUE for ID Activite 3 for CPRO
         		SELECT
         			@NUM_IBAN_ACTIVITE = NUM_IBAN_ACTIVITE,
         			@BIC_ACTIVITE = BIC_ACTIVITE,
         			@LIB_COMPTE_BANQUE = LIB_COMPTE_BANQUE
         		FROM
         			ACTIVITE
         		WHERE
         			ID_ACTIVITE = 3
         
         		-- Filling @AGENCE_LIB_VILLE for COD_AGENCE = 'NE'
         		SET	@AGENCE_LIB_VILLE =	
         				(SELECT
         					RTRIM(CASE WHEN PATINDEX('%CEDEX%', LIB_VILLE) <> 0 
         					THEN LEFT(LIB_VILLE, PATINDEX('%CEDEX%', LIB_VILLE)-1) 
         					ELSE LIB_VILLE 
         					END) AS  LIB_VILLE				
         				FROM
         					AGENCE 
         				WHERE
         					COD_AGENCE = 'NE') 
         
         		-- Filling up the names of the PARAMETRES
         		SELECT
         			@NOM_PNM_DAF = NOM_PNM_DAF,
         			@NOM_PNM_SECRETAIRE_GENERAL= NOM_PNM_SECRETAIRE_GENERAL,
         			@BANQUE_VIREMENT	= BANQUE_VIREMENT,	
         			@PIED_PAGE_C2P = PIED_PAGE_C2P,
         			@NOM_PNM_DIRECTEUR = NOM_PNM_DIRECTEUR
         		FROM
         			PARAMETRES
         
         		-- Filling up the PIED_PAGE_C2P
         		--SELECT  @PIED_PAGE_C2P = PIED_PAGE_C2P FROM PARAMETRES
         		--------------*/
         		-- END SAFI
         
         		IF(@DATE_DEBUT IS NULL AND @DATE_FIN IS NULL)
         			BEGIN
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT, 
         					ADHERENT.LIB_RAISON_SOCIALE,
         					[TRANSACTION].NUM_IBAN, 
         					[TRANSACTION].BIC, 
         					REGLEMENT_PRO.MNT_REGLE_TTC, 
         					REGLEMENT_PRO.NUM_VIREMENT, 
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- REGLEMENT_PRO.BLN_ACTIF,
         					-- REGLEMENT_PRO.BLN_EN_COURS,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND SESSION_PRO.ID_TRANSACTION_ADH = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN ETABLISSEMENT	ON [TRANSACTION].ID_ETABLISSEMENT_BENEF = ETABLISSEMENT.ID_ETABLISSEMENT
         					INNER JOIN ADHERENT			ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 1			
         				
         				UNION				
         				----- NEW ADH ---> OF	
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					ORGANISME_FORMATION.LIB_RAISON_SOCIALE,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- REGLEMENT_PRO.BLN_ACTIF,REGLEMENT_PRO.BLN_EN_COURS,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND SESSION_PRO.ID_TRANSACTION_ADH = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN ETABLISSEMENT_OF	ON	ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF
         					INNER JOIN ORGANISME_FORMATION ON ORGANISME_FORMATION.ID_OF = ETABLISSEMENT_OF.ID_OF
         				WHERE REGLEMENT_PRO.BLN_ACTIF = 1  AND REGLEMENT_PRO.BLN_EN_COURS = 1
         				----- END ADH ---> OF
         				UNION				
         									
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					ORGANISME_FORMATION.LIB_RAISON_SOCIALE,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					--REGLEMENT_PRO.BLN_ACTIF,
         					--REGLEMENT_PRO.BLN_EN_COURS,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_OF
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND  SESSION_PRO.ID_TRANSACTION_OF = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN ETABLISSEMENT_OF	ON ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF
         					INNER JOIN ORGANISME_FORMATION ON ORGANISME_FORMATION.ID_OF = ETABLISSEMENT_OF.ID_OF
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 1
         		
         				UNION 
         
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					TIERS.LIB_NOM,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- REGLEMENT_PRO.BLN_ACTIF,
         					-- REGLEMENT_PRO.BLN_EN_COURS,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO			ON	(REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_OF or REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH)
         					INNER JOIN [TRANSACTION]		ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         														AND SESSION_PRO.ID_TRANSACTION_OF = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN TIERS				ON TIERS.ID_TIERS = [TRANSACTION].ID_TIERS_BENEF
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 1
         				
         				UNION
         				-- NEW OF --> ADH		
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					ADHERENT.LIB_RAISON_SOCIALE,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- REGLEMENT_PRO.BLN_ACTIF,
         					-- REGLEMENT_PRO.BLN_EN_COURS,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO			ON	(REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_OF or REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH)
         					INNER JOIN [TRANSACTION]		ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         														AND  SESSION_PRO.ID_TRANSACTION_OF = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN ETABLISSEMENT		ON [TRANSACTION].ID_ETABLISSEMENT_BENEF = ETABLISSEMENT.ID_ETABLISSEMENT
         					INNER JOIN ADHERENT				ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 1
         				-- END OF --> ADH
         				UNION 
         
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					TIERS.LIB_NOM,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- REGLEMENT_PRO.BLN_ACTIF,
         					--REGLEMENT_PRO.BLN_EN_COURS,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND  SESSION_PRO.ID_TRANSACTION_ADH = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN TIERS			ON TIERS.ID_TIERS = [TRANSACTION].ID_TIERS_BENEF
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 1
         				ORDER BY
         					REGLEMENT_PRO.NUM_VIREMENT
         			END
         		ELSE
         			BEGIN
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					ADHERENT.LIB_RAISON_SOCIALE,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT ,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND  SESSION_PRO.ID_TRANSACTION_ADH = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN ETABLISSEMENT		ON [TRANSACTION].ID_ETABLISSEMENT_BENEF = ETABLISSEMENT.ID_ETABLISSEMENT
         					INNER JOIN ADHERENT			ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 0
         					AND REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NOT NULL
         					AND (@DATE_DEBUT IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_DEBUT) <= 0)
         					AND	(@DATE_FIN  IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_FIN ) >= 0)
         			
         				UNION
         				-- NEW ADH ---> OF
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					ORGANISME_FORMATION.LIB_RAISON_SOCIALE,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,REGLEMENT_PRO.NUM_VIREMENT , REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND  SESSION_PRO.ID_TRANSACTION_ADH = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN ETABLISSEMENT_OF	ON ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF
         					INNER JOIN ORGANISME_FORMATION	ON ORGANISME_FORMATION.ID_OF = ETABLISSEMENT_OF.ID_OF
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS=0
         					AND REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NOT NULL
         					AND  (@DATE_DEBUT IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_DEBUT) <= 0)
         					AND	(@DATE_FIN  IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_FIN ) >= 0)
         				-- END ADH ---> OF
         				UNION				
         									
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					ORGANISME_FORMATION.LIB_RAISON_SOCIALE,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT ,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_OF
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND  SESSION_PRO.ID_TRANSACTION_OF = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN ETABLISSEMENT_OF		ON ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF
         					INNER JOIN ORGANISME_FORMATION	ON ORGANISME_FORMATION.ID_OF = ETABLISSEMENT_OF.ID_OF
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 0
         					AND REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NOT NULL
         					AND (@DATE_DEBUT IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_DEBUT) <= 0)
         					AND	(@DATE_FIN  IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_FIN ) >= 0)
         
         				UNION 
         
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					TIERS.LIB_NOM,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	(REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_OF or REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH)
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND  SESSION_PRO.ID_TRANSACTION_OF = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN TIERS			ON TIERS.ID_TIERS = [TRANSACTION].ID_TIERS_BENEF
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 0
         					AND REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NOT NULL
         					AND (@DATE_DEBUT IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_DEBUT) <= 0)
         					AND	(@DATE_FIN  IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_FIN ) >= 0)
         
         				UNION 
         				-- NEW OF -- > ADH
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					ADHERENT.LIB_RAISON_SOCIALE,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND SESSION_PRO.ID_TRANSACTION_OF = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN ETABLISSEMENT	ON [TRANSACTION].ID_ETABLISSEMENT_BENEF = ETABLISSEMENT.ID_ETABLISSEMENT
         					INNER JOIN ADHERENT			ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 0
         					AND REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NOT NULL
         					AND (@DATE_DEBUT IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_DEBUT) <= 0)
         					AND	(@DATE_FIN  IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_FIN ) >= 0)
         				-- END OF -- > ADH
         				
         				UNION
         				
         				SELECT
         					CASE REGLEMENT_PRO.BLN_CRITERE
         						WHEN 1 THEN 'X'
         						ELSE ''
         					END AS BLN_CRIT,
         					TIERS.LIB_NOM,
         					[TRANSACTION].NUM_IBAN,
         					[TRANSACTION].BIC,
         					REGLEMENT_PRO.MNT_REGLE_TTC,
         					REGLEMENT_PRO.NUM_VIREMENT,
         					REGLEMENT_PRO.DAT_VALID_REGLEMENT,
         					-- SAFI AJOUTER --->
         					@NUM_IBAN_ACTIVITE			AS NUM_IBAN_ACTIVITE,
         					@BIC_ACTIVITE				AS BIC_ACTIVITE,
         					@LIB_COMPTE_BANQUE			AS LIB_COMPTE_BANQUE, 
         					@AGENCE_LIB_VILLE			AS AGENCE_LIB_VILLE, 
         					@NOM_PNM_DAF				AS NOM_PNM_DAF,
         					@NOM_PNM_SECRETAIRE_GENERAL AS NOM_PNM_SECRETAIRE_GENERAL,
         					@PIED_PAGE_C2P				AS PIED_PAGE_C2P,
         					@BANQUE_VIREMENT			AS BANQUE_VIREMENT,
         					@NOM_PNM_DIRECTEUR			AS NOM_PNM_DIRECTEUR
         					------------------*/
         				FROM
         					REGLEMENT_PRO
         					INNER JOIN SESSION_PRO		ON	REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH
         					INNER JOIN [TRANSACTION]	ON	REGLEMENT_PRO.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         													AND SESSION_PRO.ID_TRANSACTION_ADH = [TRANSACTION].ID_TRANSACTION
         					INNER JOIN TIERS			ON TIERS.ID_TIERS = [TRANSACTION].ID_TIERS_BENEF
         				-- WHERE REGLEMENT_PRO.BLN_ACTIF = 1  AND REGLEMENT_PRO.BLN_EN_COURS = 1
         				WHERE
         					REGLEMENT_PRO.BLN_ACTIF = 1
         					AND REGLEMENT_PRO.BLN_EN_COURS = 0
         					AND REGLEMENT_PRO.DAT_VALID_REGLEMENT IS NOT NULL
         					AND (@DATE_DEBUT IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_DEBUT) <= 0)
         					AND	(@DATE_FIN  IS NULL OR DATEDIFF(DAY,REGLEMENT_PRO.DAT_VALID_REGLEMENT,@DATE_FIN ) >= 0)
         				ORDER BY
         					REGLEMENT_PRO.NUM_VIREMENT
         			END
         	END

         CREATE PROCEDURE [BATCH_TRANSFERT_PRESCRIPTION_SELECTION_DEFI]
         /*
         =============================================  
         Author  : MBL
         Create date : 03/03/2015
         Description : Proc‚dure permettant de lancer des tranferts de prescription du compte Selection DEFI
         (@COD_TYPE_EVENEMENT_DOTATION_MOINS300 = 'PRESCPSD')
         
         Le traitement fait appel a la fonction de table F_TRANSFERT_PRESCRIPTION_COMPTE_SELECTION_DEFI constituant un outil d'aide … la d‚cision afin de g‚n‚rer ces transferts.
         
         -- CONDITION DE LANCEMENT
         Parametre : la valorisation du parametre @ID_GROUPE_TRAITE est optionnelle. 
         			S'il est valorise, le traitement n'est declenche que pour l'adherent de mˆme ID
         			S'il n'est pas valorise, le traitement est declenche pour tous les adherents
         -- =============================================
         */
         --DECLARE
         @NUM_ANNEE_N		INTEGER,
         @ID_GROUPE_TRAITE	INTEGER
         
         --SELECT @NUM_ANNEE_N		= 2015, @ID_GROUPE_TRAITE	=2
         
         AS
         BEGIN
         
         	IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
         	BEGIN
         		drop table #TMP_TRANSFERT
         	END
         
         
         	DECLARE 
         	@COD_TYPE_EVENEMENT				varchar(8),
         	@DAT							DATETIME,
         	@ID_TYPE_EVENEMENT_TRANSFERT	INTEGER,
         	@ID_GROUPE						INTEGER,
         	@ID_ADHERENT					INTEGER,
         	@ID_ETABLISSEMENT				INTEGER,
         	@MNT_TRANSFERT					DECIMAL(15, 2),
         	@ID_BRANCHE						INT,
         	@ID_PERIODE_N					INTEGER, 
         	@LIBL_MVT						VARCHAR(60),
         	@LIBL_EVENEMENT					VARCHAR(50),
         	@ID_ENVELOPPE					INTEGER,
         	@LIBL_ENVELOPPE					VARCHAR(50),
         	@ID_ACTIVITE					INTEGER,
         	@BLN_COMPTE_VERS_ENVELOPPE		TINYINT,
         	@ID_TRANSFERT					INT,
         	@ID_TYPE_FINANCEMENT			INT
         
         	SET @COD_TYPE_EVENEMENT	 = 'PRESCPSD'
         	
         	SELECT	@ID_PERIODE_N = ID_PERIODE
         	FROM	PERIODE
         	WHERE	NUM_ANNEE = @NUM_ANNEE_N
         	AND		ID_TYPE_PERIODE = 1 
         
         	SELECT @ID_TYPE_EVENEMENT_TRANSFERT = ID_TYPE_EVENEMENT FROM TYPE_EVENEMENT where COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT	
         	
         	IF @ID_TYPE_EVENEMENT_TRANSFERT IS NULL 
         	BEGIN
         		SELECT 'Le type d''evenement associe au code evenement ' + @COD_TYPE_EVENEMENT + ' n''existe pas'
         	END
         	ELSE
         	BEGIN
         	
         		SELECT @ID_TYPE_FINANCEMENT = 5 -- Compte selection DEFI
         		
         		SELECT t.*, ID_ACTIVITE_PLAN_N = ACTIVITE_ADH.ID_ACTIVITE
         		INTO #TMP_TRANSFERT 
         		FROM F_TRANSFERT_PRESCRIPTION_COMPTE_SELECTION_DEFI(@NUM_ANNEE_N, @ID_GROUPE_TRAITE) t
         		LEFT JOIN
         		(	
         			SELECT	R19.ID_ADHERENT,
         					ACTIVITE.ID_ACTIVITE
         			FROM		R19
         			INNER JOIN	ACTIVITE		ON R19.ID_ACTIVITE					=  ACTIVITE.ID_ACTIVITE
         			INNER JOIN	TYPE_ACTIVITE	ON TYPE_ACTIVITE.ID_TYPE_ACTIVITE	= ACTIVITE.ID_TYPE_ACTIVITE
         			WHERE	R19.ID_PERIODE = @ID_PERIODE_N
         			AND		TYPE_ACTIVITE.COD_TYPE_ACTIVITE = 'PLAN'	
         		) ACTIVITE_ADH	ON ACTIVITE_ADH	.ID_ADHERENT = t.ID_ADHERENT_CHEF_GROUPE
         
         
         		SELECT @DAT = GETDATE()
         
         		SELECT	@ID_PERIODE_N	= ID_PERIODE   
         		from	PERIODE     
         		where	NUM_ANNEE		= @NUM_ANNEE_N 
         		AND		ID_TYPE_PERIODE = 1   
         
         
         		SET @LIBL_EVENEMENT		= 'Prescription Cpt Defi Selection '	+ CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
         		SET @LIBL_MVT			= 'Prescription Cpt Defi Selection '		+ CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
         
         
         		DECLARE cu_transfert CURSOR FOR
         		SELECT ID_ADHERENT = ID_ADHERENT_CHEF_GROUPE, ID_GROUPE , ID_BRANCHE, [ID_ACTIVITE_PLAN_N], MNT_TRANSFERT_PRESCRIPTION, ID_ETABLISSEMENT_CHEF_GROUPE
         		FROM #TMP_TRANSFERT
         		WHERE MNT_TRANSFERT_PRESCRIPTION > 0
         
         		OPEN cu_transfert
         
         		FETCH cu_transfert INTO
         		@ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @ID_ACTIVITE, @MNT_TRANSFERT, @ID_ETABLISSEMENT
         
         
         		WHILE (@@FETCH_STATUS <> -1)
         		BEGIN	
         			-- Recherche de l'enveloppe de collecte PIVOT
         			SELECT		@ID_ENVELOPPE = ID_ENVELOPPE , @LIBL_ENVELOPPE = LIBL_ENVELOPPE 
         			FROM		TYPE_ENVELOPPE 
         			INNER JOIN	ENVELOPPE ON ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE
         			WHERE		TYPE_ENVELOPPE.BLN_COLLECTE = 1 
         			AND			TYPE_ENVELOPPE.ID_ACTIVITE	= @ID_ACTIVITE 
         			AND			ENVELOPPE.ID_PERIODE		= @ID_PERIODE_N
         			AND			TYPE_ENVELOPPE.ID_BRANCHE	= @ID_BRANCHE
         
         			SET @MNT_TRANSFERT = @MNT_TRANSFERT
         			SET @BLN_COMPTE_VERS_ENVELOPPE  = 1
         
         			--SELECT '@ID_TRANSFERT = INS_TRANSFERT ',
         			--	LIBL_TRANSFERT				= @LIBL_EVENEMENT,
         			--	BLN_COMPTE_VERS_ENVELOPPE	= @BLN_COMPTE_VERS_ENVELOPPE,  
         			--	ID_GROUPE					= @ID_GROUPE,
         			--	ID_ENVELOPPE				= @ID_ENVELOPPE,
         			--	DAT_TRANSFERT				= @DAT,
         			--	MNT_TRANSFERT				= @MNT_TRANSFERT, 
         			--	ID_TYPE_FINANCEMENT			= @ID_TYPE_FINANCEMENT,   -- Type de financement sur Compte Historique
         			--	ID_UTILISATEUR				= 82, 
         			--	ID_PERIODE					= @ID_PERIODE_N,
         			--	COM_TRANSFERT				= @LIBL_MVT, 
         			--	LIBL_MVT_BUDGETAIRE			= @LIBL_MVT,
         			--	ID_TYPE_EVENEMENT			= @ID_TYPE_EVENEMENT_TRANSFERT
         					
         			exec @ID_TRANSFERT = INS_TRANSFERT 
         				@LIBL_TRANSFERT				= @LIBL_EVENEMENT,
         				@BLN_COMPTE_VERS_ENVELOPPE	= @BLN_COMPTE_VERS_ENVELOPPE,  
         				@ID_GROUPE					= @ID_GROUPE,
         				@ID_ENVELOPPE				= @ID_ENVELOPPE,
         				@DAT_TRANSFERT				= @DAT,
         				@MNT_TRANSFERT				= @MNT_TRANSFERT, 
         				@ID_TYPE_FINANCEMENT		= @ID_TYPE_FINANCEMENT,   -- Type de financement sur Compte Historique
         				@ID_UTILISATEUR				= 82, 
         				@ID_PERIODE					= @ID_PERIODE_N,
         				@COM_TRANSFERT				= @LIBL_MVT, 
         				@LIBL_MVT_BUDGETAIRE		= @LIBL_MVT,
         				@ID_TYPE_EVENEMENT			= @ID_TYPE_EVENEMENT_TRANSFERT,
         				@ID_ETABLISSEMENT			= @ID_ETABLISSEMENT
         
         			FETCH cu_transfert INTO
         			@ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @ID_ACTIVITE, @MNT_TRANSFERT, @ID_ETABLISSEMENT
         
         
         		END
         
         		CLOSE cu_transfert
         		DEALLOCATE cu_transfert
         	END
         	
         	IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
         	BEGIN
         		drop table #TMP_TRANSFERT
         	END
         
         END		
GO
         
	CREATE PROCEDURE [dbo].[INS_ADHERENT]
         -- =============================================
         -- Author:		
         -- Create date: 
         -- Description:	Creation d'un Adherent
         -----------------------------------------------
         --...
         -----------------------------------------------
         -- Author:		MB
         -- Modif. date: 20090205 
         -- Description:	Rajout Abonnement automatique d'un adherent a l'EDI Salarie par defaut des sa creation
         -----------------------------------------------
         -- Author:		MB
         -- Modif. date: 20090212 
         -- Description:	Correction de la version du 20090212 - gestion des @@IDENTITY
         -- Mantis 11622
         -- =============================================
         -- Author:		RVI
         -- Modif. date: 20120125
         -- Description:	Ajout colonnes DAT_SITUATION_ECONOMIQUE && LIB_TIERS_MANDATAIRE
         -- Mantis 13165
         -- =============================================
         -- DSZ 12/07/12 13659
         -- tout ce qui est cr‚‚ dans optiform re‡oit BLN_CREATION = 0
         -- =============================================
         
         	@ID_AGENCE int = null,
         	@ID_ADRESSE_PRINCIPALE int = null,
         	@ID_MODE_PAIEMENT int,
         	@ID_BRANCHE int,
         	@ID_GROUPE int,
         	@ID_IDCC int,
         	@ID_MODE_VERSEMENT int,
         	@ID_NAF int,
         	@ID_ETABLISSEMENT_PRINCIPAL int,
         	@ID_SITUATION_FAF int,
         	@NUM_SIREN varchar(9),
         	@LIB_RAISON_SOCIALE varchar(50),
         	@LIB_SIGLE_ADHERENT varchar(15),
         	@BLN_ACTIF tinyint,
         	@BLN_ASSUJETTI_TA tinyint,
         	@BLN_GESTION_GROUPE tinyint,
         	@COM_ADHERENT varchar(255),
         	@BLN_HABILIT_ALTERNANCE tinyint,
         	@ID_UTILISATEUR int,
         	@LIB_MOT_PASSE varchar(30),
         	@EMAIL varchar(100),
         	@DAT_DEBUT_GROUPE datetime,
         	@LIB_ENSEIGNE varchar(35),
         	@BLN_MOINS_10 tinyint,
         	@ANNEE_MOINS_10 varchar(4),
         	@BLN_10_19 tinyint,
         	@ANNEE_10_19 varchar(4),
         	@BLN_20_PLUS tinyint,
         	@ANNEE_20_PLUS varchar(4),
         	@ID_TIERS_MANDATAIRE int,
         	@ID_ADHERENT_REPRENEUR int,
         	@ID_SITUATION_ECONOMIQUE int,
         	@BLN_SERVICE_PERSONNE tinyint,
         	@ID_UTILISATEUR_CREATEUR int,
         	@ID_CHARGEE_MISSION int,
         	@ID_CHARGEE_RELATION int,
         	@ID_TIERS_CABINET_COMPTABLE int,
         	@ID_SYNDICAT int,
         	@ID_FEDERATION int,
         	@SITE_WEB_ADHERENT varchar(100),
         	@ID_GROUPE_STATISTIQUE int,
         	@DAT_SITUATION_ECONOMIQUE datetime,
         	@LIB_TIERS_MANDATAIRE varchar(50)
         AS
         	DECLARE @ID_ADHERENT int    
         	if @LIB_SIGLE_ADHERENT is null
         	BEGIN
         		SET @LIB_SIGLE_ADHERENT  = SUBSTRING(@LIB_RAISON_SOCIALE, 1, 15)
         	END
         
         	insert into ADHERENT
         		(
         			ID_AGENCE,
         			ID_ADRESSE_PRINCIPALE,
         			ID_MODE_PAIEMENT,
         			ID_BRANCHE,
         			ID_GROUPE,
         			ID_IDCC,
         			ID_MODE_VERSEMENT,
         			ID_NAF,
         			ID_ETABLISSEMENT_PRINCIPAL,
         			ID_SITUATION_FAF,
         			COD_ADHERENT,
         			NUM_SIREN,
         			LIB_RAISON_SOCIALE,
         			LIB_SIGLE_ADHERENT,
         			BLN_ACTIF,
         			BLN_ASSUJETTI_TA,
         			BLN_GESTION_GROUPE,
         			COM_ADHERENT,
         			BLN_HABILIT_ALTERNANCE,
         			ID_UTILISATEUR,
         			LIB_MOT_PASSE,
         			EMAIL,
         			DAT_DEBUT_GROUPE,
         			LIB_ENSEIGNE,
         			BLN_MOINS_10,
         			ANNEE_MOINS_10,
         			BLN_10_19,
         			ANNEE_10_19,
         			BLN_20_PLUS,
         			ANNEE_20_PLUS,
         			ID_TIERS_MANDATAIRE,
         			ID_ADHERENT_REPRENEUR,
         			ID_SITUATION_ECONOMIQUE,
         			BLN_SERVICE_PERSONNE,
         			ID_UTILISATEUR_CREATEUR,
         			ID_CHARGEE_MISSION,
         			ID_CHARGEE_RELATION,
         			ID_TIERS_CABINET_COMPTABLE,
         			ID_SYNDICAT,
         			ID_FEDERATION,
         			SITE_WEB_ADHERENT,
         			ID_GROUPE_STATISTIQUE,
         			DAT_SITUATION_ECONOMIQUE,
         			LIB_TIERS_MANDATAIRE,
         			BLN_CREATION 
         		)
         		values
         		(
         			@ID_AGENCE,
         			@ID_ADRESSE_PRINCIPALE,
         			@ID_MODE_PAIEMENT,
         			@ID_BRANCHE,
         			@ID_GROUPE,
         			@ID_IDCC,
         			@ID_MODE_VERSEMENT,
         			@ID_NAF,
         			@ID_ETABLISSEMENT_PRINCIPAL,
         			@ID_SITUATION_FAF,
         			0,
         			@NUM_SIREN,
         			@LIB_RAISON_SOCIALE,
         			@LIB_SIGLE_ADHERENT,
         			@BLN_ACTIF,
         			@BLN_ASSUJETTI_TA,
         			1, --@BLN_GESTION_GROUPE, --D‚sormais, on force syst‚matiquement la gestion de Groupe … true
         			@COM_ADHERENT,
         			@BLN_HABILIT_ALTERNANCE,
         			@ID_UTILISATEUR,
         			@LIB_MOT_PASSE,
         			@EMAIL,
         			@DAT_DEBUT_GROUPE,
         			@LIB_ENSEIGNE,
         			@BLN_MOINS_10,
         			@ANNEE_MOINS_10,
         			@BLN_10_19,
         			@ANNEE_10_19,
         			@BLN_20_PLUS,
         			@ANNEE_20_PLUS,
         			@ID_TIERS_MANDATAIRE,
         			@ID_ADHERENT_REPRENEUR,
         			@ID_SITUATION_ECONOMIQUE,
         			@BLN_SERVICE_PERSONNE,
         			@ID_UTILISATEUR_CREATEUR,
         			@ID_CHARGEE_MISSION,
         			@ID_CHARGEE_RELATION,
         			@ID_TIERS_CABINET_COMPTABLE,
         			@ID_SYNDICAT,
         			@ID_FEDERATION,
         			@SITE_WEB_ADHERENT,
         			@ID_GROUPE_STATISTIQUE,
         			@DAT_SITUATION_ECONOMIQUE,
         			@LIB_TIERS_MANDATAIRE,
         			0 -- BLN_CREATION
         		)
         
         	update	ADHERENT
         	set		COD_ADHERENT = @@IDENTITY
         	where	ID_ADHERENT = @@IDENTITY
               SET @ID_ADHERENT = @@IDENTITY
         
         	/*Abonnement automatique d'un adherent a l'EDI Salarie par defaut des sa creation*/
         	 INSERT INTO EDI_UTILISATION_IMPORT_TABLE
         		  (ID_TABLE, ID_ADHERENT)
         		  SELECT ID_TABLE, @ID_ADHERENT
         		  FROM EDI_IMPORT_TABLE
         		  WHERE COD_TABLE = 'EDI_SAL_ST'  -- Standard Salari‚s
          
         	return @ID_ADHERENT     
 GO        
         
	CREATE PROCEDURE [dbo].[EDI_INS_PEC_STD]
         		@ID_LOT_IMPORT				INTEGER   ,     -- Identifiant du lot d'import en cours de traitement
         		@ID_ETABLISSEMENT_CREATEUR	INTEGER   ,		-- Identifiant de l'‚tablissemen Adh‚rent ayant transmis le fichier EDI (SIRET associ‚ au nom du fichier transmis)
         		@NUM_LIGNE_DEBUT			INTEGER  ,        
         		@NUM_LIGNE_FIN				INTEGER          
         /* ----------------------------------------------   
         Autor : MBL 
         Date Creation : 18/12/2013
         Description : Procdure d'Alimentation de l'EDI PEC a partir de la table TAMPON prealimentee
         
         
         Les differentes etapes sont :
         1- Controle des Donnees transmises 	dont les differentes etapes sont
         	1-1  : Controle que les colonnes obligatoires du modŠle d'import sont renseign‚es 
         	1-2  : CONTROLES unicite des donnees associees aux actions et aux modules de formation 	        
         	1-3  : CONTROLES SIRET ADH
         	1-4  : CONTROLES SIRET OF 	
         	1-5  : CONTROLES THEMES ACTION
         	1-6  : CONTROLES NIVEAUX ACTION
         	1-7  : CONTROLES SANCTION ACTION
         	1-8  : CONTROLES FORMACODES 
         	1-9  : CONTROLES THEMES MODULE
         	1-10 : CONTROLES CODE INITIATIVE
         	1-11 : CONTROLES DEPARTEMENT FORMATION
         	1-12 : CONTROLES DELEGATION PAIEMENT
         	1-13 : CONTROLES Montants de convention
         	1-14 : CONTROLES Sexes Stagiaires
         	1-15 : CONTROLES Tuteurs internes
         	
         	1-16 : CONTROLES Codes Public Prioritaire Plan
         	1-17 : CONTROLES Codes Objet de Formation Plan
         	1-18 : CONTROLES Codes Action Prioritaire Plan
         	1-19 : CONTROLES Codes Categorie Action PLAN
         
         	1-20 : CONTROLES Codes Public Prioritaire PP
         	1-21 : CONTROLES Codes Objet de Formation PP
         	1-22 : CONTROLES Codes Action Prioritaire PP
         	1-23 : CONTROLES Codes Categorie Action PP 
         
         	1-24 : CONTROLES Public Prioritaire Dif Prioritaire
         	1-25 : CONTROLES Codes Objet de Formation Dif Prioritaire
         	1-26 : CONTROLES Codes Action Prioritaire Dif Prioritaire
         	1-27 : CONTROLES Codes Categorie Action Dif Prioritaire
         	
         	1-28 : CONTROLES Codes Objet de Formation Formation Tuteur 
         	1-29 : CONTROLES Codes Objet de Formation Fonction Tutorale 
         	1-30 : CONTROLES Codes Objet de Formation DIF Non Prioritaire 
         	1-31 : CONTROLES Codes Categorie Action DIF Non Prioritaire 
         	1-32 : Controles des Stagiaires : Recherche salarie associe
         	1-33 : CONTROLES Tuteurs 
         	1-34 : Rejets des modules pour lesquels au moins une ligne a ‚t‚ rejet‚e : Inhibe suite a demande SANOFI
         
         2- Traitements des Donnees non rejetes :
         	2-35 : CREATION DES ACTIONS NON REJETEES	
         	2-36 : CREATION DES MODULES NON REJETES
         	2-37 : CREATION DES Sous Type de Cout des modules PEC non rejet‚s 
         	2-39 : CREATION DES Stagiaires des modules PEC non rejet‚s 
         	2-40 : AJOUT COMMENTAIRES AU MODULE SUR SALARIES REJETES 
         	2-41 : TRAITEMENT GENERATION DES LOGS DE L EDI
         	2-42 : TRAITEMENT AFFECTATION DES ACTIONS AUX AGF/CONSEILLER ET MAIL D INFO ASSOCIE
         ----------------------------------------------   
         Autor : MBL 
         Date Modification : 05/06/2014
         Description :	Seulement les salaries actifs sont rattaches a un module PEC,
         				Les salaries inactifs sont rejetes
         ----------------------------------------------   
         Autor : MBL 
         Date Modification : 13/04/2015
         Description :	Pour les actions collectives, la cible est valoris‚ … 3 (CIBLE_ACTION = 3) correspondant … une Action Groupe d'Entreprise
         ----------------------------------------------   
         Autor : MBL 
         Date Modification : 13/04/2015
         Description :	Le libell‚ de l'action ne fait plus partie de la cle de d‚tection des Actions/Modules
         ----------------------------------------------   
         Autor : MBL 
         Date Modification : 13/04/2015
         Description :	D‚tection doublons salari‚s d‚j… existant bas‚ sur le mˆme OF et plus mˆme Etablissement OF
         ----------------------------------------------   
         Autor : MBL 
         Date Modification : 08/06/2015
         Description		  :	R‚activation et D‚cloture des actions associ‚es … des modules cr‚‚s via EDI PEC
         ----------------------------------------------   
         */
         
         AS        
                
         SET NOCOUNT ON        
         
         IF OBJECT_ID('tempdb..#TMP_EDI', 'U') IS NOT NULL 
         BEGIN
         	drop table #TMP_EDI
         END
         CREATE TABLE #TMP_EDI
         (ID_DISPOSITIF int, COD_DISPOSITIF varchar(8), BLN_PLAN tinyint)
         
         DECLARE        
         		@NUM_LIGNE			INTEGER,        
         		@ID_TABLE			INTEGER,        
         		@COD_TABLE_TAMPON	VARCHAR(50),        
         		@ID_COLONNE			INTEGER,        
         		@NUM_POSITION		INTEGER,        
         		@COD_COLONNE		VARCHAR(20),        
         		@LIB_COLONNE		VARCHAR(50),        
         		@LIB_SQL			VARCHAR(2000),        
         		@VAL_COLONNE		VARCHAR(100),        
         		@I					INTEGER,        
         		@ASCII				INTEGER,        
         		@BLN_OK				TINYINT,        
         		@DAT				VARCHAR(10),        
         		@BLN_REJET			TINYINT,        
         		@CPT				INTEGER        
         
         DECLARE		
         @NUM_SIRET	  			VARCHAR	(14)	,
         @LIBL_ACTION_PEC	  	VARCHAR (100)	,
         @DAT_DEB_ACTION_PEC	  	DATETIME		,
         @DAT_FIN_ACTION_PEC	  	DATETIME		,
         @COD_THEME_GLOBAL	  	VARCHAR (5)		,
         @COD_NIVEAU_ACTION	  	VARCHAR (20)	,
         @LIBL_SANCTION	  		VARCHAR (50)	,
         @NUM_DUREE_HEURE	  	FLOAT			,
         @COD_FORMACODE	  		VARCHAR (5)		,
         @AXE_ACTION	  			VARCHAR (30)	,
         @DOMAINE_ACTION	  		VARCHAR (30)	,
         @NUM_INTERNE_ACTION	  	VARCHAR (20)	,
         @NUM_SIRET_CONTACT	  	VARCHAR	(14)	,
         @LIB_NOM_CONTACT	  	VARCHAR (35)	,
         @LIB_PNM_CONTACT	  	VARCHAR (35)	,
         @EMAIL_PRO_CONTACT	  	VARCHAR (100)	,
         @NUM_TEL_CONTACT	  	VARCHAR (10)	,
         @COD_CIVILITE	  		VARCHAR (3)		,
         @LIBL_MODULE_PEC	  	VARCHAR (100)	,
         @DAT_DEBUT	  			DATETIME		,
         @DAT_FIN	  			DATETIME		,
         @COD_THEME	  			VARCHAR (5)		,
         @COD_INITIATIVE	  		VARCHAR (1)		,
         @NUM_DUREE_H_MODULE	  	FLOAT			,
         @MNT_CONVENTION	  		FLOAT			,
         @DEPART_FORMATION	  	INTEGER			,
         @BLN_DELEG_PAIEMENT	  	VARCHAR (1)		,
         @NUM_SIRET_OF	  		VARCHAR	(14)	,
         @LIBL_OF	  			VARCHAR (100)	,
         @NUM_INTERNE	  		VARCHAR (20)	,
         @AXE_MODULE	  			VARCHAR (30)	,
         @DOMAINE_MODULE	  		VARCHAR (30)	,
         @BLN_EXTERNE	  		VARCHAR (1)		,
         @BLN_INTRA	  			VARCHAR (1)		,
         @BLN_DELE_PAIEMENT	  	VARCHAR (1)		,
         @COM_MODULE	  			VARCHAR (500)	,
         @NUM_SIRET_FORM_INT	  	VARCHAR	(14)	,
         @NIR_FORM_INT	  		VARCHAR	(15)	,
         @NOM_FORM_INT	  		VARCHAR (50)	,
         @PRENOM_FORM_INT	  	VARCHAR (50)	,
         @DAT_NAISS_FORM_INT	  	DATETIME		,
         @MATRICULE_FORM_INT	  	VARCHAR (20)	,
         @MNT_PREV_HT_CP	  		FLOAT			,
         @MNT_PREV_HT_INGE	  	FLOAT			,
         @MNT_PREV_HT_REM	  	FLOAT			,
         @MNT_PREV_HT_AF	  		FLOAT			,
         @MNT_PREV_HT_FA	  		FLOAT			,
         @MNT_PREV_HT_REPHEB	  	FLOAT			,
         @MNT_PREV_HT_ACTEVAL	FLOAT			,
         @MNT_PREV_HT_TRANSP	  	FLOAT			,
         @MNT_PREV_HT_FFRECONV	FLOAT			,
         @MNT_PREV_HT_FCT	  	FLOAT			,
         @MNT_PREV_HT_FFORM	  	FLOAT			,
         @MNT_PREV_HT_REMFORM	FLOAT			,
         @NIR_INDIVIDU	  		VARCHAR	(15)	,
         @BLN_MASCULIN			VARCHAR (1)		,
         @NOM_INDIVIDU	  		VARCHAR (50)	,
         @PRENOM_INDIVIDU	  	VARCHAR (50)	,
         @DAT_NAISSANCE	  		DATETIME		,
         @MATRICULE	  			VARCHAR (20)	,
         @BLN_TUTEUR_INTERNE	  	VARCHAR (1)		,
         @NUM_SIRET_TUTEUR	  	VARCHAR	(14)	,
         @NIR_TUTEUR	  			VARCHAR	(15)	,
         @NOM_TUTEUR	  			VARCHAR (50)	,
         @PRENOM_TUTEUR	  		VARCHAR (50)	,
         @DAT_NAISSANCE_TUTEUR	DATETIME		,
         @MATRICULE_TUTEUR	  	VARCHAR (20)	,
         @NB_H_ENGAGE_PL	  		FLOAT			,
         @NB_H_HTT_PL	  		FLOAT			,
         @COD_PUBLIC_PRIO_PL	  	VARCHAR (8)		,
         @COD_OBJET_FORM_PL	  	VARCHAR (8)		,
         @COD_ACTION_PRIO_PL	  	VARCHAR (8)	,
         @COD_CATEG_ACTION_PL	VARCHAR (8)	,
         @NB_H_ENGAGE_PP	  		FLOAT	,
         @NB_H_HTT_PP	  		FLOAT	,
         @COD_PUBLIC_PRIO_PP	  	VARCHAR (8)	,
         @COD_OBJET_FORM_PP	  	VARCHAR (8)	,
         @COD_ACTION_PRIO_PP	  	VARCHAR (8)	,
         @COD_CATEG_ACTION_PP	VARCHAR (8)	,
         @NB_H_ENGAGE_DIFP	  	FLOAT	,
         @NB_H_HTT_DIFP	  		FLOAT	,
         @COD_PUBLIC_PRIO_DIFP	VARCHAR (8)	,
         @COD_OBJET_FORM_DIFP	VARCHAR (8)	,
         @COD_ACTION_PRIO_DIFP	VARCHAR (8)	,
         @COD_CATEG_ACT_DIFP	  	VARCHAR (8)	,
         @NB_H_ENGAGE_FORMT	  	FLOAT	,
         @COD_OBJET_FORM_FORMT	VARCHAR (8)	,
         @NB_H_ENGAGE_FTUT	  	FLOAT	,
         @COD_OBJET_FORM_FTUT	VARCHAR (8)	,
         @NB_H_ENGAGE_DIFNP	  	FLOAT	,
         @NB_H_HTT_DIFNP	  		FLOAT	,
         @COD_OBJET_FORM_DIFNP	VARCHAR (8)	,
         @COD_CATEG_ACT_DIFNP	VARCHAR (8)	
         	
         DECLARE @NB_ELEMENTS_CLE	integer,
         		@NB_ELEMENT			integer,
         		@ID_COLONNE_ACTION	integer,
         		@ID_COLONNE_MODULE	integer
         		
         DECLARE 
         		@ID_ETABLISSEMENT		integer,
         		@ID_ETABLISSEMENT_OF	integer,
         		@ID_ACTION_PEC			integer,
         		@COD_ACTION_PEC			integer,
         		@ID_MODULE_PEC			integer,
         		@COD_MODULE_PEC			varchar(14),
         		@ID_THEME_ACTION		integer,
         		@ID_NIVEAU				integer,
         		@ID_SANCTION			integer,
         		@ID_FORMACODE			integer,
         		@ID_THEME_MODULE		integer,
         		@ID_DEPART_FORMATION	integer,
         		@ID_PUBLIC_PRIO_PL		integer,
         		@ID_OBJET_FORM_PL		integer,
         		@ID_ACTION_PRIO_PL		integer,
         		@ID_CATEG_ACTION_PL		integer,
         		@ID_PUBLIC_PRIO_PP		integer,
         		@ID_OBJET_FORM_PP		integer,
         		@ID_ACTION_PRIO_PP		integer,
         		@ID_CATEG_ACTION_PP		integer,
         		@ID_PUBLIC_PRIO_DIFP	integer,
         		@ID_OBJET_FORM_DIFP		integer,
         		@ID_ACTION_PRIO_DIFP	integer,
         		@ID_CATEG_ACT_DIFP		integer,
         		@ID_OBJET_FORM_FORMT	integer,
         		@ID_OBJET_FORM_FTUT		integer,
         		@ID_OBJET_FORM_DIFNP	integer,
         		@ID_CATEG_ACT_DIFNP		integer,
         		@ID_INDIVIDU			integer,
         		@ID_SALARIE				integer,
         		@ID_TUTEUR				integer,
         		@ID_SALARIE_TUTEUR		integer,
         		@ID_SALARIE_FORMATEUR_INTERNE
         								integer,
         		@BLN_PB_STAGIAIRE		tinyint,
         		@ID_UTILISATEUR_ADMIN_EDI
         								integer,
         		@ID_UTILISATEUR_ADH_EDI
         								integer,
         		@COMMENTAIRE			varchar(7600)
         
         DECLARE @NUM_DUREE_JOUR				DECIMAL(15,1),
         		@ID_AGENCE					INT,
         		@DAT_RECU					datetime,
         		@CODE_ACTION				varchar(11),  
         		@ID_CHARGEE_MISSION			INT,
         		@BLN_OK_ENGAGEMENT			tinyint,
         		@TIME_STAMP					timestamp,
         		@BLN_MODULE_EXTERNE			tinyint,
         		@ID_PERIODE					int,
         		@ID_DISPOSITIF_PAR_DEFAUT	int,
         		@BLN_DELEGATION_PAIEMENT	tinyint,
         		@ID_SOUS_TYPE_COUT			int,
         		@ID_POSTE_COUT_ENGAGE		int,
         		@COD_SOUS_TYPE_COUT			varchar(8),
         		@MNT_PREV_HT				decimal(15,2),
         		@COD_DISPOSITIF				varchar(8), 
         		@BLN_PLAN					tinyint
         
         DECLARE
         		@ID_TYPE_CONTRAT				int,
         		@ID_CSP							int,
         		@ID_CLASSIFICATION				int,
         		@ID_STATUT						int,
         		@NUM_DUREE_MENSUELLE_TRAVAIL	decimal(15,2),
         		@BRUT_CHARGE					decimal(15,2),
         		@BLN_TEMPS_PARTIEL				tinyint,
         		@CENTRE_COUT					varchar(50),
         		@ID_CODE_INSEE					int,
         		@ID_FAMILLE_PROFESSIONNELLE		int,
         		@SALAIRE_HORAIRE_NET			decimal(15,2),
         		@SALAIRE_HORAIRE_BRUT_CHARGE	decimal(15,2),
         		@SALAIRE_HORAIRE_CHARGE			decimal(15,2),
         		@MONTANT_BRUT_CHARGE			decimal(15,2),
         		@DATE_EMBAUCHE					datetime,
         		@ID_NIVEAU_AVENANT				int,
         		@ANALYTIQUE_STAGIAIRE			varchar(20),
         		@FONCTION						varchar(100),
         		@NB_HEURES_STAGIAIRE_ENG		decimal(15,2),
         		@NB_HEURES_STAGIAIRE_HTT		decimal(15,2),
         		@ID_STAGIAIRE_PEC				int,
         		@ID_UNITE_STAGIAIRE				int,
         		@ID_BRANCHE						int,
         		@ID_GROUPE						int,
         		@ID_ACTIVITE					int,
         		@ID_DISPOSITIF					int,
         		@COD_MODULE_PEC_DOUBLON			varchar(14)
         
         
         DECLARE @ID_OF							int,
         		@NUM_SIREN						varchar(9),
         		@BLN_CREATION_OF				tinyint,
         		@BLN_ETABLISSEMENT_PRINCIPAL	tinyint,
         		@ID_ADRESSE						int,
         		@ID_CONTACT						int,
         		@ID_EDI_GROUPE_EDI_PEC			int
         
         DECLARE @dbname	VARCHAR(100)
          		
         DECLARE  @BLN_DEBUG tinyint
         SET @BLN_DEBUG = 1
         
         SELECT @ID_UTILISATEUR_ADMIN_EDI = ID_UTILISATEUR
         FROM	UTILISATEUR
         WHERE	COD_UTIL = 'ADMIN_EDI'  -- Utilisateur ADMIN_EDI
         
         SELECT @ID_UTILISATEUR_ADH_EDI = ID_UTILISATEUR
         FROM	UTILISATEUR
         WHERE	COD_UTIL = 'ADH_EDI'  -- Utilisateur ADH_EDI
         
         
         -- Determination du groupe EDI associe a l'etablissement createur
         SELECT @ID_EDI_GROUPE_EDI_PEC = ID_EDI_GROUPE_EDI_PEC 
         FROM EDI_GROUPE_EDI_PEC_ETABLISSEMENT
         WHERE ID_ETABLISSEMENT = @ID_ETABLISSEMENT_CREATEUR
         
         CREATE TABLE #TMP01 -- table des lignes … problŠme
         (        
         	NUM_LIGNE  INTEGER,        
         	ID_COLONNE INTEGER,        
         	VAL_COLONNE  VARCHAR(300) COLLATE French_CI_AI ,        
         	LIB_PROBLEME VARCHAR(1000) COLLATE French_CI_AI         
         )        
                 
                 
         SELECT	@ID_TABLE = ID_TABLE ,
         		@COD_TABLE_TAMPON = COD_TABLE_TAMPON
         FROM	EDI_LOT_IMPORT
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  0', GETDATE(),  'DEBUT CONTROLES COLONNES OBLIGATOIRES', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle que les colonnes obligatoires du modŠle d'import sont renseign‚es */        
         DECLARE cu_colonne_obligatoire scroll cursor for        
         SELECT	ID_COLONNE, COD_COLONNE, LIB_COLONNE         
         FROM	EDI_IMPORT_COLONNE         
         WHERE	ID_TABLE = @ID_TABLE
         AND		BLN_OBLIGATOIRE = 1       
         ORDER BY NUM_POSITION        
                 
         OPEN  cu_colonne_obligatoire        
         FETCH cu_colonne_obligatoire INTO        
         	 @ID_COLONNE ,
         	 @COD_COLONNE ,
         	 @LIB_COLONNE
         
         
         WHILE (@@fetch_status <> -1)        
         BEGIN         
                
          SET @LIB_SQL = ''        
          SET @LIB_SQL = @LIB_SQL + 'INSERT INTO #TMP01(ID_COLONNE, VAL_COLONNE, NUM_LIGNE, LIB_PROBLEME) '        
          SET @LIB_SQL = @LIB_SQL + 'SELECT ' + CONVERT(CHAR(6), @ID_COLONNE) + ', NULL, NUM_LIGNE, ''Colonne obligatoire non renseignee '''         
          SET @LIB_SQL = @LIB_SQL + ' FROM ' + @COD_TABLE_TAMPON         
          SET @LIB_SQL = @LIB_SQL + ' WHERE (' + @COD_COLONNE + ' IS NULL OR LEN(LTRIM(ISNULL(' + @COD_COLONNE + ',0)))=0 ) AND ID_LOT_IMPORT = ' + CONVERT(CHAR(6), @ID_LOT_IMPORT)         
          --SET @LIB_SQL = @LIB_SQL + ' AND ISNULL(BLN_REJET,0) = 0'        
          --SET @LIB_SQL = @LIB_SQL + ' AND NUM_LIGNE >= ' + CONVERT(CHAR(6), @NUM_LIGNE_DEBUT)        
          --SET @LIB_SQL = @LIB_SQL + ' AND NUM_LIGNE < ' + CONVERT(CHAR(6), @NUM_LIGNE_FIN)        
          --SELECT @LIB_SQL
          EXECUTE ( @LIB_SQL )        
         
          SET @LIB_SQL = ''        
          SET @LIB_SQL = @LIB_SQL + ' UPDATE ' + @COD_TABLE_TAMPON         
          SET @LIB_SQL = @LIB_SQL + ' SET BLN_REJET = 1'        
          SET @LIB_SQL = @LIB_SQL + ' WHERE (' + @COD_COLONNE + ' IS NULL OR LEN(LTRIM(ISNULL(' + @COD_COLONNE + ',0)))=0 ) AND ID_LOT_IMPORT = ' + CONVERT(CHAR(6), @ID_LOT_IMPORT)         
          --SET @LIB_SQL = @LIB_SQL + ' AND NUM_LIGNE >= ' + CONVERT(CHAR(6), @NUM_LIGNE_DEBUT)        
          --SET @LIB_SQL = @LIB_SQL + ' AND NUM_LIGNE < ' + CONVERT(CHAR(6), @NUM_LIGNE_FIN)              
          --SELECT @LIB_SQL
          EXECUTE ( @LIB_SQL )        
                 
          FETCH cu_colonne_obligatoire INTO        
         	 @ID_COLONNE ,
         	 @COD_COLONNE ,
         	 @LIB_COLONNE
         END        
                 
         CLOSE cu_colonne_obligatoire        
         DEALLOCATE cu_colonne_obligatoire        
         /* Fin Controle colonnes obligatoires */
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  1',  GETDATE(),  'FIN CONTROLES COLONNES OBLIGATOIRES', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle de l'unicite des donnees associees aux actions et aux modules de formation */
         DECLARE cu_ctl_unicite_action CURSOR FOR
         SELECT distinct NUM_INTERNE_ACTION		
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         
         SELECT	@ID_COLONNE_ACTION = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'NUM_INTERNE_ACTION'
         
         SELECT	@ID_COLONNE_MODULE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'NUM_INTERNE'
         
         OPEN cu_ctl_unicite_action 
         
         FETCH cu_ctl_unicite_action  INTO        
         	@NUM_INTERNE_ACTION		
         
         WHILE (@@fetch_status <> -1)
         BEGIN
         
         	SELECT @NB_ELEMENTS_CLE = COUNT(*)
         	FROM
         		(
         		SELECT distinct 	
         			LIBL_ACTION_PEC			,
         			DAT_DEB_ACTION_PEC		,
         			DAT_FIN_ACTION_PEC		,
         			COD_THEME_GLOBAL		,
         			COD_NIVEAU_ACTION		,
         			LIBL_SANCTION			,
         			NUM_DUREE_HEURE			,
         			COD_FORMACODE			,
         			AXE_ACTION				,
         			DOMAINE_ACTION			,
         			NUM_INTERNE_ACTION		
         		FROM	EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         		) UNICITE_ACTION
         
         	
         	IF @NB_ELEMENTS_CLE = 1
         	-- Les donn‚es associ‚es … l'action sont homogenes
         	BEGIN
         		SET @ID_ACTION_PEC = NULL
         		SELECT TOP 1 @NUM_SIRET = NUM_SIRET
         		FROM	EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION	
         		
         		-- Test si l'action a deja ete creee.
         		-- Si c'est le cas, elle est rejetee.
         		SELECT @ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         		FROM ACTION_PEC
         		INNER JOIN NR140			ON ACTION_PEC.ID_ACTION_PEC = NR140.ID_ACTION_PEC
         		WHERE	NR140.NUM_INTERNE		= @NUM_INTERNE_ACTION
         		AND		ID_ETABLISSEMENT IN
         		(SELECT ID_ETABLISSEMENT
         		FROM EDI_GROUPE_EDI_PEC_ETABLISSEMENT
         		WHERE ID_EDI_GROUPE_EDI_PEC = @ID_EDI_GROUPE_EDI_PEC)
         
         		IF @ID_ACTION_PEC IS NOT NULL
         		BEGIN
         			UPDATE EDI_PEC_ST
         			SET		ID_ACTION_PEC = @ID_ACTION_PEC
         			WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         			AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         		END
         		
         	END
         	
         	IF @NB_ELEMENTS_CLE > 1
         	-- Plusieurs jeux de valeurs sont associes aux elements associes a une cle d'action
         	-- Les lignes associees a l'action sont rejetees car les donn‚es associees a l'action ne sont pas homogenes
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE_ACTION, @NUM_INTERNE_ACTION, 'Donnees Associees a l action non Homogenes'
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         	
         	END
         	ELSE   
         	
         	-- Un seul jeu de valeurs est associe aux elements associes a une cle d'action
         	BEGIN
                 -- Recherche des modules associes a l'action pour evaluer leur homogeneite
         		DECLARE cu_ctl_unicite_module CURSOR FOR
         		SELECT distinct 	NUM_INTERNE_ACTION		,
         							NUM_INTERNE
         		FROM	EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         
         		OPEN cu_ctl_unicite_module 
         
         		FETCH cu_ctl_unicite_module  INTO        
         			@NUM_INTERNE_ACTION		,
         			@NUM_INTERNE
         
         		WHILE (@@fetch_status <> -1)
         		BEGIN
         
         			SELECT @NB_ELEMENTS_CLE = COUNT(*)
         			FROM
         				(
         				SELECT distinct 	
         					LIBL_ACTION_PEC			,
         					DAT_DEB_ACTION_PEC		,
         					DAT_FIN_ACTION_PEC		,
         					COD_THEME_GLOBAL		,
         					COD_NIVEAU_ACTION		,
         					LIBL_SANCTION			,
         					NUM_DUREE_HEURE			,
         					COD_FORMACODE			,
         					AXE_ACTION				,
         					DOMAINE_ACTION			,
         					NUM_INTERNE_ACTION		,
         					NUM_SIRET_CONTACT	  	,
         					LIB_NOM_CONTACT	  		,
         					LIB_PNM_CONTACT	  		,
         					EMAIL_PRO_CONTACT	  	,
         					NUM_TEL_CONTACT	  		,
         					COD_CIVILITE	  		,
         					LIBL_MODULE_PEC			,
         					DAT_DEBUT				,
         					DAT_FIN					,
         					COD_THEME				,
         					COD_INITIATIVE			,
         					NUM_DUREE_H_MODULE		,
         					MNT_CONVENTION			,
         					DEPART_FORMATION		,
         					BLN_DELEG_PAIEMENT		,
         					NUM_SIRET_OF			,
         					LIBL_OF					,
         					NUM_INTERNE				,
         					AXE_MODULE				,
         					DOMAINE_MODULE			,
         					BLN_EXTERNE	  			,
         					BLN_INTRA	  			,
         					BLN_DELE_PAIEMENT	  	,
         					COM_MODULE	  			,
         					NUM_SIRET_FORM_INT	  	,
         					NIR_FORM_INT	  		,
         					NOM_FORM_INT	  		,
         					PRENOM_FORM_INT	  		,
         					DAT_NAISS_FORM_INT	  	,
         					MATRICULE_FORM_INT	  	,					
         					MNT_PREV_HT_CP			,
         					MNT_PREV_HT_INGE		,
         					MNT_PREV_HT_REM			,
         					MNT_PREV_HT_AF			,
         					MNT_PREV_HT_FA			,
         					MNT_PREV_HT_REPHEB		,
         					MNT_PREV_HT_ACTEVAL		,
         					MNT_PREV_HT_TRANSP		,
         					MNT_PREV_HT_FFRECONV	,
         					MNT_PREV_HT_FCT			,
         					MNT_PREV_HT_FFORM		,
         					MNT_PREV_HT_REMFORM							
         				FROM	EDI_PEC_ST
         				WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         				AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         				AND		NUM_INTERNE			= @NUM_INTERNE
         				) UNICITE_MODULE
         
         			IF @NB_ELEMENTS_CLE = 1 AND @ID_ACTION_PEC IS NOT NULL
         			BEGIN
         				SET @ID_MODULE_PEC  = NULL
         				
         				SELECT	@ID_MODULE_PEC = ID_MODULE_PEC
         				FROM	MODULE_PEC
         				WHERE	ID_ACTION_PEC	= @ID_ACTION_PEC 
         				AND		NUM_INTERNE		= @NUM_INTERNE
         
         				IF @ID_MODULE_PEC IS NOT NULL
         				BEGIN
         					UPDATE EDI_PEC_ST
         					SET BLN_REJET =  1 
         					WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         					AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         					AND		NUM_INTERNE			= @NUM_INTERNE
         					AND		ID_ACTION_PEC		= @ID_ACTION_PEC 
         
         					INSERT INTO #TMP01         
         					(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         					SELECT NUM_LIGNE, @ID_COLONNE_MODULE, @NUM_INTERNE_ACTION + '-' + @NUM_INTERNE , 'Donnees Associees a ce module deja enregistrees'
         					FROM EDI_PEC_ST
         					WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         					AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         					AND		NUM_INTERNE			= @NUM_INTERNE
         					AND		ID_ACTION_PEC		= @ID_ACTION_PEC 
         
         				END						
         				
         			END
         
         			IF @NB_ELEMENTS_CLE > 1
         			-- Plusieurs jeux de valeurs sont associes aux elements associes a une cle de module
         			-- Les lignes associees au module sont rejetees car les donn‚es associees au module ne sont pas homogenes
         			BEGIN
         			
         				
         				UPDATE EDI_PEC_ST
         				SET BLN_REJET =  1 
         				WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         				AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         				AND		NUM_INTERNE			= @NUM_INTERNE
         
         				INSERT INTO #TMP01         
         				(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         				SELECT NUM_LIGNE, @ID_COLONNE_MODULE, @NUM_INTERNE_ACTION + '-' + @NUM_INTERNE , 'Donnees Associees au module non Homogenes'
         				FROM EDI_PEC_ST
         				WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         				AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         				AND		NUM_INTERNE			= @NUM_INTERNE
         			
         			END
         
         			FETCH cu_ctl_unicite_module  INTO        
         				@NUM_INTERNE_ACTION		,
         				@NUM_INTERNE
         		END
         		CLOSE cu_ctl_unicite_module 
         		DEALLOCATE cu_ctl_unicite_module 
         
         	END
         
         	FETCH cu_ctl_unicite_action  INTO        
         		@NUM_INTERNE_ACTION		
         END
         CLOSE cu_ctl_unicite_action 
         DEALLOCATE cu_ctl_unicite_action 
         /* Fin Controle de l'unicite des donnees associees aux actions et aux modules de formation */
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  2',  GETDATE(),  'FIN CONTROLES unicite des donnees associees aux actions et aux modules de formation ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         
         /* Controle que la colonne SIRET de l'Adherent correspond … un ‚tablissement actif unique */
         DECLARE cu_ctl_siret CURSOR FOR
         SELECT distinct 	NUM_SIRET
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'NUM_SIRET'
         
         OPEN cu_ctl_siret 
         FETCH cu_ctl_siret 
         INTO @NUM_SIRET
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SELECT	@NB_ELEMENT = COUNT(*)
         	FROM	ETABLISSEMENT
         	WHERE	NUM_SIRET = @NUM_SIRET
         	AND		BLN_ACTIF = 1
         
         	IF @NB_ELEMENT = 1  
         	BEGIN
         		SELECT	@ID_ETABLISSEMENT = ID_ETABLISSEMENT 
         		FROM	ETABLISSEMENT
         		WHERE	NUM_SIRET = @NUM_SIRET
         		AND		BLN_ACTIF = 1
         		
         		UPDATE EDI_PEC_ST
         		SET ID_ETABLISSEMENT = @ID_ETABLISSEMENT 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		NUM_SIRET		= @NUM_SIRET
         	END
         	ELSE -- Si different de 1 c'est soit que le SIRET n'existe pas soit qu'il y en a +sieurs 
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		NUM_SIRET		= @NUM_SIRET
         
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @NUM_SIRET, CASE WHEN @NB_ELEMENT = 0 THEN 'SIRET inexistant' ELSE 'Doublons detectes au niveau SIRET Adherent' END
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		NUM_SIRET		= @NUM_SIRET
         	END
         	
         	
         	FETCH cu_ctl_siret 
         	INTO @NUM_SIRET
         	
         END
         
         CLOSE cu_ctl_siret 
         DEALLOCATE cu_ctl_siret 
         /* Fin Controle que la colonne SIRET de l'Adherent correspond … un ‚tablissement actif unique */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  3', GETDATE(),   'FIN CONTROLES SIRET ADH', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle que la colonne SIRET de l'OF contient 14 caracteres */
         DECLARE cu_ctl_siret CURSOR FOR
         SELECT distinct 	NUM_SIRET_OF
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(NUM_SIRET_OF)))> 0
         AND		LEN(RTRIM(LTRIM(NUM_SIRET_OF)))< 14
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'NUM_SIRET_OF'
         
         OPEN cu_ctl_siret 
         FETCH cu_ctl_siret 
         INTO @NUM_SIRET_OF
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         
         	UPDATE EDI_PEC_ST
         	SET BLN_REJET =  1 
         	WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         	AND		NUM_SIRET_OF	= @NUM_SIRET_OF
         
         	INSERT INTO #TMP01         
         	(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         	SELECT NUM_LIGNE, @ID_COLONNE, @NUM_SIRET_OF, 'SIRET OF incorrect (14 caracteres requis)' 
         	FROM EDI_PEC_ST
         	WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         	AND		NUM_SIRET_OF	= @NUM_SIRET_OF
         	
         	FETCH cu_ctl_siret 
         	INTO @NUM_SIRET_OF
         END
         
         CLOSE cu_ctl_siret 
         DEALLOCATE cu_ctl_siret 
         /* Fin Controle que la colonne SIRET de l'OF contient 14 caracteres */
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  4', GETDATE(),   'FIN CONTROLES SIRET OF PHASE 1', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des themes actions */
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_THEME_GLOBAL
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_THEME_GLOBAL)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_THEME_GLOBAL'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_THEME_GLOBAL
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	SET		@ID_THEME_ACTION = NULL
         	
         	SELECT	@ID_THEME_ACTION = ID_THEME
         	FROM	THEME 
         	WHERE	COD_THEME= @COD_THEME_GLOBAL
         	
         	IF @ID_THEME_ACTION IS NULL
         	BEGIN        
         		/* Theme Action non trouv‚e : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_THEME_GLOBAL	= @COD_THEME_GLOBAL
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_THEME_GLOBAL, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_THEME_GLOBAL	= @COD_THEME_GLOBAL
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_THEME_GLOBAL = @ID_THEME_ACTION
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_THEME_GLOBAL	= @COD_THEME_GLOBAL	
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_THEME_GLOBAL
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Themes actions  */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  5',  GETDATE(),  'FIN CONTROLES THEMES ACTION', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des niveaux action */
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_NIVEAU_ACTION
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_NIVEAU_ACTION)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_NIVEAU_ACTION'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_NIVEAU_ACTION
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET @ID_NIVEAU = NULL
         
         	SELECT	@ID_NIVEAU = ID_NIVEAU
         	FROM	NIVEAU
         	WHERE	COD_NIVEAU= @COD_NIVEAU_ACTION
         	
         	IF @ID_NIVEAU  IS NULL
         	BEGIN        
         		/* Niveau non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_NIVEAU_ACTION	= @COD_NIVEAU_ACTION
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_NIVEAU_ACTION, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_NIVEAU_ACTION	= @COD_NIVEAU_ACTION
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_NIVEAU_ACTION = @ID_NIVEAU
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_NIVEAU_ACTION	= @COD_NIVEAU_ACTION
         	END
         	
         	FETCH cu_ctl_colonne 
         	INTO @COD_NIVEAU_ACTION	
         END
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Niveaux */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  6',  GETDATE(),  'FIN CONTROLES NIVEAUX ACTION', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des sanctions action */
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	LIBL_SANCTION
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(LIBL_SANCTION)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'LIBL_SANCTION'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @LIBL_SANCTION
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_SANCTION = NULL
         
         	SELECT	@ID_SANCTION = ID_SANCTION
         	FROM	SANCTION
         	WHERE	LIBL_SANCTION= @LIBL_SANCTION
         	
         	IF @ID_SANCTION IS NULL   
         	BEGIN        
         		/* Sanction non trouv‚e : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		LIBL_SANCTION	= @LIBL_SANCTION
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @LIBL_SANCTION, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		LIBL_SANCTION		= @LIBL_SANCTION
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_SANCTION = @ID_SANCTION
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		LIBL_SANCTION	= @LIBL_SANCTION
         	END
         	
         	FETCH cu_ctl_colonne 
         	INTO @LIBL_SANCTION	
         END
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des sanctions action */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  7',  GETDATE(),  'FIN CONTROLES SANCTION ACTION', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des formacodes */
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_FORMACODE
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_FORMACODE)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_FORMACODE'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_FORMACODE
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_FORMACODE = NULL
         
         	SELECT	@ID_FORMACODE = ID_FORMACODE
         	FROM	FORMACODE
         	WHERE	COD_FORMACODE= @COD_FORMACODE
         	
         	IF @ID_FORMACODE IS NULL  
         	BEGIN        
         		/* Formacode non trouv‚e : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_FORMACODE	= @COD_FORMACODE
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_FORMACODE, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_FORMACODE	= @COD_FORMACODE
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_FORMACODE = @ID_FORMACODE
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_FORMACODE	= @COD_FORMACODE
         	END
         	
         	FETCH cu_ctl_colonne 
         	INTO @COD_FORMACODE	
         END
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des formacodes  */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  8',  GETDATE(),  'FIN CONTROLES FORMACODES ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des themes module */
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_THEME
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_THEME)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_THEME'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_THEME
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	SET	@ID_THEME_MODULE = NULL
         
         	SELECT	@ID_THEME_MODULE = ID_THEME
         	FROM	THEME 
         	WHERE	COD_THEME= @COD_THEME
         	
         	IF @ID_THEME_MODULE IS NULL
         	BEGIN        
         		/* Theme Module non trouv‚e : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_THEME		= @COD_THEME
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_THEME, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_THEME		= @COD_THEME
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_THEME_MODULE = @ID_THEME_MODULE
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_THEME		= @COD_THEME
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_THEME
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Themes Modules  */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  9',  GETDATE(),  'FIN CONTROLES THEMES MODULE', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Codes Initiatives */
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_INITIATIVE
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_INITIATIVE)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_INITIATIVE'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_INITIATIVE
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	IF NOT @COD_INITIATIVE IN ('E', 'S')
         	BEGIN        
         		/* Code Initiative non trouv‚e : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_INITIATIVE = @COD_INITIATIVE
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_INITIATIVE, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_INITIATIVE	= @COD_INITIATIVE
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_INITIATIVE
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Initiatives */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  10',  GETDATE(),  'FIN CONTROLES CODE INITIATIVE', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des departements de formation */
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	DEPART_FORMATION
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(DEPART_FORMATION)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'DEPART_FORMATION'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @DEPART_FORMATION
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	SET	@ID_DEPART_FORMATION = NULL
         
         	SELECT	@ID_DEPART_FORMATION = ID_DEPARTEMENT
         	FROM	DEPARTEMENT
         	WHERE	COD_DEPARTEMENT= @DEPART_FORMATION
         	
         	IF @ID_DEPART_FORMATION IS NULL 
         	BEGIN        
         		/* Departement non trouv‚e : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		DEPART_FORMATION	= @DEPART_FORMATION
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @DEPART_FORMATION, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		DEPART_FORMATION	= @DEPART_FORMATION
         		AND		LEN(RTRIM(LTRIM(COD_THEME)))> 0
         
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_DEPART_FORMATION = @ID_DEPART_FORMATION
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		DEPART_FORMATION	= @DEPART_FORMATION
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @DEPART_FORMATION
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des departements de formation   */
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  11',  GETDATE(),  'FIN CONTROLES DEPARTEMENT FORMATION', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Delegations Paiement*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	BLN_DELEG_PAIEMENT
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(BLN_DELEG_PAIEMENT)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'BLN_DELEG_PAIEMENT'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @BLN_DELEG_PAIEMENT
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	IF NOT @BLN_DELEG_PAIEMENT IN ('O', 'N')
         	BEGIN        
         		/* Delegation Paiement non trouv‚e : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_DELEG_PAIEMENT	= @BLN_DELEG_PAIEMENT
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @BLN_DELEG_PAIEMENT, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_DELEG_PAIEMENT	= @BLN_DELEG_PAIEMENT
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @BLN_DELEG_PAIEMENT
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Delegations Paiement*/
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  12A',  GETDATE(),  'FIN CONTROLES DELEGATION PAIEMENT', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Booleens Modules Externes*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	BLN_EXTERNE
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(BLN_EXTERNE)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'BLN_EXTERNE'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @BLN_EXTERNE
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	IF NOT @BLN_EXTERNE IN ('O', 'N')
         	BEGIN        
         		/* Delegation Paiement non trouv‚e : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_EXTERNE		= @BLN_EXTERNE
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, BLN_EXTERNE, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_EXTERNE	= @BLN_EXTERNE
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @BLN_EXTERNE
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Booleens Modules Externes*/
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  12A',  GETDATE(),  'FIN CONTROLES BLN_EXTERNE', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         
         /* Controle des Booleens Modules INTRA*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	BLN_INTRA
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(BLN_INTRA)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'BLN_INTRA'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @BLN_INTRA
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	IF NOT @BLN_INTRA IN ('O', 'N')
         	BEGIN        
         		/* Delegation Paiement non trouv‚e : Rejet des lignes */        
         		UPDATE	EDI_PEC_ST
         		SET		BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_INTRA		= @BLN_INTRA
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT	NUM_LIGNE, @ID_COLONNE, BLN_INTRA, 'Valeur du referentiel non autorisee' 
         		FROM	EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_INTRA		= @BLN_INTRA
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @BLN_INTRA
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Booleens Modules Intra*/
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  12C',  GETDATE(),  'FIN CONTROLES BLN_INTRA', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         
         /* Controle des Montants de convention */
         -- Les Montants de convention sont obligatoires si la formation est externe (Etablissement OF renseign‚)
         	SELECT	@ID_COLONNE = ID_COLONNE 
         	FROM	EDI_IMPORT_COLONNE
         	WHERE	ID_TABLE = @ID_TABLE
         	AND		COD_COLONNE = 'MNT_CONVENTION'
         
         	UPDATE EDI_PEC_ST
         	SET BLN_REJET =  1 
         	WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         	AND		ID_ETABLISSEMENT_OF		IS NOT NULL
         	AND		ISNULL(MNT_CONVENTION, 0)	<= 0
         
         	INSERT INTO #TMP01         
         	(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         	SELECT NUM_LIGNE, @ID_COLONNE, MNT_CONVENTION, 'Montant demand‚ OF Obligatoire pour les formations externes' 
         	FROM EDI_PEC_ST
         	WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         	AND		ID_ETABLISSEMENT_OF		IS NOT NULL
         	AND		ISNULL(MNT_CONVENTION, 0)	<= 0
         /* Fin Controle des Montants de convention */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  13',  GETDATE(),  'FIN CONTROLES Montants de convention', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Sexes Stagiaires*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	BLN_MASCULIN
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(BLN_MASCULIN)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'BLN_MASCULIN'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @BLN_MASCULIN
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	IF NOT @BLN_MASCULIN IN ('O', 'N')
         	BEGIN        
         		/* Sexe non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_MASCULIN	= @BLN_MASCULIN
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @BLN_MASCULIN, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_MASCULIN	= @BLN_MASCULIN
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @BLN_MASCULIN
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Sexes Stagiaires*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  14',  GETDATE(),  'FIN CONTROLES Sexes Stagiaires', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Bool‚ens Tuteurs internes*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	BLN_TUTEUR_INTERNE
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(BLN_TUTEUR_INTERNE)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'BLN_TUTEUR_INTERNE'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @BLN_TUTEUR_INTERNE
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	IF NOT @BLN_TUTEUR_INTERNE IN ('O', 'N')
         	BEGIN        
         		/* Bool‚en Tuteur interne non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_TUTEUR_INTERNE	= @BLN_TUTEUR_INTERNE
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @BLN_TUTEUR_INTERNE, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		BLN_TUTEUR_INTERNE	= @BLN_TUTEUR_INTERNE
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @BLN_TUTEUR_INTERNE
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Tuteurs internes*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  15',  GETDATE(),  'FIN CONTROLES Tuteurs internes', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         
         /* Controle des Codes Public Prioritaire Plan*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_PUBLIC_PRIO_PL
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_PUBLIC_PRIO_PL)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_PUBLIC_PRIO_PL'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_PUBLIC_PRIO_PL
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_PUBLIC_PRIO_PL = NULL
         
         	SELECT	@ID_PUBLIC_PRIO_PL = PUBLIC_PRIORITAIRE.ID_PUBLIC_PRIORITAIRE
         	FROM		PUBLIC_PRIORITAIRE
         	INNER JOIN	LIAISON_PUBLICPRIORITAIRE_DISPOSITIF ON PUBLIC_PRIORITAIRE.ID_PUBLIC_PRIORITAIRE = LIAISON_PUBLICPRIORITAIRE_DISPOSITIF .ID_PUBLIC_PRIORITAIRE 
         	WHERE	COD_PUBLIC_PRIORITAIRE= @COD_PUBLIC_PRIO_PL
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE BLN_PLAN =1)
         	
         	IF @ID_PUBLIC_PRIO_PL IS NULL
         	BEGIN        
         		/* Code Public Prioritaire Plan non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_PUBLIC_PRIO_PL	= @COD_PUBLIC_PRIO_PL
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_PUBLIC_PRIO_PL, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_PUBLIC_PRIO_PL	= @COD_PUBLIC_PRIO_PL
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_PUBLIC_PRIO_PL = @ID_PUBLIC_PRIO_PL
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_PUBLIC_PRIO_PL	= @COD_PUBLIC_PRIO_PL
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_PUBLIC_PRIO_PL
         
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Public Prioritaire Plan*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  16',  GETDATE(),  'FIN CONTROLES Codes Public Prioritaire Plan', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Codes Objet de Formation Plan*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_OBJET_FORM_PL
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_OBJET_FORM_PL)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_OBJET_FORM_PL'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_OBJET_FORM_PL
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_OBJET_FORM_PL = NULL
         	SELECT	@ID_OBJET_FORM_PL = OBJET_FORMATION.ID_OBJET_FORMATION
         	FROM	OBJET_FORMATION
         	INNER JOIN	LIAISON_OBJETFORMATION_DISPOSITIF ON OBJET_FORMATION.ID_OBJET_FORMATION = LIAISON_OBJETFORMATION_DISPOSITIF.ID_OBJET_FORMATION 
         	WHERE	COD_OBJET_FORMATION= @COD_OBJET_FORM_PL
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE BLN_PLAN =1)
         	
         	IF @ID_OBJET_FORM_PL IS NULL
         	BEGIN        
         		/* Code Objet de Formation Plan non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_PL	= @COD_OBJET_FORM_PL
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_OBJET_FORM_PL, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_PL	= @COD_OBJET_FORM_PL
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_OBJET_FORM_PL = @ID_OBJET_FORM_PL
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_PL	= @COD_OBJET_FORM_PL
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_OBJET_FORM_PL
         
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Objet de Formation Plan*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  17',  GETDATE(),  'FIN CONTROLES Codes Objet de Formation Plan', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Codes Action Prioritaire Plan*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_ACTION_PRIO_PL
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_ACTION_PRIO_PL)))> 0
         
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_ACTION_PRIO_PL'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_ACTION_PRIO_PL
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_ACTION_PRIO_PL = NULL
         
         	SELECT	@ID_ACTION_PRIO_PL = ACTION_PRIORITAIRE.ID_ACTION_PRIORITAIRE
         	FROM	ACTION_PRIORITAIRE
         	INNER JOIN	LIAISON_ACTIONPRIORITAIRE_DISPOSITIF ON ACTION_PRIORITAIRE.ID_ACTION_PRIORITAIRE = LIAISON_ACTIONPRIORITAIRE_DISPOSITIF.ID_ACTION_PRIORITAIRE 
         	WHERE	COD_ACTION_PRIORITAIRE= @COD_ACTION_PRIO_PL
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE BLN_PLAN =1)
         	
         	IF @ID_ACTION_PRIO_PL IS NULL
         	BEGIN        
         		/* Code Action Prioritaire Plan non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_ACTION_PRIO_PL	= @COD_ACTION_PRIO_PL
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_ACTION_PRIO_PL, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_ACTION_PRIO_PL	= @COD_ACTION_PRIO_PL
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_ACTION_PRIO_PL = @ID_ACTION_PRIO_PL
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_ACTION_PRIO_PL	= @COD_ACTION_PRIO_PL
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_ACTION_PRIO_PL
         
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Action Prioritaire Plan*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  18',  GETDATE(),  'FIN CONTROLES Codes Action Prioritaire Plan', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Codes Categorie Action PLAN*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_CATEG_ACTION_PL
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_CATEG_ACTION_PL)))> 0
         
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_CATEG_ACTION_PL'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_CATEG_ACTION_PL
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	SET	@ID_CATEG_ACTION_PL = NULL
         
         	SELECT	@ID_CATEG_ACTION_PL = CATEGORIE_ACTION.ID_CATEGORIE_ACTION
         	FROM	CATEGORIE_ACTION
         	INNER JOIN	LIAISON_CATEGORIEACTION_DISPOSITIF ON CATEGORIE_ACTION.ID_CATEGORIE_ACTION= LIAISON_CATEGORIEACTION_DISPOSITIF.ID_CATEGORIE_ACTION
         	WHERE	COD_CATEGORIE_ACTION= @COD_CATEG_ACTION_PL
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE BLN_PLAN =1)
         	
         	IF @ID_CATEG_ACTION_PL IS NULL
         	BEGIN        
         		/* Code Categorie Action PLAN non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACTION_PL	= @COD_CATEG_ACTION_PL
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_CATEG_ACTION_PL, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACTION_PL	= @COD_CATEG_ACTION_PL
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_CATEG_ACTION_PL = @ID_CATEG_ACTION_PL
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACTION_PL	= @COD_CATEG_ACTION_PL
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_CATEG_ACTION_PL
         	
         END
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Categorie Action PLAN*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  19',  GETDATE(),  'FIN CONTROLES Codes Categorie Action PLAN', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Codes Public Prioritaire PP*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_PUBLIC_PRIO_PP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_PUBLIC_PRIO_PP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_PUBLIC_PRIO_PP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_PUBLIC_PRIO_PP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         
         	SET	@ID_PUBLIC_PRIO_PP = NULL
         	SELECT	@ID_PUBLIC_PRIO_PP = PUBLIC_PRIORITAIRE.ID_PUBLIC_PRIORITAIRE
         	FROM	PUBLIC_PRIORITAIRE
         	INNER JOIN	LIAISON_PUBLICPRIORITAIRE_DISPOSITIF ON PUBLIC_PRIORITAIRE.ID_PUBLIC_PRIORITAIRE = LIAISON_PUBLICPRIORITAIRE_DISPOSITIF .ID_PUBLIC_PRIORITAIRE 
         	WHERE	COD_PUBLIC_PRIORITAIRE= @COD_PUBLIC_PRIO_PP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE COD_DISPOSITIF = 'PPPRIO')
         
         	
         	IF @ID_PUBLIC_PRIO_PP IS NULL
         	BEGIN        
         		/* Code Public Prioritaire PP non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_PUBLIC_PRIO_PP	= @COD_PUBLIC_PRIO_PP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_PUBLIC_PRIO_PP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_PUBLIC_PRIO_PP	= @COD_PUBLIC_PRIO_PP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_PUBLIC_PRIO_PP = @ID_PUBLIC_PRIO_PP
         		WHERE	ID_LOT_IMPORT	= @ID_LOT_IMPORT
         		AND		COD_PUBLIC_PRIO_PP	= @COD_PUBLIC_PRIO_PP
         	END
         	
         	FETCH cu_ctl_colonne 
         	INTO @COD_PUBLIC_PRIO_PP
         
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Public Prioritaire PP*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  20',  GETDATE(),  'FIN CONTROLES Codes Public Prioritaire PP', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Codes Objet de Formation PP*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_OBJET_FORM_PP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_OBJET_FORM_PP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_OBJET_FORM_PP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_OBJET_FORM_PP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_OBJET_FORM_PP = NULL
         	SELECT	@ID_OBJET_FORM_PP = OBJET_FORMATION.ID_OBJET_FORMATION
         	FROM	OBJET_FORMATION
         	INNER JOIN	LIAISON_OBJETFORMATION_DISPOSITIF ON OBJET_FORMATION.ID_OBJET_FORMATION = LIAISON_OBJETFORMATION_DISPOSITIF.ID_OBJET_FORMATION 
         	WHERE	COD_OBJET_FORMATION= @COD_OBJET_FORM_PP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE COD_DISPOSITIF = 'PPPRIO')
         	
         	IF @ID_OBJET_FORM_PP IS NULL
         	BEGIN        
         		/* Code Objet de Formation PP non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_PP	= @COD_OBJET_FORM_PP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_OBJET_FORM_PP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_PP	= @COD_OBJET_FORM_PP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET ID_OBJET_FORM_PP = @ID_OBJET_FORM_PP
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_PP	= @COD_OBJET_FORM_PP
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_OBJET_FORM_PP
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Objet de Formation PP*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  21',  GETDATE(),  'FIN CONTROLES Codes Objet de Formation PP', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Codes Action Prioritaire PP*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_ACTION_PRIO_PP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_ACTION_PRIO_PP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_ACTION_PRIO_PP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_ACTION_PRIO_PP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_ACTION_PRIO_PP = NULL
         
         	SELECT	@ID_ACTION_PRIO_PP = ACTION_PRIORITAIRE.ID_ACTION_PRIORITAIRE
         	FROM	ACTION_PRIORITAIRE
         	INNER JOIN	LIAISON_ACTIONPRIORITAIRE_DISPOSITIF ON ACTION_PRIORITAIRE.ID_ACTION_PRIORITAIRE = LIAISON_ACTIONPRIORITAIRE_DISPOSITIF.ID_ACTION_PRIORITAIRE 
         	WHERE	COD_ACTION_PRIORITAIRE= @COD_ACTION_PRIO_PP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE COD_DISPOSITIF = 'PPPRIO')
         
         	
         	IF @ID_ACTION_PRIO_PP IS NULL
         	BEGIN        
         		/* Code Action Prioritaire PP non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_ACTION_PRIO_PP	= @COD_ACTION_PRIO_PP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_ACTION_PRIO_PP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_ACTION_PRIO_PP	= @COD_ACTION_PRIO_PP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_ACTION_PRIO_PP	= @ID_ACTION_PRIO_PP
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_ACTION_PRIO_PP	= @COD_ACTION_PRIO_PP
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_ACTION_PRIO_PP
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Action Prioritaire PP*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  22',  GETDATE(),  'FIN CONTROLES Codes Action Prioritaire PP', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Codes Categorie Action PP*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_CATEG_ACTION_PP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_CATEG_ACTION_PP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_CATEG_ACTION_PP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_CATEG_ACTION_PP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	SET	@ID_CATEG_ACTION_PP = NULL
         
         	SELECT	@ID_CATEG_ACTION_PP = CATEGORIE_ACTION.ID_CATEGORIE_ACTION
         	FROM	CATEGORIE_ACTION
         	INNER JOIN	LIAISON_CATEGORIEACTION_DISPOSITIF ON CATEGORIE_ACTION.ID_CATEGORIE_ACTION= LIAISON_CATEGORIEACTION_DISPOSITIF.ID_CATEGORIE_ACTION
         	WHERE	COD_CATEGORIE_ACTION= @COD_CATEG_ACTION_PP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE COD_DISPOSITIF = 'PPPRIO')
         	
         	IF @ID_CATEG_ACTION_PP IS NULL
         	BEGIN        
         		/* Code Categorie Action PP non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACTION_PP	= @COD_CATEG_ACTION_PP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_CATEG_ACTION_PP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACTION_PP	= @COD_CATEG_ACTION_PP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_CATEG_ACTION_PP	= @ID_CATEG_ACTION_PP
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACTION_PP	= @COD_CATEG_ACTION_PP
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_CATEG_ACTION_PP
         	
         END
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Categorie Action PP */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  23',  GETDATE(),  'FIN CONTROLES Codes Categorie Action PP ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Codes Public Prioritaire Dif Prioritaire*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_PUBLIC_PRIO_DIFP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_PUBLIC_PRIO_DIFP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_PUBLIC_PRIO_DIFP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_PUBLIC_PRIO_DIFP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_PUBLIC_PRIO_DIFP = NULL
         	SELECT	@ID_PUBLIC_PRIO_DIFP = PUBLIC_PRIORITAIRE.ID_PUBLIC_PRIORITAIRE
         	FROM	PUBLIC_PRIORITAIRE
         	INNER JOIN	LIAISON_PUBLICPRIORITAIRE_DISPOSITIF ON PUBLIC_PRIORITAIRE.ID_PUBLIC_PRIORITAIRE = LIAISON_PUBLICPRIORITAIRE_DISPOSITIF .ID_PUBLIC_PRIORITAIRE 
         	WHERE	COD_PUBLIC_PRIORITAIRE= @COD_PUBLIC_PRIO_DIFP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF  FROM DISPOSITIF WHERE COD_DISPOSITIF = 'DIFPRIO')
         
         	
         	IF @ID_PUBLIC_PRIO_DIFP IS NULL
         	BEGIN        
         		/* Code Public Prioritaire Dif Prioritaire non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_PUBLIC_PRIO_DIFP	= @COD_PUBLIC_PRIO_DIFP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_PUBLIC_PRIO_DIFP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_PUBLIC_PRIO_DIFP	= @COD_PUBLIC_PRIO_DIFP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_PUBLIC_PRIO_DIFP		= @ID_PUBLIC_PRIO_DIFP
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_PUBLIC_PRIO_DIFP	= @COD_PUBLIC_PRIO_DIFP
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_PUBLIC_PRIO_DIFP
         
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Public Prioritaire Dif Prioritaire*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  24',  GETDATE(),  'FIN CONTROLES Public Prioritaire Dif Prioritaire', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Codes Objet de Formation Dif Prioritaire*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_OBJET_FORM_DIFP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_OBJET_FORM_DIFP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_OBJET_FORM_DIFP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_OBJET_FORM_DIFP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_OBJET_FORM_DIFP = NULL
         	SELECT	@ID_OBJET_FORM_DIFP = OBJET_FORMATION.ID_OBJET_FORMATION
         	FROM	OBJET_FORMATION
         	INNER JOIN	LIAISON_OBJETFORMATION_DISPOSITIF ON OBJET_FORMATION.ID_OBJET_FORMATION = LIAISON_OBJETFORMATION_DISPOSITIF.ID_OBJET_FORMATION 
         	WHERE	COD_OBJET_FORMATION= @COD_OBJET_FORM_DIFP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF  FROM DISPOSITIF WHERE COD_DISPOSITIF = 'DIFPRIO')
         	
         	IF @ID_OBJET_FORM_DIFP IS NULL
         	BEGIN        
         		/* Code Objet de Formation Dif Prioritaire non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_DIFP	= @COD_OBJET_FORM_DIFP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_OBJET_FORM_DIFP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_DIFP	= @COD_OBJET_FORM_DIFP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_OBJET_FORM_DIFP		= @ID_OBJET_FORM_DIFP
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_DIFP	= @COD_OBJET_FORM_DIFP
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_OBJET_FORM_DIFP
         
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Objet de Formation Dif Prioritaire*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  25',  GETDATE(),  'FIN CONTROLES Codes Objet de Formation Dif Prioritaire', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Codes Action Prioritaire Dif Prioritaire*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_ACTION_PRIO_DIFP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_ACTION_PRIO_DIFP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_ACTION_PRIO_DIFP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_ACTION_PRIO_DIFP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_ACTION_PRIO_DIFP = NULL
         	SELECT	@ID_ACTION_PRIO_DIFP = ACTION_PRIORITAIRE.ID_ACTION_PRIORITAIRE
         	FROM	ACTION_PRIORITAIRE
         	INNER JOIN	LIAISON_ACTIONPRIORITAIRE_DISPOSITIF ON ACTION_PRIORITAIRE.ID_ACTION_PRIORITAIRE = LIAISON_ACTIONPRIORITAIRE_DISPOSITIF.ID_ACTION_PRIORITAIRE 
         	WHERE	COD_ACTION_PRIORITAIRE= @COD_ACTION_PRIO_DIFP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF  FROM DISPOSITIF WHERE COD_DISPOSITIF = 'DIFPRIO')
         	
         	IF @ID_ACTION_PRIO_DIFP IS NULL
         	BEGIN        
         		/* Code Action Prioritaire Dif Prioritaire non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_ACTION_PRIO_DIFP	= @COD_ACTION_PRIO_DIFP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_ACTION_PRIO_DIFP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_ACTION_PRIO_DIFP	= @COD_ACTION_PRIO_DIFP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_ACTION_PRIO_DIFP		= @ID_ACTION_PRIO_DIFP
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_ACTION_PRIO_DIFP	= @COD_ACTION_PRIO_DIFP
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_ACTION_PRIO_DIFP
         
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Action Prioritaire Dif Prioritaire*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  26',  GETDATE(),  'FIN CONTROLES Codes Action Prioritaire Dif Prioritaire', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Codes Categorie Action Dif Prioritaire*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_CATEG_ACT_DIFP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_CATEG_ACT_DIFP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_CATEG_ACT_DIFP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_CATEG_ACT_DIFP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	SET	@ID_CATEG_ACT_DIFP = NULL
         
         	SELECT	@ID_CATEG_ACT_DIFP = CATEGORIE_ACTION.ID_CATEGORIE_ACTION
         	FROM	CATEGORIE_ACTION
         	INNER JOIN	LIAISON_CATEGORIEACTION_DISPOSITIF ON CATEGORIE_ACTION.ID_CATEGORIE_ACTION= LIAISON_CATEGORIEACTION_DISPOSITIF.ID_CATEGORIE_ACTION
         	WHERE	COD_CATEGORIE_ACTION= @COD_CATEG_ACT_DIFP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF  FROM DISPOSITIF WHERE COD_DISPOSITIF = 'DIFPRIO')
         	
         	IF @ID_CATEG_ACT_DIFP IS NULL
         	BEGIN        
         		/* Code Categorie Action Dif Prioritaire non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACT_DIFP	= @COD_CATEG_ACT_DIFP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_CATEG_ACT_DIFP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACT_DIFP	= @COD_CATEG_ACT_DIFP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_CATEG_ACT_DIFP		= @ID_CATEG_ACT_DIFP
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACT_DIFP	= @COD_CATEG_ACT_DIFP
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_CATEG_ACT_DIFP
         	
         END
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Categorie Action Dif Prioritaire */
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  27',  GETDATE(),  'FIN CONTROLES Codes Categorie Action Dif Prioritaire', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Codes Objet de Formation Formation Tuteur*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_OBJET_FORM_FORMT
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_OBJET_FORM_FORMT)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_OBJET_FORM_FORMT'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_OBJET_FORM_FORMT
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_OBJET_FORM_FORMT = NULL
         	SELECT	@ID_OBJET_FORM_FORMT = OBJET_FORMATION.ID_OBJET_FORMATION
         	FROM	OBJET_FORMATION
         	INNER JOIN	LIAISON_OBJETFORMATION_DISPOSITIF ON OBJET_FORMATION.ID_OBJET_FORMATION = LIAISON_OBJETFORMATION_DISPOSITIF.ID_OBJET_FORMATION 
         	WHERE	COD_OBJET_FORMATION= @COD_OBJET_FORM_FORMT
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF  FROM DISPOSITIF WHERE COD_DISPOSITIF = 'FORMTUT')
         	
         	IF @ID_OBJET_FORM_FORMT IS NULL
         	BEGIN        
         		/* Code Objet de Formation Formation Tuteur non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_FORMT	= @COD_OBJET_FORM_FORMT
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_OBJET_FORM_FORMT, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_FORMT	= @COD_OBJET_FORM_FORMT
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_OBJET_FORM_FORMT		= @ID_OBJET_FORM_FORMT
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_FORMT	= @COD_OBJET_FORM_FORMT
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_OBJET_FORM_FORMT
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Objet de Formation Formation Tuteur */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  28',  GETDATE(),  'FIN CONTROLES Codes Objet de Formation Formation Tuteur ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Codes Objet de Formation Fonction Tutorale*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_OBJET_FORM_FTUT
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_OBJET_FORM_FTUT)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_OBJET_FORM_FTUT'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_OBJET_FORM_FTUT
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_OBJET_FORM_FTUT = NULL
         	SELECT	@ID_OBJET_FORM_FTUT = OBJET_FORMATION.ID_OBJET_FORMATION
         	FROM	OBJET_FORMATION
         	INNER JOIN	LIAISON_OBJETFORMATION_DISPOSITIF ON OBJET_FORMATION.ID_OBJET_FORMATION = LIAISON_OBJETFORMATION_DISPOSITIF.ID_OBJET_FORMATION 
         	WHERE	COD_OBJET_FORMATION= @COD_OBJET_FORM_FTUT
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF  FROM DISPOSITIF WHERE COD_DISPOSITIF = 'FORMTUT')
         	
         	IF @ID_OBJET_FORM_FTUT IS NULL
         	BEGIN        
         		/* Code Objet de Formation Fonction Tutorale non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_FTUT	= @COD_OBJET_FORM_FTUT
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_OBJET_FORM_FTUT, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_FTUT	= @COD_OBJET_FORM_FTUT
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_OBJET_FORM_FTUT	= @ID_OBJET_FORM_FTUT
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_FTUT	= @COD_OBJET_FORM_FTUT
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_OBJET_FORM_FTUT
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Objet de Formation Fonction Tutorale */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  29',  GETDATE(),  'FIN CONTROLES Codes Objet de Formation Fonction Tutorale ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Codes Objet de Formation DIF Non Prioritaire*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_OBJET_FORM_DIFNP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_OBJET_FORM_DIFNP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_OBJET_FORM_DIFNP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_OBJET_FORM_DIFNP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET	@ID_OBJET_FORM_DIFNP = NULL
         	SELECT	@ID_OBJET_FORM_DIFNP = OBJET_FORMATION.ID_OBJET_FORMATION
         	FROM	OBJET_FORMATION
         	INNER JOIN	LIAISON_OBJETFORMATION_DISPOSITIF ON OBJET_FORMATION.ID_OBJET_FORMATION = LIAISON_OBJETFORMATION_DISPOSITIF.ID_OBJET_FORMATION 
         	WHERE	COD_OBJET_FORMATION= @COD_OBJET_FORM_DIFNP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE COD_DISPOSITIF = 'DIFNONP')
         	
         	IF @ID_OBJET_FORM_DIFNP IS NULL
         	BEGIN        
         		/* Code Objet de Formation DIF Non Prioritaire non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_DIFNP	= @COD_OBJET_FORM_DIFNP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_OBJET_FORM_DIFNP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_DIFNP	= @COD_OBJET_FORM_DIFNP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_OBJET_FORM_DIFNP		= @ID_OBJET_FORM_DIFNP
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_OBJET_FORM_DIFNP	= @COD_OBJET_FORM_DIFNP
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_OBJET_FORM_DIFNP
         	
         END
         
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Objet de Formation DIF Non Prioritaire */
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  30',  GETDATE(),  'FIN CONTROLES Codes Objet de Formation DIF Non Prioritaire ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Controle des Codes Categorie Action DIF Non Prioritaire*/
         DECLARE cu_ctl_colonne CURSOR FOR
         SELECT distinct 	COD_CATEG_ACT_DIFNP
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_CATEG_ACT_DIFNP)))> 0
         
         SELECT	@ID_COLONNE = ID_COLONNE 
         FROM	EDI_IMPORT_COLONNE
         WHERE	ID_TABLE = @ID_TABLE
         AND		COD_COLONNE = 'COD_CATEG_ACT_DIFNP'
         
         OPEN cu_ctl_colonne 
         FETCH cu_ctl_colonne 
         INTO @COD_CATEG_ACT_DIFNP
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	SET	@ID_CATEG_ACT_DIFNP = NULL
         	
         	SELECT	@ID_CATEG_ACT_DIFNP = CATEGORIE_ACTION.ID_CATEGORIE_ACTION
         	FROM	CATEGORIE_ACTION
         	INNER JOIN	LIAISON_CATEGORIEACTION_DISPOSITIF ON CATEGORIE_ACTION.ID_CATEGORIE_ACTION= LIAISON_CATEGORIEACTION_DISPOSITIF.ID_CATEGORIE_ACTION
         	WHERE	COD_CATEGORIE_ACTION= @COD_CATEG_ACT_DIFNP
         	AND		ID_DISPOSITIF IN (SELECT ID_DISPOSITIF  FROM DISPOSITIF WHERE COD_DISPOSITIF = 'DIFNONP')
         
         	IF @ID_CATEG_ACT_DIFNP IS NULL
         	BEGIN        
         		/* Code Categorie DIF Non Prioritaire PLAN non trouv‚ : Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACT_DIFNP	= @COD_CATEG_ACT_DIFNP
         
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @COD_CATEG_ACT_DIFNP, 'Valeur du referentiel non autorisee' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACT_DIFNP	= @COD_CATEG_ACT_DIFNP
         	END
         	ELSE
         	BEGIN
         		UPDATE EDI_PEC_ST
         		SET		ID_CATEG_ACT_DIFNP		= @ID_CATEG_ACT_DIFNP
         		WHERE	ID_LOT_IMPORT			= @ID_LOT_IMPORT
         		AND		COD_CATEG_ACT_DIFNP		= @COD_CATEG_ACT_DIFNP
         	END
         
         	FETCH cu_ctl_colonne 
         	INTO @COD_CATEG_ACT_DIFNP
         	
         END
         CLOSE cu_ctl_colonne 
         DEALLOCATE cu_ctl_colonne 
         /* Fin Controle des Codes Categorie Action DIF Non Prioritaire*/
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  31',  GETDATE(),  'FIN CONTROLES Codes Categorie Action DIF Non Prioritaire ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Controle des Stagiaires : Recherche salarie associe*/
         DECLARE cu_ctl_salarie CURSOR FOR
         SELECT distinct 	NIR_INDIVIDU, NOM_INDIVIDU, PRENOM_INDIVIDU, DAT_NAISSANCE, BLN_MASCULIN, MATRICULE, NUM_SIRET, ID_ETABLISSEMENT
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         
         OPEN cu_ctl_salarie 
         FETCH cu_ctl_salarie 
         INTO @NIR_INDIVIDU, @NOM_INDIVIDU, @PRENOM_INDIVIDU, @DAT_NAISSANCE, @BLN_MASCULIN, @MATRICULE, @NUM_SIRET, @ID_ETABLISSEMENT
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         
         	SET @ID_INDIVIDU = NULL
         
         	IF @NIR_INDIVIDU = ''		SET @NIR_INDIVIDU = NULL
         	IF @NOM_INDIVIDU = ''		SET @NOM_INDIVIDU = NULL
         	IF @PRENOM_INDIVIDU = ''	SET @PRENOM_INDIVIDU = NULL
         	IF @BLN_MASCULIN = ''		SET @BLN_MASCULIN = NULL
         	IF @MATRICULE = ''			SET @MATRICULE= NULL
         	
         	SELECT	@ID_COLONNE = ID_COLONNE
         	FROM	EDI_IMPORT_COLONNE
         	WHERE	ID_TABLE = @ID_TABLE
         	AND		COD_COLONNE = 'NOM_INDIVIDU'
         	
         	-- Contr“le des donn‚es obligatoires
         	-- Pour ne pas etre rejetee, les donn‚es associees aux salaries doivent repondre aux caracteristiques suivantes :
         	-- - Soit le NIR est renseign‚
         	-- - Soit le matricule est renseign‚
         	-- - Soit l'ensemble des champs Nom, Prenom, Sexe et Date de Naissance sont simultan‚ment renseign‚s
         	IF	(	LEN(ISNULL(@NIR_INDIVIDU, '')) = 0
         		AND LEN(ISNULL(@MATRICULE ,''))	= 0
         		AND 
         			(	LEN(ISNULL(@NOM_INDIVIDU, '')) = 0
         			OR	LEN(ISNULL(@PRENOM_INDIVIDU, '')) = 0
         			OR	ISNULL(@DAT_NAISSANCE, GETDATE()) = GETDATE()
         			OR	LEN(ISNULL(@BLN_MASCULIN, '')) = 0
         			) 
         		)-- champs permettant d'identifier le salarie non renseignees
         	BEGIN
         	
         		/* Donnee Obligatoires du Salarie non transmise: Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT						= @ID_LOT_IMPORT
         		AND		NUM_SIRET							= @NUM_SIRET
         		AND		COALESCE(NIR_INDIVIDU, '')			= COALESCE(@NIR_INDIVIDU, NIR_INDIVIDU, '')
         		AND		COALESCE(NOM_INDIVIDU, '')			= COALESCE(@NOM_INDIVIDU, NOM_INDIVIDU, '')
         		AND		COALESCE(PRENOM_INDIVIDU, '')		= COALESCE(@PRENOM_INDIVIDU, PRENOM_INDIVIDU, '')
         		AND		COALESCE(BLN_MASCULIN, '')			= COALESCE(@BLN_MASCULIN, BLN_MASCULIN, '')
         		AND		COALESCE(DAT_NAISSANCE, GETDATE())	= COALESCE(@DAT_NAISSANCE, DAT_NAISSANCE, GETDATE())
         		AND		COALESCE(MATRICULE, '')				= COALESCE(@MATRICULE, MATRICULE, '')
         		
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @NOM_INDIVIDU, 'Donn‚es obligatoires associees au stagiaire non renseignees' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT						= @ID_LOT_IMPORT
         		AND		NUM_SIRET							= @NUM_SIRET
         		AND		COALESCE(NIR_INDIVIDU, '')			= COALESCE(@NIR_INDIVIDU, NIR_INDIVIDU, '')
         		AND		COALESCE(MATRICULE, '')				= COALESCE(@MATRICULE, MATRICULE, '')
         		AND		COALESCE(NOM_INDIVIDU, '')			= COALESCE(@NOM_INDIVIDU, NOM_INDIVIDU, '')
         		AND		COALESCE(PRENOM_INDIVIDU, '')		= COALESCE(@PRENOM_INDIVIDU, PRENOM_INDIVIDU, '')
         		AND		COALESCE(BLN_MASCULIN, '')			= COALESCE(@BLN_MASCULIN, BLN_MASCULIN, '')
         		AND		COALESCE(DAT_NAISSANCE, GETDATE())	= COALESCE(@DAT_NAISSANCE, DAT_NAISSANCE, GETDATE())
         
         	END
         	ELSE 
         	BEGIN
         		IF @ID_ETABLISSEMENT IS NOT NULL
         		BEGIN
         			-- Contr“le que les donnees transmises pour le stagiaire correspondent … un Salarie OPTIFORM de l'etablissement
         			SELECT TOP 1 @ID_INDIVIDU	= SALARIE.ID_INDIVIDU, 
         						 @ID_SALARIE	= SALARIE.ID_SALARIE
         			FROM INDIVIDU
         			INNER JOIN SALARIE ON INDIVIDU.ID_INDIVIDU = SALARIE.ID_INDIVIDU
         			WHERE	ID_ETABLISSEMENT					= @ID_ETABLISSEMENT
         			AND		COALESCE(NIR, '')					= COALESCE(@NIR_INDIVIDU, NIR, '')
         			AND		COALESCE(MATRICULE_SALARIE, '')		= COALESCE(@MATRICULE, MATRICULE_SALARIE, '')
         			AND		COALESCE(NOM_INDIVIDU, '')			= COALESCE(@NOM_INDIVIDU, NOM_INDIVIDU, '')
         			AND		COALESCE(PRENOM_INDIVIDU, '')		= COALESCE(@PRENOM_INDIVIDU, PRENOM_INDIVIDU, '')
         			AND DAT_NAISSANCE							= ISNULL(@DAT_NAISSANCE, DAT_NAISSANCE )
         			AND BLN_MASCULIN = CASE @BLN_MASCULIN  
         											WHEN  'O' THEN 1
         											WHEN  'N' THEN 0 
         											ELSE BLN_MASCULIN 
         								END
         			AND SALARIE.BLN_ACTIF = 1
         			ORDER BY SALARIE.BLN_SALARIE_REFERENCE DESC
         		END
         		
         		IF @ID_INDIVIDU IS NULL
         		BEGIN        
         			/* Salarie non trouv‚ : Rejet des lignes */        
         			UPDATE EDI_PEC_ST
         			SET BLN_REJET =  1 
         			WHERE	ID_LOT_IMPORT						= @ID_LOT_IMPORT
         			AND		NUM_SIRET							= @NUM_SIRET
         			AND		COALESCE(NIR_INDIVIDU, '')			= COALESCE(@NIR_INDIVIDU, NIR_INDIVIDU, '')
         			AND		COALESCE(MATRICULE, '')				= COALESCE(@MATRICULE, MATRICULE, '')
         			AND		COALESCE(NOM_INDIVIDU, '')			= COALESCE(@NOM_INDIVIDU, NOM_INDIVIDU, '')
         			AND		COALESCE(PRENOM_INDIVIDU, '')		= COALESCE(@PRENOM_INDIVIDU, PRENOM_INDIVIDU, '')
         			AND		BLN_MASCULIN						= ISNULL(@BLN_MASCULIN, BLN_MASCULIN)
         			AND		COALESCE(DAT_NAISSANCE, GETDATE())	= COALESCE(@DAT_NAISSANCE, DAT_NAISSANCE, GETDATE())
         
         			INSERT INTO #TMP01         
         			(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         			SELECT NUM_LIGNE, @ID_COLONNE, @NOM_INDIVIDU, 'Pas de correspondance entre les donn‚es du Stagiaire et un Salarie EDI' 
         			FROM EDI_PEC_ST
         			WHERE	ID_LOT_IMPORT						= @ID_LOT_IMPORT
         			AND		NUM_SIRET							= @NUM_SIRET
         			AND		COALESCE(NIR_INDIVIDU, '')			= COALESCE(@NIR_INDIVIDU, NIR_INDIVIDU, '')
         			AND		COALESCE(MATRICULE, '')				= COALESCE(@MATRICULE, MATRICULE, '')
         			AND		COALESCE(NOM_INDIVIDU, '')			= COALESCE(@NOM_INDIVIDU, NOM_INDIVIDU, '')
         			AND		COALESCE(PRENOM_INDIVIDU, '')		= COALESCE(@PRENOM_INDIVIDU, PRENOM_INDIVIDU, '')
         			AND		COALESCE(BLN_MASCULIN, '')			= COALESCE(@BLN_MASCULIN, BLN_MASCULIN, '')
         			AND		COALESCE(DAT_NAISSANCE, GETDATE())	= COALESCE(@DAT_NAISSANCE, DAT_NAISSANCE, GETDATE())
         
         		END
         		ELSE
         		BEGIN
         			UPDATE EDI_PEC_ST
         			SET		ID_INDIVIDU = @ID_INDIVIDU,
         					ID_SALARIE	= @ID_SALARIE
         			WHERE	ID_LOT_IMPORT						= @ID_LOT_IMPORT
         			AND		NUM_SIRET							= @NUM_SIRET
         			AND		COALESCE(NIR_INDIVIDU, '')			= COALESCE(@NIR_INDIVIDU, NIR_INDIVIDU, '')
         			AND		COALESCE(MATRICULE, '')				= COALESCE(@MATRICULE, MATRICULE, '')
         			AND		COALESCE(NOM_INDIVIDU, '')			= COALESCE(@NOM_INDIVIDU, NOM_INDIVIDU, '')
         			AND		COALESCE(PRENOM_INDIVIDU, '')		= COALESCE(@PRENOM_INDIVIDU, PRENOM_INDIVIDU, '')
         			AND		BLN_MASCULIN						= ISNULL(@BLN_MASCULIN, BLN_MASCULIN)
         			AND		COALESCE(DAT_NAISSANCE, GETDATE())	= COALESCE(@DAT_NAISSANCE, DAT_NAISSANCE, GETDATE())
         		END
         	END	
         
         	FETCH cu_ctl_salarie 
         	INTO @NIR_INDIVIDU, @NOM_INDIVIDU, @PRENOM_INDIVIDU, @DAT_NAISSANCE, @BLN_MASCULIN, @MATRICULE, @NUM_SIRET, @ID_ETABLISSEMENT
         
         	
         END
         CLOSE cu_ctl_salarie 
         DEALLOCATE cu_ctl_salarie 
         /* Fin Controle des Stagiaires*/
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  32',  GETDATE(),  'FIN CONTROLES Stagiaires ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         
         /* Controle des Tuteurs */
         DECLARE cu_ctl_tuteur CURSOR FOR
         SELECT distinct 	BLN_TUTEUR_INTERNE, NIR_TUTEUR, NOM_TUTEUR, PRENOM_TUTEUR, DAT_NAISSANCE_TUTEUR, MATRICULE_TUTEUR, NUM_SIRET_TUTEUR
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND
         (		LEN(COALESCE(NUM_SIRET_TUTEUR, '')) > 0 
         	OR	LEN(COALESCE(NIR_TUTEUR, '')) > 0 
         	OR	LEN(COALESCE(NIR_TUTEUR, '')) > 0 
         	OR	LEN(COALESCE(NOM_TUTEUR, '')) > 0 
         	OR	LEN(COALESCE(PRENOM_TUTEUR, '')) > 0 
         	OR	LEN(COALESCE(MATRICULE_TUTEUR, '')) > 0 
         	OR	DAT_NAISSANCE_TUTEUR IS NOT NULL 
         )
         
         OPEN cu_ctl_tuteur 
         FETCH cu_ctl_tuteur 
         INTO @BLN_TUTEUR_INTERNE, @NIR_TUTEUR, @NOM_TUTEUR, @PRENOM_TUTEUR, @DAT_NAISSANCE_TUTEUR, @MATRICULE_TUTEUR, @NUM_SIRET_TUTEUR
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	SET @ID_TUTEUR = NULL
         
         	IF @BLN_TUTEUR_INTERNE != 'O' 
         	BEGIN
         		SET @BLN_TUTEUR_INTERNE = 'N'
         	END
         	IF @NUM_SIRET_TUTEUR = ''						SET @NUM_SIRET_TUTEUR = NULL
         	IF @NIR_TUTEUR = ''								SET @NIR_TUTEUR = NULL
         	IF @NOM_TUTEUR = ''								SET @NOM_TUTEUR = NULL
         	IF @PRENOM_TUTEUR = ''							SET @PRENOM_TUTEUR = NULL
         	IF @MATRICULE_TUTEUR = ''						SET @MATRICULE_TUTEUR= NULL
         	
         	SELECT	@ID_COLONNE = ID_COLONNE 
         	FROM	EDI_IMPORT_COLONNE
         	WHERE	ID_TABLE = @ID_TABLE
         	AND		COD_COLONNE = 'NOM_TUTEUR'	
         
         	IF	(	@BLN_TUTEUR_INTERNE = 'O' 
         		AND 
         			(
         				LEN(ISNULL(@NIR_TUTEUR, '')) > 0
         			OR LEN(ISNULL(@NUM_SIRET_TUTEUR,''))	>0
         			OR LEN(ISNULL(@MATRICULE_TUTEUR ,''))	>0
         			OR LEN(ISNULL(@NOM_TUTEUR, '')) > 0
         			OR LEN(ISNULL(@PRENOM_TUTEUR, '')) > 0
         			OR @DAT_NAISSANCE_TUTEUR IS NOT NULL
         			) 
         		)
         	BEGIN
         		/*  Si le stagiaire est un tuteur interne, les autres champs du tuteur ne peuvent pas ˆtre renseign‚s*/        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT								= @ID_LOT_IMPORT
         		AND		NUM_SIRET									= @NUM_SIRET
         		AND		BLN_TUTEUR_INTERNE = 'O' 
         		AND 
         			(
         				LEN(ISNULL(NIR_TUTEUR, '')) > 0
         			OR LEN(ISNULL(@NUM_SIRET_TUTEUR,''))	>0
         			OR LEN(ISNULL(MATRICULE_TUTEUR ,''))	>0
         			OR LEN(ISNULL(NOM_TUTEUR, '')) > 0
         			OR LEN(ISNULL(PRENOM_TUTEUR, '')) > 0
         			OR DAT_NAISSANCE_TUTEUR IS NOT NULL
         			) 	
         					
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @NOM_TUTEUR, 'Si le stagiaire est un tuteur interne, les autres champs du tuteur ne peuvent pas ˆtre renseign‚s ' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT						= @ID_LOT_IMPORT
         		AND		NUM_SIRET							= @NUM_SIRET
         		AND		BLN_TUTEUR_INTERNE = 'O' 
         		AND 
         			(
         				LEN(ISNULL(NIR_TUTEUR, '')) > 0
         			OR LEN(ISNULL(@NUM_SIRET_TUTEUR,''))	>0
         			OR LEN(ISNULL(MATRICULE_TUTEUR ,''))	>0
         			OR LEN(ISNULL(NOM_TUTEUR, '')) > 0
         			OR LEN(ISNULL(PRENOM_TUTEUR, '')) > 0
         			OR DAT_NAISSANCE_TUTEUR IS NOT NULL
         			) 	
         	END
         	ELSE IF	(	
         			   LEN(ISNULL(@NUM_SIRET_TUTEUR,''))	=0
         			OR LEN(ISNULL(@MATRICULE_TUTEUR ,''))	=0
         			OR LEN(ISNULL(@NOM_TUTEUR, '')) = 0
         			OR LEN(ISNULL(@PRENOM_TUTEUR, '')) = 0
         			OR @DAT_NAISSANCE_TUTEUR IS NOT NULL
         			)-- champs permettant d'identifier le tuteur renseignees
         	BEGIN
         	
         			-- Les champs minimaux permettant d'identifier le Tuteur ne sont pas renseign‚s
         			UPDATE EDI_PEC_ST
         			SET BLN_REJET =  1 
         			WHERE	ID_LOT_IMPORT								= @ID_LOT_IMPORT
         			AND		NUM_SIRET									= @NUM_SIRET_TUTEUR
         			AND		COALESCE(NIR_TUTEUR, '')					= COALESCE(@NIR_TUTEUR, NIR_TUTEUR, '')
         			AND		COALESCE(NOM_TUTEUR, '')					= COALESCE(@NOM_TUTEUR, NOM_TUTEUR, '')
         			AND		COALESCE(PRENOM_TUTEUR, '')					= COALESCE(@PRENOM_TUTEUR, PRENOM_TUTEUR, '')
         			AND		COALESCE(DAT_NAISSANCE_TUTEUR, GETDATE())	= COALESCE(@DAT_NAISSANCE_TUTEUR, DAT_NAISSANCE_TUTEUR, GETDATE())
         			AND		COALESCE(MATRICULE_TUTEUR, '')				= COALESCE(@MATRICULE_TUTEUR, MATRICULE_TUTEUR, '')
         
         			INSERT INTO #TMP01         
         			(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         			SELECT NUM_LIGNE, @ID_COLONNE, @NOM_TUTEUR, 'Donn‚es minimales permettant d identifier le tuteur non renseignees' 
         			FROM EDI_PEC_ST
         			WHERE	ID_LOT_IMPORT								= @ID_LOT_IMPORT
         			AND		NUM_SIRET_TUTEUR							= @NUM_SIRET_TUTEUR
         			AND		COALESCE(NIR_TUTEUR, '')					= COALESCE(@NIR_TUTEUR, NIR_TUTEUR, '')
         			AND		COALESCE(NOM_TUTEUR, '')					= COALESCE(@NOM_TUTEUR, NOM_TUTEUR, '')
         			AND		COALESCE(PRENOM_TUTEUR, '')					= COALESCE(@PRENOM_TUTEUR, PRENOM_TUTEUR, '')
         			AND		COALESCE(DAT_NAISSANCE_TUTEUR, GETDATE())	= COALESCE(@DAT_NAISSANCE_TUTEUR, DAT_NAISSANCE_TUTEUR, GETDATE())
         			AND		COALESCE(MATRICULE_TUTEUR, '')				= COALESCE(@MATRICULE_TUTEUR, MATRICULE_TUTEUR, '')		
         	END
         	ELSE
         	BEGIN
         	-- Donnees associees au tuteur interne renseignes
         			
         		-- Contr“le que les donnees transmises pour le Tuteur correspondent … un Salarie de l'etablissement
         		SELECT	@ID_ETABLISSEMENT = ID_ETABLISSEMENT
         		FROM	ETABLISSEMENT
         		WHERE	NUM_SIRET = @NUM_SIRET_TUTEUR	
         		
         		IF @ID_ETABLISSEMENT IS NOT NULL
         		BEGIN
         			SELECT TOP 1 @ID_TUTEUR			= SALARIE.ID_INDIVIDU,
         						 @ID_SALARIE_TUTEUR	= SALARIE.ID_SALARIE
         			FROM INDIVIDU
         			INNER JOIN SALARIE ON INDIVIDU.ID_INDIVIDU = SALARIE.ID_INDIVIDU
         			WHERE	ID_ETABLISSEMENT					= @ID_ETABLISSEMENT
         			AND		COALESCE(MATRICULE_SALARIE, '')		= COALESCE(@MATRICULE_TUTEUR, MATRICULE_SALARIE, '')
         			AND 	COALESCE(NIR, '')					= COALESCE(@NIR_TUTEUR, NIR, '')
         			AND		COALESCE(NOM_INDIVIDU, '')			= COALESCE(@NOM_TUTEUR, NOM_INDIVIDU, '')
         			AND		COALESCE(PRENOM_INDIVIDU, '')		= COALESCE(@PRENOM_TUTEUR, PRENOM_INDIVIDU, '')
         			AND		DAT_NAISSANCE						= ISNULL(@DAT_NAISSANCE_TUTEUR, DAT_NAISSANCE)
         			AND SALARIE.BLN_ACTIF = 1
         			ORDER BY SALARIE.BLN_SALARIE_REFERENCE DESC
         		END
         					
         		IF @ID_TUTEUR IS NULL
         		BEGIN        
         			/* Tuteur non trouv‚ : Rejet des lignes */        
         			UPDATE EDI_PEC_ST
         			SET BLN_REJET =  1 
         			WHERE	ID_LOT_IMPORT								= @ID_LOT_IMPORT
         			AND		NUM_SIRET_TUTEUR							= @NUM_SIRET_TUTEUR
         			AND		COALESCE(NIR_TUTEUR, '')					= COALESCE(@NIR_TUTEUR, NIR_TUTEUR, '')
         			AND		COALESCE(NOM_TUTEUR, '')					= COALESCE(@NOM_TUTEUR, NOM_TUTEUR, '')
         			AND		COALESCE(PRENOM_TUTEUR, '')					= COALESCE(@PRENOM_TUTEUR, PRENOM_TUTEUR, '')
         			AND		COALESCE(DAT_NAISSANCE_TUTEUR, GETDATE())	= COALESCE(@DAT_NAISSANCE_TUTEUR, DAT_NAISSANCE_TUTEUR, GETDATE())
         			AND		COALESCE(MATRICULE_TUTEUR, '')				= COALESCE(@MATRICULE_TUTEUR, MATRICULE_TUTEUR, '')
         
         			INSERT INTO #TMP01         
         			(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         			SELECT NUM_LIGNE, @ID_COLONNE, @NOM_TUTEUR, 'Pas de correspondance entre les donn‚es du Tuteur et un Salarie EDI' 
         			FROM EDI_PEC_ST
         			WHERE	ID_LOT_IMPORT								= @ID_LOT_IMPORT
         			AND		NUM_SIRET_TUTEUR							= @NUM_SIRET_TUTEUR
         			AND		COALESCE(NIR_TUTEUR, '')					= COALESCE(@NIR_TUTEUR, NIR_TUTEUR, '')
         			AND		COALESCE(NOM_TUTEUR, '')					= COALESCE(@NOM_TUTEUR, NOM_TUTEUR, '')
         			AND		COALESCE(PRENOM_TUTEUR, '')					= COALESCE(@PRENOM_TUTEUR, PRENOM_TUTEUR, '')
         			AND		COALESCE(DAT_NAISSANCE_TUTEUR, GETDATE())	= COALESCE(@DAT_NAISSANCE_TUTEUR, DAT_NAISSANCE_TUTEUR, GETDATE())
         			AND		COALESCE(MATRICULE_TUTEUR, '')				= COALESCE(@MATRICULE_TUTEUR, MATRICULE_TUTEUR, '')
         		END
         		ELSE
         		BEGIN
         			UPDATE EDI_PEC_ST
         			SET		ID_TUTEUR			= @ID_TUTEUR ,
         					ID_SALARIE_TUTEUR	= @ID_SALARIE_TUTEUR 
         			WHERE	ID_LOT_IMPORT								= @ID_LOT_IMPORT
         			AND		NUM_SIRET_TUTEUR							= @NUM_SIRET_TUTEUR
         			AND		COALESCE(NIR_TUTEUR, '')					= COALESCE(@NIR_TUTEUR, NIR_TUTEUR, '')
         			AND		COALESCE(NOM_TUTEUR, '')					= COALESCE(@NOM_TUTEUR, NOM_TUTEUR, '')
         			AND		COALESCE(PRENOM_TUTEUR, '')					= COALESCE(@PRENOM_TUTEUR, PRENOM_TUTEUR, '')
         			AND		COALESCE(DAT_NAISSANCE_TUTEUR, GETDATE())	= COALESCE(@DAT_NAISSANCE_TUTEUR, DAT_NAISSANCE_TUTEUR, GETDATE())
         			AND		COALESCE(MATRICULE_TUTEUR, '')				= COALESCE(@MATRICULE_TUTEUR, MATRICULE_TUTEUR, '')			
         		END
         		
         	END	
         
         	FETCH cu_ctl_tuteur 
         	INTO @BLN_TUTEUR_INTERNE, @NIR_TUTEUR, @NOM_TUTEUR, @PRENOM_TUTEUR, @DAT_NAISSANCE_TUTEUR, @MATRICULE_TUTEUR, @NUM_SIRET_TUTEUR
         
         END
         CLOSE cu_ctl_tuteur 
         DEALLOCATE cu_ctl_tuteur 
         /* Fin Controle des Tuteurs*/
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  33A',  GETDATE(),  'FIN CONTROLES Tuteurs ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         
         
         /* Controle des Formateurs Internes*/
         DECLARE cu_ctl_formateur_interne CURSOR FOR
         SELECT distinct NIR_FORM_INT, NOM_FORM_INT, PRENOM_FORM_INT, DAT_NAISS_FORM_INT, MATRICULE_FORM_INT, NUM_SIRET_FORM_INT
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND     BLN_EXTERNE = 'N'
         
         OPEN cu_ctl_formateur_interne 
         FETCH cu_ctl_formateur_interne 
         INTO @NIR_FORM_INT, @NOM_FORM_INT, @PRENOM_FORM_INT, @DAT_NAISS_FORM_INT, @MATRICULE_FORM_INT, @NUM_SIRET_FORM_INT
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         
         	SET @ID_ETABLISSEMENT	= NULL
         	SET @ID_INDIVIDU		= NULL
         
         	IF @NIR_FORM_INT = ''		SET @NIR_FORM_INT		= NULL
         	IF @NOM_FORM_INT = ''		SET @NOM_FORM_INT		= NULL
         	IF @PRENOM_FORM_INT = ''	SET @PRENOM_FORM_INT	= NULL
         	IF @MATRICULE_FORM_INT = ''	SET @MATRICULE_FORM_INT = NULL
         	IF @NUM_SIRET_FORM_INT = ''	SET @NUM_SIRET_FORM_INT = NULL
         	
         	SELECT	@ID_COLONNE = ID_COLONNE
         	FROM	EDI_IMPORT_COLONNE
         	WHERE	ID_TABLE = @ID_TABLE
         	AND		COD_COLONNE = 'NOM_FORM_INT'
         	
         	-- Contr“le des donn‚es obligatoires
         	IF	(	
         			(	
         				LEN(ISNULL(@NUM_SIRET_FORM_INT, '')) = 0
         			--OR	LEN(ISNULL(@MATRICULE_FORM_INT ,''))	= 0			
         			OR	LEN(ISNULL(@NOM_FORM_INT, '')) = 0
         			OR	LEN(ISNULL(@PRENOM_FORM_INT, '')) = 0
         			OR	ISNULL(@DAT_NAISS_FORM_INT, GETDATE()) = GETDATE()
         			) 
         		)-- champs permettant d'identifier le salarie non renseignees
         	BEGIN
         	
         		/* Donnee Obligatoires du Salarie non transmise: Rejet des lignes */        
         		UPDATE EDI_PEC_ST
         		SET BLN_REJET =  1 
         		WHERE	ID_LOT_IMPORT							= @ID_LOT_IMPORT
         		AND		COALESCE(NUM_SIRET_FORM_INT, '')		= COALESCE(@NUM_SIRET_FORM_INT, '')
         		AND		COALESCE(NIR_FORM_INT, '')				= COALESCE(@NIR_FORM_INT, NIR_FORM_INT, '')
         		AND		COALESCE(NOM_FORM_INT, '')				= COALESCE(@NOM_FORM_INT, NOM_FORM_INT, '')
         		AND		COALESCE(PRENOM_FORM_INT, '')			= COALESCE(@PRENOM_FORM_INT, PRENOM_FORM_INT, '')
         		AND		COALESCE(DAT_NAISS_FORM_INT, GETDATE())	= COALESCE(@DAT_NAISS_FORM_INT, DAT_NAISS_FORM_INT, GETDATE())
         		AND		COALESCE(MATRICULE_FORM_INT, '')		= COALESCE(@MATRICULE_FORM_INT, MATRICULE_FORM_INT, '')
         		
         		INSERT INTO #TMP01         
         		(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         		SELECT NUM_LIGNE, @ID_COLONNE, @NOM_FORM_INT, 'Donn‚es obligatoires associees au Formateur Interne non renseignees' 
         		FROM EDI_PEC_ST
         		WHERE	ID_LOT_IMPORT							= @ID_LOT_IMPORT
         		AND		COALESCE(NUM_SIRET_FORM_INT, '')		= COALESCE(@NUM_SIRET_FORM_INT, '')
         		AND		COALESCE(NIR_FORM_INT, '')				= COALESCE(@NIR_FORM_INT, NIR_FORM_INT, '')
         		AND		COALESCE(NOM_FORM_INT, '')				= COALESCE(@NOM_FORM_INT, NOM_FORM_INT, '')
         		AND		COALESCE(PRENOM_FORM_INT, '')			= COALESCE(@PRENOM_FORM_INT, PRENOM_FORM_INT, '')
         		AND		COALESCE(DAT_NAISS_FORM_INT, GETDATE())	= COALESCE(@DAT_NAISS_FORM_INT, DAT_NAISS_FORM_INT, GETDATE())
         		AND		COALESCE(MATRICULE_FORM_INT, '')		= COALESCE(@MATRICULE_FORM_INT, MATRICULE_FORM_INT, '')
         
         	END
         	ELSE 
         	BEGIN
         		SELECT	@ID_ETABLISSEMENT = ID_ETABLISSEMENT
         		FROM	ETABLISSEMENT
         		WHERE	NUM_SIRET = @NUM_SIRET_FORM_INT	
         
         		IF @ID_ETABLISSEMENT IS NOT NULL
         		BEGIN
         			-- Contr“le que les donnees transmises pour le stagiaire correspondent … un Salarie OPTIFORM de l'etablissement
         			SELECT TOP 1 @ID_SALARIE_FORMATEUR_INTERNE	= SALARIE.ID_SALARIE
         			FROM INDIVIDU
         			INNER JOIN SALARIE ON INDIVIDU.ID_INDIVIDU = SALARIE.ID_INDIVIDU
         			WHERE	ID_ETABLISSEMENT					= @ID_ETABLISSEMENT
         			AND		COALESCE(NIR, '')					= COALESCE(@NIR_FORM_INT, NIR, '')
         			AND		COALESCE(MATRICULE_SALARIE, '')		= COALESCE(@MATRICULE_FORM_INT, MATRICULE_SALARIE, '')
         			AND		COALESCE(NOM_INDIVIDU, '')			= COALESCE(@NOM_FORM_INT, NOM_INDIVIDU, '')
         			AND		COALESCE(PRENOM_INDIVIDU, '')		= COALESCE(@PRENOM_FORM_INT, PRENOM_INDIVIDU, '')
         			AND DAT_NAISSANCE							= ISNULL(@DAT_NAISS_FORM_INT, DAT_NAISSANCE )
         			AND SALARIE.BLN_ACTIF = 1
         			ORDER BY SALARIE.BLN_SALARIE_REFERENCE DESC
         		END
         		
         		IF @ID_SALARIE_FORMATEUR_INTERNE IS NULL
         		BEGIN        
         			/* Salarie non trouv‚ pour le formatteur interne : Rejet des lignes */        
         			UPDATE EDI_PEC_ST
         			SET BLN_REJET =  1 
         			WHERE	ID_LOT_IMPORT						= @ID_LOT_IMPORT
         			AND		COALESCE(NUM_SIRET_FORM_INT, '')	= COALESCE(@NUM_SIRET_FORM_INT, '')
         			AND		COALESCE(NIR_FORM_INT, '')			= COALESCE(@NIR_FORM_INT, NIR_FORM_INT, '')
         			AND		COALESCE(MATRICULE_FORM_INT, '')	= COALESCE(@MATRICULE_FORM_INT, MATRICULE_FORM_INT, '')
         			AND		COALESCE(NOM_FORM_INT, '')			= COALESCE(@NOM_FORM_INT, NOM_FORM_INT, '')
         			AND		COALESCE(PRENOM_FORM_INT, '')		= COALESCE(@PRENOM_FORM_INT, PRENOM_FORM_INT, '')
         			AND		DAT_NAISSANCE						= ISNULL(@DAT_NAISS_FORM_INT, DAT_NAISSANCE )
         
         			INSERT INTO #TMP01         
         			(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         			SELECT NUM_LIGNE, @ID_COLONNE, @NOM_FORM_INT, 'Pas de correspondance entre les donn‚es du Formateur Interne et un Salarie EDI' 
         			FROM EDI_PEC_ST
         			WHERE	ID_LOT_IMPORT						= @ID_LOT_IMPORT
         			AND		COALESCE(NUM_SIRET_FORM_INT, '')	= COALESCE(@NUM_SIRET_FORM_INT, '')
         			AND		COALESCE(NIR_FORM_INT, '')			= COALESCE(@NIR_FORM_INT, NIR_FORM_INT, '')
         			AND		COALESCE(MATRICULE_FORM_INT, '')	= COALESCE(@MATRICULE_FORM_INT, MATRICULE_FORM_INT, '')
         			AND		COALESCE(NOM_FORM_INT, '')			= COALESCE(@NOM_FORM_INT, NOM_FORM_INT, '')
         			AND		COALESCE(PRENOM_FORM_INT, '')		= COALESCE(@PRENOM_FORM_INT, PRENOM_FORM_INT, '')
         			AND		DAT_NAISS_FORM_INT					= ISNULL(@DAT_NAISS_FORM_INT, DAT_NAISS_FORM_INT)
         
         		END
         		ELSE
         		BEGIN
         			UPDATE EDI_PEC_ST
         			SET		ID_SALARIE_FORMATEUR_INTERNE		= @ID_SALARIE_FORMATEUR_INTERNE
         			WHERE	ID_LOT_IMPORT						= @ID_LOT_IMPORT
         			AND		COALESCE(NUM_SIRET_FORM_INT, '')	= COALESCE(@NUM_SIRET_FORM_INT, '')
         			AND		COALESCE(NIR_FORM_INT, '')			= COALESCE(@NIR_FORM_INT, NIR_FORM_INT, '')
         			AND		COALESCE(MATRICULE_FORM_INT, '')	= COALESCE(@MATRICULE_FORM_INT, MATRICULE_FORM_INT, '')
         			AND		COALESCE(NOM_FORM_INT, '')			= COALESCE(@NOM_FORM_INT, NOM_FORM_INT, '')
         			AND		COALESCE(PRENOM_FORM_INT, '')		= COALESCE(@PRENOM_FORM_INT, PRENOM_FORM_INT, '')
         			AND		DAT_NAISS_FORM_INT					= ISNULL(@DAT_NAISS_FORM_INT, DAT_NAISS_FORM_INT)
         			AND     BLN_EXTERNE = 'N'
         		END
         	END	
         
         	FETCH cu_ctl_formateur_interne 
         	INTO @NIR_FORM_INT, @NOM_FORM_INT, @PRENOM_FORM_INT, @DAT_NAISS_FORM_INT, @MATRICULE_FORM_INT, @NUM_SIRET_FORM_INT
         
         	
         END
         CLOSE cu_ctl_formateur_interne 
         DEALLOCATE cu_ctl_formateur_interne 
         
         -- Rejet des lignes avec formation interne sans formateur interne renseigne
         UPDATE EDI_PEC_ST
         SET BLN_REJET =  1 
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		BLN_EXTERNE  = 'N'
         AND		ID_SALARIE_FORMATEUR_INTERNE IS NULL
         
         /* Fin Controle des Stagiaires*/
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  33B',  GETDATE(),  'FIN CONTROLES Formateurs Internes ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Rejets des modules pour lesquels au moins une ligne a ‚t‚ rejet‚e*/
         /*
         Demande SANOFI : Pas de rejet de l'ensemble des salaries si 1 des salaries du module a ete rejete
         */
         --DECLARE cu_ctl_coherence_module CURSOR FOR
         --SELECT	LIBL_ACTION_PEC				,
         --		NUM_INTERNE_ACTION			,
         --		NUM_INTERNE		
         --FROM	EDI_PEC_ST
         --WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         --GROUP BY LIBL_ACTION_PEC			,
         --		NUM_INTERNE_ACTION			,
         --		NUM_INTERNE			
         --HAVING	MIN(BLN_REJET)	!=	MAX(BLN_REJET)
         
         --SELECT	@ID_COLONNE_MODULE = ID_COLONNE 
         --FROM	EDI_IMPORT_COLONNE
         --WHERE	ID_TABLE = @ID_TABLE
         --AND		COD_COLONNE = 'NUM_INTERNE'
         
         --OPEN cu_ctl_coherence_module 
         
         --FETCH cu_ctl_coherence_module INTO 
         --		@LIBL_ACTION_PEC		,
         --		@NUM_INTERNE_ACTION		,
         --		@NUM_INTERNE		
         
         --WHILE (@@FETCH_STATUS <> -1)
         --BEGIN
         
         --	INSERT INTO #TMP01         
         --	(NUM_LIGNE, ID_COLONNE, VAL_COLONNE, LIB_PROBLEME)        
         --	SELECT NUM_LIGNE, @ID_COLONNE_MODULE, @NUM_INTERNE_ACTION + '-' + @LIBL_ACTION_PEC + '-' + @NUM_INTERNE, 'Rejet Ligne car probleme detecte sur une autre ligne du module'
         --	FROM EDI_PEC_ST
         --	WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         --	AND		LIBL_ACTION_PEC		= @LIBL_ACTION_PEC		
         --	AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         --	AND		NUM_INTERNE			= @NUM_INTERNE
         --	AND		BLN_REJET			= 0
         
         --	UPDATE EDI_PEC_ST
         --	SET BLN_REJET =  1 
         --	WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         --	AND		LIBL_ACTION_PEC		= @LIBL_ACTION_PEC		
         --	AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION
         --	AND		NUM_INTERNE			= @NUM_INTERNE
         --	AND		BLN_REJET			= 0
         
         
         --	FETCH cu_ctl_coherence_module INTO 
         --		@LIBL_ACTION_PEC		,
         --		@NUM_INTERNE_ACTION		,
         --		@NUM_INTERNE		
         --END			
         --CLOSE cu_ctl_coherence_module 
         --DEALLOCATE cu_ctl_coherence_module 
         /* Fin Rejets des modules pour lesquels au mins une ligne a ‚t‚ rejet‚*/
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  34',  GETDATE(),  'FIN Rejets des modules pour lesquels au moins une ligne a ‚t‚ rejet‚e ', 'NB REJETS APRES' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         UPDATE	EDI_PEC_ST
         SET		DAT_DEBUT = DAT_DEB_ACTION_PEC
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		DAT_DEBUT IS NULL
         
         UPDATE	EDI_PEC_ST
         SET		DAT_FIN = DAT_FIN_ACTION_PEC
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		DAT_FIN IS NULL
         
         UPDATE	EDI_PEC_ST
         SET		COD_THEME = COD_THEME_GLOBAL,
         		ID_THEME_MODULE = ID_THEME_GLOBAL
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		LEN(RTRIM(LTRIM(COD_THEME)))= 0
         
         UPDATE	EDI_PEC_ST
         SET		NUM_DUREE_H_MODULE = NUM_DUREE_HEURE
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		NUM_DUREE_H_MODULE IS NULL
         
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  34', GETDATE(),   'DEBUT TRAITEMENT DES ACTIONS NON REJETES', 'NB REJETS ' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         
         /* Traitements des actions PEC non rejet‚es */
         DECLARE cu_action_pec scroll cursor for
         SELECT  DISTINCT
         	ID_ACTION_PEC			,
         	ID_THEME_GLOBAL			,
         	ID_NIVEAU_ACTION		,
         	ID_SANCTION				,
         	ID_FORMACODE			,
         	LIBL_ACTION_PEC			,
         	DAT_DEB_ACTION_PEC		,
         	DAT_FIN_ACTION_PEC		,
         	NUM_DUREE_HEURE			,
         	AXE_ACTION				,
         	DOMAINE_ACTION			,
         	NUM_INTERNE_ACTION		,
         	NUM_SIRET_CONTACT		,
         	LIB_NOM_CONTACT			,
         	LIB_PNM_CONTACT			,
         	EMAIL_PRO_CONTACT		,
         	NUM_TEL_CONTACT			,
         	COD_CIVILITE			,
         	BLN_REJET        
         
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		ID_ACTION_PEC IS NULL				-- Action non crees
         --AND		(NUM_LIGNE >= @NUM_LIGNE_DEBUT OR @NUM_LIGNE_DEBUT IS NULL)
         --AND		(NUM_LIGNE < @NUM_LIGNE_FIN OR @NUM_LIGNE_FIN IS NULL)
         AND		ISNULL(BLN_REJET, 0) = 0					
                 
                 
         OPEN  cu_action_pec        
         
         
         FETCH cu_action_pec INTO        
         	@ID_ACTION_PEC			,
         	@ID_THEME_ACTION		,
         	@ID_NIVEAU				,
         	@ID_SANCTION			,
         	@ID_FORMACODE			,
         	@LIBL_ACTION_PEC		,
         	@DAT_DEB_ACTION_PEC		,
         	@DAT_FIN_ACTION_PEC		,
         	@NUM_DUREE_HEURE		,
         	@AXE_ACTION				,
         	@DOMAINE_ACTION			,
         	@NUM_INTERNE_ACTION		,
         	@NUM_SIRET_CONTACT		,
         	@LIB_NOM_CONTACT		,
         	@LIB_PNM_CONTACT		,
         	@EMAIL_PRO_CONTACT		,
         	@NUM_TEL_CONTACT		,
         	@COD_CIVILITE			,
         	@BLN_REJET        
         
         	
         WHILE (@@fetch_status <> -1)
         BEGIN         
         	
         	SET @NUM_DUREE_JOUR = CAST(@NUM_DUREE_HEURE/7 AS DECIMAL(15,1))
         
         	SELECT	@ID_AGENCE		= ID_AGENCE, @ID_CHARGEE_MISSION = ID_CHARGEE_MISSION
         	FROM	ETABLISSEMENT
         	WHERE	ID_ETABLISSEMENT = @ID_ETABLISSEMENT_CREATEUR
         
         	SET @DAT_RECU = GETDATE()
         	SET @BLN_OK_ENGAGEMENT = 0
         	
         	IF @ID_NIVEAU IS NULL
         	BEGIN
         		SELECT	@ID_NIVEAU = ID_NIVEAU FROM NIVEAU 
         		WHERE	COD_NIVEAU = 'ND'
         	END
         
         	IF @ID_ACTION_PEC IS NULL
         	BEGIN
         	
         		SET @CODE_ACTION = NULL
         		
         		EXEC @ID_ACTION_PEC = 
         			[INS_ACTION_PRISES]  
         			@LIBL_ACTION_PEC ,  
         			@ID_THEME_ACTION ,  
         			@ID_NIVEAU ,  
         			@ID_SANCTION ,  
         			NULL, --@ID_OPERATION 
         			@DAT_DEB_ACTION_PEC ,  
         			@DAT_FIN_ACTION_PEC ,  
         			1,	-- @CIBLE_ACTION
         			1,  -- @BLN_ACTIVE
         			@ID_UTILISATEUR_ADMIN_EDI,  
         			@NUM_DUREE_JOUR ,  
         			@NUM_DUREE_HEURE ,  
         			@ID_AGENCE ,  
         			0, --@CICLE_COURT ,  
         			'Import EDI' , --@COM_ACTION ,  
         			@ID_ACTION_PEC output,  
         			@ID_FORMACODE,  
         			@DAT_RECU,  
         			NULL, --@ID_DECISION_ACTION_PEC ,  
         			@AXE_ACTION,  
         			@DOMAINE_ACTION,   
         			@ID_UTILISATEUR_ADMIN_EDI,  
         			@CODE_ACTION output,  
         			@ID_CHARGEE_MISSION,
         	 		NULL, --@NUM_ACTION 
         			NULL --@MILLESIME 
         
         		 IF @ID_ACTION_PEC IS NOT NULL
         		 BEGIN
         		 
         			UPDATE EDI_PEC_ST
         			SET		ID_ACTION_PEC		= @ID_ACTION_PEC
         			WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         			AND		NUM_INTERNE_ACTION	= @NUM_INTERNE_ACTION	
         			AND		ID_ACTION_PEC IS NULL
         				
         		 END
         		 
         		 -- Rajout du contact dans les commentaires de l'action
         		 IF @ID_ACTION_PEC IS NOT NULL
         		 BEGIN
         
         			IF LEN(LTRIM(RTRIM(@LIB_NOM_CONTACT))) > 0
         			BEGIN
         				
         				SET @COMMENTAIRE = 'EDI Nø:' + CAST(@CODE_ACTION AS VARCHAR)
         				+ ' ' + @COD_CIVILITE + ' ' + @LIB_NOM_CONTACT + ' ' + @LIB_PNM_CONTACT 
         				+ ' - ' + @EMAIL_PRO_CONTACT + ' - ' + @NUM_TEL_CONTACT 
         				+ ' - ' + 'SIRET =' + @NUM_SIRET_CONTACT		
         				
         				EXEC INS_COMMENTAIRES
         				6 ,	
         				@ID_ACTION_PEC ,
         				@COMMENTAIRE, --EOU 13144
         				@ID_UTILISATEUR_ADMIN_EDI 
         
         			END	 
         
         		 END
         		 
         		 
         	END
         
         	FETCH cu_action_pec INTO        
         		@ID_ACTION_PEC			,
         		@ID_THEME_ACTION		,
         		@ID_NIVEAU				,
         		@ID_SANCTION			,
         		@ID_FORMACODE			,
         		@LIBL_ACTION_PEC		,
         		@DAT_DEB_ACTION_PEC		,
         		@DAT_FIN_ACTION_PEC		,
         		@NUM_DUREE_HEURE		,
         		@AXE_ACTION				,
         		@DOMAINE_ACTION			,
         		@NUM_INTERNE_ACTION		,
         		@NUM_SIRET_CONTACT		,
         		@LIB_NOM_CONTACT		,
         		@LIB_PNM_CONTACT		,
         		@EMAIL_PRO_CONTACT		,
         		@NUM_TEL_CONTACT		,
         		@COD_CIVILITE			,
         		@BLN_REJET        
         END        
                 
         CLOSE cu_action_pec        
         DEALLOCATE cu_action_pec        
         /* Fin Traitements des actions PEC non rejet‚es */
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  35',  GETDATE(),  'FIN TRAITEMENT DES ACTIONS NON REJETES', 'NB REJETS ' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Traitements des modules PEC non rejet‚es */
         DECLARE cu_module_pec scroll cursor FOR
         SELECT  DISTINCT
         	ID_ACTION_PEC			,
         	ID_MODULE_PEC			,
         	LIBL_MODULE_PEC			,
         	DAT_DEBUT				,
         	DAT_FIN					,
         	ID_THEME_MODULE			,
         	COD_INITIATIVE			,
         	NUM_DUREE_H_MODULE		,
         	MNT_CONVENTION			,
         	ID_DEPART_FORMATION		,
         	BLN_DELEG_PAIEMENT		,
         	ID_ETABLISSEMENT_OF		,
         	NUM_SIRET_OF			,
         	LIBL_OF					,
         	NUM_INTERNE				,
         	AXE_MODULE				,
         	DOMAINE_MODULE			,
         	BLN_EXTERNE				,
         	BLN_INTRA				= CASE WHEN BLN_INTRA = 'O' THEN 1 ELSE 0 END,
         	ID_SALARIE_FORMATEUR_INTERNE,
         	COM_MODULE				,
         	BLN_REJET        
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		ID_ACTION_PEC IS NOT NULL
         AND		ISNULL(BLN_REJET, 0) = 0			
         FOR UPDATE
         		
         OPEN  cu_module_pec
         FETCH cu_module_pec INTO 
         	@ID_ACTION_PEC			,
         	@ID_MODULE_PEC			,
         	@LIBL_MODULE_PEC		,
         	@DAT_DEBUT				,
         	@DAT_FIN				,
         	@ID_THEME_MODULE		,
         	@COD_INITIATIVE			,
         	@NUM_DUREE_H_MODULE		,
         	@MNT_CONVENTION			,
         	@ID_DEPART_FORMATION	,
         	@BLN_DELEG_PAIEMENT		,
         	@ID_ETABLISSEMENT_OF	,
         	@NUM_SIRET_OF			,
         	@LIBL_OF				,
         	@NUM_INTERNE			,
         	@AXE_MODULE				,
         	@DOMAINE_MODULE			,
         	@BLN_EXTERNE			,
         	@BLN_INTRA				,
         	@ID_SALARIE_FORMATEUR_INTERNE,
         	@COM_MODULE				,
         	@BLN_REJET				
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         	SET @ID_ETABLISSEMENT_OF = NULL
         	SET @ID_OF = NULL
         	SET @BLN_CREATION_OF = 0
         	SET @ID_ADRESSE = NULL
         	SET @ID_CONTACT = NULL
         
         	SET @BLN_MODULE_EXTERNE = CASE WHEN @BLN_EXTERNE = 'O' THEN 1 ELSE 0 END
         
         	IF @BLN_MODULE_EXTERNE  = 1
         	BEGIN
         		-- Valorisation de l'etablissement OF
         		IF @NUM_SIRET_OF = '10000000000001' -- SIRET Etranger
         		BEGIN
         		
         			SET @NUM_SIRET_OF = '99999999900099'
         		
         			-- OF etranger - Recherche  sur la Raison sociale
         			SELECT	TOP 1 @ID_ETABLISSEMENT_OF = ID_ETABLISSEMENT_OF
         			FROM	ETABLISSEMENT_OF
         			INNER JOIN ORGANISME_FORMATION ON ORGANISME_FORMATION.ID_OF = ETABLISSEMENT_OF.ID_OF 
         			WHERE	ETABLISSEMENT_OF.NUM_SIRET = @NUM_SIRET_OF
         			AND		ORGANISME_FORMATION .LIB_RAISON_SOCIALE = @LIBL_OF
         			ORDER BY ETABLISSEMENT_OF.BLN_ACTIF DESC
         			
         		END
         		ELSE
         		BEGIN
         		
         			-- OF non etranger - Recherche sur le SIRET
         			SELECT	TOP 1 @ID_ETABLISSEMENT_OF = ID_ETABLISSEMENT_OF
         			FROM	ETABLISSEMENT_OF
         			WHERE	NUM_SIRET = @NUM_SIRET_OF
         			ORDER BY BLN_ACTIF DESC
         		
         		END
         		IF @BLN_DEBUG > 0
         		SELECT 'DEBUG  35 a',  NUM_SIRET_OF=@NUM_SIRET_OF, LIBL_OF=@LIBL_OF, ID_ETABLISSEMENT_OF=@ID_ETABLISSEMENT_OF
         		
         		IF @ID_ETABLISSEMENT_OF IS NULL
         		-- Le SIRET OF transmis ne correspond pas a un etablissement OF connu -- Creation d'un nouvel etablissement OF
         		BEGIN
         			IF @BLN_DEBUG > 0
         			SELECT 'DEBUG  35 b',  @NUM_SIRET_OF, @LIBL_OF, ' A creer'
         			
         			SET @NUM_SIREN	= LEFT(@NUM_SIRET_OF, 9)
         			SET @LIBL_OF	= LEFT(@LIBL_OF, 64)
         			
         			IF @NUM_SIRET_OF != '99999999900099'
         			BEGIN
         				-- Recherche si un OF francais existe pour le SIRET transmis
         				SELECT	TOP 1 @ID_OF = ID_OF
         				FROM	ORGANISME_FORMATION 
         				WHERE	NUM_SIRET = @NUM_SIREN
         				ORDER BY BLN_ACTIF DESC
         			END
         					
         			IF @ID_OF IS NULL
         			-- Creation d'un nouvel_OF
         			BEGIN
         				EXEC @ID_OF = INS_ORGANISME_FORMATION
         											 @ID_ADRESSE_PRINCIPALE		= NULL,
         											 @ID_GROUPE_OF				= NULL ,
         											 @ID_NAF					= 168,					-- Organisme d''enseignement
         											 @ID_DOMAINE_OF				= 1,					-- Professionnel
         											 @ID_ETABLISSEMENT_OF_PRINCIPAL=NULL,
         											 @ID_MODE_PAIEMENT			= 2,					-- virement
         											 @ID_CONDITION_REGLEMENT	= 4,
         											 @ID_ETAT_SIRET				= 1,					-- En cours validation aupres du service
         											 @ID_STATUT_JURIDIQUE		= 11,					-- inconnu
         											 @ID_TYPE_ORGANISME			= 1 ,					-- Prive 
         											 @COD_OF					= NULL,
         											 @LIB_RAISON_SOCIALE		= @LIBL_OF,
         											 @LIB_SIGLE_OF				= @LIBL_OF,
         											 @LIB_NUM_DECLARATION		= 'ETRANGER',
         											 @BLN_ACTIF					= 0,
         											 @BLN_PB_OF					= 1,					-- il faut une intervention pour retirer le pb
         											 @LIB_CAUSE_PB_OF			= 'V‚rification de la Creation Automatique EDI',
         											 @COM_OF					= 'Creation Automatique EDI',
         											 @NUM_SIRET					= @NUM_SIREN,
         											 @ID_UTILISATEUR			= @ID_UTILISATEUR_ADMIN_EDI,
         											 @BLN_GESTION_GROUPE		= 0,
         											 @ID_UTILISATEUR_CREATEUR	= @ID_UTILISATEUR_ADMIN_EDI,
         											 @DAT_SITUATION_ECONOMIQUE	= null,
         											 @LIB_TIERS_MANDATAIRE		= null
         											 
         				SET @BLN_CREATION_OF = 1
         											 
         			END
         						 
         			IF @ID_OF IS NOT NULL
         			BEGIN
         				-- SI OF cree, l'etablissement OF cree est l'etablissement principal
         				SET @BLN_ETABLISSEMENT_PRINCIPAL =  @BLN_CREATION_OF 
         
         				EXEC @ID_ETABLISSEMENT_OF = INS_ETABLISSEMENT_OF
         									@ID_ADRESSE_PRINCIPALE		= NULL,
         									@ID_OF						= @ID_OF,  
         									@ID_ETAT_SIRET				= 1,			-- En cours validation aupres du service
         									@ID_TYPE_TVA				= 1,			-- TVA Normal  
         									@COD_ETABLISSEMENT_OF		= NULL,  
         									@NUM_SIRET					= @NUM_SIRET_OF,
         									@BLN_ACTIF					= 0,
         									@BLN_PRINCIPAL				= @BLN_ETABLISSEMENT_PRINCIPAL,
         									@BLN_VALIDE					= 0, -- il faut une intervention pour le rendre valide
         									@COM_ETABLISSEMENT_OF		= 'Creation Automatique EDI',
         									@NUM_IBAN					= NULL,
         									@ID_UTILISATEUR				= @ID_UTILISATEUR_ADMIN_EDI,
         									@ID_UTILISATEUR_CREATEUR	= @ID_UTILISATEUR_ADMIN_EDI,
         									@LIB_ENSEIGNE				= @LIBL_OF,
         									@LIB_NUM_DECLARATION		= NULL
         			END
         			
         			IF @ID_ETABLISSEMENT_OF IS NOT NULL
         			BEGIN
         				-- Creation de l'adresse de l'etablissement
         				EXEC @ID_ADRESSE = INS_ADRESSE 
         							@ID_ADRESSE				= NULL,
         							@ID_ETABLISSEMENT_OF	= @ID_ETABLISSEMENT_OF, 
         							@ID_TYPE_ADRESSE		= 2, 
         							@ID_COMP_NUM_ADR		= NULL,
         							@ID_UO					= NULL,
         							@ID_ETABLISSEMENT		= NULL,
         							@ID_TIERS				= NULL,
         							@ID_TYPE_VOIE			= 1, 
         							@NUM_VOIE				= NULL,
         							@NUM_TEL				= NULL,
         							@NUM_FAX				= NULL,
         							@BLN_ACTIF				= 1,		 				-- Actif par defaut
         							@COM_ADRESSE			= 'Cr‚ation Automatique EDI',
         							@ID_UTILISATEUR			= @ID_UTILISATEUR_ADMIN_EDI, 		-- Admin par default
         							@LIB_NOM_VOIE			= 'A renseigner',
         							@LIB_COMP_VOIE			= NULL,
         							@LIB_ADR				= 'A renseigner',
         							@BLN_PRINCIPAL			= 1 ,						-- Principale par default
         							@LIB_CP_CEDEX			= '999999',
         							@LIB_VIL_CEDEX			= 'A renseigner',			-- DANGER garder le meme nom sinon recherche infructueuse
         							@ID_PAYS				= 1,						-- France par default
         							@ID_MENTION_PARTICULIERE = NULL, 
         							@LIB_MENTION_PARTICULIERE = NULL,
         							@ID_HEXAPOSTE			= NULL,
         							@EMAIL_PRO				= NULL
         			
         			-- Creation du contact principal
         				EXEC @ID_CONTACT = INS_CONTACT 
         						@ID_CIVILITE		= 1,
         						@COD_CONTACT		= NULL, 
         						@LIB_NOM_CONTACT	= '.',
         						@LIB_PNM_CONTACT	= '.',
         						@COM_CONTACT		= 'Creation Automatique EDI'
         
         				EXEC INS_NR34 
         								@ID_CONTACT		= @ID_CONTACT,
         								@ID_FONCTION	= NULL,	
         								@NUM_TEL		= NULL,
         								@NUM_PORT		= NULL,
         								@NUM_FAX		= NULL,
         								@BLN_PRINCIPAL  = 1,
         								@BLN_ACTIF		= 1,
         								@EMAIL_PRO		= NULL,
         								@ID_ADRESSE		= @ID_ADRESSE,
         								@ID_ETABLISSEMENT_OF = @ID_ETABLISSEMENT_OF,
         								@LIB_TITRE		= NULL,
         								@EMAIL_PERS		= NULL
         
         				-- Creation de la transaction de reglement principale
         				EXEC UPD_GESTION_TRANSACTION_ETABLISSEMENT_OF 
         							@ID_OF					= @ID_OF,
         							@ID_ETABLISSEMENT_OF	= @ID_ETABLISSEMENT_OF,
         							@ID_ADRESSE				= @ID_ADRESSE,
         							@ID_UTILISATEUR			= @ID_UTILISATEUR_ADMIN_EDI,
         							@BLN_PRINCIPAL			= 1,
         							@NUM_IBAN				= NULL,
         							@BIC					= NULL
         			
         				-- Mise a jour de l'adresse principal et de l'etablissemennt princip OF et Etablissement OF
         				UPDATE	ETABLISSEMENT_OF 
         				SET		ID_ADRESSE_PRINCIPALE	= @ID_ADRESSE
         				WHERE	ID_ETABLISSEMENT_OF		= @ID_ETABLISSEMENT_OF 
         				
         				IF @BLN_CREATION_OF = 1
         				BEGIN
         					UPDATE	ORGANISME_FORMATION
         					SET		ID_ADRESSE_PRINCIPALE			= @ID_ADRESSE,
         							ID_ETABLISSEMENT_OF_PRINCIPAL	= @ID_ETABLISSEMENT_OF
         					WHERE	ID_OF	= @ID_OF
         					
         				END
         			
         			END
         			
         		END
         	END
         	
         		SET @NUM_DUREE_JOUR = CAST(@NUM_DUREE_H_MODULE/7 AS DECIMAL(15,1))
         	
         	SELECT	@ID_PERIODE = ID_PERIODE 
         	FROM	PERIODE 
         	WHERE	NUM_ANNEE = YEAR(@DAT_DEBUT)
         	AND		ID_TYPE_PERIODE = 1
         
         	SET @ID_DISPOSITIF_PAR_DEFAUT = NULL
         	SELECT @ID_DISPOSITIF_PAR_DEFAUT = ID_DISPOSITIF
         	FROM ETABLISSEMENT
         	INNER JOIN R19			ON ETABLISSEMENT.ID_ADHERENT = R19.ID_ADHERENT
         	INNER JOIN ACTIVITE		ON ACTIVITE.ID_ACTIVITE = R19.ID_ACTIVITE	
         	INNER JOIN DISPOSITIF	ON ACTIVITE.ID_ACTIVITE = DISPOSITIF.ID_ACTIVITE
         	WHERE ID_ETABLISSEMENT = @ID_ETABLISSEMENT
         	AND	ID_PERIODE = @ID_PERIODE 
         	AND ID_TYPE_ACTIVITE = 1
         	
         	IF @ID_DISPOSITIF_PAR_DEFAUT IS NULL 
         	SET @ID_DISPOSITIF_PAR_DEFAUT = (SELECT ID_DISPOSITIF FROM DISPOSITIF WHERE COD_DISPOSITIF = 'P50+')
         	
         	SET @BLN_DELEGATION_PAIEMENT = CASE WHEN @BLN_DELEG_PAIEMENT = 'O' THEN 1 ELSE 0 END
         	
         	
         	IF @ID_MODULE_PEC IS NULL -- MODULE INEXISTANT A CREER
         	BEGIN
         		IF @BLN_DEBUG > 0
         		BEGIN
         			SELECT
         			 LIBL_MODULE_PEC  = @LIBL_MODULE_PEC ,  
         			 ID_ACTION_PEC  = @ID_ACTION_PEC ,  
         			 ID_ETABLISSEMENT_OF  = @ID_ETABLISSEMENT_OF ,  
         			 ID_THEME_MODULE = @ID_THEME_MODULE ,  
         			 ID_STAGE =NULL, -- @ID_STAGE ,  
         			 ID_FORMACODE  = @ID_FORMACODE ,  
         			 BLN_ACTIF = 1 , -- @BLN_ACTIF 
         			 BLN_IMPUTABLE  = 1 , --@BLN_IMPUTABLE 
         			 BLN_MODULE_EXTERNE = @BLN_MODULE_EXTERNE,  
         			 ID_UTILISATEUR_EDI = @ID_UTILISATEUR_ADMIN_EDI,  
         			 NUM_INTERNE = @NUM_INTERNE ,  
         			 DAT_DEBUT = @DAT_DEBUT ,  
         			 DAT_FIN  = @DAT_FIN ,  
         			 NUM_DUREE_JOUR = @NUM_DUREE_JOUR ,  
         			 NUM_DUREE_H_MODULE = @NUM_DUREE_H_MODULE,  
         			 MNT_CONVENTION = @MNT_CONVENTION,  
         			 ID_PERIODE = @ID_PERIODE,  
         			 COM_MODULE_PEC  = 'Import EDI' , -- @COM_MODULE_PEC   
         			 ID_DEPART_FORMATION = @ID_DEPART_FORMATION,  
         			 BLN_INTRA  = @BLN_INTRA ,  
         			 AXE_MODULE = @AXE_MODULE ,  
         			 DOMAINE_MODULE  = @DOMAINE_MODULE ,  
         			 ID_MODALITE_FORMATION   = 1, --@ID_MODALITE_FORMATION  
         			 BLN_DELEGATION_PAIEMENT = @BLN_DELEGATION_PAIEMENT,  
         			 ID_UTILISATEUR_EDI = @ID_UTILISATEUR_ADMIN_EDI,  
         			 ID_DISPOSITIF_PAR_DEFAUT = @ID_DISPOSITIF_PAR_DEFAUT,  
         			 ID_MODULE_PEC = @ID_MODULE_PEC ,  
         			 ID_CRITERE_CHIFFRAGE     = 4 , -- @ID_CRITERE_CHIFFRAGE    ,  
         			 BLN_CATALOGUE = 0 -- @BLN_CATALOGUE 
         		END
         		
         		-- Creation du module
         		EXEC INS_MODULE_PEC        
         		 @LIBL_MODULE_PEC ,  
         		 @ID_ACTION_PEC ,  
         		 @ID_ETABLISSEMENT_OF ,  
         		 @ID_THEME_MODULE ,  
         		 NULL, -- @ID_STAGE ,  
         		 @ID_FORMACODE ,  
         		 1 , -- @BLN_ACTIF 
         		 1 , --@BLN_IMPUTABLE 
         		 @BLN_MODULE_EXTERNE,  
         		 @ID_UTILISATEUR_ADMIN_EDI,  
         		 @NUM_INTERNE ,  
         		 @DAT_DEBUT ,  
         		 @DAT_FIN ,  
         		 @NUM_DUREE_JOUR ,  
         		 @NUM_DUREE_H_MODULE,  
         		 @MNT_CONVENTION,  
         		 @ID_PERIODE,  
         		 'Import EDI' , -- @COM_MODULE_PEC   
         		 @ID_DEPART_FORMATION,  
         		 @BLN_INTRA ,  
         		 @AXE_MODULE ,  
         		 @DOMAINE_MODULE ,  
         		 1, --@ID_MODALITE_FORMATION  
         		 @BLN_DELEGATION_PAIEMENT,  
         		 @ID_UTILISATEUR_ADMIN_EDI,  
         		 @ID_DISPOSITIF_PAR_DEFAUT,  
         		 @ID_MODULE_PEC output,  
         		 4 , -- @ID_CRITERE_CHIFFRAGE    ,  
         		 0 -- @BLN_CATALOGUE 
         		 
         		 IF @ID_MODULE_PEC IS NOT NULL
         		 BEGIN
         		 
         			UPDATE EDI_PEC_ST
         			SET		ID_MODULE_PEC		= @ID_MODULE_PEC
         			WHERE	ID_LOT_IMPORT		= @ID_LOT_IMPORT
         			AND		ID_ACTION_PEC		= @ID_ACTION_PEC		
         			AND		NUM_INTERNE			= @NUM_INTERNE
         			AND		ID_MODULE_PEC IS NULL
         			
         
         ----------------------------------------------   
         -- MBL Modification du 08/06/2015:
         -- R‚activation et D‚cloture des actions associ‚es … des modules cr‚‚s via EDI PEC
         ----------------------------------------------   
         			IF EXISTS (SELECT 1 FROM ACTION_PEC WHERE BLN_ACTIF = 0 AND ID_ACTION_PEC = @ID_ACTION_PEC)
         			BEGIN
         				UPDATE ACTION_PEC 
         				SET BLN_ACTIF = 1 
         				WHERE ID_ACTION_PEC = @ID_ACTION_PEC
         			END
         
         			IF EXISTS (SELECT 1 FROM ACTION_PEC WHERE DAT_CLOTURE IS NOT NULL AND ID_ACTION_PEC = @ID_ACTION_PEC)
         			BEGIN
         				UPDATE ACTION_PEC 
         				SET DAT_CLOTURE = NULL 
         				WHERE ID_ACTION_PEC = @ID_ACTION_PEC
         			END
         			
         		 END	
         	END
         	
         	IF @ID_MODULE_PEC IS NOT NULL 
         	BEGIN
         		IF LEN(LTRIM(RTRIM(@COM_MODULE))) > 0
         		BEGIN
         			
         			SELECT	@COD_MODULE_PEC = COD_MODULE_PEC
         			FROM	 MODULE_PEC
         			WHERE	ID_MODULE_PEC = @ID_MODULE_PEC 
         
         			SET @COMMENTAIRE = 'EDI Nø:' + CAST(@COD_MODULE_PEC AS VARCHAR)
         			+ ' ' + @COM_MODULE
         			
         			EXEC INS_COMMENTAIRES
         			6							,	
         			@ID_ACTION_PEC				,
         			@COMMENTAIRE				, 
         			@ID_UTILISATEUR_ADH_EDI			-- Commentaires EDI Adherent 
         
         		END	 	
         	END
         	
         	IF @ID_MODULE_PEC IS NOT NULL AND @ID_SALARIE_FORMATEUR_INTERNE IS NOT NULL
         	BEGIN
         
         		SELECT
         			@ID_INDIVIDU					= ID_INDIVIDU			,
         			@ID_ETABLISSEMENT				= ID_ETABLISSEMENT		,
         			@ID_TYPE_CONTRAT				= ID_TYPE_CONTRAT		,
         			@ID_CSP							= ID_CSP				,
         			@ID_CLASSIFICATION				= ID_CLASSIFICATION		,
         			@ID_STATUT						= ID_STATUT				,
         			@NUM_DUREE_MENSUELLE_TRAVAIL	= NUM_DUREE_MENSUELLE_TRAVAIL,
         			@MATRICULE						= MATRICULE_SALARIE,
         			@BRUT_CHARGE					= SALAIRE_HORAIRE_CHARGE	,
         			@BLN_TEMPS_PARTIEL				= BLN_TEMPS_PARTIEL		,
         			@CENTRE_COUT					= CENTRE_COUT				,
         			@ID_CODE_INSEE					= ID_CODE_INSEE,
         			@ID_FAMILLE_PROFESSIONNELLE		= ID_FAMILLE_PROFESSIONNELLE ,
         			@SALAIRE_HORAIRE_CHARGE			= SALAIRE_HORAIRE_CHARGE,
         			@SALAIRE_HORAIRE_NET			= SALAIRE_HORAIRE_NET		,
         			@SALAIRE_HORAIRE_BRUT_CHARGE	= SALAIRE_HORAIRE_BRUT_CHARGE,
         			@MONTANT_BRUT_CHARGE			= MONTANT_BRUT_CHARGE		,
         			@DATE_EMBAUCHE					= DATE_EMBAUCHE			,
         			@ID_NIVEAU_AVENANT				= ID_NIVEAU_AVENANT,
         			@ANALYTIQUE_STAGIAIRE			= ANALYTIQUE_STAGIAIRE	,
         			@FONCTION						= FONCTION				
         		FROM SALARIE 
         		WHERE ID_SALARIE = @ID_SALARIE_FORMATEUR_INTERNE	
         
         		SELECT 	@ID_BRANCHE				= ID_BRANCHE
         		FROM ETABLISSEMENT
         		WHERE ID_ETABLISSEMENT	= @ID_ETABLISSEMENT
         		
         		EXEC [INS_FORMATEUR_INTERNE_PEC]
         		NULL , --@COD_FORMATEUR_INTERNE_PEC 
         		@ID_TYPE_CONTRAT ,
         		@ID_BRANCHE,
         		@ID_CSP ,
         		@ID_INDIVIDU ,
         		@ID_ETABLISSEMENT,
         		@ID_MODULE_PEC,
         		@ID_CLASSIFICATION,
         		@SALAIRE_HORAIRE_CHARGE,
         		0 ,						-- @NB_HEURES_HORS_TT 
         		NULL,					--@COM_FORMATEUR_INTERNE_PEC ,
         		@NUM_DUREE_H_MODULE,	--@NB_HEURE_DISPENSEE ,
         		@SALAIRE_HORAIRE_NET ,
         		@DATE_EMBAUCHE ,
         		@MONTANT_BRUT_CHARGE,
         		@SALAIRE_HORAIRE_BRUT_CHARGE,
         		@ANALYTIQUE_STAGIAIRE,
         		@FONCTION,
         		0,						-- @BLN_SUIVI_FORMATION_FORMATEUR tinyint,
         		0,						-- @BLN_SUIVI_FORMATION_TUTEUR tinyint,
         		0						-- @BLN_FORME_REGULIEREMENT_INTERNE tinyint
         	
         	END
         
         	
         	FETCH cu_module_pec INTO 
         		@ID_ACTION_PEC			,
         		@ID_MODULE_PEC			,
         		@LIBL_MODULE_PEC		,
         		@DAT_DEBUT				,
         		@DAT_FIN				,
         		@ID_THEME_MODULE		,
         		@COD_INITIATIVE			,
         		@NUM_DUREE_H_MODULE		,
         		@MNT_CONVENTION			,
         		@ID_DEPART_FORMATION	,
         		@BLN_DELEG_PAIEMENT		,
         		@ID_ETABLISSEMENT_OF	,
         		@NUM_SIRET_OF			,
         		@LIBL_OF				,
         		@NUM_INTERNE			,
         		@AXE_MODULE				,
         		@DOMAINE_MODULE			,
         		@BLN_EXTERNE			,
         		@BLN_INTRA				,		
         		@ID_SALARIE_FORMATEUR_INTERNE,
         		@COM_MODULE				,
         		@BLN_REJET        
         END
         CLOSE cu_module_pec
         DEALLOCATE cu_module_pec
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  36',  GETDATE(),  'FIN TRAITEMENT DES MODULES NON REJETES', 'NB REJETS ' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Traitements Sous Type de Cout  des modules PEC non rejet‚es */
         DECLARE cu_module_pec scroll cursor for
         SELECT  DISTINCT
         	ID_MODULE_PEC			,
         	MNT_PREV_HT_CP			,
         	MNT_PREV_HT_INGE		,
         	MNT_PREV_HT_REM		,
         	MNT_PREV_HT_AF			,
         	MNT_PREV_HT_FA			,
         	MNT_PREV_HT_REPHEB		,
         	MNT_PREV_HT_ACTEVAL	,
         	MNT_PREV_HT_TRANSP		,
         	MNT_PREV_HT_FFRECONV	,
         	MNT_PREV_HT_FCT		,
         	MNT_PREV_HT_FFORM		,
         	MNT_PREV_HT_REMFORM	,	
         	BLN_REJET        
         FROM	EDI_PEC_ST
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		ID_MODULE_PEC IS NOT NULL
         AND		ISNULL(BLN_REJET, 0) = 0	
         
         
         OPEN  cu_module_pec
         FETCH cu_module_pec  INTO
         	@ID_MODULE_PEC			,
         	@MNT_PREV_HT_CP			,
         	@MNT_PREV_HT_INGE		,
         	@MNT_PREV_HT_REM		,
         	@MNT_PREV_HT_AF			,
         	@MNT_PREV_HT_FA			,
         	@MNT_PREV_HT_REPHEB		,
         	@MNT_PREV_HT_ACTEVAL	,
         	@MNT_PREV_HT_TRANSP		,
         	@MNT_PREV_HT_FFRECONV	,
         	@MNT_PREV_HT_FCT		,
         	@MNT_PREV_HT_FFORM		,
         	@MNT_PREV_HT_REMFORM	,	
         	@BLN_REJET        
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	DECLARE cu_sous_type_cout CURSOR FOR
         	SELECT 	COD_SOUS_TYPE_COUT, ID_SOUS_TYPE_COUT
         	FROM SOUS_TYPE_COUT
         	WHERE	BLN_ACTIF = 1
         	
         	OPEN cu_sous_type_cout 
         	FETCH cu_sous_type_cout INTO @COD_SOUS_TYPE_COUT, @ID_SOUS_TYPE_COUT
         	WHILE(@@FETCH_STATUS <> -1)
         	BEGIN
         			
         		
         		IF		@COD_SOUS_TYPE_COUT = 'CP' SET @MNT_PREV_HT = @MNT_PREV_HT_CP
         		ELSE IF @COD_SOUS_TYPE_COUT = 'INGE' SET @MNT_PREV_HT = @MNT_PREV_HT_INGE
         		ELSE IF @COD_SOUS_TYPE_COUT = 'REM' SET @MNT_PREV_HT = @MNT_PREV_HT_REM
         		ELSE IF @COD_SOUS_TYPE_COUT = 'AF' SET @MNT_PREV_HT = @MNT_PREV_HT_AF
         		ELSE IF @COD_SOUS_TYPE_COUT = 'FA' SET @MNT_PREV_HT = @MNT_PREV_HT_FA
         		ELSE IF @COD_SOUS_TYPE_COUT = 'REPHEB' SET @MNT_PREV_HT = @MNT_PREV_HT_REPHEB
         		ELSE IF @COD_SOUS_TYPE_COUT = 'ACTEVAL' SET @MNT_PREV_HT = @MNT_PREV_HT_ACTEVAL
         		ELSE IF @COD_SOUS_TYPE_COUT = 'TRANSP' SET @MNT_PREV_HT = @MNT_PREV_HT_TRANSP
         		ELSE IF @COD_SOUS_TYPE_COUT = 'FFRECONV' SET @MNT_PREV_HT = @MNT_PREV_HT_FFRECONV
         		ELSE IF @COD_SOUS_TYPE_COUT = 'FCT' SET @MNT_PREV_HT = @MNT_PREV_HT_FCT
         		ELSE IF @COD_SOUS_TYPE_COUT = 'FFORM' SET @MNT_PREV_HT = @MNT_PREV_HT_FFORM
         		ELSE IF @COD_SOUS_TYPE_COUT = 'REMFORM' SET @MNT_PREV_HT = @MNT_PREV_HT_REMFORM
         		ELSE SET @MNT_PREV_HT = NULL
         		
         		SET @MNT_PREV_HT = ISNULL(@MNT_PREV_HT, 0)
         		IF ISNULL(@MNT_PREV_HT, 0) > 0 
         		OR 
         		EXISTS (SELECT 1 FROM POSTE_COUT_ENGAGE WHERE ID_MODULE_PEC = @ID_MODULE_PEC AND ID_SOUS_TYPE_COUT = @ID_SOUS_TYPE_COUT )
         		BEGIN
         		EXEC @ID_POSTE_COUT_ENGAGE  = UPD_SOUS_TYPE_COUT_MODULE_PEC
         			@ID_SOUS_TYPE_COUT  ,  
         			@ID_MODULE_PEC		,  
         			@MNT_PREV_HT		,    
         			@MNT_PREV_HT		,  
         			@ID_POSTE_COUT_ENGAGE output  
         		END
         					
         		FETCH cu_sous_type_cout INTO @COD_SOUS_TYPE_COUT, @ID_SOUS_TYPE_COUT
         	END
         	
         	CLOSE cu_sous_type_cout 
         	DEALLOCATE cu_sous_type_cout 
         		
         	
         	FETCH cu_module_pec  INTO
         		@ID_MODULE_PEC			,
         		@MNT_PREV_HT_CP			,
         		@MNT_PREV_HT_INGE		,
         		@MNT_PREV_HT_REM		,
         		@MNT_PREV_HT_AF			,
         		@MNT_PREV_HT_FA			,
         		@MNT_PREV_HT_REPHEB		,
         		@MNT_PREV_HT_ACTEVAL	,
         		@MNT_PREV_HT_TRANSP		,
         		@MNT_PREV_HT_FFRECONV	,
         		@MNT_PREV_HT_FCT		,
         		@MNT_PREV_HT_FFORM		,
         		@MNT_PREV_HT_REMFORM	,	
         		@BLN_REJET        
         END
         CLOSE cu_module_pec
         DEALLOCATE cu_module_pec
         /* Fin Traitements Sous Type de Cout  des modules PEC non rejet‚es */
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  37',  GETDATE(),  'FIN TRAITEMENT Sous Type de Cout  des modules PEC non rejet‚es ', 'NB REJETS ' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         /* Traitements Pr‚alable de RAZ des Stagiaires des modules PEC non rejet‚es */
         DECLARE cu_raz_stagiaire_pec scroll cursor for
         SELECT  DISTINCT
         	ID_MODULE_PEC			
         FROM	EDI_PEC_ST	
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		ID_MODULE_PEC IS NOT NULL
         AND		ISNULL(BLN_REJET, 0) = 0	
         OPEN cu_raz_stagiaire_pec 
         
         FETCH cu_raz_stagiaire_pec INTO @ID_MODULE_PEC
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         	UPDATE STAGIAIRE_PEC 
         	SET NB_HEURE_ENGAGE= 0, NB_HEURE_REM = 0
         	WHERE ID_MODULE_PEC= @ID_MODULE_PEC
         	
         	UPDATE UNITE_STAGIAIRE
         	SET NB_HEURE_ENGAGE=0, NB_HEURE_HTT=0, NB_HEURE_REM=0
         	FROM  UNITE_STAGIAIRE
         	INNER JOIN STAGIAIRE_PEC ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC .ID_STAGIAIRE_PEC
         	WHERE ID_MODULE_PEC= @ID_MODULE_PEC
         
         
         	FETCH cu_raz_stagiaire_pec INTO @ID_MODULE_PEC
         END
         
         CLOSE cu_raz_stagiaire_pec 
         DEALLOCATE cu_raz_stagiaire_pec 
         /* Fin Traitements Pr‚alable de RAZ des Stagiaires des modules PEC non rejet‚es */
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  38',  GETDATE(),  'FIN TRAITEMENT Pr‚alable de RAZ des Stagiaires des modules PEC non rejet‚es ', 'NB REJETS ' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* Traitements Stagiaires des modules PEC non rejet‚es */
         DECLARE cu_stagiaire_pec scroll cursor for
         SELECT  DISTINCT
         	ID_ACTION_PEC			, 
         	NUM_INTERNE_ACTION		,
         	ID_MODULE_PEC			,
         	ID_ETABLISSEMENT		,
         	ID_INDIVIDU				,
         	ID_SALARIE				,
         	CASE BLN_TUTEUR_INTERNE		WHEN 'O' THEN 1 ELSE 0 END,
         	ID_TUTEUR				,
         	ID_SALARIE_TUTEUR		,
         	NB_H_ENGAGE_PL , NB_H_HTT_PL , 	ID_PUBLIC_PRIO_PL, ID_OBJET_FORM_PL, ID_ACTION_PRIO_PL, ID_CATEG_ACTION_PL,
         	NB_H_ENGAGE_PP , NB_H_HTT_PP , ID_PUBLIC_PRIO_PP, ID_OBJET_FORM_PP, ID_ACTION_PRIO_PP, ID_CATEG_ACTION_PP,
         	NB_H_ENGAGE_DIFP , NB_H_HTT_DIFP , ID_PUBLIC_PRIO_DIFP, ID_OBJET_FORM_DIFP, ID_ACTION_PRIO_DIFP, ID_CATEG_ACT_DIFP,
         	NB_H_ENGAGE_FORMT , ID_OBJET_FORM_FORMT, 
         	NB_H_ENGAGE_FTUT , ID_OBJET_FORM_FTUT, 
         	NB_H_ENGAGE_DIFNP,	NB_H_HTT_DIFNP, ID_OBJET_FORM_DIFNP, ID_CATEG_ACT_DIFNP
         	
         FROM	EDI_PEC_ST	
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		ID_MODULE_PEC IS NOT NULL
         AND		ISNULL(BLN_REJET, 0) = 0	
         
         OPEN cu_stagiaire_pec 
         
         FETCH cu_stagiaire_pec 	INTO
         	@ID_ACTION_PEC			, 
         	@NUM_INTERNE_ACTION		,
         	@ID_MODULE_PEC			,
         	@ID_ETABLISSEMENT		,
         	@ID_INDIVIDU			,
         	@ID_SALARIE				,
         	@BLN_TUTEUR_INTERNE		,
         	@ID_TUTEUR				,
         	@ID_SALARIE_TUTEUR		,
         	@NB_H_ENGAGE_PL , @NB_H_HTT_PL , 	@ID_PUBLIC_PRIO_PL, @ID_OBJET_FORM_PL, @ID_ACTION_PRIO_PL, @ID_CATEG_ACTION_PL,
         	@NB_H_ENGAGE_PP , @NB_H_HTT_PP , @ID_PUBLIC_PRIO_PP, @ID_OBJET_FORM_PP, @ID_ACTION_PRIO_PP, @ID_CATEG_ACTION_PP,
         	@NB_H_ENGAGE_DIFP , @NB_H_HTT_DIFP , @ID_PUBLIC_PRIO_DIFP, @ID_OBJET_FORM_DIFP, @ID_ACTION_PRIO_DIFP, @ID_CATEG_ACT_DIFP,
         	@NB_H_ENGAGE_FORMT , @ID_OBJET_FORM_FORMT, 
         	@NB_H_ENGAGE_FTUT , @ID_OBJET_FORM_FTUT, 
         	@NB_H_ENGAGE_DIFNP,	@NB_H_HTT_DIFNP, @ID_OBJET_FORM_DIFNP, @ID_CATEG_ACT_DIFNP
         	
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         
         	SET @ID_STAGIAIRE_PEC		= NULL
         	SET @ID_BRANCHE				= NULL
         	SET @ID_GROUPE				= NULL
         	SET @ID_ACTIVITE			= NULL
         	SET @ID_DISPOSITIF			= NULL
         	SET	@ID_INDIVIDU			= NULL
         		
         	SELECT 	TOP 1 
         			@ID_BRANCHE				= ID_BRANCHE,
         			@ID_GROUPE				= ID_GROUPE,
         			@ID_ACTIVITE			= R19.ID_ACTIVITE
         	FROM ETABLISSEMENT
         	INNER JOIN R19		ON ETABLISSEMENT.ID_ADHERENT = R19.ID_ADHERENT
         	INNER JOIN ACTIVITE	ON ACTIVITE.ID_ACTIVITE = R19.ID_ACTIVITE	
         	INNER JOIN PERIODE	ON R19.ID_PERIODE = PERIODE.ID_PERIODE 
         	WHERE ID_ETABLISSEMENT	= @ID_ETABLISSEMENT
         	AND ID_TYPE_ACTIVITE = 1
         	ORDER BY PERIODE.NUM_ANNEE DESC
         
         	SELECT	@ID_PERIODE			= ID_PERIODE ,
         			@NUM_DUREE_H_MODULE = NUM_DUREE_HEURE
         	FROM	MODULE_PEC
         	WHERE	ID_MODULE_PEC = @ID_MODULE_PEC
         	
         
         	SELECT
         		@ID_INDIVIDU					= ID_INDIVIDU			,
         		@ID_TYPE_CONTRAT				= ID_TYPE_CONTRAT		,
         		@ID_CSP							= ID_CSP				,
         		@ID_CLASSIFICATION				= ID_CLASSIFICATION		,
         		@ID_STATUT						= ID_STATUT				,
         		@NUM_DUREE_MENSUELLE_TRAVAIL	= NUM_DUREE_MENSUELLE_TRAVAIL,
         		@MATRICULE						= MATRICULE_SALARIE,
         		@BRUT_CHARGE					= SALAIRE_HORAIRE_CHARGE	,
         		@BLN_TEMPS_PARTIEL				= BLN_TEMPS_PARTIEL		,
         		@CENTRE_COUT					= CENTRE_COUT				,
         		@ID_CODE_INSEE					= ID_CODE_INSEE,
         		@ID_FAMILLE_PROFESSIONNELLE		= ID_FAMILLE_PROFESSIONNELLE ,
         		@SALAIRE_HORAIRE_NET			= SALAIRE_HORAIRE_NET		,
         		@SALAIRE_HORAIRE_BRUT_CHARGE	= SALAIRE_HORAIRE_BRUT_CHARGE,
         		@MONTANT_BRUT_CHARGE			= MONTANT_BRUT_CHARGE		,
         		@DATE_EMBAUCHE					= DATE_EMBAUCHE			,
         		@ID_NIVEAU_AVENANT				= ID_NIVEAU_AVENANT,
         		@ANALYTIQUE_STAGIAIRE			= ANALYTIQUE_STAGIAIRE	,
         		@FONCTION						= FONCTION				
         	FROM SALARIE 
         	WHERE ID_SALARIE = @ID_SALARIE	
         	
         	--SET @ID_STAGIAIRE_PEC = NULL
         	--SELECT @ID_STAGIAIRE_PEC = ID_STAGIAIRE_PEC
         	--FROM STAGIAIRE_PEC
         	--WHERE	ID_MODULE_PEC = @ID_MODULE_PEC 
         	--AND		ID_INDIVIDU = @ID_INDIVIDU
         
         	SET @NB_HEURES_STAGIAIRE_ENG	= @NB_H_ENGAGE_PL + @NB_H_ENGAGE_PP + @NB_H_ENGAGE_DIFP + @NB_H_ENGAGE_FORMT + @NB_H_ENGAGE_FTUT + @NB_H_ENGAGE_DIFNP
         	SET @NB_HEURES_STAGIAIRE_HTT	= @NB_H_HTT_PL + @NB_H_HTT_PP + @NB_H_HTT_DIFP +  @NB_H_HTT_DIFNP
         
         
         	-- Rajout de l'‚tablissement du stagiaire a l'action.
         	IF NOT EXISTS (SELECT 1 FROM NR140 WHERE ID_ACTION_PEC = @ID_ACTION_PEC AND ID_ETABLISSEMENT = @ID_ETABLISSEMENT)
         	BEGIN
         		EXEC INS_ETABLISSEMENT_ACTION 
         		@ID_ACTION_PEC, 
         		@ID_ETABLISSEMENT, 
         		@NUM_INTERNE_ACTION
         		
         		SELECT @NB_ELEMENT = COUNT(*)
         		FROM NR140
         		WHERE ID_ACTION_PEC = @ID_ACTION_PEC		
         		IF @NB_ELEMENT > 1
         		BEGIN
         			UPDATE ACTION_PEC 
         			SET CIBLE_ACTION = 3
         			WHERE ID_ACTION_PEC = @ID_ACTION_PEC		
         		END
         		
         	END
         
         	EXEC UPD_STAGIAIRE_MODULE_PEC
         		@ID_MODULE_PEC					,
         		@ID_INDIVIDU					,
         		@ID_ETABLISSEMENT				,
         		@ID_TYPE_CONTRAT				,
         		1								,-- ID_CATEGORIE_ACTION		
         		@ID_CSP							,
         		@ID_CLASSIFICATION				,
         		@ID_STATUT						,
         		@NUM_DUREE_MENSUELLE_TRAVAIL	,
         		@NB_HEURES_STAGIAIRE_ENG		, -- DUREE_PREVUE
         		0								, -- DUREE_REALISEE
         		@MATRICULE						,
         		NULL							, -- AGENCE
         		NULL							, -- DR
         		@MONTANT_BRUT_CHARGE			,
         		@BLN_TUTEUR_INTERNE				,
         		@NB_HEURES_STAGIAIRE_HTT		, -- NB_HEURES
         		'Import EDI'					, -- COM_STAGIAIRE	
         		@BLN_TEMPS_PARTIEL				,
         		@CENTRE_COUT					,
         		@ID_CODE_INSEE					,
         		@ID_FAMILLE_PROFESSIONNELLE		,
         		@SALAIRE_HORAIRE_NET			,
         		@SALAIRE_HORAIRE_BRUT_CHARGE	,
         		@MONTANT_BRUT_CHARGE			,
         		@DATE_EMBAUCHE					,
         		@ID_NIVEAU_AVENANT				,
         		@ANALYTIQUE_STAGIAIRE			,
         		@ID_TUTEUR						,
         		@TIME_STAMP						,
         		@ID_STAGIAIRE_PEC		output	,
         		NULL							, --ID_SESSION		
         		@ID_BRANCHE						,
         		@ID_GROUPE						,
         		@ID_ACTIVITE					,
         		@NB_HEURES_STAGIAIRE_ENG		, -- NB_HEURE_REM
         		@FONCTION						,
         		NULL							, --ID_SUIVI_STAGIAIRE_OBJECTIF_ISSU
         		NULL							, --ID_SUIVI_STAGIAIRE_IMMEDIAT
         		NULL							  -- ID_SUIVI_STAGIAIRE_TROIS_MOIS =NULL
         		
         	UPDATE UNITE_STAGIAIRE 
         	SET NB_HEURE_ENGAGE=0, NB_HEURE_HTT=0, NB_HEURE_REM=0
         	WHERE ID_STAGIAIRE_PEC = @ID_STAGIAIRE_PEC
         
         	
         	-- Construction Requete Dynamique permettant d'alimenter la table #TMP_EDI 
         	-- avec le curseur renvoye par la PROC STOCKEE
         	-- LEC_GRP_DISPOSITIF_STAGIAIRE_CREATION @ID_STAGIAIRE = NULL, @ID_ETABLISSEMENT = @ID_ETABLISSEMENT, @ID_PERIODE =@ID_PERIODE, @ID_ACTIVITE =@ID_ACTIVITE
         	
         	SELECT @LIB_SQL = 'EXEC '
         
         	IF @@SERVERNAME = 'WS5'			SET @dbname = 'C2P_PROD'
         	IF @@SERVERNAME = 'DEFISRV06'	SET @dbname = 'C2P_PROD_BO'
         	IF @@SERVERNAME = 'DEFISRV03'	SET @dbname = 'C2P_RECETTE'
         	
         	SELECT @LIB_SQL = @LIB_SQL + @@SERVERNAME + '.' + @dbname + '.dbo.LEC_GRP_DISPOSITIF_STAGIAIRE_CREATION '+  
         	+'@ID_STAGIAIRE = NULL,'
         	+'@ID_ETABLISSEMENT = '+ CAST(@ID_ETABLISSEMENT AS VARCHAR)+','
         	+'@ID_PERIODE =' + CAST(@ID_PERIODE AS VARCHAR)+ ','
         	+'@ID_ACTIVITE = ' + CAST(@ID_ACTIVITE AS VARCHAR)+  '' ;      
         	delete from #TMP_EDI
         	SELECT @LIB_SQL = 'SELECT ID_DISPOSITIF = ID, COD_DISPOSITIF = COD, BLN_PLAN '
         			+' FROM OPENROWSET('
         			+'''SQLNCLI'', ''Server=' + @@SERVERNAME + ';Trusted_Connection=yes;'',''' 
         			+ 
         			@LIB_SQL + ''')' ;      	
         	INSERT INTO #TMP_EDI EXEC (@LIB_SQL )
         	-- Fin Construction Requete Dynamique permettant d'alimenter la table #TMP_EDI
         	
         	
         --	SELECT * FROM #TMP_EDI
         	DECLARE cu_dispositif_stagiaire CURSOR FOR	
         	SELECT * FROM #TMP_EDI
         	
         	OPEN cu_dispositif_stagiaire
         	FETCH cu_dispositif_stagiaire INTO @ID_DISPOSITIF, @COD_DISPOSITIF, @BLN_PLAN
         	WHILE (@@FETCH_STATUS <> -1)
         	BEGIN
         		--SELECT @ID_DISPOSITIF, @COD_DISPOSITIF, @BLN_PLAN
         		
         
         		IF @BLN_PLAN = 1 
         		BEGIN
         
         			SET @ID_UNITE_STAGIAIRE = NULL
         			exec UPD_DISPOSITIF_STAGIAIRE 
         			@ID_STAGIAIRE_PEC,
         			@ID_DISPOSITIF,			--@ID_DISPOSITIF,
         			@NB_H_ENGAGE_PL,		--@NB_HEURES_PREVU,
         			0,						--@NB_HEURES_REALISEE=0,
         			0,						--@REFUS=0,
         			@ID_UNITE_STAGIAIRE output,
         			@ID_PUBLIC_PRIO_PL,		--@ID_PUBLIC_PRIORITAIRE
         			@ID_OBJET_FORM_PL,		--@ID_OBJET_FORMATION
         			@ID_ACTION_PRIO_PL,		--@ID_ACTION_PRIORITAIRE
         			@ID_CATEG_ACTION_PL,	--@ID_CATERIE_ACTION
         			@NB_H_HTT_PL,			--@NB_HEURE_HTT
         			@NB_H_ENGAGE_PL			--@NB_HEURE_REM
         		END
         		ELSE IF @COD_DISPOSITIF = 'PPPRIO'
         		BEGIN
         			
         			SET @ID_UNITE_STAGIAIRE = NULL
         			exec UPD_DISPOSITIF_STAGIAIRE 
         			@ID_STAGIAIRE_PEC,
         			@ID_DISPOSITIF,			--@ID_DISPOSITIF,
         			@NB_H_ENGAGE_PP,		--@NB_HEURES_PREVU,
         			0,						--@NB_HEURES_REALISEE=0,
         			0,						--@REFUS=0,
         			@ID_UNITE_STAGIAIRE output,
         			@ID_PUBLIC_PRIO_PP,		--@ID_PUBLIC_PRIORITAIRE
         			@ID_OBJET_FORM_PP,		--@ID_OBJET_FORMATION
         			@ID_ACTION_PRIO_PP,		--@ID_ACTION_PRIORITAIRE
         			@ID_CATEG_ACTION_PP,	--@ID_CATERIE_ACTION
         			@NB_H_HTT_PP,			--@NB_HEURE_HTT
         			@NB_H_ENGAGE_PP			--@NB_HEURE_REM
         			
         		END				
         		ELSE IF @COD_DISPOSITIF = 'DIFPRIO'
         		BEGIN
         			
         			SET @ID_UNITE_STAGIAIRE = NULL
         			exec UPD_DISPOSITIF_STAGIAIRE 
         			@ID_STAGIAIRE_PEC,
         			@ID_DISPOSITIF,			--@ID_DISPOSITIF,
         			@NB_H_ENGAGE_DIFP,		--@NB_HEURES_PREVU,
         			0,						--@NB_HEURES_REALISEE=0,
         			0,						--@REFUS=0,
         			@ID_UNITE_STAGIAIRE output,
         			@ID_PUBLIC_PRIO_DIFP,	--@ID_PUBLIC_PRIORITAIRE
         			@ID_OBJET_FORM_DIFP,	--@ID_OBJET_FORMATION
         			@ID_ACTION_PRIO_DIFP,	--@ID_ACTION_PRIORITAIRE
         			@ID_CATEG_ACT_DIFP,		--@ID_CATERIE_ACTION
         			@NB_H_HTT_DIFP,			--@NB_HEURE_HTT
         			@NB_H_ENGAGE_DIFP		--@NB_HEURE_REM
         			
         		END				
         		ELSE IF @COD_DISPOSITIF = 'FORMTUT'
         		BEGIN
         			
         			SET @ID_UNITE_STAGIAIRE = NULL
         			exec UPD_DISPOSITIF_STAGIAIRE 
         			@ID_STAGIAIRE_PEC,
         			@ID_DISPOSITIF,			--@ID_DISPOSITIF,
         			@NB_H_ENGAGE_FORMT,		--@NB_HEURES_PREVU,
         			0,						--@NB_HEURES_REALISEE=0,
         			0,						--@REFUS=0,
         			@ID_UNITE_STAGIAIRE output,
         			NULL,					--@ID_PUBLIC_PRIORITAIRE
         			@ID_OBJET_FORM_FORMT,	--@ID_OBJET_FORMATION
         			NULL,					--@ID_ACTION_PRIORITAIRE
         			NULL,					--@ID_CATERIE_ACTION
         			0,						--@NB_HEURE_HTT
         			@NB_H_ENGAGE_FORMT		--@NB_HEURE_REM
         		END						
         		ELSE IF @COD_DISPOSITIF = 'FONCTUT'
         		BEGIN			
         			SET @ID_UNITE_STAGIAIRE = NULL
         			exec UPD_DISPOSITIF_STAGIAIRE 
         			@ID_STAGIAIRE_PEC,
         			@ID_DISPOSITIF,			--@ID_DISPOSITIF,
         			@NB_H_ENGAGE_FTUT,		--@NB_HEURES_PREVU,
         			0,						--@NB_HEURES_REALISEE=0,
         			0,						--@REFUS=0,
         			@ID_UNITE_STAGIAIRE output,
         			NULL,					--@ID_PUBLIC_PRIORITAIRE
         			@ID_OBJET_FORM_FTUT,	--@ID_OBJET_FORMATION
         			NULL,					--@ID_ACTION_PRIORITAIRE
         			NULL,					--@ID_CATERIE_ACTION
         			0,						--@NB_HEURE_HTT
         			@NB_H_ENGAGE_FTUT		--@NB_HEURE_REM
         		END						
         		ELSE IF @COD_DISPOSITIF = 'DIFNONP'
         		BEGIN			
         			SET @ID_UNITE_STAGIAIRE = NULL
         			exec UPD_DISPOSITIF_STAGIAIRE 
         			@ID_STAGIAIRE_PEC,
         			@ID_DISPOSITIF,			--@ID_DISPOSITIF,
         			@NB_H_ENGAGE_DIFNP,		--@NB_HEURES_PREVU,
         			0,						--@NB_HEURES_REALISEE=0,
         			0,						--@REFUS=0,
         			@ID_UNITE_STAGIAIRE output,
         			NULL,					--@ID_PUBLIC_PRIORITAIRE
         			@ID_OBJET_FORM_DIFNP,	--@ID_OBJET_FORMATION
         			NULL,					--@ID_ACTION_PRIORITAIRE
         			@ID_CATEG_ACT_DIFNP,	--@ID_CATERIE_ACTION
         			@NB_H_HTT_DIFNP,		--@NB_HEURE_HTT
         			@NB_H_ENGAGE_DIFNP		--@NB_HEURE_REM
         		END						
         		ELSE 
         		BEGIN			
         			SET @ID_UNITE_STAGIAIRE = NULL
         			exec UPD_DISPOSITIF_STAGIAIRE 
         			@ID_STAGIAIRE_PEC,
         			@ID_DISPOSITIF,			--@ID_DISPOSITIF,
         			0,						--@NB_HEURES_PREVU,
         			0,						--@NB_HEURES_REALISEE=0,
         			0,						--@REFUS=0,
         			@ID_UNITE_STAGIAIRE output,
         			NULL,					--@ID_PUBLIC_PRIORITAIRE
         			NULL,					--@ID_OBJET_FORMATION
         			NULL,					--@ID_ACTION_PRIORITAIRE
         			NULL,					--@ID_CATERIE_ACTION
         			0,						--@NB_HEURE_HTT
         			0						--@NB_HEURE_REM
         		END					
         			
         		FETCH cu_dispositif_stagiaire INTO @ID_DISPOSITIF, @COD_DISPOSITIF, @BLN_PLAN
         	END
         	CLOSE cu_dispositif_stagiaire
         	DEALLOCATE cu_dispositif_stagiaire
         	
         	
         
         	
         	FETCH cu_stagiaire_pec 	INTO
         		@ID_ACTION_PEC			, 
         		@NUM_INTERNE_ACTION		,
         		@ID_MODULE_PEC			,
         		@ID_ETABLISSEMENT		,
         		@ID_INDIVIDU			,
         		@ID_SALARIE				,
         		@BLN_TUTEUR_INTERNE		,
         		@ID_TUTEUR				,
         		@ID_SALARIE_TUTEUR		,
         		@NB_H_ENGAGE_PL , @NB_H_HTT_PL , 	@ID_PUBLIC_PRIO_PL, @ID_OBJET_FORM_PL, @ID_ACTION_PRIO_PL, @ID_CATEG_ACTION_PL,
         		@NB_H_ENGAGE_PP , @NB_H_HTT_PP , @ID_PUBLIC_PRIO_PP, @ID_OBJET_FORM_PP, @ID_ACTION_PRIO_PP, @ID_CATEG_ACTION_PP,
         		@NB_H_ENGAGE_DIFP , @NB_H_HTT_DIFP , @ID_PUBLIC_PRIO_DIFP, @ID_OBJET_FORM_DIFP, @ID_ACTION_PRIO_DIFP, @ID_CATEG_ACT_DIFP,
         		@NB_H_ENGAGE_FORMT , @ID_OBJET_FORM_FORMT, 
         		@NB_H_ENGAGE_FTUT , @ID_OBJET_FORM_FTUT, 
         		@NB_H_ENGAGE_DIFNP,	@NB_H_HTT_DIFNP, @ID_OBJET_FORM_DIFNP, @ID_CATEG_ACT_DIFNP
         END
         
         
         CLOSE cu_stagiaire_pec 
         DEALLOCATE  cu_stagiaire_pec 
         /* Fin Traitements Stagiaires des modules PEC non rejet‚es */
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  39',  GETDATE(),  'FIN TRAITEMENT Stagiaires des modules PEC non rejet‚es ', 'NB REJETS ' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         	
         
         /* AJOUT COMMENTAIRES AU MODULE SUR SALARIES REJETES */
         DECLARE cu_commentaire_module_salarie_rejete CURSOR FOR
         SELECT ID_MODULE_PEC, NOM_INDIVIDU, PRENOM_INDIVIDU, MATRICULE 
         FROM EDI_PEC_ST 
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		ID_MODULE_PEC IS NOT NULL
         AND		ISNULL(BLN_REJET, 1) = 1
         
         OPEN cu_commentaire_module_salarie_rejete 
         FETCH cu_commentaire_module_salarie_rejete 
         INTO @ID_MODULE_PEC, @NOM_INDIVIDU, @PRENOM_INDIVIDU, @MATRICULE 
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         		SELECT	@COD_MODULE_PEC = COD_MODULE_PEC, @ID_ACTION_PEC = ID_ACTION_PEC
         		FROM	 MODULE_PEC
         		WHERE	ID_MODULE_PEC = @ID_MODULE_PEC 
         	
         		SET @COMMENTAIRE = 'EDI MODULE Nø:' + CAST(@COD_MODULE_PEC AS VARCHAR)
         		+ '. Le Salarie ' + @NOM_INDIVIDU + ' ' + @PRENOM_INDIVIDU + ' de matricule ' + @MATRICULE + ' a ete rejete lors de l''import EDI'
         		
         		EXEC INS_COMMENTAIRES
         		6							,	
         		@ID_ACTION_PEC				,
         		@COMMENTAIRE				, 
         		@ID_UTILISATEUR_ADH_EDI			-- Commentaires EDI Adherent 
         
         	FETCH cu_commentaire_module_salarie_rejete 
         	INTO @ID_MODULE_PEC, @NOM_INDIVIDU, @PRENOM_INDIVIDU, @MATRICULE 
         
         END
         
         CLOSE cu_commentaire_module_salarie_rejete 
         DEALLOCATE cu_commentaire_module_salarie_rejete 
         /* FIN AJOUT COMMENTAIRES AU MODULE SUR SALARIES REJETES */
         
         
         /* AJOUT COMMENTAIRES AU MODULE SUR SALARIES EN DOUBLON*/
         DECLARE cu_commentaire_module_salarie_doublon CURSOR FOR
         
         SELECT	EDI_PEC_ST.ID_MODULE_PEC, 
         		INDIVIDU.NOM_INDIVIDU,
         		INDIVIDU.PRENOM_INDIVIDU,
         		EDI_PEC_ST .MATRICULE,
         		AUTRE_MODULE.COD_MODULE_PEC
         		
         	
         FROM EDI_PEC_ST 
         INNER JOIN MODULE_PEC		EDI_MODULE		ON EDI_MODULE.ID_MODULE_PEC			= EDI_PEC_ST .ID_MODULE_PEC
         LEFT JOIN ETABLISSEMENT_OF	EDI_ETAB_OF		ON EDI_ETAB_OF	.ID_ETABLISSEMENT_OF= EDI_MODULE .ID_ETABLISSEMENT_OF
         
         INNER JOIN MODULE_PEC		AUTRE_MODULE	ON	AUTRE_MODULE.DAT_DEBUT			= EDI_MODULE.DAT_DEBUT 										-- Meme Date de Debut
         											AND AUTRE_MODULE.DAT_FIN			= EDI_MODULE.DAT_FIN										-- Meme Date de Fin
         											AND AUTRE_MODULE.NUM_DUREE_HEURE	= EDI_MODULE.NUM_DUREE_HEURE								-- Meme Duree
         											AND AUTRE_MODULE.ID_MODULE_PEC		!= EDI_MODULE.ID_MODULE_PEC									-- Module PEC different du module importe
         											AND AUTRE_MODULE.ID_ACTION_PEC		!= EDI_MODULE.ID_ACTION_PEC 								-- Action PEC different de l'action importee
         											AND AUTRE_MODULE.BLN_ACTIF			= 1															-- Module Actif					
         
         INNER JOIN ETABLISSEMENT_OF	AUTRE_MODUL_ETAB_OF		
         											ON AUTRE_MODUL_ETAB_OF	.ID_ETABLISSEMENT_OF	= AUTRE_MODULE .ID_ETABLISSEMENT_OF
         											AND ISNULL(EDI_ETAB_OF.ID_OF, -1) = ISNULL(AUTRE_MODUL_ETAB_OF.ID_OF, -1)	-- Meme OF
         											
         INNER JOIN STAGIAIRE_PEC					ON	STAGIAIRE_PEC.ID_MODULE_PEC = AUTRE_MODULE.ID_MODULE_PEC
         											AND STAGIAIRE_PEC.ID_SESSION_PEC IS NULL
         											AND STAGIAIRE_PEC.ID_ETABLISSEMENT= EDI_PEC_ST.ID_ETABLISSEMENT						-- Meme etablissement							
         											
         INNER JOIN INDIVIDU							ON STAGIAIRE_PEC .ID_INDIVIDU	= INDIVIDU.ID_INDIVIDU 
         											AND	INDIVIDU.NOM_INDIVIDU		= EDI_PEC_ST.NOM_INDIVIDU COLLATE FRENCH_CI_AI
         											AND	INDIVIDU.PRENOM_INDIVIDU	= EDI_PEC_ST.PRENOM_INDIVIDU COLLATE FRENCH_CI_AI
         											AND INDIVIDU.DAT_NAISSANCE		= EDI_PEC_ST.DAT_NAISSANCE
         											AND INDIVIDU.BLN_MASCULIN		= CASE WHEN EDI_PEC_ST.BLN_MASCULIN = 'O' THEN 1 ELSE 0 END							
         WHERE	ID_LOT_IMPORT = @ID_LOT_IMPORT
         AND		EDI_PEC_ST .ID_MODULE_PEC IS NOT NULL
         AND		EDI_PEC_ST .ID_SALARIE IS NOT NULL
         AND		ISNULL(EDI_PEC_ST .BLN_REJET, 1) = 0		-- Pas de rejet
         ORDER BY ID_LOT_IMPORT,
         EDI_MODULE.ID_MODULE_PEC DESC,
         INDIVIDU.NOM_INDIVIDU,
         INDIVIDU.PRENOM_INDIVIDU,
         AUTRE_MODULE.ID_MODULE_PEC
         
         
         OPEN cu_commentaire_module_salarie_doublon 
         FETCH cu_commentaire_module_salarie_doublon 
         INTO @ID_MODULE_PEC, @NOM_INDIVIDU, @PRENOM_INDIVIDU, @MATRICULE, @COD_MODULE_PEC_DOUBLON
         
         WHILE (@@FETCH_STATUS <> -1)
         BEGIN
         
         		SELECT	@COD_MODULE_PEC = COD_MODULE_PEC, @ID_ACTION_PEC = ID_ACTION_PEC
         		FROM	 MODULE_PEC
         		WHERE	ID_MODULE_PEC = @ID_MODULE_PEC 
         	
         		SET @COMMENTAIRE = 'EDI MODULE Nø:' + CAST(@COD_MODULE_PEC AS VARCHAR)
         		+ '. Le Salarie ' + @NOM_INDIVIDU + ' ' + @PRENOM_INDIVIDU + ' de matricule ' + @MATRICULE + ' est deja associe au module ' + @COD_MODULE_PEC_DOUBLON + ' pour les memes dates/dur‚e/OF'
         		
         		EXEC INS_COMMENTAIRES
         		6							,	
         		@ID_ACTION_PEC				,
         		@COMMENTAIRE				, 
         		@ID_UTILISATEUR_ADH_EDI			-- Commentaires EDI Adherent 
         
         	FETCH cu_commentaire_module_salarie_doublon 
         	INTO @ID_MODULE_PEC, @NOM_INDIVIDU, @PRENOM_INDIVIDU, @MATRICULE , @COD_MODULE_PEC_DOUBLON
         
         END
         
         CLOSE cu_commentaire_module_salarie_doublon 
         DEALLOCATE cu_commentaire_module_salarie_doublon 
         /* FIN AJOUT COMMENTAIRES AU MODULE SUR SALARIES EN DOUBLON */
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  40',  GETDATE(),  'FIN TRAITEMENT AJOUT COMMENTAIRES AU MODULE SUR SALARIES REJETES', 'NB REJETS ' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         INSERT INTO EDI_LOG
         (ID_LOT_IMPORT, NUM_LIGNE, LIB_PROBLEME)
         SELECT	@ID_LOT_IMPORT, TMP.NUM_LIGNE, LIB_PROBLEME = COL.LIB_COLONNE  + ' (' + COL.COD_COLONNE + '-col nø' + CAST(COL.NUM_POSITION AS VARCHAR) + ') : [' + ISNULL(TMP.VAL_COLONNE, '') + '] ' + TMP.LIB_PROBLEME         
         FROM	#TMP01 TMP
         JOIN	EDI_IMPORT_COLONNE COL        
         ON		TMP.ID_COLONNE = COL.ID_COLONNE
         ORDER BY NUM_LIGNE, TMP.ID_COLONNE
         
         
         /* GENERATION DES LOGS DE L'EDI*/
         EXEC EDI_GENERATION_LOG_EDI	@ID_LOT_IMPORT,	@ID_EDI_GROUPE_EDI_PEC		
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  41',  GETDATE(),  'FIN TRAITEMENT GENERATION DES LOGS DE L EDI', 'NB REJETS ' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
         
         /* AFFECTATION DES ACTIONS AUX AGF/CONSEILLER ET MAIL D'INFO ASSOCIE*/
         EXEC EDI_PEC_AFFECTION_ACTION @ID_LOT_IMPORT,	@ID_EDI_GROUPE_EDI_PEC		
         
         IF @BLN_DEBUG > 0
         SELECT 'DEBUG  42',  GETDATE(),  'FIN TRAITEMENT AFFECTATION DES ACTIONS AUX AGF/CONSEILLER ET MAIL D INFO ASSOCIE', 'NB REJETS ' , COUNT(*) FROM EDI_PEC_ST WHERE ID_LOT_IMPORT = @ID_LOT_IMPORT AND BLN_REJET = 1
         
GO         

         -- =============================================================
         -- Author		: WOOLLAMS
         -- Create date	: 23 janvier 2008
         -- Description	: Lettre de relance d'engagement pour l'of
         -- =============================================================
         -- Modif du 20/09/12 par DSZ - 13697 : num tel dynamique
         -- =============================================================
         -- LDE/OPA 23/05/2014 : #213 En tant qu'utilisateur OPTIFORM, 
         -- lorsque j'‚dite un courrier (PEC, PRO) … destination d'un Adh‚rent ou d'un OF, je veux que, 
         -- l'adresse … afficher dans la zone "correspondance" soit l'adresse TSA propre … ce type de courrier 
         -- si le mode "TSA" s'applique sinon qu'elle soit l'adresse actuelle
         -- =======================================================================================================================================
         
         CREATE PROCEDURE [dbo].[EDT_LETTRE_RELANCE_ENGAGEMENT_CONTRAT_PRO_OF]
         	@ID_ETABLISSEMENT	INT,
         	@ID_BENEFICIAIRE	INT,
         	@TYPE_BENEFICIAIRE	INT,
         	@ID_ADRESSE			INT,
         	@ID_CONTACT			INT,
         	@COD_CONTRAT_PRO	VARCHAR(10)
         AS
         BEGIN
         
         	DECLARE @relance INT
         	SET @relance = 1
         	
         	
         	-- Contact principal par d‚faut mais pas de "."
         	IF @ID_CONTACT IS NULL
         	SELECT @ID_CONTACT = NR31.ID_CONTACT
         	FROM NR31
         		INNER JOIN CONTACT ON CONTACT.ID_CONTACT = NR31.ID_CONTACT
         	WHERE BLN_PRINCIPAL = 1
         		AND BLN_ACTIF = 1
         		AND ID_ETABLISSEMENT = @ID_BENEFICIAIRE
         		AND CONTACT.LIB_NOM_CONTACT <> '.';
         
         	
         	SELECT	CONTRAT_PRO.ID_CONTRAT_PRO,
         			CONTRAT_PRO.COD_CONTRAT_PRO,
         			ADHERENT.COD_ADHERENT,
         			CONTRAT_PRO.LIB_LIEU_FORMATION,
         			CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_DEB_CONTRAT, 3) AS DAT_DEBUT,
         			CONVERT(VARCHAR(8),CONTRAT_PRO.DAT_FIN_CONTRAT, 3) AS DAT_FIN,
         			CM.LIB_NOM			as LIB_NOM_CONSEILLER,
         			CM.LIB_PNM			as LIB_PNM_CONSEILLER,
         			CR.ID_UTILISATEUR	as ID_UTIL,
         			CR.LIB_PNM			as LIB_PRENOM_CHARGE_RELATION,
         			CR.LIB_NOM			as LIB_NOM_CHARGE_RELATION,
         			CR.LIB_VILLE,
         			CR.EMAIL			as EMAIL_CHARGE_RELATION,
         			INDIVIDU.NOM_INDIVIDU, 
         			INDIVIDU.PRENOM_INDIVIDU
         	INTO	#TMP_CONTRAT_PRO 
         	FROM	CONTRAT_PRO
         	JOIN	SALARIE_PRO 
         	ON		CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO 
         	JOIN	INDIVIDU 
         	ON		SALARIE_PRO.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU
         	JOIN	ETABLISSEMENT 
         	ON		CONTRAT_PRO.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT 
         	JOIN	ADHERENT 
         	ON		ETABLISSEMENT.ID_ADHERENT = ADHERENT.ID_ADHERENT
         	JOIN	AGENCE ON CONTRAT_PRO.ID_AGENCE = AGENCE.ID_AGENCE
         	LEFT JOIN 
         			ETABLISSEMENT_OF 
         	ON		ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_ETABLISSEMENT
         	LEFT JOIN 
         			ORGANISME_FORMATION 
         	ON		ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF
         	left join
         			utilisateur CR -- charg‚ de relation
         	on		ETABLISSEMENT.id_chargee_relation = CR.id_utilisateur
         	left join
         			utilisateur CM -- charg‚ de mission
         	on		ETABLISSEMENT.id_chargee_mission = CM.id_utilisateur
         	WHERE   CONTRAT_PRO.COD_CONTRAT_PRO = @COD_CONTRAT_PRO;
         
         	
         	SELECT  MODULE_PRO.ID_MODULE_PRO, 
         			MODULE_PRO.COD_MODULE_PRO, 
         			MODULE_PRO.BLN_SUBROGE, 
         			MODULE_PRO.DAT_DEBUT, 
         			MODULE_PRO.DAT_FIN, 
         			MODULE_PRO.NB_UNITE_FORMATION, 
         			MODULE_PRO.BLN_OK_PIECE,
         			MODULE_PRO.LIBL_MODULE_PRO, 
         			MODULE_PRO.NB_UNITE_TOTALE,
         			ORGANISME_FORMATION.COD_OF,
         			ORGANISME_FORMATION.LIB_RAISON_SOCIALE
         	INTO	#TMP_MODULE_PRO  
         	FROM	#TMP_CONTRAT_PRO 
         			INNER JOIN MODULE_PRO ON #TMP_CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         									AND MODULE_PRO.BLN_SUBROGE = 1 AND MODULE_PRO.BLN_OK_PIECE = 0 
         			INNER JOIN ETABLISSEMENT_OF ON MODULE_PRO.ID_ETABLISSEMENT_OF = ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF
         						and MODULE_PRO.ID_ETABLISSEMENT_OF =@ID_ETABLISSEMENT
         			INNER JOIN ORGANISME_FORMATION ON ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF;
         
         	
         	SELECT	#TMP_MODULE_PRO.COD_MODULE_PRO,
         			#TMP_MODULE_PRO.LIBL_MODULE_PRO,
         			#TMP_MODULE_PRO.LIB_RAISON_SOCIALE,
         			CONVERT(VARCHAR(8),#TMP_MODULE_PRO.DAT_DEBUT, 3) AS DAT_DEBUT,
         			dbo.GetFrenchCurrencyFormat(CAST(#TMP_MODULE_PRO.NB_UNITE_TOTALE as MONEY))  AS NB_UNITE_FORMATION,
         			PIECE_PRO.ID_PIECE_PRO, 
         			PIECE_PRO.LIBC_PIECE_PRO, 
         			PIECE_PRO.LIBL_PIECE_PRO,
         			ARRIVEE_PIECE_PRO.ID_ARRIVEE_PIECE_PRO, 
         			ARRIVEE_PIECE_PRO.BLN_ACTIF,
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_1,
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_2,
         			ARRIVEE_PIECE_PRO.DAT_RELANCE_ENGAGE_3
         	INTO	#TMP_PIECES_MODULE_PRO
         	FROM	#TMP_MODULE_PRO
         			INNER JOIN ARRIVEE_PIECE_PRO ON #TMP_MODULE_PRO.ID_MODULE_PRO = ARRIVEE_PIECE_PRO.ID_MODULE_PRO 
         			INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         					AND PIECE_PRO.BLN_MODULE = 1 AND PIECE_PRO.BLN_CONTRAT = 0 AND PIECE_PRO.BLN_ACTIF=1 AND PIECE_PRO.BLN_BLOQUANT_ENGAGEMENT = 1
         	ORDER BY #TMP_MODULE_PRO.DAT_DEBUT;
         
         		select	@relance =	case 
         								when DAT_RELANCE_ENGAGE_2 is not null then 3
         								when DAT_RELANCE_ENGAGE_1 is not null then 2
         								else 1
         							end
         		from	#TMP_PIECES_MODULE_PRO;
         
         
         	
         	WITH XMLNAMESPACES (
         		DEFAULT 'EDT_LETTRE_RELANCE_ENGAGEMENT_CONTRAT_PRO_OF'
         	)
         			
         	SELECT
         			-- R‚cup‚ration des informations sur le contact et le b‚n‚ficiaire
         			dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) as BENEFICIAIRE,
         			(
         				SELECT @relance
         				FOR XML RAW('NUM_RELANCE'), ELEMENTS, TYPE
         			),
         			(
         				SELECT	              	
         					
         					(
         						SELECT	isnull(COD_ADHERENT, '')				as COD_ADHERENT,
         								isnull(LIB_PRENOM_CHARGE_RELATION, '')	as LIB_PRENOM_CONTACT,
         								isnull(LIB_NOM_CHARGE_RELATION, '')		as LIB_NOM_CONTACT,
         								isnull(EMAIL_CHARGE_RELATION, '')		as EMAIL,
         								-- R‚cup‚ration des informations sur l'‚metteur
         								dbo.GetXmlAdrUtilAvecTel(ENTETE.ID_UTIL, 0, 0) as EMETTEUR,
         								(
         									SELECT	TOP 1 COD_OF
         									FROM #TMP_MODULE_PRO
         									FOR XML PATH(''), TYPE
         								),						
         								-- r‚f‚rence du contrat pro
         								(
         									SELECT	#TMP_CONTRAT_PRO.COD_CONTRAT_PRO
         									FROM	#TMP_CONTRAT_PRO
         									FOR XML PATH(''), TYPE 
         								),
         								rtrim(case when patindex('%CEDEX%', LIB_VILLE) <> 0 then left(LIB_VILLE, patindex('%CEDEX%', LIB_VILLE)-1) else LIB_VILLE end) as LIB_VILLE, -- retraitement de la ville au cas o— de la forme COMMUNE CEDEX 999
         								dbo.GetFullDate(getDate()) as DATE,		
         								(
         									SELECT	top 1
         											dbo.GetContactSalutation(@ID_CONTACT, 1)
         									FROM CIVILITE
         									FOR XML PATH('POLITESSE_HAUT'), TYPE
         								)
         
         						FROM #TMP_CONTRAT_PRO AS REFERENCE     
         						FOR XML AUTO, ELEMENTS, TYPE
         					)
         				FROM #TMP_CONTRAT_PRO AS ENTETE     
         				FOR XML AUTO, ELEMENTS, TYPE
         			),
         
         			(
         				
         				
         				
         				SELECT	CORPS.COD_CONTRAT_PRO,
         						CORPS.PRENOM_INDIVIDU,
         						CORPS.NOM_INDIVIDU,
         						CORPS.DAT_DEBUT,
         						CORPS.DAT_FIN,
         						CORPS.LIB_LIEU_FORMATION,
         						CASE WHEN @relance = 3 THEN(
         							SELECT 'Sans r‚ponse de votre part sous quinzaine nous cl“turerons ce dossier.'
         							FOR XML PATH('PREVENTION'), TYPE 
         						)END ,
         						CASE WHEN ((SELECT  COUNT(*) FROM #TMP_PIECES_MODULE_PRO) > 0) THEN (
         							SELECT 
         								(
         									SELECT DISTINCT	#TMP_PIECES_MODULE_PRO.DAT_DEBUT,
         													#TMP_PIECES_MODULE_PRO.NB_UNITE_FORMATION,
         													#TMP_PIECES_MODULE_PRO.COD_MODULE_PRO, 
         													--#TMP_PIECES_MODULE_PRO.LIB_RAISON_SOCIALE,
         													#TMP_PIECES_MODULE_PRO.LIBL_MODULE_PRO, 
         													#TMP_PIECES_MODULE_PRO.LIBC_PIECE_PRO
         									FROM		#TMP_PIECES_MODULE_PRO
         									WHERE		#TMP_PIECES_MODULE_PRO.BLN_ACTIF = 0 --CELUI DE ARRIVEE_PIECE_PRO
         									FOR XML RAW('PIECE_MANQUANTE'), ELEMENTS, TYPE, ROOT('PIECES_MANQUANTES')
         								),
         								(
         									SELECT DISTINCT	#TMP_PIECES_MODULE_PRO.DAT_DEBUT,
         													#TMP_PIECES_MODULE_PRO.NB_UNITE_FORMATION,
         													#TMP_PIECES_MODULE_PRO.COD_MODULE_PRO, 
         													--#TMP_PIECES_MODULE_PRO.LIB_RAISON_SOCIALE,
         													#TMP_PIECES_MODULE_PRO.LIBL_MODULE_PRO, 
         													#TMP_PIECES_MODULE_PRO.LIBC_PIECE_PRO,
         													MOTIF_NON_CONFORM_PIECE_PRO.LIBC_MOTIF_NON_CONFORM_PIECE_PRO
         									FROM    #TMP_PIECES_MODULE_PRO
         											INNER JOIN NR410 ON #TMP_PIECES_MODULE_PRO.ID_ARRIVEE_PIECE_PRO = NR410.ID_ARRIVEE_PIECE_PRO
         											INNER JOIN MOTIF_NON_CONFORM_PIECE_PRO ON NR410.ID_MOTIF_NON_CONFORM_PIECE_PRO = MOTIF_NON_CONFORM_PIECE_PRO.ID_MOTIF_NON_CONFORM_PIECE_PRO
         									WHERE		#TMP_PIECES_MODULE_PRO.BLN_ACTIF = 1 --CELUI DE ARRIVEE_PIECE_PRO
         									FOR XML RAW('PIECES_NON_CONFORME'), ELEMENTS, TYPE, ROOT('PIECES_NON_CONFORMES')
         								)
         							FOR XML RAW('TABLEAU_MODULE'),ELEMENTS, TYPE
         						)END,
         						(
         							SELECT	top 1 dbo.GetContactSalutation(@ID_CONTACT, 0)
         							FROM CIVILITE
         							FOR XML PATH('POLITESSE_BAS'), TYPE
         						)
         				FROM #TMP_CONTRAT_PRO AS CORPS     
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

         CREATE PROCEDURE AIDE_RECUPERATION_MONTANTS_DUS 
         (
         	@ID_PERIODE int,
         	@ID_ADHERENT int,
         	@MASSE_SALARIALE float
         )
         AS
         BEGIN
         	DECLARE @ID_ACTIVITE int;						-- Identifiant de l'activit? en cours
         	DECLARE @LIB_ACTIVITE varchar(100);				-- Libell? de l'activit? en cours
         	DECLARE @TAUX_GLOBAL_APPEL float;				-- Taux relatif ? l'activit? en cours
         	DECLARE @TOTAL_DU float;						-- Total d? dans l'activit? en cours
         	DECLARE @MNT_HT_REGLE float;					-- Montant r?gl?
         	DECLARE @MNT_HT_TOTAL_REGLE float;				-- Montant r?gl? total
         	DECLARE @MNT_HT_DEDUCTION float;				-- Montant d?duit
         	DECLARE @MNT_HT_TOTAL_DEDUCTION float;			-- Montant d?duit total
         	DECLARE @MTN_A_REGLER float;					-- Montant total restant ? r?gler
         	DECLARE @TAUX_TVA float;						-- Taux de la TVA
         	DECLARE @MONTANT_TVA float						-- Montant de la TVA d?
         	
         	CREATE TABLE #MONTANTS_DUS
         	(
         		LIB_ACTIVITE	varchar(60),
         		MNT_DU			float
         	);
         
         --	PRINT 'Masse salariale de r?f?rence : ' + @MASSE_SALARIALE + ' ?'
         --	PRINT ''
         	SET @MTN_A_REGLER = 0;
         	DECLARE CURSOR_ACTIVITE CURSOR FOR 
         	SELECT
         		ACTIVITE.ID_ACTIVITE,
         		ACTIVITE.LIBL_ACTIVITE
         	FROM
         		R19	INNER JOIN ACTIVITE	ON R19.ID_ACTIVITE = ACTIVITE.ID_ACTIVITE
         	WHERE
         		R19.ID_PERIODE = @ID_PERIODE
         		AND R19.ID_ADHERENT = @ID_ADHERENT	
         	
         	-- Parcours des activit?s concern?es pour trouver le montant d?
         	OPEN CURSOR_ACTIVITE
         	FETCH NEXT FROM CURSOR_ACTIVITE INTO @ID_ACTIVITE, @LIB_ACTIVITE
         	WHILE @@FETCH_STATUS = 0
         	BEGIN
         		SELECT
         				@TAUX_GLOBAL_APPEL = TAU_GLO_APPEL
         		FROM
         				PARAMETRE_GLOBAL	INNER JOIN BRANCHE	ON PARAMETRE_GLOBAL.ID_REGIME = BRANCHE.ID_REGIME
         									INNER JOIN ADHERENT	ON BRANCHE.ID_BRANCHE = ADHERENT.ID_BRANCHE
         									INNER JOIN R20		ON PARAMETRE_GLOBAL.ID_TYPE_ASSUJETISSEMENT = R20.ID_TYPE_ASSUJETISSEMENT
         		WHERE
         				ADHERENT.ID_ADHERENT = @ID_ADHERENT
         			AND	R20.ID_ADHERENT = ADHERENT.ID_ADHERENT
         			AND	R20.ID_PERIODE = @ID_PERIODE
         			AND	PARAMETRE_GLOBAL.ID_PERIODE = @ID_PERIODE
         			AND	PARAMETRE_GLOBAL.ID_ACTIVITE = @ID_ACTIVITE
         		
         		-- Calcule du montant total d?
         --		PRINT @LIB_ACTIVITE + ' : ' + cast(@TAUX_GLOBAL_APPEL as varchar(10)) + '%'
         		SET @TOTAL_DU = @TAUX_GLOBAL_APPEL * @MASSE_SALARIALE
         --		PRINT 'D? total : ' + LTRIM(STR(@TOTAL_DU, 10, 2)) + ' ?'
         		
         		-- Calcule du montant d?j? r?gl?
         		DECLARE CURSOR_MONTANT_REGLE CURSOR FOR
         		SELECT
         				POSTE_IMPUTATION.MNT_HT
         		FROM
         				POSTE_IMPUTATION	INNER JOIN POSTE_VERSEMENT	ON	POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT
         									INNER JOIN VERSEMENT		ON	POSTE_VERSEMENT.ID_VERSEMENT = VERSEMENT.ID_VERSEMENT
         		WHERE
         				POSTE_IMPUTATION.ID_ACTIVITE = @ID_ACTIVITE
         			AND	POSTE_VERSEMENT.BLN_ACTIF = 1
         			AND POSTE_VERSEMENT.ID_ADHERENT_BENEFICIAIRE = @ID_ADHERENT
         			AND VERSEMENT.BLN_ACTIF = 1
         			AND VERSEMENT.BLN_IMPAYE= 0
         			AND POSTE_VERSEMENT.ID_PERIODE = @ID_PERIODE
         		
         		SET @MNT_HT_TOTAL_REGLE = 0
         		OPEN CURSOR_MONTANT_REGLE
         		FETCH NEXT FROM CURSOR_MONTANT_REGLE INTO @MNT_HT_REGLE
         		WHILE @@FETCH_STATUS = 0
         			BEGIN
         				SET @MNT_HT_TOTAL_REGLE = @MNT_HT_TOTAL_REGLE + @MNT_HT_REGLE
         				SET @TOTAL_DU = @TOTAL_DU - @MNT_HT_REGLE
         --				PRINT '  D?j? pay? : ' + LTRIM(STR(@MNT_HT_REGLE, 10, 2)) + ' ?'
         				FETCH NEXT FROM CURSOR_MONTANT_REGLE INTO @MNT_HT_REGLE
         			END
         --		PRINT '  Montant total d?j? r?gl? : ' +  + LTRIM(STR(@MNT_HT_TOTAL_REGLE, 10, 2)) + ' ?'
         --		PRINT '  Reste ? payer : ' + LTRIM(STR(@TOTAL_DU, 10, 2)) + ' ?'
         		CLOSE CURSOR_MONTANT_REGLE
         		DEALLOCATE CURSOR_MONTANT_REGLE
         
         		-- Calcule des d?ductions
         		DECLARE CURSOR_DEDUCTION CURSOR FOR
         		SELECT
         				DEDUCTION.MNT_HT
         		FROM
         				DEDUCTION
         		WHERE
         				DEDUCTION.ID_ADHERENT = @ID_ADHERENT
         			AND	DEDUCTION.ID_PERIODE = @ID_PERIODE
         			AND	DEDUCTION.ID_ACTIVITE = @ID_ACTIVITE
         		
         		SET @MNT_HT_TOTAL_DEDUCTION = 0
         		OPEN CURSOR_DEDUCTION
         		FETCH NEXT FROM CURSOR_DEDUCTION INTO @MNT_HT_DEDUCTION
         		WHILE @@FETCH_STATUS = 0
         			BEGIN
         				SET @MNT_HT_TOTAL_DEDUCTION = @MNT_HT_TOTAL_DEDUCTION + @MNT_HT_DEDUCTION
         				SET @TOTAL_DU = @TOTAL_DU - @MNT_HT_DEDUCTION
         --				PRINT '  D?duction : ' + LTRIM(STR(@MNT_HT_DEDUCTION, 10, 2)) + ' ?'
         				FETCH NEXT FROM CURSOR_DEDUCTION INTO @MNT_HT_DEDUCTION
         			END
         --		PRINT '  Montant total d?ductions : ' +  + LTRIM(STR(@MNT_HT_TOTAL_DEDUCTION, 10, 2)) + ' ?'
         --		PRINT '  Reste ? payer : ' + LTRIM(STR(@TOTAL_DU, 10, 2)) + ' ?'
         		CLOSE CURSOR_DEDUCTION
         		DEALLOCATE CURSOR_DEDUCTION
         		
         		INSERT INTO #MONTANTS_DUS values (@LIB_ACTIVITE, @TOTAL_DU)
         --		SET @MTN_A_REGLER = @MTN_A_REGLER + @TOTAL_DU
         		
         --		PRINT ''
         		FETCH NEXT FROM CURSOR_ACTIVITE INTO @ID_ACTIVITE, @LIB_ACTIVITE
         	END
         	CLOSE CURSOR_ACTIVITE
         	DEALLOCATE CURSOR_ACTIVITE
         
         	-- Add the SELECT statement with parameter references here
         	SELECT * FROM #MONTANTS_DUS
         END
         
         
	-- =============================================    
         -- Author:  APA    
         -- Create date: 17/02/2012    
         -- Description: Enregistrement des associations module / facture    
         -- =============================================    
         -- Author:  EOU    
         -- Create date: 17/10/2012    
         -- Description: 14047    
         -- =============================================    
         -- =============================================    
         -- Author:  MBL  
         -- Modif date: 10/01/2013    
         -- Description: Rattachement automatique de l'etablissement OF de la facture au l'action   
         --    lorsqu 'il s'agit d'une action/contrat de reprise NESSIE  
         -- =============================================    
         -- Author:  MBL  
         -- Modif date: 24/01/2013    
         -- Description: Correction Bug lie au rattachement des factures associees a un etablissement ADH
         -- =============================================    
         CREATE PROCEDURE [dbo].[INS_MODULE_FACTURE]    
          @ID_FACTURE INT,    
          @ID_MODULE_PEC INT,    
          @ID_MODULE_PRO INT    
         AS    
           
         BEGIN    
              
          IF @ID_MODULE_PEC IS NOT NULL   
           AND NOT EXISTS (  
               SELECT 1     
               FROM MODULE_FACTURE     
               WHERE     
               ID_MODULE_PEC = @ID_MODULE_PEC   
               AND ID_FACTURE = @ID_FACTURE  
               )        
          BEGIN    
           
         	INSERT INTO MODULE_FACTURE (ID_MODULE_PRO,ID_MODULE_PEC, ID_FACTURE)    
         	VALUES (@ID_MODULE_PRO, @ID_MODULE_PEC, @ID_FACTURE)    
         
         
         	-- Rajout de l'etablissement OF de la facture comme etablissement OF potentiel du module PEC associe    
         	INSERT INTO REPRISE_MODULE_NESSIE  
         	(ID_ACTION_PEC, ID_ETABLISSEMENT_OF)  
         	SELECT DISTINCT ACTION_PEC.ID_ACTION_PEC, FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF  
         	FROM ACTION_PEC   
         	INNER JOIN MODULE_PEC    ON ACTION_PEC .ID_ACTION_PEC  = MODULE_PEC.ID_ACTION_PEC  
         	INNER JOIN FACTURE     ON FACTURE.ID_FACTURE = @ID_FACTURE  
         	LEFT  JOIN REPRISE_MODULE_NESSIE ON REPRISE_MODULE_NESSIE.ID_ETABLISSEMENT_OF = FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF   
         		   AND REPRISE_MODULE_NESSIE.ID_ACTION_PEC   = MODULE_PEC.ID_ACTION_PEC  
         	WHERE MODULE_PEC.ID_MODULE_PEC = @ID_MODULE_PEC  
         	AND  ACTION_PEC.BLN_REPRISE_NESSIE = 1  
         	AND   REPRISE_MODULE_NESSIE.ID_ACTION_PEC IS NULL  
         	AND FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF  IS NOT NULL
         
          END  
            
          IF @ID_MODULE_PRO IS NOT NULL   
           AND NOT EXISTS (  
               SELECT 1     
               FROM MODULE_FACTURE     
               WHERE     
               ID_MODULE_PEC = @ID_MODULE_PRO  
               AND ID_FACTURE = @ID_FACTURE  
               )    
          BEGIN  
         
         		INSERT INTO MODULE_FACTURE (ID_MODULE_PRO,ID_MODULE_PEC, ID_FACTURE)    
         		VALUES (@ID_MODULE_PRO, @ID_MODULE_PEC, @ID_FACTURE)    
          
         		INSERT INTO REPRISE_CONTRAT_NESSIE  
         		(ID_CONTRAT_PRO, ID_ETABLISSEMENT_OF)  
         		SELECT DISTINCT CONTRAT_PRO.ID_CONTRAT_PRO, FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF  
         		FROM CONTRAT_PRO  
         		INNER JOIN MODULE_PRO    ON CONTRAT_PRO.ID_CONTRAT_PRO= MODULE_PRO.ID_CONTRAT_PRO  
         		INNER JOIN FACTURE     ON FACTURE.ID_FACTURE = @ID_FACTURE  
         		LEFT  JOIN REPRISE_CONTRAT_NESSIE ON REPRISE_CONTRAT_NESSIE.ID_ETABLISSEMENT_OF =  FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF   
         			   AND REPRISE_CONTRAT_NESSIE.ID_CONTRAT_PRO  = MODULE_PRO.ID_CONTRAT_PRO  
         		WHERE MODULE_PRO.ID_MODULE_PRO = @ID_MODULE_PRO  
         		AND  CONTRAT_PRO.BLN_REPRISE_NESSIE = 1  
         		AND   REPRISE_CONTRAT_NESSIE.ID_CONTRAT_PRO IS NULL  
         		AND FACTURE.ID_EMETTEUR_ETABLISSEMENT_OF  IS NOT NULL
          END  
            
         SELECT 0 --success    
         END

		 
		 
		CREATE PROCEDURE [dbo].[LEC_DET_TRANSACTION]
          @ID_TRANSACTION int
         AS
         --===========================================
         -- DSZ 06/09/2011 12839
         -- voir C2P_SFD_GESTION TRANSACTION_2.2.doc du 05/09/2011
         -- "une transaction de sous-type rŠglement, on contr“le ses d‚pENDances : 
         -- si au moins une ® partie (ADH ou OF) de sessions pro ¯ ou demANDe de rŠglement 
         -- … laquelle elle est rattach‚e est Bap‚e et non r‚gl‚e (ou dont le rŠglement n'est pas valid‚): 
         -- desactivation impossible"
         -- recuperation de cette info
         --===========================================
         -- EOU 03/04/2012 ajout LIBL_TRANSACTION et BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE
         -- 13323 
         -- =============================================
         -- HBO - 141113 - M16371: Lot 1 - ModIFication structure de donn‚es / proc‚dures stock‚es
         -- =============================================
         -- HBO - #803 - Read only id_contact without name
         -- =============================================
         BEGIN
          DECLARE
           @BLN_DESACTIVATION_POSSIBLE int,
           @ID_SOUS_TYPE int,
           @id int = 0
         
          SELECT
           @ID_SOUS_TYPE = ID_SOUS_TYPE_TRANSACTION 
          FROM
           [TRANSACTION]
          WHERE
           ID_TRANSACTION = @ID_TRANSACTION
         
          IF (@ID_SOUS_TYPE <> 4) --reglement
          BEGIN
           SET @BLN_DESACTIVATION_POSSIBLE = 1
          END
          ELSE
          BEGIN
           SELECT top 1
            @id = ID_POSTE_COUT_REGLE
           FROM
            POSTE_COUT_REGLE
            left join reglement
             on POSTE_COUT_REGLE.ID_REGLEMENT = REGLEMENT.ID_REGLEMENT
           WHERE
            POSTE_COUT_REGLE.ID_TRANSACTION = @ID_TRANSACTION
            AND DAT_BAP is not null
            AND (POSTE_COUT_REGLE.ID_REGLEMENT is null or REGLEMENT.DAT_VALID_REGLEMENT is null)
         
           IF (@id <> 0 AND @id is not null)
           BEGIN
            SET @BLN_DESACTIVATION_POSSIBLE = 0
           END
           ELSE --pcr pas trouv‚, on cherche sessions pro
           BEGIN
            SELECT top 1
             @id = ID_SESSION_PRO
            FROM
             SESSION_PRO
             LEFT JOIN REGLEMENT_PRO AS REGL_ADH
              ON REGL_ADH.ID_REGLEMENT_PRO = ID_REGLEMENT_PRO_ADH
             LEFT JOIN REGLEMENT_PRO AS REGL_OF
              ON REGL_OF.ID_REGLEMENT_PRO = ID_REGLEMENT_PRO_OF
            WHERE 
             (
              SESSION_PRO.ID_TRANSACTION_ADH = @ID_TRANSACTION
              AND
              (
               DAT_BAP_ADH IS NOT NULL
               AND
               (
                ID_REGLEMENT_PRO_ADH is null
                or regl_adh.DAT_VALID_REGLEMENT is null
               )
              )
             )
             OR
             (
              SESSION_PRO.ID_TRANSACTION_OF= @ID_TRANSACTION
              AND
              (
               DAT_BAP_OF is not null
               AND
               (
                ID_REGLEMENT_PRO_OF is null
                or regl_of.DAT_VALID_REGLEMENT is null
               )
              )
             )
         
            IF (@id <> 0 AND @id is not null)
            BEGIN
             SET @BLN_DESACTIVATION_POSSIBLE = 0
            END
            ELSE
            BEGIN
             SET @BLN_DESACTIVATION_POSSIBLE = 1
            END
           END
          END
         
          SELECT
           [TRANSACTION].ID_ADRESSE,
           [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF,
           [TRANSACTION].ID_TIERS_BENEF,
           [TRANSACTION].ID_ETABLISSEMENT_BENEF,
           [TRANSACTION].ID_CONTACT,
           [TRANSACTION].ID_ETABLISSEMENT_OF_DEST,
           [TRANSACTION].ID_ETABLISSEMENT_DEST,
           [TRANSACTION].ID_TRANSACTION,
           [TRANSACTION].ID_TYPE_TRANSACTION,
           [TRANSACTION].ID_SOUS_TYPE_TRANSACTION,
           [TRANSACTION].ID_ACTIVITE,
           [TRANSACTION].BLN_ACTIF,
           [TRANSACTION].NUM_IBAN,
           [TRANSACTION].BLN_TRANSACTION_REGLEMENT_PRINCIPAL,
           [TRANSACTION].LIBL_TRANSACTION,
           [TRANSACTION].BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE,
           [TRANSACTION].DAT_MODIF,
           [TRANSACTION].TIME_STAMP,
           [TRANSACTION].DAT_CREATION,
           [TRANSACTION].ID_UTILISATEUR,
           [TRANSACTION].ID_MODE_ENVOI_DOC,
           [TRANSACTION].BIC,
           UTILISATEUR.COD_UTIL,
           CASE
            WHEN [TRANSACTION].BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE = 0 THEN  ADRESSE.LIB_ADR +' '+ ADRESSE.LIB_CP_CEDEX+' '+ADRESSE.LIB_VIL_CEDEX 
            ELSE null 
           END AS ADRESSE_BENEFICIAIRE,
           ETABLISSEMENT_DESTINATAIRE.ID_ADHERENT AS ID_ADHERENT_DEST,
           ETABLISSEMENT_OF_DESTINATAIRE.ID_OF AS ID_OF_DEST,
           ETABLISSEMENT_BENEFICIAIRE.ID_ADHERENT AS ID_ADHERENT_BENEF,
           ETABLISSEMENT_OF_BENEFICIAIRE.ID_OF AS ID_OF_BENEF,
           @BLN_DESACTIVATION_POSSIBLE as BLN_DESACTIVATION_POSSIBLE
          FROM
           [TRANSACTION]
           LEFT OUTER JOIN ETABLISSEMENT AS ETABLISSEMENT_BENEFICIAIRE
            ON [TRANSACTION].ID_ETABLISSEMENT_BENEF = ETABLISSEMENT_BENEFICIAIRE.ID_ETABLISSEMENT
           LEFT OUTER JOIN ETABLISSEMENT_OF AS ETABLISSEMENT_OF_BENEFICIAIRE
            ON [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF = ETABLISSEMENT_OF_BENEFICIAIRE.ID_ETABLISSEMENT_OF
           LEFT OUTER JOIN UTILISATEUR
            ON [TRANSACTION].ID_UTILISATEUR = UTILISATEUR.ID_UTILISATEUR
           LEFT OUTER JOIN ETABLISSEMENT AS ETABLISSEMENT_DESTINATAIRE
            ON [TRANSACTION].ID_ETABLISSEMENT_DEST = ETABLISSEMENT_DESTINATAIRE.ID_ETABLISSEMENT
           LEFT OUTER JOIN ETABLISSEMENT_OF AS ETABLISSEMENT_OF_DESTINATAIRE
            ON [TRANSACTION].ID_ETABLISSEMENT_OF_DEST = ETABLISSEMENT_OF_DESTINATAIRE.ID_ETABLISSEMENT_OF
           LEFT JOIN ADRESSE
            on ADRESSE.ID_ADRESSE = [TRANSACTION].ID_ADRESSE
          WHERE
           [TRANSACTION].ID_TRANSACTION = @ID_TRANSACTION
         END

         -- =============================================
         -- Author:		DSZ
         -- Create date: 24/02/2012
         -- Description:	lec det module PEC pour le code donn‚. remplir le code avec '0' si besoin
         -- =============================================
         -- DSZ 13313 ajout BLN_DETACHABLE
         -- =============================================
         
         CREATE PROCEDURE [dbo].[LEC_DET_MODULE_PEC_PAR_CODE] 
         	@COD_MODULE varchar(14),
         	@ID_FACTURE int
         AS
         BEGIN
         	SET NOCOUNT ON;
         	declare @len int
         	select @len = LEN(@COD_MODULE)
         	if (@len < 14)
         		set @COD_MODULE = REPLICATE('0',14-@len)+ @COD_MODULE
         		
         	SELECT 
         		MODULE_PEC.ID_MODULE_PEC,
         		COD_MODULE_PEC,
         		LIBL_MODULE_PEC,
         		@ID_FACTURE as ID_FACTURE,
         		case when COUNT(POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE) > 0 then 0 else 1 end as BLN_DETACHABLE
         	FROM 
         		MODULE_PEC 
         		left join POSTE_COUT_REGLE on (POSTE_COUT_REGLE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         										and POSTE_COUT_REGLE.ID_FACTURE = @ID_FACTURE
         										and POSTE_COUT_REGLE.BLN_ACTIF = 1)
         	where
         		COD_MODULE_PEC = @COD_MODULE
         	group by
         		MODULE_PEC.ID_MODULE_PEC,
         		COD_MODULE_PEC,
         		LIBL_MODULE_PEC
         END


         ----------------------------------------------  
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         ----------------------------------------------  
         
         CREATE PROCEDURE [dbo].[UPD_DOTATION]  
         	@ID_DOTATION INT,  
         	@LIBC_DOTATION VARCHAR(20),  
         	@LIBL_DOTATION VARCHAR(50),  
         	@ID_FINANCEUR INT,
         	@ID_ENVELOPPE INT,
         	@MNT_PROVISIONNEL DECIMAL(18,2),
         	@MNT_ENGAGE DECIMAL(18,2),
         	@MNT_REEL DECIMAL(18,2),
         	@ID_UTILISATEUR INT,
         	@DAT_DOTATION DATETIME,
         	@DAT_DECISION DATETIME,
         	@COM_DOTATION VARCHAR(255),
         	@BLN_EXTERNE TINYINT,
         	@TIME_STAMP TIMESTAMP
         	
         AS 
          
         UPDATE DOTATION SET
         LIBC_DOTATION = @LIBC_DOTATION,
         LIBL_DOTATION = @LIBL_DOTATION,  
         DAT_DOTATION = @DAT_DOTATION, 
         DAT_DECISION = @DAT_DECISION,
         DAT_MODIF = GETDATE(),
         ID_ENVELOPPE = @ID_ENVELOPPE,
         ID_TIERS_FINANCEUR = @ID_FINANCEUR,
         ID_UTILISATEUR = @ID_UTILISATEUR,
         MNT_PREVISIONNEL = @MNT_PROVISIONNEL,
         MNT_ENGAGE = @MNT_ENGAGE,
         MNT_REEL = @MNT_REEL,
         COM_DOTATION = @COM_DOTATION,
         BLN_EXTERNE = @BLN_EXTERNE
         FROM DOTATION
         WHERE 
         	ID_DOTATION  = @ID_DOTATION AND
         	TIME_STAMP = @TIME_STAMP
         
         IF @@ROWCOUNT = 0   
         BEGIN  
            IF EXISTS(SELECT * FROM DOTATION WHERE ID_DOTATION  = @ID_DOTATION)      
            BEGIN  
               /* Problme de Concurrence d'accs */  
               RAISERROR('Problme de Concurrence d''accs', 16, 1)
               RETURN -1  
            END     
         END
            
GO           
           
         -- =============================================  
         -- Author		 : KW  
         -- Create date   : 16 octobre 2007  
         -- Description   : Lecture des iban par rapport a une session pro  
         -- =============================================  
         -- Author		 : RMA & ASD  
         -- Create date   : 04 decembre 2008  
         -- Description   : pas de traitement transaction principale + La transaction de la facture est pris ene compte  
         -- =============================================  
         -- Author		 : AMA 
         -- Create date   : 08 d‚cembre 2008  
         -- Description   : Le destinataire n'est pas l'‚tablissement principal mais l'‚tablissement
         --				   lui mˆme
         -- =============================================  
         -- Author		 : RMA 
         -- Create date   : 16 d‚cembre 2008  
         -- Description   : ajout d'un champ ChŠque inactif, pour la reprise
         -- =============================================  
         
           
         CREATE PROCEDURE [dbo].[LEC_GRP_IBAN_SESSION_PRO]  
          @ID_MODULE_PRO INT,  
          @ID_SESSION_PRO INT,  
          @TYPE INT, --TYPE 0 = adherent, TYPE 1 = OF  
          @BLN_TRANSACTION_REGLEMENT_PRINCIPAL TINYINT  
         AS  
         BEGIN  
           
          CREATE TABLE #TEMP_INFO_SESSION  
           (  
            ID_SESSION_PRO		INT,  
            ID_MODULE_PRO		INT,  
            ID_ETABLISSEMENT_OF	INT,  
            ID_ETABLISSEMENT		INT,  
            ID_ADHERENT			INT,  
         --AMA
            ID_ETABLISSEMENT_ETABLISSEMENT	INT,  
         --AMA
            NUM_VIREMENT			INT,  
            NUM_CHEQUE			VARCHAR(10), 
            DAT_VALID_REGLEMENT	DATETIME,  
            ID_REGLEMENT_PRO		INT,  
            ID_TRANSACTION		INT
           )   
           
           
          IF @ID_SESSION_PRO IS NULL   
           BEGIN  
            INSERT INTO  #TEMP_INFO_SESSION  
            SELECT DISTINCT  SESSION_PRO.ID_SESSION_PRO,  
                 MODULE_PRO.ID_MODULE_PRO,  
                 MODULE_PRO.ID_ETABLISSEMENT_OF,  
                 CONTRAT_PRO.ID_ETABLISSEMENT,  
                 ETABLISSEMENT.ID_ADHERENT,  
         		--AMA
         		ETABLISSEMENT.ID_ETABLISSEMENT,
                 --ADHERENT.ID_ETABLISSEMENT_PRINCIPAL,  
         		--AMA
                 NULL AS NUM_VIREMENT,
         		NULL AS NUM_CHEQUE,  
                 NULL AS DAT_VALID_REGLEMENT,  
                 NULL AS ID_REGLEMENT_PRO,  
         		NULL AS ID_TRANSACTION  
            FROM MODULE_PRO  
              LEFT JOIN SESSION_PRO ON SESSION_PRO.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO   
              LEFT JOIN CONTRAT_PRO ON MODULE_PRO.ID_CONTRAT_PRO = CONTRAT_PRO.ID_CONTRAT_PRO  
              LEFT JOIN ETABLISSEMENT ON CONTRAT_PRO.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT  
              LEFT JOIN ADHERENT ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT  
            WHERE  ((@ID_SESSION_PRO IS NULL) OR (SESSION_PRO.ID_SESSION_PRO = @ID_SESSION_PRO))  
              AND MODULE_PRO.ID_MODULE_PRO = @ID_MODULE_PRO  
           END  
          ELSE  
           BEGIN  
            INSERT INTO  #TEMP_INFO_SESSION  
            SELECT DISTINCT  SESSION_PRO.ID_SESSION_PRO,  
                 MODULE_PRO.ID_MODULE_PRO,  
                 MODULE_PRO.ID_ETABLISSEMENT_OF,  
                 CONTRAT_PRO.ID_ETABLISSEMENT,  
                 ETABLISSEMENT.ID_ADHERENT,  
         --AMA
         		ETABLISSEMENT.ID_ETABLISSEMENT,
                 --ADHERENT.ID_ETABLISSEMENT_PRINCIPAL,  
         --AMA
                 CASE WHEN @TYPE  = 0 THEN  
                  REGLEMENT_PRO_ADH.NUM_VIREMENT  
                 ELSE  
                  REGLEMENT_PRO_OF.NUM_VIREMENT  
                 END AS NUM_VIREMENT,  
         
                 CASE WHEN @TYPE  = 0 THEN  
                  REGLEMENT_PRO_ADH.NUM_CHEQUE  
                 ELSE  
                  REGLEMENT_PRO_OF.NUM_CHEQUE  
                 END AS NUM_CHEQUE,  
         
                  CASE WHEN @TYPE  = 0 THEN  
                  REGLEMENT_PRO_ADH.DAT_VALID_REGLEMENT  
                  ELSE  
                  REGLEMENT_PRO_OF.DAT_VALID_REGLEMENT  
                  END AS DAT_VALID_REGLEMENT,  
           
                 CASE WHEN @TYPE  = 0 THEN  
                  SESSION_PRO.ID_REGLEMENT_PRO_ADH  
                 ELSE  
                  SESSION_PRO.ID_REGLEMENT_PRO_OF  
                 END AS ID_REGLEMENT_PRO,  
                 CASE WHEN @TYPE  = 0 THEN  
                  SESSION_PRO.ID_TRANSACTION_ADH  
                 ELSE  
                  SESSION_PRO.ID_TRANSACTION_OF  
                 END AS ID_TRANSACTION  
           
            FROM SESSION_PRO  
              INNER JOIN MODULE_PRO  ON SESSION_PRO.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO   
              INNER JOIN CONTRAT_PRO ON MODULE_PRO.ID_CONTRAT_PRO = CONTRAT_PRO.ID_CONTRAT_PRO  
              LEFT JOIN ETABLISSEMENT ON CONTRAT_PRO.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT  
              LEFT JOIN ADHERENT ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT  
              LEFT JOIN  REGLEMENT_PRO REGLEMENT_PRO_ADH ON SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGLEMENT_PRO_ADH.ID_REGLEMENT_PRO  
              LEFT JOIN  REGLEMENT_PRO REGLEMENT_PRO_OF ON SESSION_PRO.ID_REGLEMENT_PRO_OF = REGLEMENT_PRO_OF.ID_REGLEMENT_PRO  
            WHERE SESSION_PRO.ID_SESSION_PRO = @ID_SESSION_PRO   
           END  
           
           
          --SELECT * FROM #TEMP_INFO_SESSION  
           
          IF @TYPE = 1 --Cas OF  
           BEGIN  
           
              SELECT DISTINCT [TRANSACTION].ID_TRANSACTION    AS ID_TRANSACTION,  
                  [TRANSACTION].NUM_IBAN        AS NUM_IBAN,   
                  TIERS.ID_TIERS          AS ID_TIERS_BENEF,  
                  ETABLISSEMENT.ID_ADHERENT       AS ID_ADHERENT_BENEF,  
                  ETABLISSEMENT.ID_ETABLISSEMENT      AS ID_ETABLISSEMENT_BENEF,  
                  ETABLISSEMENT_OF.ID_OF        AS ID_OF_BENEF,  
                  ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF     AS ID_ETABLISSEMENT_OF_BENEF,  
                  #TEMP_INFO_SESSION.NUM_VIREMENT      AS NUM_VIREMENT,  
         		 #TEMP_INFO_SESSION.NUM_CHEQUE      AS NUM_CHEQUE,  
                  #TEMP_INFO_SESSION.DAT_VALID_REGLEMENT    AS DAT_VALID_REGLEMENT,  
                  [TRANSACTION].BLN_ACTIF        AS BLN_ACTIF,  
                  [TRANSACTION].BLN_TRANSACTION_REGLEMENT_PRINCIPAL AS BLN_TRANSACTION_REGLEMENT_PRINCIPAL  
              FROM   #TEMP_INFO_SESSION  
                  INNER JOIN [TRANSACTION] ON #TEMP_INFO_SESSION.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION  
                  LEFT JOIN ETABLISSEMENT ON [TRANSACTION].ID_ETABLISSEMENT_BENEF = ETABLISSEMENT.ID_ETABLISSEMENT   
                  LEFT JOIN ETABLISSEMENT_OF ON [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF = ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF   
                  LEFT JOIN TIERS ON [TRANSACTION].ID_TIERS_BENEF = TIERS.ID_TIERS  
                  INNER JOIN REGLEMENT_PRO ON #TEMP_INFO_SESSION.ID_REGLEMENT_PRO = REGLEMENT_PRO.ID_REGLEMENT_PRO  
           
           END  
          ELSE  
           BEGIN  
           
              SELECT DISTINCT  
                  [TRANSACTION].ID_TRANSACTION       AS ID_TRANSACTION,  
                  [TRANSACTION].NUM_IBAN        AS NUM_IBAN,  
                  TIERS.ID_TIERS          AS ID_TIERS_BENEF,  
                  #TEMP_INFO_SESSION.ID_ADHERENT      AS ID_ADHERENT_BENEF,  
         		 --AMA 08/12/2008 Le b‚n‚ficiaire n'est pas l'‚tablissement principal mais l'adh‚rent lui mˆme		 
         		 #TEMP_INFO_SESSION.ID_ETABLISSEMENT   AS ID_ETABLISSEMENT_BENEF,
                  --#TEMP_INFO_SESSION.ID_ETABLISSEMENT_PRINCIPAL   AS ID_ETABLISSEMENT_BENEF,  
         		 --FIN AMA08/12/2008
                  ETABLISSEMENT_OF.ID_OF        AS ID_OF_BENEF,  
                  ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF     AS ID_ETABLISSEMENT_OF_BENEF,  
                  #TEMP_INFO_SESSION.NUM_VIREMENT      AS NUM_VIREMENT,  
         		 #TEMP_INFO_SESSION.NUM_CHEQUE      AS NUM_CHEQUE,  
                  #TEMP_INFO_SESSION.DAT_VALID_REGLEMENT    AS DAT_VALID_REGLEMENT,  
                  [TRANSACTION].BLN_ACTIF        AS BLN_ACTIF,  
                  [TRANSACTION].BLN_TRANSACTION_REGLEMENT_PRINCIPAL AS BLN_TRANSACTION_REGLEMENT_PRINCIPAL  
              FROM   #TEMP_INFO_SESSION  
                  INNER JOIN [TRANSACTION] ON #TEMP_INFO_SESSION.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION  
                  LEFT JOIN ETABLISSEMENT_OF ON [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF = ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF   
                  LEFT JOIN TIERS ON [TRANSACTION].ID_TIERS_BENEF = TIERS.ID_TIERS  
                  INNER JOIN REGLEMENT_PRO ON #TEMP_INFO_SESSION.ID_REGLEMENT_PRO = REGLEMENT_PRO.ID_REGLEMENT_PRO  
           END  
         END  
           
           
           

	CREATE PROCEDURE [dbo].[ARCHIVER_EDITION]
          @ID_ETABLISSEMENT_DESTINATAIRE INT,
          @TYPE_EMETTEUR VARCHAR(3),
          @ID_DOSSIER INT,
          @TYPE_DOSSIER VARCHAR(3),
          @TYPE_DOCUMENT VARCHAR(10),
          @FICHIER_EDITION VARCHAR(255),
          @RESULT_FICHIER_EDITION VARCHAR(255) OUTPUT
         AS
         
         -- =============================================
         -- Author:  M. ELHABOUSSI
         -- Create date: 10/06/2013
         -- Description: AJOUT DEMANDES INFORMATIONS COMPLEMENTAIRES
         -- MANTIS  : B15254 [Paniers] R‚alisation des PS Paniers
         -- =============================================
         -- Author  : M. ELHABOUSSI
         -- Update date : 18/06/2013
         -- Description : Ajout de la jointure avec la vue VUE_MODULES_VISIBLES_PANIERS 
         -- MANTIS  : B15254 [Paniers] R‚alisation des PS Paniers
         -- =============================================
         -- ASD/TLE 16/07/2013 : correction du select output en PEC
         -- =============================================
         -- DSZ #480 15/04/2015 : suite … l'ajout du paramŠtre @reference dans ins_edition; suppression des curseurs
         -- =============================================
         
         
         BEGIN
          
           DECLARE @ID_DOC_MODULE_EDITION int
           DECLARE @ID_LOT_EDITION int
           DECLARE @ID_ADHERENT_DEST int
           DECLARE @ID_OF_DEST int
           DECLARE @ID_ETABLISSEMENT_DEST int
           DECLARE @ID_ETABLISSEMENT_OF_DEST int
           DECLARE @ID_TIERS_BENEF int
           DECLARE @ID_ETABLISSEMENT_BENEF int
           DECLARE @ID_ETABLISSEMENT_OF_BENEF int
           DECLARE @ID_CONTACT int
           DECLARE @ID_MODE_ENVOI_DOC int
         
           DECLARE @COD_DOCUMENT VARCHAR(20)
         
         
           DECLARE @ID_BATCH_MODULE_EDITION int
           DECLARE @ID_DOCUMENT int
           DECLARE @ID_TYPE_TRANSACTION int
           DECLARE @ID_SOUS_TYPE_TRANSACTION int
           DECLARE @ID_ACTIVITE int
           DECLARE @ID_UTILISATEUR int
           DECLARE @REFERENCE VARCHAR(100)
         
          
          SET @ID_UTILISATEUR = 79
          IF @TYPE_EMETTEUR = 'ADH'
          BEGIN
           SET @ID_OF_DEST = null
           SET @ID_ETABLISSEMENT_OF_DEST = null
           SET @ID_ETABLISSEMENT_DEST = @ID_ETABLISSEMENT_DESTINATAIRE
          END
         
          IF @TYPE_EMETTEUR = 'OF'
          BEGIN
           SET @ID_ADHERENT_DEST = null
           SET @ID_ETABLISSEMENT_DEST = null
           SET @ID_ETABLISSEMENT_OF_DEST = @ID_ETABLISSEMENT_DESTINATAIRE
         
          END
         
           
           SET @COD_DOCUMENT = 
           CASE 
          WHEN @TYPE_EMETTEUR = 'OF' AND @TYPE_DOSSIER = 'PEC' AND @TYPE_DOCUMENT = 'REL_DPC' THEN 'NC'
          WHEN @TYPE_EMETTEUR = 'OF' AND @TYPE_DOSSIER = 'PEC' AND @TYPE_DOCUMENT = 'REL_DR' THEN 'LET_PEC_REGL_OF'
          WHEN @TYPE_EMETTEUR = 'ADH' AND @TYPE_DOSSIER = 'PEC' AND @TYPE_DOCUMENT = 'REL_DR' THEN 'LET_PEC_REGL_AD'
          WHEN @TYPE_EMETTEUR = 'ADH' AND @TYPE_DOSSIER = 'PEC' AND @TYPE_DOCUMENT = 'REL_DPC' THEN 'LET_PEC_REL_ADH'
          WHEN @TYPE_EMETTEUR = 'OF' AND @TYPE_DOSSIER = 'PRO' AND @TYPE_DOCUMENT = 'REL_DPC' THEN 'REL_ENG_OF_PRO'
          WHEN @TYPE_EMETTEUR = 'OF' AND @TYPE_DOSSIER = 'PRO' AND @TYPE_DOCUMENT = 'REL_DR' THEN 'REL_REG_OF_PRO'
          WHEN @TYPE_EMETTEUR = 'ADH' AND @TYPE_DOSSIER = 'PRO' AND @TYPE_DOCUMENT = 'REL_DPC' THEN 'REL_ENG_ADH_PRO'
          WHEN @TYPE_EMETTEUR = 'ADH' AND @TYPE_DOSSIER = 'PRO' AND @TYPE_DOCUMENT = 'REL_DR' THEN 'REL_REG_ADH_PRO'
           END;
         
           SELECT @ID_DOCUMENT = DOCUMENT.ID_DOCUMENT FROM DOCUMENT WHERE DOCUMENT.COD_DOCUMENT = @COD_DOCUMENT
         
           SET @ID_BATCH_MODULE_EDITION = 0
           SET @ID_DOC_MODULE_EDITION = 0
         
         
         
           EXECUTE @ID_LOT_EDITION =  [INS_LOT_EDITION] 
            @ID_BATCH_MODULE_EDITION
           ,@ID_DOCUMENT
           ,@ID_TYPE_TRANSACTION
           ,@ID_SOUS_TYPE_TRANSACTION
           ,@ID_ACTIVITE
           ,@ID_UTILISATEUR
         
         
         
           IF @TYPE_DOSSIER = 'PEC'
           BEGIN
          set @reference = (select distinct  MODULE_PEC.COD_MODULE_PEC  + ',' 
           from  MODULE_PEC
            INNER JOIN VUE_MODULES_VISIBLES_PANIERS MODULES_VISIBLES ON MODULES_VISIBLES.id_module_pec = MODULE_PEC.ID_MODULE_PEC 
            AND MODULE_PEC.ID_ACTION_PEC = @ID_DOSSIER
             FOR XML PATH(''))
             
          EXECUTE [INS_EDITION] 
             @ID_DOC_MODULE_EDITION
            ,@ID_LOT_EDITION
            ,@ID_ADHERENT_DEST
            ,@ID_OF_DEST
            ,@ID_ETABLISSEMENT_DEST
            ,@ID_ETABLISSEMENT_OF_DEST
            ,@ID_TIERS_BENEF
            ,@ID_ETABLISSEMENT_BENEF
            ,@ID_ETABLISSEMENT_OF_BENEF
            ,@ID_CONTACT
            ,@FICHIER_EDITION
            ,1
            ,@REFERENCE
         
          Select @RESULT_FICHIER_EDITION = EDITION.FICHIER_EDITION 
          FROM EDITION
          WHERE EDITION.ID_LOT_EDITION = @ID_LOT_EDITION
           END
         
           IF @TYPE_DOSSIER = 'PRO'
           BEGIN
          set @reference = (select distinct MODULE_PRO.COD_MODULE_PRO  + ',' 
           from  MODULE_PRO
            INNER JOIN VUE_MODULES_VISIBLES_PANIERS MODULES_VISIBLES ON MODULES_VISIBLES.id_module_pro = MODULE_PRO.ID_MODULE_PRO 
            AND MODULE_PRO.ID_CONTRAT_PRO = @ID_DOSSIER
             FOR XML PATH(''))
             
          EXECUTE [INS_EDITION] 
             @ID_DOC_MODULE_EDITION
            ,@ID_LOT_EDITION
            ,@ID_ADHERENT_DEST
            ,@ID_OF_DEST
            ,@ID_ETABLISSEMENT_DEST
            ,@ID_ETABLISSEMENT_OF_DEST
            ,@ID_TIERS_BENEF
            ,@ID_ETABLISSEMENT_BENEF
            ,@ID_ETABLISSEMENT_OF_BENEF
            ,@ID_CONTACT
            ,@FICHIER_EDITION
            ,1
            ,@REFERENCE
          
          Select @RESULT_FICHIER_EDITION = e.FICHIER_EDITION 
          FROM EDITION e
          WHERE e.ID_LOT_EDITION = @ID_LOT_EDITION 
           END
         END


         CREATE PROCEDURE [dbo].[UPD_ENVELOPPE]  
         	@ID_ENVELOPPE INT,  
         	@LIBC_ENVELOPPE VARCHAR(20),  
         	@LIBL_ENVELOPPE VARCHAR(50),  
         	@DAT_DEBUT DATETIME,  
         	@DAT_FIN DATETIME,  
         	@ID_PERIODE INTEGER,
         	@ID_TYPE_ENVELOPPE INT,
         	@ID_UTILISATEUR INT,
         	@BLN_ACTIF TINYINT,
         	@COM_ENVELOPPE VARCHAR(2585),
         	@TIME_STAMP TIMESTAMP
         AS  
         
         IF @TIME_STAMP IS NULL
         BEGIN
         	UPDATE ENVELOPPE SET
         		LIBC_ENVELOPPE = @LIBC_ENVELOPPE,
         		LIBL_ENVELOPPE = @LIBL_ENVELOPPE,  
         		DAT_DEBUT= @DAT_DEBUT, 
         		DAT_FIN = @DAT_FIN,
         		BLN_ACTIF = @BLN_ACTIF,
         		DAT_MODIF = GETDATE(),
         		ID_TYPE_ENVELOPPE = @ID_TYPE_ENVELOPPE,
         		ID_UTILISATEUR = @ID_UTILISATEUR,
         		COM_ENVELOPPE = @COM_ENVELOPPE
         		FROM ENVELOPPE
         	WHERE ID_ENVELOPPE  = @ID_ENVELOPPE
         END
         ELSE
         BEGIN
         	UPDATE ENVELOPPE SET
         		LIBC_ENVELOPPE = @LIBC_ENVELOPPE,
         		LIBL_ENVELOPPE = @LIBL_ENVELOPPE,  
         		DAT_DEBUT= @DAT_DEBUT, 
         		DAT_FIN = @DAT_FIN,
         		BLN_ACTIF = @BLN_ACTIF,
         		DAT_MODIF = GETDATE(),
         		ID_TYPE_ENVELOPPE = @ID_TYPE_ENVELOPPE,
         		ID_UTILISATEUR = @ID_UTILISATEUR,
         		COM_ENVELOPPE = @COM_ENVELOPPE
         		FROM ENVELOPPE
         	WHERE ID_ENVELOPPE  = @ID_ENVELOPPE AND
         		TIME_STAMP = @TIME_STAMP
         
         	IF @@ROWCOUNT = 0   
         	BEGIN  
         	   IF EXISTS(SELECT * FROM ENVELOPPE WHERE ID_ENVELOPPE  = @ID_ENVELOPPE)      
         	   BEGIN  
         		  /* Problme de Concurrence d'accs */  
         		  RAISERROR('Problme de Concurrence d''accs', 16, 1)
         		  RETURN -1  
         	   END     
         	END
         END
        
GO         
         -- =============================================
         -- Author:		HBT
         -- Create date: 10/02/2012
         -- Description:	sous types de cout autres pour edition pour  FICHE_DOSSIER 
         -- =============================================
         -- Author:		EOU
         -- Create date: 07/03/2012
         -- Description:	Ajout distinct sur les select count id_dispositif 13321
         -- =============================================
         CREATE PROCEDURE [dbo].[LEC_DISPOSITIF_MODULE_PEC_FICHEDOSSIER] 
         	@ID_MODULE int
         AS
         
         BEGIN
         
         DECLARE @nbDispositif  int
         DECLARE @COD_DISPOSITIF  VARCHAR(100)
         
         SET @nbDispositif=
         (
         select COUNT(*) from (
         SELECT      distinct DISPOSITIF.ID_DISPOSITIF
         FROM STAGIAIRE_PEC 
         INNER JOIN UNITE_STAGIAIRE on UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC 
         INNER JOIN DISPOSITIF on DISPOSITIF.ID_DISPOSITIF = UNITE_STAGIAIRE.ID_DISPOSITIF
         WHERE 
         	STAGIAIRE_PEC.ID_MODULE_PEC =@id_module 
         	and UNITE_STAGIAIRE.NB_HEURE_ENGAGE > 0
         	and STAGIAIRE_PEC.ID_SESSION_PEC is NULL
         	group by DISPOSITIF.ID_DISPOSITIF--, STAGIAIRE_PEC.id_STAGIAIRE_PEC
         	
         	) as count1)
         IF @nbDispositif = 1
         	BEGIN
         		SET @COD_DISPOSITIF =(SELECT   DISTINCT DISPOSITIF.COD_DISPOSITIF as COD_DISPOSITIF
         		FROM STAGIAIRE_PEC 
         		INNER JOIN UNITE_STAGIAIRE on UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC 
         		INNER JOIN DISPOSITIF on DISPOSITIF.ID_DISPOSITIF = UNITE_STAGIAIRE.ID_DISPOSITIF
         		WHERE 
         		STAGIAIRE_PEC.ID_MODULE_PEC =@ID_MODULE 
         		and UNITE_STAGIAIRE.NB_HEURE_ENGAGE > 0
         		and STAGIAIRE_PEC.ID_SESSION_PEC is NULL)
         	END
         	ELSE IF @nbDispositif > 1
         	BEGIN
         			Set @nbDispositif= (SELECT  Count(*) from (select distinct DISPOSITIF.COD_DISPOSITIF
         				FROM STAGIAIRE_PEC 
         				INNER JOIN UNITE_STAGIAIRE on UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC 
         				INNER JOIN DISPOSITIF on DISPOSITIF.ID_DISPOSITIF = UNITE_STAGIAIRE.ID_DISPOSITIF
         				WHERE 
         				STAGIAIRE_PEC.ID_MODULE_PEC =@ID_MODULE 
         				and UNITE_STAGIAIRE.NB_HEURE_ENGAGE > 0
         				and STAGIAIRE_PEC.ID_SESSION_PEC is NULL
         				and DISPOSITIF.BLN_PLAN !=1) as count2)
         				
         			IF @nbDispositif = 0
         			BEGIN
         			SET @COD_DISPOSITIF = 'Plan'
         			END
         			ELSE IF @nbDispositif = 1
         			BEGIN
         					SET @COD_DISPOSITIF = (SELECT TOP 1  DISPOSITIF.COD_DISPOSITIF as COD_DISPOSITIF
         					FROM STAGIAIRE_PEC 
         					INNER JOIN UNITE_STAGIAIRE on UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC 
         					INNER JOIN DISPOSITIF on DISPOSITIF.ID_DISPOSITIF = UNITE_STAGIAIRE.ID_DISPOSITIF
         					WHERE 
         					STAGIAIRE_PEC.ID_MODULE_PEC =@ID_MODULE 
         					and UNITE_STAGIAIRE.NB_HEURE_ENGAGE > 0
         					and STAGIAIRE_PEC.ID_SESSION_PEC is NULL
         					and DISPOSITIF.BLN_PLAN !=1)
         			END
         			ELSE
         			BEGIN
         	 				SET @COD_DISPOSITIF ='Multiple'
         			END
         	END
         
         SELECT 	@COD_DISPOSITIF AS  COD_DISPOSITIF
         
         END
         		 		 
		 -- =============================================
         -- HBO - 141113 - M16371: Lot 1 - Modification structure de donn‚es / proc‚dures stock‚es
         -- =============================================
         -- HBO - 201113 - M16378: Lot 1 - Editions
         -- =============================================
         CREATE PROCEDURE EDIT_REMISE_BANCAIRE
         	@IDS_BORDEREAU varchar(500)	-- List of ID_BORDEREAU separated with ',' without spaces - i.e.: 1,2,3,4,5,6
         with recompile
         AS
         	BEGIN
         		DECLARE @Item int
         
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
         			SELECT @IDS_BORDEREAU -- Put the last item in
         
         		SELECT
         			UTILISATEUR.LIB_NOM,
         			UTILISATEUR.LIB_PNM,
         			BORDEREAU.ID_BORDEREAU,
         			BORDEREAU.COD_BORDEREAU,
         			LOT_REMISE_BANCAIRE.COD_LOT_REMISE_BANCAIRE,
         			LOT_REMISE_BANCAIRE.DAT_LOT_REMISE_BANCAIRE,
         			SUM(VERSEMENT.MNT_VERSEMENT) as MNT_BORDEREAU,
         			count(VERSEMENT.ID_VERSEMENT) as NB,
         			-- Information of Bank Account 
         			TRANSIT.NUM_IBAN_TRANSIT as NUM_COMPTE,
         			TRANSIT.BIC_TRANSIT as BIC,
         			TRANSIT.LIB_COMPTE_BANQUE AS LIB_COMPT_BANQUE
         		FROM
         			BORDEREAU
         			INNER JOIN UTILISATEUR			ON BORDEREAU.ID_UTILISATEUR = UTILISATEUR.ID_UTILISATEUR
         			INNER JOIN VERSEMENT			ON (VERSEMENT.ID_BORDEREAU = BORDEREAU.ID_BORDEREAU) /*AND (VERSEMENT. ID_MODE_VERSEMENT = 1)*/
         			--INNER JOIN POSTE_VERSEMENT		ON POSTE_VERSEMENT.ID_VERSEMENT = VERSEMENT.ID_VERSEMENT
         			--INNER JOIN POSTE_IMPUTATION		ON POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT
         			INNER JOIN LOT_REMISE_BANCAIRE	ON LOT_REMISE_BANCAIRE.ID_LOT_REMISE_BANCAIRE = BORDEREAU.ID_LOT_REMISE_BANCAIRE,
         			TRANSIT
         		WHERE
         			BORDEREAU.ID_BORDEREAU in (select Item from #List)
         			AND (VERSEMENT.BLN_ACTIF = 1)
         			--AND (POSTE_VERSEMENT.BLN_ACTIF = 1)
         		GROUP BY
         			UTILISATEUR.LIB_NOM,
         			UTILISATEUR.LIB_PNM,
         			BORDEREAU.ID_BORDEREAU,
         			BORDEREAU.COD_BORDEREAU,
         			LOT_REMISE_BANCAIRE.COD_LOT_REMISE_BANCAIRE,
         			LOT_REMISE_BANCAIRE.DAT_LOT_REMISE_BANCAIRE,
         			TRANSIT.NUM_IBAN_TRANSIT,
         			TRANSIT.BIC_TRANSIT,
         			TRANSIT.LIB_COMPTE_BANQUE 
         	END


         CREATE PROCEDURE [BATCH_TRANSFERT_DOTATION_SUPPLEMENTAIRE_DEFI_GESTION_PME_2015]
         
         /*
         =============================================  
         Author  : MBL
         Create date : 19/11/2015
         Description : Proc‚dure permettant de lancer des tranferts des dotations suppl‚mentaires DEFI GESTION 2015 pour les adh‚rents de type PME (champ application P10-49)
         sur le compte VO OBLIGATOIRE	(@COD_TYPE_EVENEMENT_DOTATION = 'DOTPME15')
         Le traitement fait appel a la fonction de table F_TRANSFERT_DOTATION_SUPPLEMENTAIRE_DEFI_GESTION_PME_2015 constituant un outil d'aide … la d‚cision 
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
         	@ID_TYPE_FINANCEMENT_COMPTE_VO	INT
         
         	SELECT @NUM_ANNEE_N = 2015
         
         	SET @COD_TYPE_EVENEMENT_DOTATION = 'PMESUP15'
         	SELECT @ID_TYPE_EVENEMENT_TRANSFERT = ID_TYPE_EVENEMENT
         	FROM TYPE_EVENEMENT
         	WHERE COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT_DOTATION 
         
         		
         	SELECT t.*, ADHERENT.ID_ETABLISSEMENT_PRINCIPAL
         	INTO #TMP_TRANSFERT 
         	FROM F_TRANSFERT_DOTATION_SUPPLEMENTAIRE_DEFI_GESTION_PME_2015(@NUM_ANNEE_N, @ID_ADHERENT_TRAITE) t
         	INNER JOIN ADHERENT ON ADHERENT.ID_ADHERENT = t.ID_ADHERENT
         	INNER JOIN ETABLISSEMENT ON ETABLISSEMENT.ID_ETABLISSEMENT = ADHERENT.ID_ETABLISSEMENT_PRINCIPAL
         
         
         	SELECT @DAT = GETDATE()
         
         	SELECT @ID_TYPE_FINANCEMENT_COMPTE_VO = 4 -- Compte Plan Obligatoire
         
         	SELECT	@ID_PERIODE_N	= ID_PERIODE   
         	from	PERIODE     
         	where	NUM_ANNEE		= @NUM_ANNEE_N -1
         	AND		ID_TYPE_PERIODE = 1   
         
         	SELECT	@ID_PERIODE_N_PLUS1		= ID_PERIODE
         	from	PERIODE     
         	where	NUM_ANNEE				= @NUM_ANNEE_N 
         	AND		ID_TYPE_PERIODE			= 1   
         
         	SET @LIBL_EVENEMENT		= 'Dotation Suppl‚mentaire DEFI GESTION 10-49 '	+ CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
         	SET @LIBL_MVT			= 'Dotation Suppl‚mentaire DEFI GESTION 10-49 '	+ CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
         
         
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
         		--	LIBL_TRANSFERT					= @LIBL_EVENEMENT,
         		--	BLN_COMPTE_VERS_ENVELOPPE		= @BLN_COMPTE_VERS_ENVELOPPE,  
         		--	ID_GROUPE						= @ID_GROUPE,
         		--	ID_ENVELOPPE					= @ID_ENVELOPPE,
         		--	DAT_TRANSFERT					= @DAT,
         		--	MNT_TRANSFERT					= @MNT_TRANSFERT, 
         		--	ID_TYPE_FINANCEMENT_COMPTE_VO	= @ID_TYPE_FINANCEMENT_COMPTE_VO,   
         		--	ID_UTILISATEUR					= 82, 
         		--	ID_PERIODE						= @ID_PERIODE_N_PLUS1,
         		--	COM_TRANSFERT					= @LIBL_MVT, 
         		--	LIBL_MVT_BUDGETAIRE				= @LIBL_MVT,
         		--	ID_TYPE_EVENEMENT				= @ID_TYPE_EVENEMENT_TRANSFERT,
         		--	ID_ETABLISSEMENT				= @ID_ETABLISSEMENT
         				
         		exec @ID_TRANSFERT = INS_TRANSFERT 
         			@LIBL_TRANSFERT					= @LIBL_EVENEMENT,
         			@BLN_COMPTE_VERS_ENVELOPPE		= @BLN_COMPTE_VERS_ENVELOPPE,  
         			@ID_GROUPE						= @ID_GROUPE,
         			@ID_ENVELOPPE					= @ID_ENVELOPPE,
         			@DAT_TRANSFERT					= @DAT,
         			@MNT_TRANSFERT					= @MNT_TRANSFERT, 
         			@ID_TYPE_FINANCEMENT			= @ID_TYPE_FINANCEMENT_COMPTE_VO,   -- Type de financement sur Compte Historique
         			@ID_UTILISATEUR					= 82, 
         			@ID_PERIODE						= @ID_PERIODE_N_PLUS1,
         			@COM_TRANSFERT					= @LIBL_MVT, 
         			@LIBL_MVT_BUDGETAIRE			= @LIBL_MVT,
         			@ID_TYPE_EVENEMENT				= @ID_TYPE_EVENEMENT_TRANSFERT,
         			@ID_ETABLISSEMENT				= @ID_ETABLISSEMENT
         
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
         
         
         -- =============================================
         -- Author:		Lam
         -- Create date: xx xxx. 2007
         -- Description:	proc‚dure netoyant les recus
         -- Author:		Say
         -- Modified date: 30 mai 2007
         -- comment		: changement du systŠme de suppression
         -- Author:		  Brugait
         -- Modified date: 6 mars 2008
         -- comment		: suppression de donn‚es de la nouvelle table RECU_MODE_VERSEMENT
         -- =============================================
         CREATE PROCEDURE [dbo].[DEL_RECU_ANNULATION]
         	@ID_LAST_RECU			int,
         	@ID_TYPE_RECU			tinyint = 1
         AS
         BEGIN
         SET NOCOUNT ON
         
         DECLARE @DEL_OK int
         
         -- si des recus existent lors du premier passage, la suppression de tous les recus n'est possible que si ils ont ‚t‚ cr‚‚s le mˆme jour
         select	@DEL_OK = coalesce(datediff(day, min(dat_recu), max(dat_recu)), 0)
         from	recu
         
         	IF (@ID_LAST_RECU > 0) OR (@ID_LAST_RECU = 0 AND @DEL_OK = 0)
         	BEGIN
         		DELETE FROM RECU_MODE_VERSEMENT
         		WHERE
         			ID_RECU IN (SELECT ID_RECU FROM RECU WHERE BLN_ACTIF > 1 AND ID_RECU > @ID_LAST_RECU AND ID_TYPE_RECU = @ID_TYPE_RECU)
         
         		DELETE FROM POSTE_RECU 
         		WHERE 
         			ID_RECU IN (SELECT ID_RECU FROM RECU WHERE BLN_ACTIF > 1 AND ID_RECU > @ID_LAST_RECU AND ID_TYPE_RECU = @ID_TYPE_RECU)
         
         		UPDATE	RECU 
         		SET		ID_RECU_REMP = NULL
         		WHERE	ID_RECU_REMP IN (SELECT ID_RECU FROM RECU WHERE BLN_ACTIF > 1 AND ID_RECU > @ID_LAST_RECU AND ID_TYPE_RECU = @ID_TYPE_RECU)
         
         		DELETE FROM RECU 
         		WHERE 
         			BLN_ACTIF > 1 AND ID_RECU > @ID_LAST_RECU AND ID_TYPE_RECU = @ID_TYPE_RECU
         	END
         END
         
          -- =============================================
         -- AUTHOR: MB (POUR LE FAF PROPRET)
         -- MODIF. DATE: 12/02/2008
         -- DESCRIPTION: - MODIFICATION POUR EMPECHER DE SUPPRIMER LES MOUVEMENTS BUDGETAIRES ASSOCIES AUX REGLEMENTS
         -- =============================================
         -- AUTHOR: SAF & ASD
         -- MODIF. DATE: 18/03/2009
         -- DESCRIPTION: - RCUPRATION DE LA VERSION POUR C2P
         -- =============================================
         -- AUTHOR: DSZ
         -- MODIF. DATE: 17/06/2009
         -- DESCRIPTION: MANTIS 12080. MODIFICATION BLN_FINANCEMENT_OK - 
         --        NE FAIRE QUE SI LE MODULE A UN STAGIAIRE DE CET TABLISSEMENT.
         -- =============================================
         -- AUTHOR: DSZ
         -- MODIF. DATE: 19/03/2010
         -- DESCRIPTION: 12307: NE PAS TOUCHER A BLN_OK_FINANCEMENT DES MODULES ENGAGS
         --============================================================
         -- DSZ 24/09/2010 12475
         -- C2P_SFD_PEC_2.30.DOC P 70 "SI L'TABLISSEMENT EST CHANG AVANT ENGAGEMENT, LE GROUPE DOIT ÒTRE MIS · JOUR."
         --                         CETTE RÔGLE EST GALEMENT APPLICABLE POUR LA BRANCHE 
         --============================================================
         -- DSZ 24/09/2010 12553
         -- SI L'TABLISSEMENT EST CHANG APRÔS DESENGAGEMENT, GROUPE ET BRANCHE MISES · JOUR
         --============================================================
         -- DSZ 07/01/2011 12258
         -- SFD _PEC 2.33 DU 23/12/2010
         	--	CHANGEMENT DE L'ADHRENT OU L'TABLISSEMENT
         	--ON TROUVE TOUS LES MOUVEMENTS BUDGTAIRES TYPE P ASSOCIS ET ON CRE LES MOUVEMENTS TYPE P (COD TYPE 19) AVEC LE MÒME MONTANT ET SIGNE NGATIF.
         --============================================================  
         -- MB MODIF DU 2011/01/13
         -- CORRECTION URGENTE DES ABERRATIONS DE LA VERSION PRECEDENTE :
         -- LES MAJ DE LA TABLE STAGIAIRE_PEC EST REALISE DE MANIERE INCOHERENTE SUITE AUX DERNIERES MODIFICATIONS
         --============================================================  
         -- ASD 20110128
         -- CONVERGENCE DE DEUX DERNIÔRES CORRECTIONS QUI AVAIENT T FAITES DANS DES VERSIONS DISTINCTES
         --============================================================
         -- DSZ 05/04/2011 12736
         -- RENOMMAGE PROCEDURE ACTION_CREER_MVTS_INVERSES => ACTION_CREER_MVTS_P_INVERSES
         --============================================================
         -- DSZ 11/12/11 13040
         -- SUITE · L'HISTORISATION DE L'ACTIVIT : SI L'TABLISSEMENT A CHANG, MODIFIER L'ACTIVIT DES STAGIARES
         --============================================================
         -- DSZ 31/05/12 13093
         -- SI CHANGEMENT D'TABLISSEMENT ENTRAINE CHANGEMENT DE L'ADHERENT, ALORS SUPPRIMER LES PIECES PEC (NON CONFORMES OU ABSENTES) ATTACHS 
         -- · L'ANCIEN ADHRENT DANS TOUS LES MODULES DE L'ACTION CONCERNE
         --============================================================
         --DSZ 04/06/12 13093
         -- AJOUT RECALCUL BLN_OK_PIECE DE TOUS LES MODULES DE L'ACTION
         --============================================================
         -- 07/02/2013 LDE
         -- 14822: [AGPP] : 8 : AGPP FONCTIONNEL EN DESCENTE EN COMPTA, ETEBAC ET VIREMENTS INTERNES (-> EN PROD LE 28/2)
         -- REMPLACEMENT DE != 3 POUR DTECTER LE PLAN
         --============================================================
         -- 04/06/2013 EOU
         -- 15242
         --=============================================
         -- HBO - 20140903 #481
         --=============================================
         CREATE PROCEDURE [DBO].[UPD_ETABLISSEMENT_ACTION]
         	@ID_ACTION INT,  
         	@ID_ETABLISSEMENT INT,  
         	@NUM_ACTION VARCHAR(20),
         	@ID_ETABLISSEMENT_OLD INT
         AS 
         BEGIN
         	
         	SET NOCOUNT ON
         
         	IF @ID_ETABLISSEMENT_OLD IS NULL
         	BEGIN
         		SET @ID_ETABLISSEMENT_OLD = @ID_ETABLISSEMENT
         	END	
         	ELSE
         	BEGIN
         		IF @ID_ETABLISSEMENT_OLD <> @ID_ETABLISSEMENT
         		BEGIN	
         			--AJOUT DSZ 17/06/09 MANTIS 12080
         			DECLARE @BLN_STAG_CNT INT
         			
         			SELECT 
         				@BLN_STAG_CNT = COUNT(ID_STAGIAIRE_PEC)
         			FROM
         				STAGIAIRE_PEC
         				INNER JOIN MODULE_PEC
         					ON STAGIAIRE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         			WHERE
         				STAGIAIRE_PEC.ID_SESSION_PEC IS NULL
         				AND STAGIAIRE_PEC.ID_ETABLISSEMENT = @ID_ETABLISSEMENT_OLD
         				AND MODULE_PEC.ID_ACTION_PEC = @ID_ACTION
         
         			
         			IF (@BLN_STAG_CNT >0)
         			BEGIN
         			-- <-- FIN AJOUT DSZ
         				UPDATE
         					POSTE_COUT_ENGAGE
         				SET
         					BLN_OK_FINANCEMENT = 0
         				WHERE
         					ID_MODULE_PEC IN 
         					(
         						SELECT
         							ID_MODULE_PEC
         						FROM
         							MODULE_PEC
         						WHERE
         							ID_ACTION_PEC = @ID_ACTION
         					)
         					AND DAT_DESENGAGEMENT IS NULL
         					AND ID_ENGAGEMENT IS NULL 
         
         				--UPDATE HISTORISATION DANS STAGIAIRES DE L'ANCIEN ETABLISSEMENT
         
         				DECLARE @ID_BRANCHE INT  
         				DECLARE @ID_GROUPE INT  
         				DECLARE @ID_ADHERENT INT
         				
         				SELECT   
         					@ID_BRANCHE = ID_BRANCHE,   
         					@ID_GROUPE = ID_GROUPE ,
         					@ID_ADHERENT = ID_ADHERENT
         				FROM
         					ETABLISSEMENT
         				WHERE
         					ID_ETABLISSEMENT = @ID_ETABLISSEMENT
         
         				--DSZ 11/12/11 REFAIT POUR HISTORISER L'ID_ACTIVITE						
         				UPDATE
         					STAGIAIRE_PEC
         				SET
         					ID_ETABLISSEMENT = @ID_ETABLISSEMENT , 
         					ID_BRANCHE = @ID_BRANCHE,  
         					ID_GROUPE = @ID_GROUPE , 
         					ID_ACTIVITE = ISNULL(R19.ID_ACTIVITE, 4) --PAR DFAUT 10-49
         				FROM
         					STAGIAIRE_PEC
         					INNER JOIN MODULE_PEC ON (STAGIAIRE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC)
         					LEFT JOIN R19 ON (		R19.ID_ADHERENT =  @ID_ADHERENT
         										AND R19.ID_PERIODE = MODULE_PEC.ID_PERIODE
         										AND R19.ID_ACTIVITE IN (SELECT ID_ACTIVITE FROM GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')))		-- LDE 07/02/2013 #14822
         				WHERE   
         					ID_ETABLISSEMENT = @ID_ETABLISSEMENT_OLD  
         					AND MODULE_PEC.ID_ACTION_PEC = @ID_ACTION  
         				
         				-- EOU 04/06/2013 REFAIT HISTORISER LES DISPOSITIF PLAN DES STAGIAIRES
         				UPDATE
         					UNITE_STAGIAIRE
         				SET
         					ID_DISPOSITIF = (SELECT TOP 1 ID_DISPOSITIF FROM DISPOSITIF
         					 WHERE DISPOSITIF.ID_ACTIVITE = STAGIAIRE_PEC.ID_ACTIVITE 
         					 AND DISPOSITIF.LIBC_DISPOSITIF LIKE '%PLAN%')
         				FROM UNITE_STAGIAIRE
         					INNER JOIN STAGIAIRE_PEC ON UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
         					INNER JOIN MODULE_PEC ON (STAGIAIRE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC)
         					INNER JOIN DISPOSITIF ON DISPOSITIF.ID_DISPOSITIF = UNITE_STAGIAIRE.ID_DISPOSITIF					
         				WHERE   
         					DISPOSITIF.LIBC_DISPOSITIF LIKE '%PLAN%'
         					AND DISPOSITIF.ID_ACTIVITE <> STAGIAIRE_PEC.ID_ACTIVITE
         					AND STAGIAIRE_PEC.ID_ETABLISSEMENT = @ID_ETABLISSEMENT   
         					AND MODULE_PEC.ID_ACTION_PEC = @ID_ACTION  
         			END  --IF (@BLN_STAG_CNT >0)
         
         
         			--AJOUT DSZ 31/05/2012 13093
         			DECLARE @ID_ADH INT
         			DECLARE @ID_ADH_OLD INT
         			SELECT @ID_ADH = ID_ADHERENT FROM ETABLISSEMENT WHERE ID_ETABLISSEMENT = @ID_ETABLISSEMENT 
         			SELECT @ID_ADH_OLD = ID_ADHERENT FROM ETABLISSEMENT WHERE ID_ETABLISSEMENT = @ID_ETABLISSEMENT_OLD 
         			IF @ID_ADH <> @ID_ADH_OLD	 -- SI CHANGEMENT DE L'ADHERENT
         			BEGIN
         				SELECT
         					ID_ARRIVEE_PIECE_PEC 
         				INTO
         					#PIECES_TO_DELETE
         				FROM
         					ARRIVEE_PIECE_PEC 
         					INNER JOIN MODULE_PEC ON (ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC )
         					INNER JOIN PIECE_PEC ON (ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC)
         				WHERE
         					MODULE_PEC.ID_ACTION_PEC = @ID_ACTION --MODULES DE L'ACTION CONCERNE
         					AND PIECE_PEC.BLN_ADHERENT = 1 --SEULEMENT LES PIECES DE L'ADHERENT
         					AND ARRIVEE_PIECE_PEC.ID_ADHERENT  = @ID_ADH_OLD --SEULEMENT LES PIECES DE L'ANCIEN ADHRENT
         
         			DELETE
         				NR210 
         			FROM
         				NR210 
         				INNER JOIN #PIECES_TO_DELETE ON (NR210.ID_ARRIVEE_PIECE_PEC = #PIECES_TO_DELETE.ID_ARRIVEE_PIECE_PEC)
         			
         			DELETE
         				ARRIVEE_PIECE_PEC  
         			FROM
         				ARRIVEE_PIECE_PEC 
         				INNER JOIN #PIECES_TO_DELETE ON (ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC = #PIECES_TO_DELETE.ID_ARRIVEE_PIECE_PEC);
         			
         			-- ON REGARDE SI TOUTES LES PIECES SONT PRESENTES ET CONFORMES
         			WITH PIECES_COUNT
         			AS
         			(
         				SELECT
         					MODULE_PEC.ID_MODULE_PEC, 
         					COUNT(ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC) AS CNT_MANQUANTS
         				FROM
         					MODULE_PEC 
         					LEFT JOIN ARRIVEE_PIECE_PEC ON (ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC )
         					LEFT JOIN PIECE_PEC ON PIECE_PEC.ID_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_PIECE_PEC
         											AND
         											(
         												PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1
         												OR PIECE_PEC.BLN_BLOQUANT_REGLEMENT = 1
         											)
         											AND
         											(
         												ARRIVEE_PIECE_PEC.BLN_ACTIF = 0
         												OR ARRIVEE_PIECE_PEC.BLN_CONFORME = 0
         											)
         				WHERE	MODULE_PEC.ID_ACTION_PEC = @ID_ACTION 
         				GROUP BY MODULE_PEC.ID_MODULE_PEC
         			)
         			UPDATE
         				MODULE_PEC
         			SET
         				BLN_OK_PIECE = (CASE WHEN PIECES_COUNT.CNT_MANQUANTS = 0 THEN 1
         											ELSE 0
         										END)
         			FROM
         				MODULE_PEC 
         				INNER JOIN  PIECES_COUNT ON (PIECES_COUNT.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC);
         		END
         		
           
         		END   --IF @ID_ETABLISSEMENT_OLD <> @ID_ETABLISSEMENT
         	END   --@ID_ETABLISSEMENT_OLD IS NOT NULL
         	
         	UPDATE
         		NR140
         	SET
         		ID_ETABLISSEMENT = @ID_ETABLISSEMENT,
         		NUM_INTERNE = @NUM_ACTION
         	WHERE
         		ID_ACTION_PEC = @ID_ACTION
         		AND ID_ETABLISSEMENT = @ID_ETABLISSEMENT_OLD  
         END

 CREATE PROCEDURE [dbo].[LEC_GRP_ETABLISSEMENT_FOR_MODULE_PEC_FICHEDOSSIER]
         	@ID_MODULE		int
         AS
         ------------------------------------------------------------
         -- ASD 07/01/2013 14822 : remplacer la condition !=3 via type_activite 
         ------------------------------------------------------------
         BEGIN
         	DECLARE @ID_PREV_SESSION	int
         	DECLARE @COUNT				int
         	DECLARE @NB_STAGIAIRE		int
         	DECLARE @STAGIAIRE		VARCHAR(254)
         	DECLARE @COD_MOD			varchar(254)
         
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
         
         
         CREATE TABLE #MES_STAGIAIRES (ID_STAGIAIRE_PEC INT,ID_MODULE INT)
         INSERT 	INTO #MES_STAGIAIRES
         SELECT
         	STAGIAIRE_PEC.ID_STAGIAIRE_PEC,
         	@ID_MODULE
         FROM		 
         	STAGIAIRE_PEC
         WHERE
         		@ID_MODULE		is not null	AND 
         		STAGIAIRE_PEC.ID_MODULE_PEC = @ID_MODULE	AND 
         		STAGIAIRE_PEC.ID_SESSION_PEC is null
         
         SELECT DISTINCT
                 @ID_MODULE as ID_MODULE_PEC ,
         		ADHERENT.COD_ADHERENT as COD_ADHERENT,
         		ADHERENT.LIB_RAISON_SOCIALE as LIB_RAISON_SOCIALE,
         		ADRESSE.LIB_CP_CEDEX + ' ' + ADRESSE.LIB_VIL_CEDEX AS ADRESSE,
         		BRANCHE.LIBL_BRANCHE as LIBL_BRANCHE,
         		OPTIONS.COD_OPTION as OPTION_GROUP,
         		ETABLISSEMENT.ID_ETABLISSEMENT as ID_ETABLISSEMENT
         		--GROUPE.COD_GROUPE as OPTION_GROUP
         FROM	
         		MODULE_PEC   
         		INNER JOIN #MES_STAGIAIRES on MODULE_PEC.ID_MODULE_PEC =#MES_STAGIAIRES.ID_MODULE
         		INNER JOIN STAGIAIRE_PEC ON STAGIAIRE_PEC.ID_STAGIAIRE_PEC=#MES_STAGIAIRES.ID_STAGIAIRE_PEC
         		INNER JOIN BRANCHE		ON BRANCHE.ID_BRANCHE = STAGIAIRE_PEC.ID_BRANCHE
         		INNER JOIN INDIVIDU		ON INDIVIDU.ID_INDIVIDU = STAGIAIRE_PEC.ID_INDIVIDU
         		INNER JOIN ETABLISSEMENT ON ETABLISSEMENT.ID_ETABLISSEMENT	= STAGIAIRE_PEC.ID_ETABLISSEMENT
         		INNER JOIN ADHERENT		ON ADHERENT.ID_ADHERENT				= ETABLISSEMENT.ID_ADHERENT
         		LEFT JOIN ADRESSE on ETABLISSEMENT.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE 
         		
         		LEFT JOIN GROUPE on (STAGIAIRE_PEC.ID_GROUPE = GROUPE.ID_GROUPE)
         		LEFT JOIN R20bis on GROUPE.ID_GROUPE = R20bis.ID_GROUPE and R20bis.ID_PERIODE = MODULE_PEC.ID_PERIODE 
         		LEFT JOIN OPTIONS on R20bis.ID_OPTION = OPTIONS.ID_OPTION
         		LEFT OUTER JOIN STAGIAIRE_PEC STP ON STAGIAIRE_PEC.ID_STAGIAIRE_TUTEUR = STP.ID_STAGIAIRE_PEC
         		LEFT OUTER JOIN INDIVIDU ISTP ON STP.ID_INDIVIDU = ISTP.ID_INDIVIDU
         		LEFT JOIN R19 on (R19.ID_ADHERENT = ADHERENT.ID_ADHERENT and R19.ID_ACTIVITE  in (select id_activite from GET_IDS_ACTIVITE_BY_COD_TYPE_ACTIVITE('PLAN')) -- 14822
         						and R19.ID_PERIODE = (select ID_PERIODE from PERIODE where id_type_periode =1 and bln_en_cours = 1))
         END   
         

CREATE PROCEDURE [dbo].[CLOTURER_MODULE_PEC]
          @ID_MODULE_PEC INT,
          @UserId   INT
         AS
         -- =============================================
         -- Author:  DSZ
         -- Create date: 17/02/2102
         -- Description: 13261 : Procedure de cloture du module PEC
         -- =============================================
         -- DSZ 25/05/2012 13500
         -- ajout desengagement des pce engages
         -- =============================================
         -- HBO - 20140731 #148A
         -- =============================================
         -- HBO - 20140805 #144
         -- =============================================
         -- Author : MBL
         -- Modif date: 01/10/2114
         -- Description: Inhibition temporaire de la fonctionnalite de Module PEC pour empecher la generation des MVTS BUDGETAIRES errones
         --    A SUPPRIMER APRES REVISION DE LA CLOTURE MODULE
         -- =============================================
         -- Author : MBL
         -- Modif date: 13/10/2114
         -- Description: L'inihibition temporaire est debloquee pour les utilisateurs de profil Administration rŠglement
         --              utilisant cette PS dans le cadre de la cloture automatique de module apres validation des DR
         --              pour lesquelles une demande de cloture est effectu‚e
         --    A SUPPRIMER APRES REVISION DE LA CLOTURE MODULE
         -- =============================================
         -- LDE - 16/02/2015 - #526 Retrait de l'impossibilit‚ de cl“turer
         -- =============================================
         BEGIN
          DECLARE
           @DAT_CLOTURE DATETIME,
           @ID_PROFIL int
           
           
          SELECT
           @DAT_CLOTURE = DAT_CLOTURE --VERIFIER SI LE MODULE N'EST PAS DEJA CLOTURE
          FROM
           MODULE_PEC
          WHERE
           ID_MODULE_PEC = @ID_MODULE_PEC
             
          IF @DAT_CLOTURE IS NULL --SINON ON NE FAIT RIEN
          BEGIN
           BEGIN TRY
            BEGIN TRAN CLOTURE_MODULE_PEC
             --DAT CLOTURE
             UPDATE
              MODULE_PEC
             SET
              DAT_CLOTURE = GETDATE()
             WHERE
              ID_MODULE_PEC = @ID_MODULE_PEC
         
             -- #144 : Recablage Cloture Module
             DECLARE
              @EventTypeId INT,
              @EventName VARCHAR(50),
              @EventDate DATETIME,
              @EventComment VARCHAR(7600)
         
             SELECT
              @EventTypeId = ID_TYPE_EVENEMENT,
              @EventName = LIBL_TYPE_EVENEMENT
             FROM
              TYPE_EVENEMENT
             WHERE
              COD_TYPE_EVENEMENT = 'CLOMOPEC'
         
             SET @EventComment = 'Origine : Cl“ture Module PEC OPTIFORM'
         
             DECLARE @ID_POSTE_COUT_ENGAGE INT
             DECLARE CURSOR_INCURRED_POST_COST CURSOR FOR
              SELECT
               PCE.ID_POSTE_COUT_ENGAGE
              FROM
               MODULE_PEC MP
               LEFT JOIN POSTE_COUT_ENGAGE PCE
                ON PCE.ID_MODULE_PEC = MP.ID_MODULE_PEC
              WHERE
               MP.ID_MODULE_PEC = @ID_MODULE_PEC
               AND PCE.ID_POSTE_COUT_ENGAGE IS NOT NULL
               AND PCE.DAT_DESENGAGEMENT IS NULL
              ORDER BY
               MP.ID_MODULE_PEC,
               PCE.ID_SOUS_TYPE_COUT,
               PCE.ID_POSTE_COUT_ENGAGE
         
             OPEN CURSOR_INCURRED_POST_COST
             FETCH NEXT FROM CURSOR_INCURRED_POST_COST INTO
              @ID_POSTE_COUT_ENGAGE
         
             WHILE (@@Fetch_Status <> -1)
             BEGIN
              SET @EventDate = GETDATE()
         
              EXEC MVT_BUDGETAIRE_PEC_INS_EVENEMENT_PEC
               @EventName,
               @EventDate,
               @EventTypeId,
               @ID_POSTE_COUT_ENGAGE,
               NULL,
               @UserId,
               @EventComment
         
              FETCH NEXT FROM CURSOR_INCURRED_POST_COST INTO
               @ID_POSTE_COUT_ENGAGE
             END
         
             CLOSE CURSOR_INCURRED_POST_COST
             DEALLOCATE CURSOR_INCURRED_POST_COST
            COMMIT TRAN CLOTURE_MODULE_PEC
           END TRY
           BEGIN CATCH
            SELECT
             ERROR_NUMBER() as ErrorNumber,
             ERROR_MESSAGE() as ErrorMessage,
             ERROR_LINE(),
             ERROR_PROCEDURE();
             
            -- Test XACT_STATE for 1 or -1.
            -- XACT_STATE = 0 means there is no transaction and
            -- a COMMIT or ROLLBACK would generate an error.
         
            -- Test if the transaction is uncommittable.
            IF (XACT_STATE()) = -1
            BEGIN
             PRINT
              N'The transaction CLOTURE_MODULE_PEC is in an uncommittable state. ' +
              'Rolling back transaction.'
             ROLLBACK TRANSACTION CLOTURE_MODULE_PEC;
            END;
         
            -- Test if the transaction is active and valid.
            IF (XACT_STATE()) = 1
            BEGIN
             PRINT
              N'The transaction CLOTURE_MODULE_PEC is committable. ' +
              'Committing transaction.'
             COMMIT TRANSACTION CLOTURE_MODULE_PEC;
            END;
           END CATCH
          END
         END


 -- =============================================
         -- HBO - 141113 - M16371: Lot 1 - Modification structure de donn‚es / proc‚dures stock‚es
         -- =============================================
         -- HBO - 201113 - M16378: Lot 1 - Editions
         -- =============================================
         CREATE PROCEDURE EDIT_REMISE_BANCAIRE_CHEQUES
         	@IDS_BORDEREAU varchar(500)	-- List of ID_BORDEREAU separated with ',' without spaces - i.e.: 1,2,3,4,5,6
         with recompile
         AS
         	BEGIN
         		DECLARE @Item int
         
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
         			SELECT @IDS_BORDEREAU -- Put the last item in
         
         		SELECT
         			UTILISATEUR.LIB_NOM,
         			UTILISATEUR.LIB_PNM,
         			BORDEREAU.ID_BORDEREAU,
         			BORDEREAU.COD_BORDEREAU,
         			LOT_REMISE_BANCAIRE.COD_LOT_REMISE_BANCAIRE,
         			LOT_REMISE_BANCAIRE.DAT_LOT_REMISE_BANCAIRE,
         			SUM(VERSEMENT.MNT_VERSEMENT) as MNT_BORDEREAU,
         			count(VERSEMENT.ID_VERSEMENT) as NB,
         			TRANSIT.NUM_IBAN_TRANSIT as NUM_COMPTE,
         			TRANSIT.BIC_TRANSIT as BIC,
         			TRANSIT.LIB_COMPTE_BANQUE AS LIB_COMPT_BANQUE	
         		FROM
         			BORDEREAU
         			INNER JOIN UTILISATEUR			ON BORDEREAU.ID_UTILISATEUR = UTILISATEUR.ID_UTILISATEUR
         			INNER JOIN VERSEMENT			ON (VERSEMENT.ID_BORDEREAU = BORDEREAU.ID_BORDEREAU) /*AND (VERSEMENT. ID_MODE_VERSEMENT = 1)*/
         			--INNER JOIN POSTE_VERSEMENT		ON POSTE_VERSEMENT.ID_VERSEMENT = VERSEMENT.ID_VERSEMENT
         			--INNER JOIN POSTE_IMPUTATION		ON POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT
         			INNER JOIN LOT_REMISE_BANCAIRE	ON LOT_REMISE_BANCAIRE.ID_LOT_REMISE_BANCAIRE = BORDEREAU.ID_LOT_REMISE_BANCAIRE,
         			TRANSIT	
         		WHERE
         			BORDEREAU.ID_BORDEREAU in (select Item from #List)
         			AND (VERSEMENT.BLN_ACTIF = 1)
         			--AND (POSTE_VERSEMENT.BLN_ACTIF = 1)
         		GROUP BY
         			UTILISATEUR.LIB_NOM,
         			UTILISATEUR.LIB_PNM,
         			BORDEREAU.ID_BORDEREAU,
         			BORDEREAU.COD_BORDEREAU,
         			LOT_REMISE_BANCAIRE.COD_LOT_REMISE_BANCAIRE,
         			LOT_REMISE_BANCAIRE.DAT_LOT_REMISE_BANCAIRE,
         			TRANSIT.NUM_IBAN_TRANSIT,
         			TRANSIT.BIC_TRANSIT,
         			TRANSIT.LIB_COMPTE_BANQUE
         	END


 CREATE PROCEDURE [dbo].[EditionSyntheseComptesAdherent]  
          @GroupId INT,  
          @StartDate DATETIME,  
          @EndDate DATETIME  
         AS  
         -- =================================================================================  
         -- HBO - #761  
         -- =================================================================================  
         -- OPA - #789 En tant qu'utilisateur Optiform, lorsque j'‚dite la SynthŠse de comptes Adh‚rent,   
         -- je souhaite avoir le d‚tail des op‚rations lorsqu'il y a eu des rŠglements sup‚rieurs …   
         -- l'engagement initial sur un ou plusieurs comptes  
         -- =======================================================================================================================  
         -- DSZ - #761 calcul somme engagement complementaire. Il faut soustraire certains engagament recredit‚s qui sont  
         -- pris en compte deux (ou plus) fois : en fait le montant -x est tagg‚ "3-recredit" et le montant +x "5.3"  
         -- =======================================================================================================================  
         -- DSZ 11/05/2015 #854 : Les transferts automatiques : agr‚g‚s par ann‚e et par TYPE_EVENEMENT.  Afficher le libell‚ du dernier mouvement budg‚taire  
         -- #855 dans la rubrique 2, les reliquats soient  calcul‚s en fonction des mvts budg‚taires du type d'‚v‚nement "versement"   
         -- et du type de versement "versement obligatoire" et affich‚s que pour le 'compte historique'  
         -- =================================================================================  
         -- DSZ 12/05/2015 #895 : tous mouvements budg‚taires non "tagg‚s" uniquement r‚els dans une rubrique   
         -- doivent ˆtre dynamiquement repris dans "Autres op‚rations"   
         -- =================================================================================  
         -- DSZ 15/05/2015 #855 reprise des modifs perdus en cours des merges  
         -- Cosmetique : clause where ajout‚ pour LIST_SOUS_TYPE_COUT  
         -- =================================================================================  
         -- MBL 02/06/2015 :  
         -- Correction calcul du reliquat 
         --
         -- Aucune recherche / rŠgle de gestion ne doivent se baser sur le contenu libell‚. 
         -- En effet, les rŠgles de valorisation du libell‚ peuvent varier au cours du temps
         -- Il reste d'autres cas de ce type a corriger : LIBL_MVT_BUDGETAIRE LIKE '%volontaire%'
         -- =================================================================================  
         -- DSZ 07/06/2015 US#929 dans la rubrique montant vers il faut mettre le montant brut de mes versements volontaires
         -- =================================================================================  
         BEGIN  
          DECLARE  
           @LIB_GROUPE VARCHAR(100),  
           @NUM_ANNEE VARCHAR(4)  
           
          SELECT @LIB_GROUPE = LIB_GROUPE + ' (' + COD_GROUPE + ')'  
          FROM GROUPE  
          WHERE ID_GROUPE = @GroupId    
           
          SET @NUM_ANNEE = YEAR([dbo].[GetShortDate](@StartDate))    
           
          SELECT DISTINCT  
           MVT.ID_TYPE_FINANCEMENT,  
           TF.LIBL_TYPE_FINANCEMENT,  
           SUM  
           (  
            CASE  
             WHEN  P_E_R IN ('R') AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112) < convert(varchar(8), @StartDate, 112)  
              THEN MNT_MVT_BUDGETAIRE * (CASE PB.SENS WHEN 'D' THEN -1 ELSE 1 END)  
             ELSE  0  
            END  
           ) MNT_INITIAL_R,  
           SUM  
           (  
            CASE  
             WHEN  P_E_R IN ('R','E') AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112) < convert(varchar(8), @StartDate, 112)  
              THEN MNT_MVT_BUDGETAIRE * (CASE PB.SENS WHEN 'D' THEN -1 ELSE 1 END)  
             ELSE  0  
            END  
           ) MNT_INITIAL_E,  
           SUM  
           (  
            CASE  
             WHEN  P_E_R IN ('R','E','P') AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112) < convert(varchar(8), @StartDate, 112)  
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
           ) MNT_FINAL_P,  
           CAST(0 AS DECIMAL(18,2)) TOTAL_FONDS_RECREDITES_NEW,  
           CAST(0 AS DECIMAL(18,2)) TOTAL_FONDS_RECREDITES_OLD,  
           CAST(0 AS DECIMAL(18,2)) TOTAL_FONDS_RECREDITES,  
           CAST(0 AS DECIMAL(18,2)) MNT_ENGAGE_ET_REGLE,  
           CAST(0 AS DECIMAL(18,2)) MNT_ENGAGE_ET_NON_REGLE,  
           CAST(0 AS DECIMAL(18,2)) MNT_ENGAGE_COMPL,  
           CAST(0 AS DECIMAL(18,2)) MNT_SOLD_DISPO,  
           CAST(0 AS DECIMAL(18,2)) TOTAL_MOBILISABLE,  
           CAST(0 AS DECIMAL(18,2)) MNT_RELIQUATS  
          INTO  
           #TempTableForEdition  
          FROM  
           MVT_BUDGETAIRE MVT  
           INNER JOIN COMPTE CPT  
            ON CPT.ID_COMPTE = MVT.ID_COMPTE  
           INNER JOIN TYPE_FINANCEMENT TF  
            ON TF.ID_TYPE_FINANCEMENT = CPT.ID_TYPE_FINANCEMENT  
           INNER JOIN PARAMETRAGE_BUDGETAIRE PB  
            ON PB.ID_TYPE_MOUVEMENT = MVT.ID_TYPE_MOUVEMENT  
             AND PB.ID_TYPE_COMPTE = CPT.ID_TYPE_COMPTE  
          WHERE  
           CPT.ID_GROUPE = @GroupId  
           AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112)<= convert(varchar(8), @EndDate, 112)  
           AND ABS(MVT.MNT_MVT_BUDGETAIRE) > 0  
          GROUP BY  
           MVT.ID_TYPE_FINANCEMENT,  
           TF.LIBL_TYPE_FINANCEMENT  
          ORDER BY  
           MVT.ID_TYPE_FINANCEMENT  
           
          -- #TempAllMvtBudgetaires : Liste de tous les mouvements budgetaires associes au groupe sur la periode  
          SELECT  
           MVT.ID_MVT_BUDGETAIRE,  
           MVT.ID_TYPE_MOUVEMENT,  
           MVT.ID_PERIODE_FISC,  
           TM.LIBC_TYPE_MOUVEMENT,  
           MVT.DAT_MVT_BUDGETAIRE,  
           MVT.LIBL_MVT_BUDGETAIRE,  
           MVT.MNT_MVT_BUDGETAIRE,  
           MVT.ID_EVENEMENT,  
           MVT.ID_COMPTE,  
           MVT.P_E_R,  
           MVT.DAT_MVT_BUDGETAIRE AS DAT_SAISIE_VERSEMENT,  
           VERSEMENT.ID_VERSEMENT,  
           PERIODE.NUM_ANNEE AS ANNEE_IMPUT,  
           PS.ID_TYPE_VERSEMENT,  
           PS.ID_POSTE_VERSEMENT,  
           TV.COD_TYPE_VERSEMENT,  
           TE.ID_TYPE_EVENEMENT,  
           TE.COD_TYPE_EVENEMENT,  
           MVT.ID_MODULE_PEC,  
           MVT.ID_CONTRAT_PRO,  
           ID_SOUS_TYPE_COUT = ISNULL(PCR.ID_SOUS_TYPE_COUT, PCE.ID_SOUS_TYPE_COUT),  
           ID_TYPE_FINANCEMENT,  
           DAT_ENCAISSEMENT,  
           UPPER (LIBL_MODE_VERSEMENT) + ' - ' + CAST(VERSEMENT.ID_VERSEMENT  AS VARCHAR(15)) AS LIB_VERSEMENT,  
           R.DAT_REGLEMENT  
          INTO  
           #TempAllMvtBudgetaires  
          FROM  
           MVT_BUDGETAIRE MVT  
           INNER JOIN EVENEMENT  
            ON MVT.ID_EVENEMENT  = EVENEMENT.ID_EVENEMENT  
           INNER JOIN TYPE_EVENEMENT TE  
            ON TE.ID_TYPE_EVENEMENT = EVENEMENT.ID_TYPE_EVENEMENT  
           INNER JOIN TYPE_MOUVEMENT TM  
            ON TM.ID_TYPE_MOUVEMENT = MVT.ID_TYPE_MOUVEMENT  
           LEFT JOIN VERSEMENT  
            ON VERSEMENT.ID_VERSEMENT = EVENEMENT.ID_VERSEMENT  
           LEFT JOIN MODE_VERSEMENT  
            ON VERSEMENT.ID_MODE_VERSEMENT = MODE_VERSEMENT.ID_MODE_VERSEMENT  
           LEFT JOIN POSTE_VERSEMENT PS  
            ON PS.ID_VERSEMENT   = VERSEMENT.ID_VERSEMENT  
            AND MVT.ID_PERIODE_FISC=ps.ID_PERIODE  
           LEFT JOIN PERIODE  
            ON PERIODE.ID_PERIODE = PS.ID_PERIODE  
           LEFT JOIN TYPE_VERSEMENT TV  
            ON TV.ID_TYPE_VERSEMENT = PS.ID_TYPE_VERSEMENT  
           LEFT JOIN POSTE_COUT_ENGAGE PCE  
            ON PCE.ID_POSTE_COUT_ENGAGE = EVENEMENT.ID_POSTE_COUT_ENGAGE  
           LEFT JOIN POSTE_COUT_REGLE PCR  
            ON PCR.ID_POSTE_COUT_REGLE = EVENEMENT.ID_POSTE_COUT_REGLE  
           LEFT JOIN REGLEMENT R  
            ON R.ID_REGLEMENT = PCR.ID_REGLEMENT  
          WHERE  
           MVT.ID_GROUPE = @GroupId  
           AND convert(varchar(8), MVT.DAT_MVT_BUDGETAIRE, 112) >= convert(varchar(8), @StartDate, 112)  
           AND convert(varchar(8), MVT.DAT_MVT_BUDGETAIRE, 112) <= convert(varchar(8), @EndDate, 112)  
           AND ABS(MVT.MNT_MVT_BUDGETAIRE) > 0  
           
          ------------------ #TMP_MVT_BUD_ENG_ET_REGLE  ------------------    
          -- #TMP_MVT_BUD_ENG_ET_REGLE : Liste de tous les mouvements budgetaires associes au groupe imputant le compte groupe sur la periode  
          SELECT  
           MVT.ID_MVT_BUDGETAIRE,  
           MVT.ID_ADHERENT,  
           MVT.ID_TYPE_MOUVEMENT,  
           TYPE_MOUVEMENT.LIBC_TYPE_MOUVEMENT,  
           MVT.ID_EVENEMENT,  
           MVT.DAT_MVT_BUDGETAIRE,  
           LIBL_TYPE_FINANCEMENT,  
           PERIODE.NUM_ANNEE,  
           TE.ID_TYPE_EVENEMENT,  
           MNT_MVT_GROUPE = MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'C' THEN 1 ELSE -1 END),  
           MNT_DEBIT = MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 1 ELSE 0 END),  
           MNT_CREDIT = MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 0 ELSE 1 END),  
           LIBL_MVT_BUDGETAIRE,  
           LIBL_PERIODE = 'Ann‚e Rbt. P10+ ' + CAST(NUM_ANNEE AS VARCHAR),  
           P_E_R,  
           TAG = CAST(NULL AS VARCHAR(50)),  
           COMPTE.ID_TYPE_FINANCEMENT,  
           MVT.ID_MODULE_PEC,  
           MVT.MNT_MVT_BUDGETAIRE,  
           PARAMETRAGE_BUDGETAIRE.SENS,  
           EVENEMENT.ID_POSTE_COUT_REGLE,  
           EVENEMENT.ID_POSTE_COUT_ENGAGE,  
           R.DAT_REGLEMENT  
          INTO  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          FROM  
           MVT_BUDGETAIRE MVT  
           INNER JOIN EVENEMENT  
            ON MVT.ID_EVENEMENT = EVENEMENT.ID_EVENEMENT  
           INNER JOIN COMPTE  
            ON MVT.ID_COMPTE = COMPTE.ID_COMPTE  
           INNER JOIN TYPE_COMPTE  
            ON COMPTE.ID_TYPE_COMPTE = TYPE_COMPTE.ID_TYPE_COMPTE  
           INNER JOIN TYPE_MOUVEMENT  
            ON MVT.ID_TYPE_MOUVEMENT = TYPE_MOUVEMENT.ID_TYPE_MOUVEMENT  
           INNER JOIN PARAMETRAGE_BUDGETAIRE  
            ON PARAMETRAGE_BUDGETAIRE.ID_TYPE_MOUVEMENT = TYPE_MOUVEMENT.ID_TYPE_MOUVEMENT  
            AND PARAMETRAGE_BUDGETAIRE.ID_TYPE_COMPTE = TYPE_COMPTE.ID_TYPE_COMPTE  
           INNER JOIN PERIODE  
            ON COMPTE.ID_PERIODE = PERIODE.ID_PERIODE  
           INNER JOIN TYPE_FINANCEMENT  
            ON COMPTE.ID_TYPE_FINANCEMENT = TYPE_FINANCEMENT.ID_TYPE_FINANCEMENT  
           INNER JOIN TYPE_EVENEMENT TE  
            ON TE.ID_TYPE_EVENEMENT = EVENEMENT.ID_TYPE_EVENEMENT  
           LEFT JOIN POSTE_COUT_REGLE PCR  
            ON PCR.ID_POSTE_COUT_REGLE = EVENEMENT.ID_POSTE_COUT_REGLE  
           LEFT JOIN REGLEMENT R  
            ON R.ID_REGLEMENT = PCR.ID_REGLEMENT  
          WHERE  
           COMPTE.ID_GROUPE = @GroupId  
           AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112) >= convert(varchar(8), @StartDate, 112)  
           AND convert(varchar(8), DAT_MVT_BUDGETAIRE, 112) <= convert(varchar(8), @EndDate, 112)  
           AND MVT.ID_COMPTE IS NOT NULL  
           AND ABS(MVT.MNT_MVT_BUDGETAIRE ) > 0  
          ORDER BY  
           COMPTE.ID_TYPE_FINANCEMENT,  
           NUM_ANNEE,  
           DAT_MVT_BUDGETAIRE,  
           P_E_R  
           
           
          SELECT   
           MVT.ID_TYPE_FINANCEMENT,  
           MPEC.ID_MODULE_PEC,  
           MPEC.COD_MODULE_PEC,  
           STC.ID_SOUS_TYPE_COUT,  
           STC.COD_SOUS_TYPE_COUT,   
           STC.LIBL_SOUS_TYPE_COUT,  
           MIN(R.DAT_REGLEMENT) AS DAT_REGLEMENT,  
           SUM  
           (  
            CASE  
             WHEN  MVT.P_E_R IN ('E')  
              THEN MVT.MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN -1 ELSE 1 END)  
             ELSE  0  
            END  
           ) AS MNT_INITIAL_E,  
           SUM  
           (  
            CASE  
             WHEN  MVT.P_E_R IN ('R')  
              THEN MVT.MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 1 ELSE -1 END)  
             ELSE  0  
            END  
           ) AS MNT_REGLE,  
           SUM  
           (  
            CASE  
             WHEN  MVT.P_E_R IN ('R')  
              THEN MVT.MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 1 ELSE -1 END)  
             WHEN  MVT.P_E_R IN ('E')  
              THEN MVT.MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 1 ELSE -1 END)  
             ELSE  0  
            END  
           ) AS MNT  
          INTO  
           #TempTableForEditionCompl  
          FROM   
           #TMP_MVT_BUD_ENG_ET_REGLE MVT  
           INNER JOIN MODULE_PEC MPEC  
            ON MVT.ID_MODULE_PEC = MPEC.ID_MODULE_PEC  
           LEFT JOIN POSTE_COUT_REGLE PCR  
            ON PCR.ID_POSTE_COUT_REGLE = MVT.ID_POSTE_COUT_REGLE  
           LEFT JOIN POSTE_COUT_ENGAGE PCE  
            ON PCE.ID_POSTE_COUT_ENGAGE = MVT.ID_POSTE_COUT_ENGAGE  
           LEFT JOIN REGLEMENT R  
            ON R.ID_REGLEMENT = PCR.ID_REGLEMENT  
           LEFT JOIN SOUS_TYPE_COUT STC  
            ON PCR.ID_SOUS_TYPE_COUT = STC.ID_SOUS_TYPE_COUT OR PCE.ID_SOUS_TYPE_COUT = STC.ID_SOUS_TYPE_COUT  
          WHERE  
           ABS(MVT.MNT_MVT_BUDGETAIRE) > 0  
           AND STC.ID_SOUS_TYPE_COUT IS NOT NULL  
           AND convert(varchar(8), R.DAT_REGLEMENT, 112) >= convert(varchar(8), @StartDate, 112)  
           AND convert(varchar(8), R.DAT_REGLEMENT, 112) <= convert(varchar(8), @EndDate, 112)  
          GROUP BY  
           MVT.ID_TYPE_FINANCEMENT,  
           MPEC.ID_MODULE_PEC,  
           MPEC.COD_MODULE_PEC,  
           STC.ID_SOUS_TYPE_COUT,  
           STC.COD_SOUS_TYPE_COUT,  
           STC.LIBL_SOUS_TYPE_COUT  
          HAVING  
           CAST(ABS(SUM(MNT_MVT_GROUPE)) AS DECIMAL(18,2)) > 0  
           AND  
           SUM  
           (  
            CASE  
             WHEN  MVT.P_E_R IN ('E')  
              THEN MVT.MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN -1 ELSE 1 END)  
             WHEN  MVT.P_E_R IN ('R')  
              THEN MVT.MNT_MVT_BUDGETAIRE * (CASE SENS WHEN 'D' THEN 1 ELSE -1 END)  
             ELSE  0  
            END  
           ) > 0  
           AND  
           NOT EXISTS  
           (  
            SELECT 1  
            FROM  
             #TempAllMvtBudgetaires TMP_E  
            WHERE  
             TMP_E.ID_MODULE_PEC = MPEC.ID_MODULE_PEC  
             AND TMP_E.ID_SOUS_TYPE_COUT = STC.ID_SOUS_TYPE_COUT  
             AND  
             (  
              TMP_E.COD_TYPE_EVENEMENT IN ('ENGAGMT')  
              OR LIBL_MVT_BUDGETAIRE LIKE 'REP%'  -- Reprise   
             )  
           )  
           
           
          ---------------------------- FIN 0.Report du solde disponible ---------------------------------------------------------  
          ----------------- 1. TOUTE LES VERSEMENT VOLONTAIRE  DE 'Date de d‚but' … 'Date de fin' -------------------------------  
          UPDATE  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          SET  
           TAG = '1-VV'  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
           INNER JOIN #TempAllMvtBudgetaires  
            ON #TMP_MVT_BUD_ENG_ET_REGLE.ID_MVT_BUDGETAIRE = #TempAllMvtBudgetaires.ID_MVT_BUDGETAIRE  
          WHERE  
           COD_TYPE_VERSEMENT = 'VOLONT' -- Versement Volontaire  
           AND TAG IS NULL  
           
          -- On recupere dans #TMP_VERSEMENT2 tous les versements vers‚s pour l'activit‚ P10+ (Montant Vers‚ HT) sur le compte adherent  
          SELECT DISTINCT  
           LIB_VERSEMENT,  
           C.ID_TYPE_MOUVEMENT,  
           C.LIBC_TYPE_MOUVEMENT,  
           T.MNT_MVT_BUDGETAIRE,  
           T.DAT_SAISIE_VERSEMENT,  
           T.ID_VERSEMENT,  
           T.ANNEE_IMPUT,  
           T.ID_TYPE_VERSEMENT,  
           T.P_E_R,  
           DAT_ENCAISSEMENT,  
           T.ID_TYPE_FINANCEMENT ,
           SUM(POSTE_IMPUTATION.MNT_HT) as MNT_VERSEMENT_HT  
         
          INTO  
           #TMP_VERSEMENT  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE C  
           INNER JOIN #TempAllMvtBudgetaires T  
            ON C.ID_MVT_BUDGETAIRE = T.ID_MVT_BUDGETAIRE  
           inner join 
           POSTE_VERSEMENT on POSTE_VERSEMENT.ID_VERSEMENT = T.ID_VERSEMENT
           inner join POSTE_IMPUTATION on POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT 
          WHERE  
           TAG = '1-VV'  
           AND T.COD_TYPE_VERSEMENT = 'VOLONT' -- Versement Volontaire  
           AND T.ID_COMPTE IS NOT NULL  
           group by 
          LIB_VERSEMENT,  
          C.ID_TYPE_MOUVEMENT,  
          C.LIBC_TYPE_MOUVEMENT,  
          T.MNT_MVT_BUDGETAIRE,  
          T.DAT_SAISIE_VERSEMENT,  
          T.ID_VERSEMENT,  
          T.ANNEE_IMPUT,  
          T.ID_TYPE_VERSEMENT,  
          T.P_E_R,  
          DAT_ENCAISSEMENT,  
          T.ID_TYPE_FINANCEMENT  
            
          SELECT  
           LIB_VERSEMENT,  
           ANNEE_IMPUT,  
           CASE P_E_R  
            WHEN  'P'  
             THEN DAT_ENCAISSEMENT  
            ELSE  NULL  
           END AS DAT_PREV,  
           CASE P_E_R  
            WHEN  'P'  
             THEN MNT_MVT_BUDGETAIRE  
            ELSE  0.00  
           END AS MNT_PREV,  
           CASE P_E_R  
            WHEN  'P'  
             THEN NULL  
            ELSE  DAT_SAISIE_VERSEMENT  
           END AS DAT_SAISIE_VERSEMENT,  
           CASE P_E_R  
            WHEN  'P'  
             THEN 0.00  
            ELSE  MNT_VERSEMENT_HT  
           END AS MNT_VERSE_HT,  
           CASE P_E_R  
            WHEN  'P'  
             THEN 0.00  
            ELSE  MNT_MVT_BUDGETAIRE  
           END AS MNT_MOBILISABLE_HT,  
           ID_TYPE_FINANCEMENT  
          INTO  
           #TempVersements  
          FROM  
           #TMP_VERSEMENT B  
          WHERE  
           (  
            DAT_ENCAISSEMENT IS NOT NULL  
            AND P_E_R = 'P'  
           )  
           OR P_E_R = 'R'  
           
          ---------------------------- FIN DE PARTI 1. VERSEMENT VOLONTAIRES -------------------------------------------------------  
          ---------------------------- DEBUT DE 2. RELIQUATS PLAN DE FORMATION -----------------------------------------------------  
          -- 2 RELIQUAT PLAN DE FORMATION    
           
            
           
          UPDATE  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          SET  
           TAG = '2-RELIQ'  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
           INNER JOIN #TempAllMvtBudgetaires  
            ON #TMP_MVT_BUD_ENG_ET_REGLE.ID_MVT_BUDGETAIRE = #TempAllMvtBudgetaires.ID_MVT_BUDGETAIRE  
          WHERE  
           COD_TYPE_VERSEMENT = 'OBLIG'  -- OBLIGATOIRE  
           AND #TMP_MVT_BUD_ENG_ET_REGLE.P_E_R = 'R'  
           AND TAG IS NULL  
           
          declare @ID_COMPTE_HISTORIQUE int  
         
          -- MBL 02/06/2015 :  
          -- Aucune recherche / rŠgle de gestion ne doivent se baser sur le contenu libell‚. 
          -- En effet, les rŠgles de valorisation du libell‚ peuvent varier au cours du temps
          --select @ID_COMPTE_HISTORIQUE = ID_TYPE_FINANCEMENT from TYPE_FINANCEMENT where LIBC_TYPE_FINANCEMENT = 'Compte Historique'  
          select @ID_COMPTE_HISTORIQUE = ID_TYPE_FINANCEMENT from TYPE_FINANCEMENT where COD_TYPE_FINANCEMENT = '1'  -- 'Compte Historique'  
          UPDATE  
           #TempTableForEdition  
          SET  
           MNT_RELIQUATS =  
            (  
             SELECT SUM(MNT_MVT_GROUPE)  
             FROM #TMP_MVT_BUD_ENG_ET_REGLE  
             WHERE TAG = '2-RELIQ' AND #TMP_MVT_BUD_ENG_ET_REGLE.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT)  
          where ID_TYPE_FINANCEMENT = @ID_COMPTE_HISTORIQUE --#855 uniquement pour compte historique     
           
          ---------------------------- FIN DE 2. RELIQATS PLAN DE FORMATION ---------------------------------------------------------  
          ---------------------------- DEBUT DE 3. TOTAL DES FONDS RECREDITES -------------------------------------------------------  
           
          -- 3.1 .Dont Montant Desengage sur les dossiers annee N suite a cloture  
          -- Gestion de la cloture Module et de la decloture  
          UPDATE  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          SET  
           TAG = '3-RECREDIT'  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
           INNER JOIN #TempAllMvtBudgetaires  
            ON #TMP_MVT_BUD_ENG_ET_REGLE.ID_MVT_BUDGETAIRE = #TempAllMvtBudgetaires.ID_MVT_BUDGETAIRE  
          WHERE  
           #TMP_MVT_BUD_ENG_ET_REGLE.P_E_R = 'E'  
           AND NUM_ANNEE >= @NUM_ANNEE  
           AND TAG IS NULL  
           AND  
           (  
            EXISTS  
            (  
             SELECT  
              TMP_E.ID_MODULE_PEC,  
              SUM(MNT_MVT_BUDGETAIRE)  
             FROM  
              #TempAllMvtBudgetaires TMP_E  
             WHERE  
              TMP_E.ID_MODULE_PEC = #TempAllMvtBudgetaires.ID_MODULE_PEC  
              AND TMP_E.ID_SOUS_TYPE_COUT = #TempAllMvtBudgetaires.ID_SOUS_TYPE_COUT  
              AND TMP_E.COD_TYPE_EVENEMENT IN ('CLOTACT', 'CLOMOPEC', 'ANCLOPEC') -- Cloture Action, Cloture Module et Annulation Cloture  
             GROUP BY  
              TMP_E.ID_MODULE_PEC  
             HAVING  
              CAST(ABS(SUM(MNT_MVT_BUDGETAIRE)) AS DECIMAL(18,2)) > 0  
            )  
           )  
            
          UPDATE  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          SET  
           TAG = '3-RECREDIT'  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
           INNER JOIN #TempAllMvtBudgetaires  
            ON #TMP_MVT_BUD_ENG_ET_REGLE.ID_MVT_BUDGETAIRE = #TempAllMvtBudgetaires.ID_MVT_BUDGETAIRE  
          WHERE  
           #TMP_MVT_BUD_ENG_ET_REGLE.P_E_R IN ('E', 'R')  
           AND #TempAllMvtBudgetaires.ID_MODULE_PEC IS NOT NULL  
           AND NUM_ANNEE >= @NUM_ANNEE  
           AND TAG IS NULL  
           AND  
           (  
            NOT EXISTS  
            (  
             SELECT 1  
             FROM  
              #TempAllMvtBudgetaires TMP_E  
             WHERE  
              TMP_E.ID_MODULE_PEC = #TempAllMvtBudgetaires.ID_MODULE_PEC  
              AND TMP_E.ID_SOUS_TYPE_COUT = #TempAllMvtBudgetaires.ID_SOUS_TYPE_COUT  
              AND TMP_E.COD_TYPE_EVENEMENT IN ('ENGAGMT')  
              AND TMP_E.DAT_MVT_BUDGETAIRE <= #TempAllMvtBudgetaires.DAT_MVT_BUDGETAIRE  
            )  
           )  
           AND COD_TYPE_EVENEMENT NOT IN ('ENGAGMT', 'REGLEMT' , 'REGCOURT')  
           AND  
           (  
            NOT EXISTS  
            (  
             SELECT 1  
             FROM  
              #TempAllMvtBudgetaires TMP_E  
             WHERE  
              TMP_E.ID_MODULE_PEC = #TempAllMvtBudgetaires.ID_MODULE_PEC  
              AND TMP_E.ID_SOUS_TYPE_COUT = #TempAllMvtBudgetaires.ID_SOUS_TYPE_COUT  
              AND LIBL_MVT_BUDGETAIRE LIKE 'REP%'  -- Reprise  
            )  
           )  
           
          SELECT DISTINCT  
           SUM(MNT_MVT_GROUPE) TOTAL_FONDS_RECREDITES_NEW,  
           ID_TYPE_FINANCEMENT  
          INTO  
           #TempRecredit1  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          WHERE  
           TAG = '3-RECREDIT'  
          GROUP BY  
           TAG,  
           ID_TYPE_FINANCEMENT  
           
          -- 3.2 .Dont Montant Desengage ou Regle sur les dossiers ANTERIEURES et Non engage sur la periode  
          UPDATE  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          SET  
           TAG = '3-RECREDIT'  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
           INNER JOIN #TempAllMvtBudgetaires  
            ON #TMP_MVT_BUD_ENG_ET_REGLE.ID_MVT_BUDGETAIRE = #TempAllMvtBudgetaires.ID_MVT_BUDGETAIRE  
          WHERE  
           #TMP_MVT_BUD_ENG_ET_REGLE.P_E_R IN ('E', 'R')  
           AND #TempAllMvtBudgetaires.ID_MODULE_PEC IS NOT NULL  
           AND NUM_ANNEE < @NUM_ANNEE  
           AND TAG IS NULL  
           AND  
           (  
            NOT EXISTS  
            (  
             SELECT 1  
             FROM  
              #TempAllMvtBudgetaires TMP_E  
             WHERE  
              TMP_E.ID_MODULE_PEC = #TempAllMvtBudgetaires.ID_MODULE_PEC  
              AND TMP_E.ID_SOUS_TYPE_COUT = #TempAllMvtBudgetaires.ID_SOUS_TYPE_COUT  
              AND TMP_E.COD_TYPE_EVENEMENT IN ('ENGAGMT')  
              AND TMP_E.DAT_MVT_BUDGETAIRE < #TempAllMvtBudgetaires.DAT_MVT_BUDGETAIRE  
            )  
           )  
           AND COD_TYPE_EVENEMENT NOT IN ('ENGAGMT', 'REGLEMT' , 'REGCOURT')  
           AND  
           (  
            NOT EXISTS  
            (  
             SELECT 1  
             FROM  
              #TempAllMvtBudgetaires TMP_E  
             WHERE  
              TMP_E.ID_MODULE_PEC = #TempAllMvtBudgetaires.ID_MODULE_PEC  
              AND TMP_E.ID_SOUS_TYPE_COUT = #TempAllMvtBudgetaires.ID_SOUS_TYPE_COUT  
              AND LIBL_MVT_BUDGETAIRE LIKE 'REP%'  -- Reprise  
            )  
           )  
           
           
          SELECT DISTINCT  
           SUM(MNT_MVT_GROUPE) TOTAL_FONDS_RECREDITES_OLD,  
           ID_TYPE_FINANCEMENT  
          INTO  
           #TempRecredit2  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          WHERE  
           TAG = '3-RECREDIT'  
          GROUP BY  
           TAG,  
           ID_TYPE_FINANCEMENT  
            
          UPDATE  
           #TempTableForEdition  
          SET  
           TOTAL_FONDS_RECREDITES_NEW = CAST(ISNULL(A.TOTAL_FONDS_RECREDITES_NEW,0)AS DECIMAL(18,2)),  
           TOTAL_FONDS_RECREDITES_OLD = CAST(ISNULL(B.TOTAL_FONDS_RECREDITES_OLD,0)AS DECIMAL(18,2)) ,  
           TOTAL_FONDS_RECREDITES = CAST(ISNULL(A.TOTAL_FONDS_RECREDITES_NEW,0)AS DECIMAL(18,2)) + CAST(ISNULL(B.TOTAL_FONDS_RECREDITES_OLD,0)AS DECIMAL(18,2))  
          FROM  
           #TempRecredit1 A  
           FULL OUTER JOIN #TempRecredit2 B  
            ON A.ID_TYPE_FINANCEMENT = B.ID_TYPE_FINANCEMENT  
          WHERE  
           A.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
           OR B.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
           
          --------------------------- FIN DE 3. TOTAL DES FONDS RECREDITES -------------------------------------------  
          ---------------------------- DEBUT DE 5. ENGAGEMENTS---------------------------------------------------------  
           
          --5.1 Formation engages et reglees    
          -- Part 5.1  
          UPDATE  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          SET  
           TAG = '5.1'  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
           INNER JOIN #TempAllMvtBudgetaires  
            ON #TMP_MVT_BUD_ENG_ET_REGLE.ID_MVT_BUDGETAIRE = #TempAllMvtBudgetaires.ID_MVT_BUDGETAIRE  
          WHERE  
           #TMP_MVT_BUD_ENG_ET_REGLE.ID_MODULE_PEC IS NOT NULL  
           AND COD_TYPE_EVENEMENT IN ('REGLEMT' , 'REGCOURT')  
           AND #TMP_MVT_BUD_ENG_ET_REGLE.P_E_R = 'R'  
           AND TAG IS NULL  
           AND  
           (  
            EXISTS  
            (  
             SELECT 1  
             FROM  
              #TempAllMvtBudgetaires TMP_E  
             WHERE  
              TMP_E.ID_MODULE_PEC = #TempAllMvtBudgetaires.ID_MODULE_PEC  
              AND TMP_E.ID_SOUS_TYPE_COUT = #TempAllMvtBudgetaires.ID_SOUS_TYPE_COUT  
              AND  
              (  
               TMP_E.COD_TYPE_EVENEMENT IN  ('ENGAGMT')  
               OR LIBL_MVT_BUDGETAIRE LIKE 'REP%'  -- Reprise  
              )  
            )  
           )  
           
          SELECT  
           NUM_ANNEE,  
           MNT_REGLE = -SUM(MNT_MVT_GROUPE),  
           ID_TYPE_FINANCEMENT  
          INTO  
           #FOR_ENGAGE_ET_REGLE  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          WHERE  
           TAG = '5.1'  
          GROUP BY  
           NUM_ANNEE,  
           ID_TYPE_FINANCEMENT  
           
          UPDATE  
           #TempTableForEdition  
          SET  
           MNT_ENGAGE_ET_REGLE = (  
            SELECT  SUM(MNT_REGLE)  
            FROM  #FOR_ENGAGE_ET_REGLE  
            WHERE  #FOR_ENGAGE_ET_REGLE.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
            GROUP BY ID_TYPE_FINANCEMENT)  
            
          -- 5.2 Formation engage et non reglees  
          UPDATE  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          SET  
           TAG = '5.2'  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
           INNER JOIN #TempAllMvtBudgetaires  
            ON #TMP_MVT_BUD_ENG_ET_REGLE.ID_MVT_BUDGETAIRE = #TempAllMvtBudgetaires.ID_MVT_BUDGETAIRE  
          WHERE  
           #TMP_MVT_BUD_ENG_ET_REGLE.ID_MODULE_PEC IS NOT NULL  
           AND #TMP_MVT_BUD_ENG_ET_REGLE.P_E_R = 'E'  
           AND TAG IS NULL  
           AND  
           (  
            EXISTS  
            (  
             SELECT 1  
             FROM  
              #TempAllMvtBudgetaires TMP_E  
             WHERE  
              TMP_E.ID_MODULE_PEC = #TempAllMvtBudgetaires.ID_MODULE_PEC  
              AND TMP_E.ID_SOUS_TYPE_COUT = #TempAllMvtBudgetaires.ID_SOUS_TYPE_COUT  
              AND  
              (  
               TMP_E.COD_TYPE_EVENEMENT IN ('ENGAGMT')  
               OR LIBL_MVT_BUDGETAIRE LIKE 'REP%'  -- Reprise   
              )  
            )  
           )  
           
          SELECT  
           NUM_ANNEE,  
           MNT_REGLE = -SUM(MNT_MVT_GROUPE),  
           ID_TYPE_FINANCEMENT  
          INTO  
           #FOR_ENGAGE_ET_NON_REGLE  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          WHERE  
           TAG = '5.2'  
          GROUP BY  
           NUM_ANNEE,  
           ID_TYPE_FINANCEMENT  
           
          UPDATE  
           #TempTableForEdition  
          SET  
           MNT_ENGAGE_ET_NON_REGLE =  
            (  
             SELECT  SUM(MNT_REGLE)  
             FROM  #FOR_ENGAGE_ET_NON_REGLE  
             WHERE  #FOR_ENGAGE_ET_NON_REGLE.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
             GROUP BY ID_TYPE_FINANCEMENT)  
           
          --5.3 CALCUL DES REGLEMENTS COMPLEMENTAIRES  
          -- M‚thodologie:  
          -- Pour un module et un sous type de cout, si pour les evenements de reglements,  
          -- la somme des mvt budg de rŠglement (P_E_R = 'R') > la somme des mvt budg d'engagement (P_E_R = 'E')  
          -- sur des engagements non r‚alis‚es sur la periode  
          -- alors il y a engagement compl‚mentaire  
          UPDATE  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          SET  
           TAG = '5.3'  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
           INNER JOIN #TempAllMvtBudgetaires  
            ON #TMP_MVT_BUD_ENG_ET_REGLE.ID_MVT_BUDGETAIRE = #TempAllMvtBudgetaires.ID_MVT_BUDGETAIRE  
          WHERE  
           #TMP_MVT_BUD_ENG_ET_REGLE.ID_MODULE_PEC IS NOT NULL  
           AND COD_TYPE_EVENEMENT IN ('REGLEMT' , 'REGCOURT')  
           AND #TMP_MVT_BUD_ENG_ET_REGLE.P_E_R IN ('E', 'R')  
           AND convert(varchar(8), #TempAllMvtBudgetaires.DAT_REGLEMENT, 112) >= convert(varchar(8), @StartDate, 112)  
           AND convert(varchar(8), #TempAllMvtBudgetaires.DAT_REGLEMENT, 112) <= convert(varchar(8), @EndDate, 112)  
           AND NOT EXISTS  
           (  
            SELECT 1  
            FROM  
             #TempAllMvtBudgetaires TMP_E  
            WHERE  
             TMP_E.ID_MODULE_PEC = #TempAllMvtBudgetaires.ID_MODULE_PEC  
             AND TMP_E.ID_SOUS_TYPE_COUT = #TempAllMvtBudgetaires.ID_SOUS_TYPE_COUT  
             AND  
             (  
              TMP_E.COD_TYPE_EVENEMENT IN ('ENGAGMT')  
              OR LIBL_MVT_BUDGETAIRE LIKE 'REP%'  -- Reprise   
             )  
           )  
           AND TAG IS NULL  
           
          SELECT  
           NUM_ANNEE,  
           MNT_REGLE = -SUM(MNT_MVT_GROUPE),  
           ID_TYPE_FINANCEMENT,  
           LIBL_TYPE_FINANCEMENT  
          INTO  
           #FOR_ENGAGE_COMPL  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE  
          WHERE  
           TAG = '5.3'  
          GROUP BY    
           NUM_ANNEE,  
           ID_TYPE_FINANCEMENT,  
           LIBL_TYPE_FINANCEMENT  
          HAVING CAST(ABS(SUM(MNT_MVT_GROUPE)) AS DECIMAL(18,2)) > 0  
           
          UPDATE  
           #TempTableForEdition  
          SET  
           MNT_ENGAGE_COMPL =  
            (  
             SELECT coalesce(SUM(#TempTableForEditionCompl.MNT), 0)  
             FROM  #TempTableForEditionCompl   
             WHERE #TempTableForEditionCompl.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT)  
           
          ---------------------------- FIN  DE 5. ENGAGEMENTS---------------------------------------------------------  
           
          ---------------------------- DEBUT DE 4. REGULARISATION (VIREMENTS, RESTITUTIONS,..) ---------------------  
          -- On prend tous les mouvements de reglement ou d'engagement imput‚s sur le compte avec TAG vierge  
            
            
            
          UPDATE  
           #TMP_MVT_BUD_ENG_ET_REGLE   
          SET  
           TAG = '4-REGUL'  
          FROM  
           #TMP_MVT_BUD_ENG_ET_REGLE   
           INNER JOIN #TempAllMvtBudgetaires  
            ON #TMP_MVT_BUD_ENG_ET_REGLE.ID_MVT_BUDGETAIRE = #TempAllMvtBudgetaires.ID_MVT_BUDGETAIRE   
          WHERE  
           #TMP_MVT_BUD_ENG_ET_REGLE.P_E_R IN ('E', 'R')  
           AND TAG IS NULL   
           
          CREATE TABLE #TempRegularisations  
          (  
           DAT_REGUL   DATETIME,  
           LIB_REGULARISATION VARCHAR(70),  
           MNT_REGUL DECIMAL(18,2),  
           ID_TYPE_FINANCEMENT INT  
          )  
            
           
          declare @id_transfert_manuel int  
          select @id_transfert_manuel = id_type_evenement from type_EVENEMENT where COD_TYPE_EVENEMENT = 'TRANSFER'  
            
          --#854 tout ce qui n'est pas transfert ou est transfert manuel va tel quel  
           INSERT INTO  
            #TempRegularisations  
           SELECT  
            DAT_MVT_BUDGETAIRE AS DAT_REGUL,  
            LIBL_MVT_BUDGETAIRE + '(' + CAST(NUM_ANNEE AS VARCHAR(4) ) + ')' AS LIB_REGULARISATION,  
            ISNULL(MNT_MVT_GROUPE,0) AS MNT_REGUL,  
           ID_TYPE_FINANCEMENT  
           FROM  
            #TMP_MVT_BUD_ENG_ET_REGLE  
            INNER JOIN EVENEMENT ON EVENEMENT.ID_EVENEMENT = #TMP_MVT_BUD_ENG_ET_REGLE.ID_EVENEMENT   
           WHERE  
            TAG = '4-REGUL'  
             AND   
             (EVENEMENT.ID_TRANSFERT IS NULL   
             or  
             EVENEMENT.ID_TYPE_EVENEMENT = @id_transfert_manuel)  
           ORDER BY  
            DAT_MVT_BUDGETAIRE,  
            LIBC_TYPE_MOUVEMENT;  
           
           --#854 Les transferts automatiques : agr‚g‚s par ann‚e et par TYPE_EVENEMENT  
           with tran_auto as   
           (  
           SELECT  
            MAX(DAT_MVT_BUDGETAIRE) AS DAT_REGUL,  
            MAX(id_mvt_budgetaire) as id,  
            SUM(MNT_MVT_GROUPE) AS MNT_REGUL,  
            ID_TYPE_FINANCEMENT  
           FROM  
            #TMP_MVT_BUD_ENG_ET_REGLE  
            INNER JOIN EVENEMENT ON EVENEMENT.ID_EVENEMENT = #TMP_MVT_BUD_ENG_ET_REGLE.ID_EVENEMENT   
            INNER JOIN TYPE_EVENEMENT ON  TYPE_EVENEMENT.ID_TYPE_EVENEMENT =  #TMP_MVT_BUD_ENG_ET_REGLE.ID_TYPE_EVENEMENT  
           WHERE  
            TAG = '4-REGUL'  
            AND EVENEMENT.ID_TRANSFERT IS NOT NULL   
            and EVENEMENT.ID_TYPE_EVENEMENT <> @id_transfert_manuel  
           GROUP BY  
            #TMP_MVT_BUD_ENG_ET_REGLE.ID_TYPE_EVENEMENT,  
            #TMP_MVT_BUD_ENG_ET_REGLE.NUM_ANNEE,  
            ID_TYPE_FINANCEMENT  
           )  
           INSERT INTO  
            #TempRegularisations  
           SELECT  
            DAT_REGUL,  
            MVT_BUDGETAIRE.LIBL_MVT_BUDGETAIRE  AS LIB_REGULARISATION,  
            MNT_REGUL,  
            tran_auto.ID_TYPE_FINANCEMENT  
           FROM  
            tran_auto  
            INNER JOIN MVT_BUDGETAIRE  ON  tran_auto.ID =  MVT_BUDGETAIRE.ID_MVT_BUDGETAIRE   
             
          ------------------- #895 : tout ce qui n'est pas tagg‚ est recuper‚ finalement dans la table regularisations (= "Autres op‚rations")  
          INSERT INTO  
            #TempRegularisations  
           SELECT  
           DAT_MVT_BUDGETAIRE AS DAT_REGUL,  
           LIBL_MVT_BUDGETAIRE AS LIB_REGULARISATION,  
           ISNULL(MNT_MVT_GROUPE,0) AS MNT_REGUL,  
           ID_TYPE_FINANCEMENT  
           FROM  
            #TMP_MVT_BUD_ENG_ET_REGLE  
           WHERE  
            TAG is null  
            and P_E_R = 'R'  
           
           
           
          ---------------------------- FIN DE 4. REGULARISATION (VIREMENTS, RESTITUTIONS,..) ---------------------------------------------------------    
          ------------------ Total Mobilisable plan (0+1+2+3+4)    
           
          UPDATE  
           #TempTableForEdition  
          SET  
           TOTAL_MOBILISABLE =  
            CAST(ISNULL(MNT_INITIAL_E,0)AS DECIMAL(18,2))  
            + CAST(ISNULL((SELECT SUM(MNT_MOBILISABLE_HT) FROM #TempVersements WHERE #TempVersements.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT GROUP BY ID_TYPE_FINANCEMENT),0)AS DECIMAL(18,2))  
            + CAST(ISNULL(MNT_RELIQUATS,0)AS DECIMAL(18,2))  
            + CAST(ISNULL(TOTAL_FONDS_RECREDITES,0)AS DECIMAL(18,2))  
            + CAST(ISNULL((SELECT  SUM(MNT_REGUL) FROM #TempRegularisations WHERE #TempRegularisations.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT GROUP BY ID_TYPE_FINANCEMENT),0)AS DECIMAL(18,2))  
           
          UPDATE  
           #TempTableForEdition  
          SET  
           MNT_SOLD_DISPO = ISNULL(TOTAL_MOBILISABLE,0) - ISNULL(MNT_ENGAGE_ET_REGLE,0) - ISNULL(MNT_ENGAGE_ET_NON_REGLE, 0) - ISNULL(MNT_ENGAGE_COMPL, 0)  
            
          SET  
           @EndDate   = dbo.GetMinDate(GETDATE(), @EndDate)  
          SET  
           @StartDate = dbo.GetMinDate(@StartDate, @EndDate)  
            
          DECLARE  
           @ANNEE_DEBUT INT,  
           @ANNEE_FIN INT  
          SET  
           @ANNEE_DEBUT = YEAR([dbo].[GetShortDate](@StartDate))-1  
          SET  
           @ANNEE_FIN = YEAR([dbo].[GetShortDate](@EndDate)) -1;  
           
          ----------------------------  FIN DE 6. COMPTE PREVISIONNEL ----------------------------------------------    
          --------------------------     Generation de fichiers XML ------------------------------    
          SELECT  
          (  
           SELECT  
            LIBL_TYPE_FINANCEMENT AS LIBL_TYPE_FINANCEMENT,  
            (  
             SELECT  
              UPPER(LIBL_TYPE_FINANCEMENT) AS LIBL_TYPE_FINANCEMENT,  
              dbo.[GetShortDate](@StartDate) AS DATE_DEBUT,  
              dbo.[GetShortDate](@EndDate)  AS DATE_FIN,  
              @LIB_GROUPE AS LIB_GROUPE,  
              dbo.[GetShortDate](@StartDate) AS DAT_SOLD_DISPONIBLE,  
              dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT_INITIAL_E,0.00) AS DECIMAL(18,2)))  AS MNT_SOLD_DISPONIBLE  
             FOR XML RAW ('ENTETE'),ELEMENTS, TYPE  
            ),  
            -- Part 1  
            (  
             SELECT  
              (  
               SELECT  
                LIB_VERSEMENT,  
                ISNULL(CAST(ANNEE_IMPUT AS VARCHAR(4)), '-') AS ANNEE_IMPUT,  
                ISNULL(dbo.GetShortDate(DAT_SAISIE_VERSEMENT), '-') AS DAT_VERS,  
                CASE MNT_VERSE_HT  
                 WHEN  0.00  
                  THEN NULL  
                 ELSE  dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT_VERSE_HT,0.00) AS DECIMAL(18,2)))  
                END AS MNT_VERSE_HT,  
                CASE MNT_MOBILISABLE_HT  
                 WHEN  0.00  
                  THEN NULL  
                 ELSE  dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT_MOBILISABLE_HT,0.00) AS DECIMAL(18,2)))  
                END AS MNT_MOBILISABLE_HT  
               FROM  
                #TempVersements  
               WHERE  
                #TempVersements.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
               ORDER BY  
                COALESCE(DAT_SAISIE_VERSEMENT, DAT_PREV),  
                LIB_VERSEMENT  
               FOR XML RAW('LIST_VERSEMENT'), ELEMENTS, TYPE  
              )  
             FOR XML RAW('VERSEMENT'), ELEMENTS, TYPE  
            ),  
            -- Part 1 - Total  
            (  
             SELECT  
              dbo.GetFrenchCurrencyFormat(CAST(COALESCE(SUM(MNT_VERSE_HT),0.00) AS DECIMAL(18,2)))  AS TOTAL_MNT_VERSE_HT,  
              dbo.GetFrenchCurrencyFormat(CAST(COALESCE(SUM(MNT_MOBILISABLE_HT),0.00) AS DECIMAL(18,2))) AS TOTAL_MNT_MOBILISABLE_HT  
             FROM  
              #TempVersements  
             WHERE  
              #TempVersements.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
             FOR XML RAW('TOTAL_LIST_VERSEMENT'), ELEMENTS, TYPE  
            ),  
            -- Part 2  
            (  
             SELECT  
              NUM_ANNEE,  
              CASE  
               WHEN  NUM_ANNEE >= 2012  
                THEN '30 juin '  
               ELSE  '30 sept. '  
              END  
              + CAST(NUM_ANNEE + 1 as VARCHAR(4))  AS RELIQUAT_LIB_MOIS,  
              dbo.GetFrenchCurrencyFormat(cast(SUM(MNT_MVT_GROUPE) as DECIMAL(18,2))) as MNT_RELIQUAT  
             FROM  
              #TMP_MVT_BUD_ENG_ET_REGLE  
             WHERE  
              -- MBL 02/06/2015 :  Correction calcul du reliquat 
              -- On doit se bas‚ sur les ‚l‚ments tagg‚s : pourquoi repartir sur d'autres regles de gestion
              -- Ne jamais se bas‚ sur la valeur d'un libell‚ qui peut ‚voluer a tout moment
              -- LIBL_MVT_BUDGETAIRE like 'VO Reliquat normal%'  
              #TMP_MVT_BUD_ENG_ET_REGLE.TAG = '2-RELIQ'
              AND #TMP_MVT_BUD_ENG_ET_REGLE.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
             GROUP BY  
              NUM_ANNEE  
             ORDER BY  
              NUM_ANNEE  
             FOR XML RAW('RELIQUATS'), ELEMENTS, TYPE  
            ),  
            -- Part 3  
            (  
             SELECT  
              dbo.GetFrenchCurrencyFormat(CAST(COALESCE(TOTAL_FONDS_RECREDITES,0.00) AS DECIMAL(18,2)))  AS TOTAL_FONDS_RECREDITES,  
              YEAR([dbo].[GetShortDate](@StartDate)) AS ANNEE_FONDS_RECREDITES_SUR,  
              dbo.[GetShortDate](@StartDate) AS DAT_FONDS_RECREDITES_SUR,  
              dbo.GetFrenchCurrencyFormat(CAST(COALESCE(TOTAL_FONDS_RECREDITES_NEW,0.00) AS DECIMAL(18,2)))  AS MNT_FONDS_RECREDITES_SUR,  
              dbo.GetFrenchCurrencyFormat(CAST(COALESCE(TOTAL_FONDS_RECREDITES_OLD,0.00) AS DECIMAL(18,2)))  AS MNT_FONDS_RECREDITES_ANT  
             FOR XML RAW('Recredit'), ELEMENTS, TYPE  
            ),  
            -- Part 4  
            (  
             SELECT  
              (  
               SELECT  
                ISNULL(dbo.GetShortDate(DAT_REGUL), '') AS DAT_REGUL,  
                LIB_REGULARISATION AS LIB_REGULARISATION,  
                dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT_REGUL,0.00) AS DECIMAL(18,2))) AS MNT_REGUL  
               FROM  
                #TempRegularisations  
               WHERE  
                #TempRegularisations.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
               FOR XML RAW('LIST_REGULARISATION'), ELEMENTS, TYPE  
              )  
             FOR XML RAW('REGULARISATION'), ELEMENTS, TYPE  
            ),  
            -- Ligne Total Mobilisable plan ( part 0 + part 1 + part2 + part3 + part4)  
            dbo.GetFrenchCurrencyFormat(CAST(COALESCE(TOTAL_MOBILISABLE ,0.00) AS DECIMAL(18,2))) AS MNT_MOBILISABLE,  
            -- Part 5     -- ENGAGEMENT  
            (  
             SELECT  
              -- Part 5.1  
              dbo.GetFrenchCurrencyFormat(CAST(COALESCE(-ABS(MNT_ENGAGE_ET_REGLE), 0.00) AS DECIMAL(18,2))) AS MNT_ENGAGE_ET_REGLE,  
              (  
              SELECT  
               (  
                SELECT  
                 CAST(NUM_ANNEE AS VARCHAR(4)) as NUM_ANNEE,  
                 dbo.GetFrenchCurrencyFormat(CAST(COALESCE(-ABS(MNT_REGLE),0.00)AS DECIMAL(18,2))) AS MNT_REGLE  
                FROM  
                 #FOR_ENGAGE_ET_REGLE  
                WHERE  
                 #FOR_ENGAGE_ET_REGLE.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
                ORDER BY  
                 NUM_ANNEE  
                FOR XML RAW('FORM_ENG_ET_REG'), ELEMENTS, TYPE  
               )  
               FOR XML RAW('ENG_ET_REG'), ELEMENTS, TYPE  
              ),  
              -- Part 5.2  
              dbo.GetFrenchCurrencyFormat(CAST(COALESCE(-ABS(MNT_ENGAGE_ET_NON_REGLE), 0.00) AS DECIMAL(18,2))) AS MNT_ENGAGE_ET_NON_REGLE,  
              (  
              SELECT  
               (  
                SELECT  
                 CAST(NUM_ANNEE AS VARCHAR(4)) as NUM_ANNEE,  
                 dbo.GetFrenchCurrencyFormat(CAST(COALESCE(-ABS(MNT_REGLE),0.00)AS DECIMAL(18,2))) AS MNT_REGLE  
                FROM  
                 #FOR_ENGAGE_ET_NON_REGLE  
                WHERE  
                 #FOR_ENGAGE_ET_NON_REGLE.ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
                ORDER BY  
                 NUM_ANNEE  
                FOR XML RAW('FORM_ENG_ET_NON_REG'), ELEMENTS, TYPE  
               )   
               FOR XML RAW('ENG_ET_NON_REG'), ELEMENTS, TYPE  
              ),  
              -- Part 5.3  
              dbo.GetFrenchCurrencyFormat(CAST(COALESCE(-ABS(MNT_ENGAGE_COMPL) ,0.00) AS DECIMAL(18,2))) AS MNT_ENGAGE_COMPL       
             FOR XML RAW('ENG'), ELEMENTS, TYPE  
            ),  
            --  Part 5 TOTAL  -- modif SBR du 06/06/11, split des coalesce pour calcul correcte  
            dbo.GetFrenchCurrencyFormat(CAST(  
            + COALESCE(-MNT_ENGAGE_ET_REGLE, 0.00)  
            + COALESCE(-MNT_ENGAGE_ET_NON_REGLE, 0.00)  
            + COALESCE(-MNT_ENGAGE_COMPL, 0.00)  
            AS DECIMAL(18,2))) AS MNT_FINANCEMENT,  
            -- Ligne Total de Sold Disponible Plan    
            dbo.GetShortDate(@EndDate) AS DAT_SOLD_DISPO,  
            dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT_SOLD_DISPO,0.00) AS DECIMAL(18,2)))  AS MNT_SOLD_DISPO,  
            dbo.GetShortDate(@EndDate) AS DATE_SOLD_REEL,  
            dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT_FINAL_R,0.00) AS DECIMAL(18,2))) AS MNT_SOLD_REEL,  
            -- Part 7 - CONTROLE  
            dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT_INITIAL_E,0.00) AS DECIMAL(18,2)))  AS MNT_SOLD_DISPO_THEORIQUE  
           FROM  
            #TempTableForEdition  
           FOR XML RAW('TypFin'), ELEMENTS,TYPE  
          ),   
          (  
           SELECT      
           (  
            SELECT  
             dbo.[GetShortDate](@StartDate) AS DATE_DEBUT,  
             dbo.[GetShortDate](@EndDate)  AS DATE_FIN,  
             @LIB_GROUPE AS LIB_GROUPE        
            FOR XML RAW ('ENTETE'),ELEMENTS, TYPE  
           ),  
           (  
            SELECT  
             UPPER(LIBL_TYPE_FINANCEMENT) AS LIBL_TYPE_FINANCEMENT,  
             (  
              SELECT  
               dbo.GetShortDate(DAT_REGLEMENT) AS DATE,  
               COD_MODULE_PEC AS COD_MODULE_PEC,  
               COD_SOUS_TYPE_COUT AS COD_SOUS_TYPE_COUT,  
               dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT_INITIAL_E,0.00) AS DECIMAL(18,2))) AS MNT_INITIAL_E,  
               dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT_REGLE,0.00) AS DECIMAL(18,2))) AS MNT_REGLE,  
               dbo.GetFrenchCurrencyFormat(CAST(COALESCE(MNT,0.00) AS DECIMAL(18,2))) AS MNT  
              FROM  
               #TempTableForEditionCompl  
              WHERE  
               ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
              ORDER BY  
               DAT_REGLEMENT  
              FOR XML RAW('LIST_VERSEMENT'), ELEMENTS, TYPE  
             ),  
             (  
              SELECT  
               dbo.GetFrenchCurrencyFormat(CAST(COALESCE(SUM(MNT_INITIAL_E),0.00) AS DECIMAL(18,2))) AS TOTAL_MNT_INITIAL_E,  
               dbo.GetFrenchCurrencyFormat(CAST(COALESCE(SUM(MNT_REGLE),0.00) AS DECIMAL(18,2))) AS TOTAL_MNT_REGLE,  
               dbo.GetFrenchCurrencyFormat(CAST(COALESCE(SUM(MNT),0.00) AS DECIMAL(18,2))) AS TOTAL_MNT  
              FROM  
               #TempTableForEditionCompl  
              WHERE  
               ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
              FOR XML RAW('TOTAL_LIST_VERSEMENT'), ELEMENTS, TYPE  
             )  
            FOR XML RAW('LIST_TYPE_FINANCEMENT'), ELEMENTS, TYPE  
           ),  
           (  
            SELECT    
            (    
             SELECT distinct  
              COD_SOUS_TYPE_COUT AS COD_SOUS_TYPE_COUT,  
              LIBL_SOUS_TYPE_COUT AS LIBL_SOUS_TYPE_COUT  
             FROM  
              #TempTableForEditionCompl  
             WHERE  
              ID_TYPE_FINANCEMENT = #TempTableForEdition.ID_TYPE_FINANCEMENT  
             FOR XML RAW('SOUS_TYPE_COUT'), ELEMENTS, TYPE  
            )     
            FOR XML RAW('LIST_SOUS_TYPE_COUT'), ELEMENTS, TYPE  
           )     
           FROM  
            #TempTableForEdition  
           WHERE  
            ID_TYPE_FINANCEMENT   
            IN (SELECT DISTINCT   
              #TempTableForEditionCompl.ID_TYPE_FINANCEMENT   
             FROM   
              #TempTableForEditionCompl   
             WHERE   
              #TempTableForEditionCompl.MNT <> 0)    
           FOR XML RAW('ENG_COMPL'), ELEMENTS, TYPE    
          )  
          FOR XML RAW('SYNTH_CPT_ADH'), ELEMENTS,TYPE  
         END


         -- =============================================
         -- Author:		HBT
         -- Create date: 10/02/2012
         -- Description:	Sous types de cout pour edition pour  FICHE_DOSSIER 
         -- =============================================
         -- Author:		EOU
         -- Modiffed date: 05/03/2012
         -- Description:	Retour du motif de non conformit‚ 13197
         -- =============================================
         
         CREATE PROCEDURE LEC_GRP_PIECES_MODULE_PEC_FICHEDOSSIER
         	@ID_MODULE int
         AS
         
         BEGIN
         	-- Recherche de l'action si le module est renseigne
             DECLARE @ID_ACTION int
         	IF @ID_MODULE IS NOT NULL
         	BEGIN
         		SET @ID_ACTION = (SELECT ID_ACTION_PEC FROM MODULE_PEC WHERE ID_MODULE_PEC = @ID_MODULE)
         	END;
         	
         		
         	DECLARE @TMP_PIECES_STAGIAIRES TABLE
         	(
         	    LIBL_MOTIF_NON_CONFORM_PIECE VARCHAR(50),--13197 EOU
         		LIBL_PIECE_PEC VARCHAR(50),
         		ID_ADHERENT INT NULL,
         		ID_STAGIAIRE_PEC INT NULL,
         		BLN_ADHERENT TINYINT,
         		BLN_STAGIAIRE TINYINT,
         		ID_ARRIVEE_PIECE_PEC INT NULL,
         		TYPE_PIECE VARCHAR(50)
         	)
         	
         	INSERT INTO @TMP_PIECES_STAGIAIRES
         		SELECT --toutes les pieces de tous les stagiaires du module dispositif NULL
         		    MOTIF_NON_CONFORM_PIECE.LIBL_MOTIF_NON_CONFORM_PIECE,
         			LIBL_PIECE_PEC,
         			NULL AS ID_ADHERENT,
         			STAGIAIRE_PEC.ID_STAGIAIRE_PEC,
         			BLN_ADHERENT,
         			BLN_STAGIAIRE,
         			ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC,
         			'Stagiaire' AS TYPE_PIECE
         		FROM PIECE_PEC
         			INNER JOIN STAGIAIRE_PEC 
         				ON (STAGIAIRE_PEC.ID_MODULE_PEC = @ID_MODULE AND 
         										 STAGIAIRE_PEC.ID_SESSION_PEC IS NULL)
         			LEFT JOIN ARRIVEE_PIECE_PEC ON (ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC   AND
         											ARRIVEE_PIECE_PEC.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC)
         			LEFT JOIN NR210 on NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC --13197 EOU
         			LEFT JOIN MOTIF_NON_CONFORM_PIECE on MOTIF_NON_CONFORM_PIECE.ID_MOTIF_NON_CONFORM_PIECE = NR210.ID_MOTIF_NON_CONFORM_PIECE
         		WHERE 	
         			BLN_POSTE_COUT_REGLE = 0 AND
         			BLN_ADHERENT = 0 AND
         			BLN_SESSION = 0  AND
         			BLN_STAGIAIRE = 1 AND
         			PIECE_PEC.BLN_ACTIF = 1 AND
         			PIECE_PEC.ID_DISPOSITIF IS NULL 
         		
         		UNION
         		
         		SELECT --toutes les pieces de tous les stagiaires du module dispositif NOT NULL
         		    MOTIF_NON_CONFORM_PIECE.LIBL_MOTIF_NON_CONFORM_PIECE,
         			LIBL_PIECE_PEC,
         			NULL AS ID_ADHERENT,
         			STAGIAIRE_PEC.ID_STAGIAIRE_PEC,
         			BLN_ADHERENT,
         			BLN_STAGIAIRE,
         			ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC,
         			'Stagiaire' AS TYPE_PIECE
         		FROM PIECE_PEC
         			INNER JOIN STAGIAIRE_PEC 
         				ON (STAGIAIRE_PEC.ID_MODULE_PEC = @ID_MODULE AND 
         										 STAGIAIRE_PEC.ID_SESSION_PEC IS NULL)
         			INNER JOIN UNITE_STAGIAIRE ON (UNITE_STAGIAIRE.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC AND
         											PIECE_PEC.ID_DISPOSITIF = UNITE_STAGIAIRE.ID_DISPOSITIF AND
         											UNITE_STAGIAIRE.NB_HEURE_ENGAGE > 0)
         			LEFT JOIN ARRIVEE_PIECE_PEC ON (ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC AND
         											ARRIVEE_PIECE_PEC.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC
         											)
         		LEFT JOIN NR210 on NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC--13197 EOU
         		LEFT JOIN MOTIF_NON_CONFORM_PIECE on MOTIF_NON_CONFORM_PIECE.ID_MOTIF_NON_CONFORM_PIECE = NR210.ID_MOTIF_NON_CONFORM_PIECE
         		WHERE 	
         			BLN_POSTE_COUT_REGLE = 0 AND
         			BLN_ADHERENT = 0 AND
         			BLN_SESSION = 0 AND 
         			BLN_STAGIAIRE = 1 AND
         			PIECE_PEC.BLN_ACTIF = 1 AND
         			PIECE_PEC.ID_DISPOSITIF IS NOT NULL 
         	
         	DECLARE @FIRSTSTAGIAIREID INT
         	SET @FIRSTSTAGIAIREID = 
         	 (SELECT TOP 1 TMP_PIECES_STAGIAIRES.ID_STAGIAIRE_PEC
         	  FROM @TMP_PIECES_STAGIAIRES AS TMP_PIECES_STAGIAIRES
         	  LEFT JOIN STAGIAIRE_PEC ON (STAGIAIRE_PEC.ID_STAGIAIRE_PEC = TMP_PIECES_STAGIAIRES.ID_STAGIAIRE_PEC)
         	  LEFT JOIN INDIVIDU ON (STAGIAIRE_PEC.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU)
         	  ORDER BY INDIVIDU.NOM_INDIVIDU, INDIVIDU.PRENOM_INDIVIDU);
         		
         	WITH tmp_all_pieces AS
         	(
         		SELECT --toutes les pieces de tous les adherents de l'action
         			MOTIF_NON_CONFORM_PIECE.LIBL_MOTIF_NON_CONFORM_PIECE,
         			LIBL_PIECE_PEC,
         			ADHERENT_ACTION.ID_ADHERENT,
         			NULL AS ID_STAGIAIRE_PEC,
         			BLN_ADHERENT,
         			BLN_STAGIAIRE,
         			ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC,
         			'Adh‚rent' AS TYPE_PIECE
         		FROM PIECE_PEC
         			INNER JOIN 
         				(SELECT distinct ID_ADHERENT FROM etablissement
         					INNER JOIN NR140 ON (NR140.ID_ACTION_PEC = @ID_ACTION AND
         										 NR140.ID_ETABLISSEMENT = etablissement.ID_ETABLISSEMENT)
         				) AS adherent_action
         				ON (1=1)
         			LEFT JOIN ARRIVEE_PIECE_PEC ON (ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC AND
         											ARRIVEE_PIECE_PEC.ID_ADHERENT = adherent_action.ID_ADHERENT AND
         											ARRIVEE_PIECE_PEC.ID_MODULE_PEC = @ID_MODULE
         											)
         		    LEFT JOIN NR210 on NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC--13197 EOU
         			LEFT JOIN MOTIF_NON_CONFORM_PIECE on MOTIF_NON_CONFORM_PIECE.ID_MOTIF_NON_CONFORM_PIECE = NR210.ID_MOTIF_NON_CONFORM_PIECE
         		WHERE 	
         			PIECE_PEC.BLN_POSTE_COUT_REGLE = 0  AND
         			PIECE_PEC.BLN_ADHERENT = 1 	 		AND
         			PIECE_PEC.BLN_SESSION = 0 			AND
         			PIECE_PEC.BLN_ACTIF = 1
         		
         		UNION
         		
         		SELECT --tous les pieces du module
         		    MOTIF_NON_CONFORM_PIECE.LIBL_MOTIF_NON_CONFORM_PIECE,
         			LIBL_PIECE_PEC,
         			NULL AS ID_ADHERENT,
         			NULL AS ID_STAGIAIRE_PEC,
         			BLN_ADHERENT,
         			BLN_STAGIAIRE,
         			ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC,
         			'Module' AS TYPE_PIECE
         		FROM PIECE_PEC
         			LEFT JOIN ARRIVEE_PIECE_PEC ON (ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC   AND
         											ARRIVEE_PIECE_PEC.ID_MODULE_PEC = @ID_MODULE)
         			LEFT JOIN NR210 on NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC--13197 EOU
         			LEFT JOIN MOTIF_NON_CONFORM_PIECE on MOTIF_NON_CONFORM_PIECE.ID_MOTIF_NON_CONFORM_PIECE = NR210.ID_MOTIF_NON_CONFORM_PIECE
         		WHERE 	
         			BLN_POSTE_COUT_REGLE = 0 AND
         			BLN_ADHERENT = 0 AND
         			BLN_SESSION = 0 AND
         			BLN_STAGIAIRE = 0 AND
         			BLN_MODULE = 1 AND
         			PIECE_PEC.BLN_ACTIF = 1
         			
         		UNION
         		
         		SELECT 
         			TMP_PIECES_STAGIAIRES.LIBL_MOTIF_NON_CONFORM_PIECE,
         			TMP_PIECES_STAGIAIRES.LIBL_PIECE_PEC,
         			TMP_PIECES_STAGIAIRES.ID_ADHERENT,
         			TMP_PIECES_STAGIAIRES.ID_STAGIAIRE_PEC,
         			TMP_PIECES_STAGIAIRES.BLN_ADHERENT,
         			TMP_PIECES_STAGIAIRES.BLN_STAGIAIRE,
         			TMP_PIECES_STAGIAIRES.ID_ARRIVEE_PIECE_PEC,
         			TMP_PIECES_STAGIAIRES.TYPE_PIECE
         		FROM @TMP_PIECES_STAGIAIRES AS TMP_PIECES_STAGIAIRES
         		LEFT JOIN ARRIVEE_PIECE_PEC ON (ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC = TMP_PIECES_STAGIAIRES.ID_ARRIVEE_PIECE_PEC)
         		
         		WHERE TMP_PIECES_STAGIAIRES.ID_STAGIAIRE_PEC = @FIRSTSTAGIAIREID
         		OR ARRIVEE_PIECE_PEC.BLN_ACTIF = 0
         		OR ARRIVEE_PIECE_PEC.BLN_CONFORME = 0
         	)
         
         	SELECT 
         		tmp_all_pieces.LIBL_MOTIF_NON_CONFORM_PIECE as MOTIF_NON_CONFORM_PIECE,--13197 EOU
         		tmp_all_pieces.LIBL_PIECE_PEC AS LIBELLE_PIECE_PEC,
         		CASE 
         			WHEN tmp_all_pieces.BLN_STAGIAIRE = 1 THEN INDIVIDU.NOM_INDIVIDU +' '+LEFT(INDIVIDU.PRENOM_INDIVIDU, 1)
         			WHEN tmp_all_pieces.BLN_ADHERENT = 1 THEN ADHERENT.LIB_RAISON_SOCIALE
         			ELSE NULL
         		END AS INFORMATION,
         		CASE
         			WHEN
         				ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC IS NULL 
         			THEN 1 
         			ELSE ARRIVEE_PIECE_PEC.BLN_ACTIF
         		END AS PRESENT,
         		CASE
         			WHEN 
         				ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC IS NULL
         			THEN 1
         			ELSE ARRIVEE_PIECE_PEC.BLN_CONFORME
         		END AS CONFORME,
         		CASE
         			WHEN
         				ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NOT NULL 
         				THEN 'Relance 3: ' + CONVERT(varchar(10), ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3, 3)
         			WHEN
         				ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NOT NULL 
         				THEN 'Relance 2: ' + CONVERT(varchar(10), ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2, 3)
         			WHEN
         				ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 IS NOT NULL 
         				THEN 'Relance 1: ' + CONVERT(varchar(10), ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1, 3)
         			ELSE NULL
         		END AS DATE_RELANCE,
         		CASE
         			WHEN
         				ARRIVEE_PIECE_PEC.DAT_RELANCE_REGLE_3 IS NOT NULL 
         				THEN 'Relance 3: ' + CONVERT(varchar(10), ARRIVEE_PIECE_PEC.DAT_RELANCE_REGLE_3, 3)
         			WHEN
         				ARRIVEE_PIECE_PEC.DAT_RELANCE_REGLE_2 IS NOT NULL 
         				THEN 'Relance 2: ' + CONVERT(varchar(10), ARRIVEE_PIECE_PEC.DAT_RELANCE_REGLE_2, 3)
         			WHEN
         				ARRIVEE_PIECE_PEC.DAT_RELANCE_REGLE_1 IS NOT NULL 
         				THEN 'Relance 1: ' + CONVERT(varchar(10), ARRIVEE_PIECE_PEC.DAT_RELANCE_REGLE_1, 3)
         			ELSE NULL
         		END AS DATE_NIVEAU_REGLE
         	FROM tmp_all_pieces
         	LEFT JOIN ARRIVEE_PIECE_PEC ON (ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC = tmp_all_pieces.ID_ARRIVEE_PIECE_PEC)
         	LEFT JOIN STAGIAIRE_PEC ON (STAGIAIRE_PEC.ID_STAGIAIRE_PEC = tmp_all_pieces.ID_STAGIAIRE_PEC)
         	LEFT JOIN INDIVIDU ON (STAGIAIRE_PEC.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU )
         	LEFT JOIN ADHERENT ON (ADHERENT.ID_ADHERENT = tmp_all_pieces.ID_ADHERENT)
         
         	order by tmp_all_pieces.TYPE_PIECE, INFORMATION
         		
         END


         --==================================================================
         -- Author	: woollams
         -- Date		: 20 fevrier 2008
         -- comment	: Ajout des bln_actif pour action pec
         --			  et module_pece
         --==================================================================
         -- Author	: MB
         -- Date		: 04/04/2008
         -- comment	: Ajout du parametre @NBR et de la possibilite de compter les occurences ramenees
         --==================================================================
         -- Author	: SBR
         -- Date		: 02/06/08
         -- comment	: Gestion particuliŠre pour les actions collectives: on filtre sur le CM renseign‚ au niveau de l'action
         --			  La proc‚dure renvoi la concat‚nation de COD_ACTION_PEC et ANNEE_ACTION_PEC (via la fonction GetActionPECCode) 
         --			  dans le champ _ACTION_PEC au lieu de ID_ACTION_PEC			 
         --==================================================================
         -- Author	: AMA
         -- Date		: 09/06/08
         -- comment	: Correction bug li‚ … la modification pr‚c‚dente
         --			  Le type de retour de #TEMP_DESTINATAIRES._ACTION_PEC passe de int … VARCHAR(11)
         --==================================================================
         -- Author	: AMA
         -- Date		: 15/09/2008
         -- comment	: Le destinataire potentiel 
         --=============================================================
         -- 26/12/08 par SBRU - l'‚tablissement qui fait la formation est renvoy‚ au lieu de l'‚tablissement principal
         --=============================================================
         -- 26/12/08 par BBL - Modification plan execution
         --=============================================================
         -- LDE 31/10/2012 13784: EVOL - Evoluer la s‚lection des destinataires de relance instruction pec - 2.5
         -- =============================================================
         
         CREATE PROCEDURE [dbo].[LEC_GRP_ETABLISSEMENT_RELANCE_ENGAGEMENT_ADH]
         	@ID_USER	INT,
         	@NBR		INT = NULL OUTPUT
         WITH RECOMPILE
         AS
         
         BEGIN
         
         	/** Table contenant la liste des destinataires potentiels ****************************************/
         	CREATE TABLE #TEMP_DESTINATAIRES (
         				ID_ETABLISSEMENT int,
         				ID_ADHERENT int,
         				COD_ADHERENT int,
         				_ACTION_PEC varchar(11)
         	)
         
         	/*** Table temporaire contenant des modules ayant deja un poste cout regl‚ ***********************/
         	SELECT	DISTINCT ID_ACTION_PEC
         	INTO	#TEMP_POSTE_COUT
         	FROM	MODULE_PEC
         	JOIN	POSTE_COUT_REGLE
         	ON		MODULE_PEC.ID_MODULE_PEC = POSTE_COUT_REGLE.ID_MODULE_PEC
         	WHERE MODULE_PEC.BLN_ACTIF = 1 AND MODULE_PEC.BLN_OK_PIECE = 0																				-- LDE 31/10/2012 13784
         
         
         	/* MAJ MB du 09/10/2007
         	-- On ne filtre plus sur les pieces associees a l'adherent
         	-- Toutes les pieces bloquant l'engagement sont relancees y compris celles non associees a l'adherent.
         	*/
         	/* MAJ MB du 16/10/2007
         	-- Correction bug sur la requete de selection des potentiels
         	*/
         
         	/*** S‚lection des destinataires potentiels li‚es … l'adherent*/
         	INSERT	INTO #TEMP_DESTINATAIRES
         	SELECT	DISTINCT 
         			ETABLISSEMENT.ID_ETABLISSEMENT,
         			ADHERENT.ID_ADHERENT,
         			ADHERENT.COD_ADHERENT, 
         			dbo.GetActionPECCode(ACTION_PEC.COD_ACTION_PEC, ACTION_PEC.ANNEE_ACTION_PEC)  as _ACTION_PEC
         
         	FROM    ARRIVEE_PIECE_PEC
         	JOIN	PIECE_PEC 
         	ON		ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC 
         	JOIN	MODULE_PEC 
         	ON		ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC 
         	JOIN	ACTION_PEC 
         	ON		MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         	JOIN	NR140 
         	ON		NR140.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         	JOIN	ETABLISSEMENT 
         	ON		NR140.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT
         	JOIN	ADHERENT 
         	ON		ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         	CROSS JOIN PARAMETRES
         
         	WHERE	(((ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 IS NULL) 
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NULL) 
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL))
         
         			OR ((ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 IS NOT NULL)
         				AND (GETDATE()-ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 >= PARAMETRES.DELAI_RELANCE_PEC)
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NULL) 
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL))
         
         			OR ((ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NOT NULL)
         				AND (GETDATE()-ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 >= PARAMETRES.DELAI_RELANCE_PEC)  
         				AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL)))
         
         			AND (ACTION_PEC.ID_ACTION_PEC not in (SELECT ID_ACTION_PEC FROM #TEMP_POSTE_COUT))
         			AND (MODULE_PEC.BLN_OK_PIECE = 0)
         			AND (PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1)
         			AND	(ACTION_PEC.BLN_ACTIF = 1) 
         			AND (MODULE_PEC.BLN_ACTIF = 1)
         			AND (PIECE_PEC.BLN_ACTIF=1)
         			AND ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 1) AND (EXISTS (select 1 from NR210 WHERE NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC))))
         			AND (ACTION_PEC.ID_UTILISATEUR = @ID_USER OR MODULE_PEC.ID_UTILISATEUR = @ID_USER OR ETABLISSEMENT.ID_CHARGEE_RELATION = @ID_USER)	-- LDE 31/10/2012 13784
         
         		/*** S‚lection des destinataires potentiels li‚es au stagiaire*/
         		INSERT	INTO #TEMP_DESTINATAIRES
         		SELECT  DISTINCT ETABLISSEMENT.ID_ETABLISSEMENT,
         				   		 ADHERENT.ID_ADHERENT,
         						 ADHERENT.COD_ADHERENT,
         						 dbo.GetActionPECCode(ACTION_PEC.COD_ACTION_PEC, ACTION_PEC.ANNEE_ACTION_PEC)  as _ACTION_PEC
         
         		FROM		ARRIVEE_PIECE_PEC
         		JOIN		PIECE_PEC ON ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC  
         		JOIN		STAGIAIRE_PEC ON ARRIVEE_PIECE_PEC.ID_STAGIAIRE_PEC = STAGIAIRE_PEC.ID_STAGIAIRE_PEC 
         		JOIN		ETABLISSEMENT ON STAGIAIRE_PEC.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT
         		JOIN		MODULE_PEC ON ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC 
         		JOIN		ACTION_PEC ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         		JOIN		ADHERENT ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         		CROSS JOIN	PARAMETRES
         
         		WHERE	(((ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 IS NULL) 
         						AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NULL) 
         						AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL))
         
         					OR ((ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 IS NOT NULL)
         						AND (GETDATE()-ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 >= PARAMETRES.DELAI_RELANCE_PEC)
         						AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NULL) 
         						AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL))
         
         					OR ((ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 IS NOT NULL)
         						AND (GETDATE()-ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 >= PARAMETRES.DELAI_RELANCE_PEC)  
         						AND (ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 IS NULL)))
         
         					AND (ACTION_PEC.ID_ACTION_PEC not in (SELECT ID_ACTION_PEC FROM #TEMP_POSTE_COUT))
         					AND (MODULE_PEC.BLN_OK_PIECE = 0)
         					AND (PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1)
         					AND	(ACTION_PEC.BLN_ACTIF = 1) 
         					AND (MODULE_PEC.BLN_ACTIF = 1)
         					AND (PIECE_PEC.BLN_ACTIF=1)
         					AND (PIECE_PEC.BLN_STAGIAIRE =1)
         					AND ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 0) OR ((ARRIVEE_PIECE_PEC.BLN_ACTIF = 1) AND ( EXISTS (select 1 from NR210 WHERE NR210.ID_ARRIVEE_PIECE_PEC = ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC))))
         					AND (ACTION_PEC.ID_UTILISATEUR = @ID_USER OR MODULE_PEC.ID_UTILISATEUR = @ID_USER OR ETABLISSEMENT.ID_CHARGEE_RELATION = @ID_USER)	-- LDE 31/10/2012 13784
         
         
         	IF @NBR IS NULL
         	BEGIN
         			SELECT     DISTINCT ID_ETABLISSEMENT,
         								ID_ADHERENT,
         								COD_ADHERENT,
         								_ACTION_PEC
         			FROM	#TEMP_DESTINATAIRES
         			ORDER BY COD_ADHERENT,_ACTION_PEC
         	END
         	ELSE
         	BEGIN
         			SELECT     DISTINCT ID_ETABLISSEMENT,
         								ID_ADHERENT,
         								COD_ADHERENT,
         								_ACTION_PEC
         			INTO #TMP1
         			FROM	#TEMP_DESTINATAIRES
         			ORDER BY COD_ADHERENT,_ACTION_PEC
         
         		SELECT @NBR = COUNT( *) 
         		FROM	#TMP1 , NR67bis
         		WHERE	NR67bis.ID_ETABLISSEMENT = #TMP1.ID_ETABLISSEMENT
         		AND		NR67bis.ID_DOCUMENT = 27
         	END
         END
         
 CREATE PROCEDURE [dbo].[EDT_LETTRE_REJET_ADH_ACTION]
          @ID_ETABLISSEMENT INT,
          @ID_BENEFICIAIRE INT,
          @TYPE_BENEFICIAIRE INT,
          @ID_ADRESSE INT,
          @ID_CONTACT INT,
          @COD_MODULE_PEC VARCHAR(14)
         AS
         -- =======================================================================================================================================
         -- Author : SBR
         -- Date  : 27/05/2008
         -- comment : Le paramŠtre @COD_MODULE_PEC passe de varchar(10) … varchar(14)
         --     La partie Emetteur est renseign‚e … partir de la fonction GetXmlAdrChargeRelation (au lieu de GetXmlAgenceContact)
         -- =======================================================================================================================================
         -- 06/03/09 par SBR - adaptation gabarit suite groupe de travail sur les ‚ditions
         -- =======================================================================================================================================
         -- 19/09/12 par DSZ - 13697 : Rendre dynamique le nø de t‚l‚phone 
         -- =======================================================================================================================================
         -- 05/06/2013 par EOU - 14966
         -- =======================================================================================================================================
         -- LDE/OPA 23/05/2014 : #213 En tant qu'utilisateur OPTIFORM, 
         -- lorsque j'‚dite un courrier (PEC, PRO) … destination d'un Adh‚rent ou d'un OF, je veux que, 
         -- l'adresse … afficher dans la zone "correspondance" soit l'adresse TSA propre … ce type de courrier 
         -- si le mode "TSA" s'applique sinon qu'elle soit l'adresse actuelle
         -- =======================================================================================================================================
         -- OPA #810 En tant qu'utilisateur d'Optiform, je souhaite que le courrier de refus d'une prise en charge PEC affiche un texte explicatif 
         -- plut“t que le ou les les libell‚s de motifs de refus
         -- =======================================================================================================================================
         BEGIN
          DECLARE
           @LibelleFonction VARCHAR(50),
           @RaisonSociale VARCHAR(64),
           @id_action_pec INT,
           @id_module_pec INT
         
          SELECT
           @id_action_pec = ID_ACTION_PEC,
           @id_module_pec = ID_MODULE_PEC
          FROM
           MODULE_PEC
          WHERE
           COD_MODULE_PEC = @COD_MODULE_PEC
         
          -- Contact principal par d‚faut mais pas de "."
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
            AND CONTACT.LIB_NOM_CONTACT <> '.';
          END
         
          SELECT
           @RaisonSociale = COALESCE(dbo.IS_EMPTY(ETABLISSEMENT.LIB_ENSEIGNE), ADHERENT.LIB_RAISON_SOCIALE)
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
           LEFT JOIN FONCTION
            ON FONCTION.ID_FONCTION = NR31.ID_FONCTION
          WHERE
           NR31.ID_CONTACT = @ID_CONTACT
           AND NR31.ID_ETABLISSEMENT = @ID_BENEFICIAIRE
         
          -- Si action individuelle: CR de l'‚tablissement et CM de l'‚tablissement
          -- Si action collective: CR = CM = responsable des actions collectives renseign‚ dans le champ CM au niveau de l'action
          SELECT
           ETABLISSEMENT.ID_ETABLISSEMENT,
           ADHERENT.ID_ADHERENT,
           ADHERENT.COD_ADHERENT,
           ETABLISSEMENT.NUM_SIRET,
           ADHERENT.LIB_RAISON_SOCIALE,
           CM.LIB_NOM           AS LIB_NOM_CONSEILLER,
           CM.LIB_PNM           AS LIB_PNM_CONSEILLER,
           CR.ID_UTILISATEUR         AS ID_UTIL,
           CR.LIB_PNM           AS LIB_PRENOM_CHARGE_RELATION,
           CR.LIB_NOM           AS LIB_NOM_CHARGE_RELATION,
           CR.LIB_VILLE          AS LIB_VILLE,
           CR.EMAIL           AS EMAIL_CHARGE_RELATION,
           CAST(ACTION_PEC.COD_ACTION_PEC AS VARCHAR) + '/' 
           + CAST(ACTION_PEC.ANNEE_ACTION_PEC AS VARCHAR)  AS COD_ACTION
          INTO
           #TEMP_REFERENCE
          FROM
           ADHERENT
           INNER JOIN ETABLISSEMENT
            ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
             AND ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
           INNER JOIN NR140
            ON ETABLISSEMENT.ID_ETABLISSEMENT = NR140.ID_ETABLISSEMENT
             AND NR140.ID_ACTION_PEC = @ID_ACTION_PEC
           INNER JOIN ACTION_PEC
            ON ACTION_PEC.ID_ACTION_PEC = NR140.ID_ACTION_PEC
           LEFT JOIN UTILISATEUR CR -- charg‚ de relation ou responsable des actions collectives
            ON CASE
              WHEN  (ACTION_PEC.CIBLE_ACTION = 1 OR ACTION_PEC.BLN_REPRISE_ADHOC = 1)
               THEN ETABLISSEMENT.ID_CHARGEE_RELATION
              ELSE  ACTION_PEC.ID_CHARGEE_MISSION 
             END = CR.ID_UTILISATEUR
           LEFT JOIN UTILISATEUR CM -- charg‚ de mission ou responsable des actions collectives
            ON CASE
              WHEN  (ACTION_PEC.CIBLE_ACTION = 1 OR ACTION_PEC.BLN_REPRISE_ADHOC = 1)
               THEN ETABLISSEMENT.ID_CHARGEE_MISSION
              ELSE  ACTION_PEC.ID_CHARGEE_MISSION
             END = CM.ID_UTILISATEUR;
         
          SELECT
           MODULE_PEC.ID_MODULE_PEC,
           MODULE_PEC.COD_MODULE_PEC, 
           MODULE_PEC.LIBL_MODULE_PEC, 
           MODULE_PEC.DAT_DEBUT,
           MODULE_PEC.DAT_FIN,
           MODULE_PEC.NUM_DUREE_HEURE,
           ORGANISME_FORMATION.LIB_SIGLE_OF,
           STAGIAIRE.CIVILITE,
           STAGIAIRE.PRENOM,
           STAGIAIRE.NOM
           + CASE
            WHEN  (STAGIAIRE.STAGIAIRE_MULTIPLE = 'oui')
             THEN ' (1er stagiaire)'
            ELSE  ''
           END AS NOM
          INTO
           #TEMP_MODULE
          FROM
           MODULE_PEC
           LEFT JOIN ETABLISSEMENT_OF
            ON MODULE_PEC.ID_ETABLISSEMENT_OF = ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF
           LEFT JOIN ORGANISME_FORMATION
            ON ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF
           LEFT JOIN
           (
            SELECT TOP 1
             T1.ID_MODULE_PEC,
             CASE
              WHEN  T2.BLN_MASCULIN = 1
               THEN 'Monsieur '
              ELSE  'Madame '
             END AS CIVILITE,
             T2.PRENOM_INDIVIDU + ' ' AS PRENOM,
             T2.NOM_INDIVIDU AS NOM,
             CASE
              WHEN  (SELECT COUNT(*) FROM STAGIAIRE_PEC WHERE ID_MODULE_PEC = T1.ID_MODULE_PEC GROUP BY ID_MODULE_PEC) > 1
               THEN 'oui'
              ELSE  'non'
             END AS STAGIAIRE_MULTIPLE
            FROM
             STAGIAIRE_PEC T1
             INNER JOIN INDIVIDU T2
              ON T1.ID_INDIVIDU = T2.ID_INDIVIDU
            WHERE
             T1.ID_MODULE_PEC = @ID_MODULE_PEC
           ) AS STAGIAIRE
            ON MODULE_PEC.ID_MODULE_PEC = STAGIAIRE.ID_MODULE_PEC
          WHERE
           MODULE_PEC.COD_MODULE_PEC = @COD_MODULE_PEC;
         
          WITH XMLNAMESPACES
          (
           DEFAULT 'LETTRE_REJET_ADH'
          )
          
          SELECT
          -- R‚cup‚ration des informations sur le contact et le b‚n‚ficiaire
          --dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) as BENEFICIAIRE,
          (
           SELECT
            @RaisonSociale AS LIB_RAISON_SOCIALE,
            (
             SELECT
              ISNULL(CONTACT_CIVILITE.LIBL_CIVILITE, '')
             FROM
              CIVILITE AS CONTACT_CIVILITE
             WHERE
              CONTACT.ID_CIVILITE = CONTACT_CIVILITE.ID_CIVILITE
             FOR XML AUTO, ELEMENTS, TYPE
            ),
            isnull(CONTACT.LIB_NOM_CONTACT, '') AS LIB_NOM_CONTACT,
            isnull(CONTACT.LIB_PNM_CONTACT, '') AS LIB_PNM_CONTACT,
            BENEFICIARE.LIB_ADR,
            ISNULL(BENEFICIARE.LIB_COMP_VOIE, '') AS LIB_COMP_VOIE,
            BENEFICIARE.LIB_CP_CEDEX ,
            BENEFICIARE.LIB_VIL_CEDEX
           FROM
            ADRESSE  AS BENEFICIARE
            LEFT OUTER JOIN CONTACT
             ON CONTACT.ID_CONTACT = @ID_CONTACT
              AND CONTACT.LIB_NOM_CONTACT <> '.'
              AND CONTACT.LIB_NOM_CONTACT NOT LIKE 'contact%'
           WHERE
            BENEFICIARE.ID_ADRESSE = @ID_ADRESSE
           FOR XML AUTO, ELEMENTS, TYPE
          ),
          (
           SELECT
            ISNULL(COD_ADHERENT, '')    AS COD_ADHERENT,
            ISNULL(LIB_PRENOM_CHARGE_RELATION, '') AS LIB_PRENOM_CHARGE_RELATION,
            ISNULL(LIB_NOM_CHARGE_RELATION, '')  AS LIB_NOM_CHARGE_RELATION,
            ISNULL(EMAIL_CHARGE_RELATION, '')  AS EMAIL,
            -- R‚cup‚ration des informations sur l'‚metteur
            dbo.GetXmlAdrUtilAvecTel(ENTETE.ID_UTIL, (SELECT TOP 1 BLN_TSA FROM ETABLISSEMENT WHERE ID_ETABLISSEMENT = @ID_BENEFICIAIRE), 1),
            --(
            -- SELECT
            --  PARAMETRES.FAF_NAME    AS NOM,
            --  EMETTEUR.LIB_ADR1    AS ADRESSE1,
            --  ISNULL(EMETTEUR.LIB_ADR2, '') AS ADRESSE2,     
            --  EMETTEUR.COD_POSTAL    AS CP,
            --  EMETTEUR.LIB_VILLE    AS VILLE,
            --  ISNULL(EMETTEUR.NUM_TEL,'')  AS TEL
            -- FROM
            --  UTILISATEUR AS EMETTEUR
            --  CROSS JOIN PARAMETRES 
            -- WHERE
            --  EMETTEUR.ID_UTILISATEUR = ENTETE.ID_UTIL
            -- FOR XML PATH('EMETTEUR'), TYPE
            --),
            RTRIM(CASE WHEN PATINDEX('%CEDEX%', LIB_VILLE) <> 0 THEN LEFT(LIB_VILLE, PATINDEX('%CEDEX%', LIB_VILLE)-1) ELSE LIB_VILLE END) AS LIB_VILLE, -- retraitement de la ville au cas o— de la forme COMMUNE CEDEX 999
            dbo.GetFullDate(GETDATE()) AS DATE, 
            ISNULL(COD_ACTION,'') AS  COD_ACTION,   
            (
             SELECT TOP 1
              dbo.GetContactSalutation(@ID_CONTACT, 1)
             FROM
              CIVILITE
             FOR XML PATH('POLITESSE_HAUT'), TYPE
            )
           FROM
            #TEMP_REFERENCE AS ENTETE
           FOR XML AUTO, ELEMENTS, TYPE
          ),
          (
           SELECT 
            CORPS.LIBL_MODULE_PEC,
            CONVERT(VARCHAR(10),CORPS.DAT_DEBUT, 103) AS DAT_DEBUT,
            CONVERT(VARCHAR(10),CORPS.DAT_FIN, 103)  AS DAT_FIN,
            CIVILITE,
            PRENOM,
            NOM,
            (
             SELECT
              MOTIF_NON_IMPUTABILITE.LIB_EDT_MOTIF_NON_IMPUTABILITE
             FROM
              #TEMP_MODULE
              INNER JOIN NR204
               ON #TEMP_MODULE.ID_MODULE_PEC = NR204.ID_MODULE_PEC
              INNER JOIN MOTIF_NON_IMPUTABILITE
               ON NR204.ID_MOTIF_NON_IMPUTABILITE = MOTIF_NON_IMPUTABILITE.ID_MOTIF_NON_IMPUTABILITE
             FOR XML RAW('MOTIF_NON_IMPUTABILITE'), ELEMENTS, TYPE, ROOT('MOTIFS')
            ),
            (
             SELECT top 1
              dbo.GetContactSalutation(@ID_CONTACT, 1)
             FROM
              CIVILITE
             FOR XML PATH('POLITESSE_BAS'), TYPE
            )
           FROM
            #TEMP_MODULE AS CORPS
           FOR XML AUTO, ELEMENTS, TYPE
          ),
          (
           SELECT
            [SIGNATURE].LIB_PNM_CONSEILLER,
            [SIGNATURE].LIB_NOM_CONSEILLER
           FROM
            #TEMP_REFERENCE AS [SIGNATURE]
           FOR XML AUTO, ELEMENTS, TYPE
          )
          FROM
           #TEMP_MODULE AS LETTRE
          FOR XML AUTO, ELEMENTS
         END


         -- =============================================
         -- TLE 16/10/2013 : 16240 : Modification taille NUM_IBAN (de 27 … 34)
         -- =============================================
         CREATE  PROCEDURE [dbo].[LEC_DET_RESTRICTION_NUM_FACTURE]
         	@ID_POSTE_COUT_REGLE AS INT,
         	@ID_MODULE AS INT,
         	@ID_ADHERENT AS INT,
         	@ID_OF AS INT,
         	@NUM_FACTURE AS VARCHAR(34), -- NUM_IBAN
         	@DAT_VALID_REGLEMENT AS DATETIME OUT,
         	@PERMIT BIT OUT
         	
         AS
         
         BEGIN
         
         	--------------------------------------------------------------------------------------------
         	--                           Declare and initialitation variables                         --
         	--------------------------------------------------------------------------------------------	
         
         	DECLARE @COUNT_FACTURE AS INT
         	SET @COUNT_FACTURE = 0
         	SET @PERMIT = 1
         
         	SELECT 
         		@COUNT_FACTURE = COUNT(T.NUM_IBAN)
         	FROM 
         		[TRANSACTION] AS T
         		LEFT JOIN ETABLISSEMENT_OF AS ETA_OF ON T.ID_ETABLISSEMENT_OF_BENEF = ETA_OF.ID_ETABLISSEMENT_OF
         	WHERE
         		ETA_OF.ID_OF = @ID_OF
         		AND T.NUM_IBAN = @NUM_FACTURE
         		AND T.NUM_IBAN IS NOT NULL 
         		AND T.NUM_IBAN <> ''
         
         	--PRINT ' Result: ' + STR(@COUNT_FACTURE) + ' ocurrences ORGANISME_FORMATION:  ' + STR(@ID_OF) + ' using the NUM_FACTURE = ' + @NUM_FACTURE
         	IF @COUNT_FACTURE > 0
         		BEGIN
         
         			--PRINT N' Already exists this NUM_FACTURE to the ID_OF'
         			SET @PERMIT = 0
         
         		END
         	ELSE
         		BEGIN
         
         			SELECT 
         				@COUNT_FACTURE = COUNT(T.NUM_IBAN)
         			FROM 
         				[TRANSACTION] AS T
         				LEFT JOIN ETABLISSEMENT_OF AS ETA_OF ON T.ID_ETABLISSEMENT_OF_BENEF = ETA_OF.ID_ETABLISSEMENT_OF
         				LEFT JOIN MODULE_PEC AS M ON ETA_OF.ID_OF = M.ID_ETABLISSEMENT_OF
         			WHERE
         				ETA_OF.ID_OF = @ID_OF AND
         				T.NUM_IBAN = @NUM_FACTURE AND
         				T.NUM_IBAN IS NOT NULL AND
         				T.NUM_IBAN <> ''
         
         			--PRINT ' Result: ' + STR(@COUNT_FACTURE) + ' ocurrences MODULE using the NUM_FACTURE = ' + @NUM_FACTURE
         			IF @COUNT_FACTURE = 1
         				BEGIN
         
         					--PRINT N' Exists one MODULE vinculate with this NUM_FACTURE.'
         					SET @PERMIT = 0
         
         				END
         		END
         
         	IF @PERMIT = 1
         		BEGIN
         	
         			SELECT 
         				@PERMIT = CASE 
         					WHEN P.ID_MODULE_PEC = @ID_MODULE THEN 1
         					ELSE 0
         				END
         			FROM 
         				ADHERENT AS A
         				INNER JOIN ETABLISSEMENT AS E ON A.ID_ADHERENT = E.ID_ADHERENT
         				INNER JOIN [TRANSACTION] AS T ON T.ID_ETABLISSEMENT_BENEF = E.ID_ETABLISSEMENT
         				INNER JOIN POSTE_COUT_REGLE AS P ON P.ID_TRANSACTION = T.ID_TRANSACTION
         				
         			WHERE
         				A.ID_ADHERENT = @ID_ADHERENT AND
         				T.NUM_IBAN = @NUM_FACTURE AND 
         				T.NUM_IBAN IS NOT NULL AND 
         				T.NUM_IBAN <> ''
         
         			GROUP BY (P.ID_MODULE_PEC)
         
         			IF @PERMIT IS NULL
         				BEGIN
         
         					SET @PERMIT = 1
         					--PRINT ' Result: 0 ocurrences ADHERENT:  ' + STR(@ID_ADHERENT) + ' using the NUM_FACTURE = ' + @NUM_FACTURE
         
         				END
         			ELSE
         				BEGIN
         		
         					SET @PERMIT = 0
         					--PRINT ' Result: ' + STR(1) + ' ocurrences ADHERENT:  ' + STR(@ID_ADHERENT) + ' using the NUM_FACTURE = ' + @NUM_FACTURE
         
         				END
         
         		END
         
         	IF @PERMIT = 1
         		BEGIN
         
         			SELECT  
         				@DAT_VALID_REGLEMENT = DAT_VALID_REGLEMENT
         			FROM 
         				MODULE_PEC AS M
         				INNER JOIN POSTE_COUT_REGLE AS P ON P.ID_MODULE_PEC = M.ID_MODULE_PEC
         				INNER JOIN REGLEMENT AS R ON R.ID_REGLEMENT = P.ID_REGLEMENT
         			WHERE
         				P.ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE
         
         		END
         
         END
         
    CREATE PROCEDURE [dbo].[INS_SESSION_PEC]
          @DAT_DEBUT       DATETIME, 
          @DAT_FIN       DATETIME, 
          @NUM_DUREE_JOUR      DECIMAL(18,2), 
          @NUM_DUREE_HEURE     DECIMAL(18,2), 
          @COM_SESSION      VARCHAR(255), 
          @ID_UTILISATEUR      INT,
           @DAT_LETTRE_REMBOURSEMENT_SALAIRE DATETIME, 
          @ID_MODULE_PEC      INT, 
          @BLN_ACTIF       TINYINT
         AS
         -- =============================================
         -- Author:  Say
         -- Create date: 19 juin 2007
         -- Description: cr‚ation de session PEC
         -- ---------------------------------------------
         -- Author:  Say
         -- Modif. date: 28 juin 2007
         -- Description: correction sur le compteur
         -- =============================================
         -- Author  : Safiyulla SPC
         -- Modif. date : 22/04/2008
         -- Description : COD_SESSION_PEC passe a 14 caracteres
         --      et son format.
         -- =============================================
         -- Author  : Safiyulla SPC
         -- Modif. date : 06/02/2009
         -- Description : Correction de Anne for code session
         -- =============================================
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         -- =============================================
         BEGIN
          DECLARE
           @COD_SESSION_PEC   VARCHAR(17),
           @COD_MODULE     VARCHAR(9),
           @COUNT_SESSION    INT,
           @TEXTSession    VARCHAR(2),
           @ACTION_NUM_ANNEE_CREATION VARCHAR(4)
         
          --1. Count of Module
          SET
           @COD_MODULE = LEFT((SELECT COD_MODULE_PEC FROM MODULE_PEC WHERE ID_MODULE_PEC = @ID_MODULE_PEC),9)
         
          --2.  Count of Session
          SELECT 
           @COUNT_SESSION = count(*)   
          FROM
           SESSION_PEC 
          WHERE
           ID_MODULE_PEC = @ID_MODULE_PEC
         
          IF (@COUNT_SESSION IS NULL)
          BEGIN
           SET @TEXTSession = '01'
          END
          ELSE
          BEGIN
           SET @COUNT_SESSION = @COUNT_SESSION + 1
           SET @TEXTSession = RIGHT(REPLICATE('0',1) + cast(@COUNT_SESSION as varchar(2)),2)
          END
         
          DECLARE
           @COD_ACTION int
         
          SELECT
           @COD_ACTION = ID_ACTION_PEC
          FROM
           MODULE_PEC
          WHERE
           ID_MODULE_PEC = @ID_MODULE_PEC
         
          SELECT 
           @ACTION_NUM_ANNEE_CREATION = CONVERT(VARCHAR(4), ANNEE_ACTION_PEC) 
          FROM
           ACTION_PEC
          WHERE
           ID_ACTION_PEC = (SELECT ID_ACTION_PEC FROM MODULE_PEC WHERE ID_MODULE_PEC = @ID_MODULE_PEC)
          
          IF (@ACTION_NUM_ANNEE_CREATION IS NULL)
          BEGIN
           SET @ACTION_NUM_ANNEE_CREATION = RTRIM(LTRIM(STR(YEAR(GETDATE()))))
          END
          
          SET @COD_SESSION_PEC = @COD_MODULE +' '+ @TEXTSession + '/' +  @ACTION_NUM_ANNEE_CREATION
         
          INSERT INTO SESSION_PEC
          (
           COD_SESSION_PEC,
           DAT_DEBUT,
           DAT_FIN, 
           NUM_DUREE_JOUR,
           NUM_DUREE_HEURE, 
           DAT_CREATION,
           DAT_MODIF, 
           COM_SESSION,
           ID_UTILISATEUR, 
           DAT_LETTRE_REMBOURSEMENT_SALAIRE, 
           ID_MODULE_PEC, 
           BLN_ACTIF
          )
          VALUES
          (
           @COD_SESSION_PEC,
           @DAT_DEBUT,
           @DAT_FIN,
           @NUM_DUREE_JOUR,@NUM_DUREE_HEURE,
           getdate(),
           getdate(),
           @COM_SESSION,
           @ID_UTILISATEUR,
            @DAT_LETTRE_REMBOURSEMENT_SALAIRE,
           @ID_MODULE_PEC,
           @BLN_ACTIF
          )
          
          RETURN @@IDENTITY
         END


CREATE PROCEDURE [dbo].[INS_EXPORT_D3R]  
          @ID_DOSSIER INT,
          @TYPE_DOSSIER VARCHAR(3)
         AS
         -- ============================================================
         -- ARI + HBO : #907 : Ajout du dossier dans la table EXPORT_D3R
         -- ============================================================
         BEGIN
          INSERT INTO EXPORT_D3R
          (
           ID_DOSSIER,
           TYPE_DOSSIER,
           DAT_INSERTION
          )
          VALUES
          (
           @ID_DOSSIER,
           @TYPE_DOSSIER,
           GETDATE()
          )
         END


         CREATE PROCEDURE [BATCH_TRANSFERT_DOTATION_ABONDEMENT_VO_OPTION_PERFORMANCE]
         /*
         =============================================  
         Author  : MBL
         Create date : 03/03/2015
         Description : Proc‚dure permettant de lancer des tranferts des dotations 
         d'abondement sur le Compte Versement Obligatoire			(@COD_TYPE_EVENEMENT_DOTATION_ABONDEMENT_VO_OPTION_PERFORMANCE = 'DOTABOVO')
         pour les groupes ayant souscrit une des Options PERFORMANCE
         
         Le traitement fait appel a la fonction de table F_TRANSFERT_DOTATION_ABONDEMENT_VO_OPTION_PERFORMANCE constituant un outil d'aide … la d‚cision 
         afin de g‚n‚rer ces transferts.
         
         -- CONDITION DE LANCEMENT
         Parametre : la valorisation du parametre @ID_ADHERENT_TRAITE est optionnelle. 
         			S'il est valorise, le traitement n'est declenche que pour l'adherent de mˆme ID
         			S'il n'est pas valorise, le traitement est declenche pour tous les adherents
         =============================================  
         -- Author		: MBL
         -- Create date	: 21/04/2015
         -- Description	: Alimentation de l'‚tablissement lors de la cr‚ation transfert (colonne TRANSFERT.ID_ETABLISSEMENT)
         -- =============================================
         */
         @COD_TYPE_EVENEMENT_DOTATION_ABONDEMENT_VO_OPTION_PERFORMANCE	varchar(8),
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
         	@ID_ACTIVITE_PLAN				INTEGER,
         	@BLN_COMPTE_VERS_ENVELOPPE		TINYINT,
         	@ID_TRANSFERT					INT,
         	@ID_TYPE_FINANCEMENT			INT
         
         	SELECT @NUM_ANNEE_N = 2014
         
         	select @ID_TYPE_EVENEMENT_TRANSFERT = ID_TYPE_EVENEMENT FROM TYPE_EVENEMENT where COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT_DOTATION_ABONDEMENT_VO_OPTION_PERFORMANCE	
         	
         	IF @ID_TYPE_EVENEMENT_TRANSFERT IS NULL 
         	BEGIN
         		SELECT 'Le type d''evenement associe au code evenement passe en parametre : ' + @COD_TYPE_EVENEMENT_DOTATION_ABONDEMENT_VO_OPTION_PERFORMANCE + ' n''existe pas'
         	END
         	ELSE
         	BEGIN
         
         		SELECT @ID_TYPE_FINANCEMENT = 4 -- Compte Versement Obligatoire
         		
         		SELECT t.*, ADHERENT.ID_ETABLISSEMENT_PRINCIPAL
         		INTO #TMP_TRANSFERT 
         		FROM F_TRANSFERT_DOTATION_ABONDEMENT_VO_OPTION_PERFORMANCE(@NUM_ANNEE_N, @ID_ADHERENT_TRAITE, @COD_TYPE_EVENEMENT_DOTATION_ABONDEMENT_VO_OPTION_PERFORMANCE) t
         		INNER JOIN ADHERENT ON ADHERENT.ID_ADHERENT = t.ID_ADHERENT
         
         --SELECT * FROM  #TMP_TRANSFERT 
         
         		SELECT @DAT = GETDATE()
         
         		SELECT	@ID_PERIODE_N	= ID_PERIODE   
         		from	PERIODE     
         		where	NUM_ANNEE		= @NUM_ANNEE_N 
         		AND		ID_TYPE_PERIODE = 1   
         
         		SELECT	@ID_PERIODE_N_PLUS1		= ID_PERIODE
         		from	PERIODE     
         		where	NUM_ANNEE				= @NUM_ANNEE_N + 1
         		AND		ID_TYPE_PERIODE			= 1   
         
         		SET @LIBL_EVENEMENT		= 'Dotation Abondement Cpt VO Option Performance '	+ CAST(@NUM_ANNEE_N + 1 AS VARCHAR(4)) 
         		SET @LIBL_MVT			= 'Abondement compl‚mentaire Option Performance '		+ CAST(@NUM_ANNEE_N + 1 AS VARCHAR(4)) 
         
         
         		DECLARE cu_transfert CURSOR FOR
         		SELECT ID_ADHERENT,  ID_GROUPE = ID_GROUPE_DOTATION, ID_BRANCHE, [ID_ACTIVITE_PLAN_N+1], MNT_TRANSFERT, ID_ETABLISSEMENT_PRINCIPAL
         		FROM #TMP_TRANSFERT
         		WHERE ABS(MNT_TRANSFERT) > 0
         
         		OPEN cu_transfert
         
         		FETCH cu_transfert INTO
         		@ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @ID_ACTIVITE_PLAN, @MNT_TRANSFERT, @ID_ETABLISSEMENT
         
         
         		WHILE (@@FETCH_STATUS <> -1)
         		BEGIN	
         			-- Recherche de l'enveloppe de collecte PIVOT
         			SELECT		@ID_ENVELOPPE = ID_ENVELOPPE , @LIBL_ENVELOPPE = LIBL_ENVELOPPE 
         			FROM		TYPE_ENVELOPPE 
         			INNER JOIN	ENVELOPPE ON ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE
         			WHERE		TYPE_ENVELOPPE.BLN_COLLECTE = 1 
         			AND			TYPE_ENVELOPPE.ID_ACTIVITE	= @ID_ACTIVITE_PLAN
         			AND			ENVELOPPE.ID_PERIODE		= @ID_PERIODE_N_PLUS1
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
         			--	ID_TYPE_FINANCEMENT			= @ID_TYPE_FINANCEMENT,   -- Type de financement sur Compte Historique
         			--	ID_UTILISATEUR				= 82, 
         			--	ID_PERIODE					= @ID_PERIODE_N_PLUS1,
         			--	COM_TRANSFERT				= @LIBL_MVT, 
         			--	LIBL_MVT_BUDGETAIRE			= @LIBL_MVT,
         			--	ID_TYPE_EVENEMENT			= @ID_TYPE_EVENEMENT_TRANSFERT
         					
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
         			@ID_ADHERENT, @ID_GROUPE, @ID_BRANCHE, @ID_ACTIVITE_PLAN, @MNT_TRANSFERT, @ID_ETABLISSEMENT
         
         
         		END
         
         		CLOSE cu_transfert
         		DEALLOCATE cu_transfert
         	END
         	
         	IF OBJECT_ID('tempdb..#TMP_TRANSFERT', 'U') IS NOT NULL 
         	BEGIN
         		drop table #TMP_TRANSFERT
         	END
         
         END		
         

         -- =============================================
         -- Author:		Dorota Szeliga
         -- Create date: 03/04/2008
         -- Description:	Chiffrage PEC. voir Annexe au C2P_SFD_PEC.doc
         -- Attention: il y a uniquement les regles de chiffrage ici. 
         -- pour tout ce qui est enveloppe et compte courant voir INS_PLAN_FINANCEMENT_US_CHIFFRAGE 	
         -----------------------------------------------
         -- 30/04/08 DSZE
         -- Verification d'existence d'option pour entreprise est faite dans INS_PLAN_FINANCEMENT_US_CHIFFRAGE
         -----------------------------------------------
         -- 10/10/09 AMA
         -- Les troncatures sont maintenant effectu‚es apr‚s ce traitement 
         -- pour renforcer la coh‚rence du calcul (annulation de la correction
         -- du 03/10/09, l'op‚ration ‚tant maintenant effectu‚e dans )
         -- INS_PLAN_FINANCEMENT_US_CHIFFRAGE
         -----------------------------------------------
         -- 07/12/09 DSZ
         -- passer les montants a INS_PLAN_FINANCEMENT_US_CHIFFRAGE 	en tant que decimal(18,2)
         -----------------------------------------------
         -- 01/03/10 DSZ
         -- EVOL 12302 RŠgle chiffrage s‚niors (ajout critŠre age)
         -----------------------------------------------
         -- 16/04/10 DSZ
         -- EVOL 12302 Modif la fa‡on de choisir les lignes des rŠgles si plusieurs
         -----------------------------------------------
         -- 12/01/2011 DSZ
         -- 12570 ne pas prendre en compte les dispositifs refuses
         -----------------------------------------------
         -- 13/07/2011 ASD
         -- 12745 Ajout critŠre chiffrage sur effectif (param‚tr‚ uniquement en pharma)
         --       Ajout du paramŠtre @ID_ETAB_STAGIAIRE
         -----------------------------------------------
         -- 06/10/2011 DSZ
         -- 12901 Sp‚cif C2P_SFD_PEC_2.37.doc p 153:
         -- L'ƒge du stagiaire, calcul‚ entre la date de d‚but de formation 
         -- (ACTION_PEC.DAT_DEBUT sauf pour le dispositif fonction tutorale (DISPOSITIF.ID_DISPOSITF = 7) 
         -- o— la date qui sert au calcul de l'ƒge est la date MODULE_PEC.DAT_DEBUT) 
         -- et la date de naissance : INDIVIDU.DAT_NAISSANCE). 
         -----------------------------------------------
         -- 22/11/2011 DSZ
         -- 12975 bien differencier les nulls des 0 dans le param‚tres
         -----------------------------------------------
         -- 23/01/2012 DSZ 
         -- 13143 selon DEFI_SFD_PEC_3.2.doc du 23/01/2012 : ajout parametres  BLN_EXTERNE et BLN_CATALOGUE
         -----------------------------------------------
         -- 23/01/2012 DSZ 13142 utiliser la nouvelle fonction GET_EFFECTIF_ETABLISSEMENT (2.0.0.6)
         -----------------------------------------------
         -- 08/02/2012 DSZ 13129 modif calcul PT 
         -- ajout calcul ratio(stag, disp) = nb_h_prevues(stag, disp, 'HTT')/ (nb_h_pr‚vues (stag, disp))
         -----------------------------------------------
         -- 08/03/2012 DSZ 13305 pourcentage AIC
         -- prise en compte de la nouvelle colonnes des regles chiffrage : POURC_AIC
         -----------------------------------------------
         -- 29/03/2012 DSZ 13377 
         -- eviter les faux messages d'erreur. si PT pour HTT, PT pour TT aussi
         -----------------------------------------------
         -- 30/03/2012 DSZ 13397
         -- vu avec DNE : test sur PT heures  HTT
         -----------------------------------------------
         -- 30/03/2012 ASD 13397
         -- vu avec DNE : activation du POURC_HORAIRE sur les TYPE_CALCUL au R‚el
         --               et donc activation du calcul du DIFF sur ces mˆmes types
         -----------------------------------------------
         -- 25/05/2012 SLAH 13504 
         -- Ajoue de RATIO_REM
         -----------------------------------------------
         -- 18/06/2012 DSZ 13588
         -- Impact du formateur interne dans le chiffrage PEC
         -----------------------------------------------
         -- 10/09/2012 ASD 13882
         -- Correction impact du formateur interne dans le chiffrage PEC pour r‚partition TT/HTT
         ----------------------------------------------  
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         ----------------------------------------------  
         -- DSZ 25/09/2103 : 16177 : taux passent sur decimal(10,5)
         -- =============================================
         
         
         CREATE PROCEDURE [dbo].[CHIFFRAGE_PEC] 
         	@ID_MODULE_PEC INTEGER,
         	@ID_STAGIAIRE INTEGER,
         	@ID_ETAB_STAGIAIRE INTEGER,
         	@ID_BRANCHE INTEGER, 
         	@ID_SANCTION INTEGER, 
         	@ID_MOD_FORM INTEGER,
         	@SOMME_HEURES DECIMAL(18,2),
         	@SALAIRE_HORAIRE_BRUT_CHARGE DECIMAL(18,2),
         	@SALAIRE_HORAIRE_NET DECIMAL(18,2),
         	@BLN_EXTERNE TINYINT,
         	@BLN_CATALOGUE TINYINT,
         	@SOMME_FORMA_INTERNES DECIMAL(18,2)
         AS
         BEGIN
         	SET NOCOUNT ON;
         
         
         ---------------------------------------------------------------
         -------------TAUX_HEBERGEMENT pour BRANCHE
         	declare @taux_hebergement decimal(10,5)
         	select @taux_hebergement = TAUX_HEBERGEMENT 
         	from PLAFONDS_HEBERGEMENT
         	where ID_BRANCHE = @ID_BRANCHE
         
         
         ---------------------------------------------------------------
         -------------- table temporaire pour mont_chiffre (grille Sous-type co–ts)
         -------------- "Dans un premier temps, on distingue les natures d'heures" 
         --------------- on aura donc un montant chiffre pour chaque nature d'heure
         	CREATE TABLE #CHIFFRAGE_MNT_CHIFFRE
         	(ID_SOUSTYPE_COUT INT NOT NULL,
         	ID_POSTE_COUT_ENGAGE INT NOT NULL,
         	NATURE_HEURE VARCHAR(3) NULL, 
         	MNT_CHIFFRE decimal(18,2) NULL,
         	TYPE_CALCUL VARCHAR(200))
         
         ---------------------------------------------------------------
         -------------- table temporaire pour resultats finals
         	CREATE TABLE #CHIFFRAGE_FINAL
         		(ID_DISPOSITIF int not null,
         		ID_UNITE_STAGIAIRE int not null,
         		ID_POSTE_COUT_ENGAGE int not null,
         		MONTANT decimal(18,2) null,		
         		COMMENT varchar(200))
         
         ---------------------------------------------------------------
         --------------- table temporaire pour les montants differentiels
         	CREATE TABLE #CHIFFRAGE_DIFF
         		(ID_POSTE_COUT_ENGAGE int not null,
         		 ID_DISPOSITIF int not null,
         		 DIFF decimal(18,2) not null)		
         
         
         --ajout ASD 13/07/2011 EVOL 12745
         ----------------Effectif de l'‚tablissement du STAGIAIRE au d‚but de formation
         
         declare @EffectifAdhStagiaire int
         set @EffectifAdhStagiaire= dbo.GET_EFFECTIF_ETABLISSEMENT(@ID_ETAB_STAGIAIRE)
         
         
         
         ---------------------------------------------------------------
         --------------On prend en compte d'abord chaque unite stagiaire pour le stagiaire
         -------------- uniquement avec les heures >0, sinon il y a rien a calculer
         	declare @NB_HEURES_PREVU_HTT decimal(18,2)
         	declare @NB_HEURES_PREVU_TT decimal(18,2)
         	declare @NB_HEURE_REM decimal(18,2)
         	declare @ID_DISPOSITIF int
         	declare @ID_UNITE_STAGIAIRE int
         
         	-- identification unique des unit‚s stagiaire (US) et dispositifs
             -- pour ce stagiaire PEC 
         	DECLARE curs_soustype_dispositif CURSOR FOR 
         	select 
         		ID_DISPOSITIF,
         		isnull(UNITE_STAGIAIRE.NB_HEURE_HTT,0),
         		isnull(UNITE_STAGIAIRE.NB_HEURE_ENGAGE,0) - isnull(UNITE_STAGIAIRE.NB_HEURE_HTT,0),
         		UNITE_STAGIAIRE.NB_HEURE_REM,
         		UNITE_STAGIAIRE.ID_UNITE_STAGIAIRE
         	from UNITE_STAGIAIRE
         	where ID_STAGIAIRE_PEC = @ID_STAGIAIRE
         	and (isnull(UNITE_STAGIAIRE.MNT_ENGAGE_HT,0) >0 or
         		 isnull(UNITE_STAGIAIRE.NB_HEURE_ENGAGE,0) >0)
         --ajout DSZ 12/11/2011
         	and UNITE_STAGIAIRE.BLN_REFUS = 0
         
         	OPEN curs_soustype_dispositif
         	-- pour chaque dispositif/unite stagiaire :
         	FETCH NEXT FROM curs_soustype_dispositif 
         	INTO @ID_DISPOSITIF, @NB_HEURES_PREVU_HTT, @NB_HEURES_PREVU_TT,@NB_HEURE_REM, @ID_UNITE_STAGIAIRE
         
         	WHILE @@FETCH_STATUS = 0
         	BEGIN
         		delete from #CHIFFRAGE_MNT_CHIFFRE
         
         	-- DSZ 12901 06/10/11 code deplace ici et modifie
         	------------------AGE de STAGIAIRE au d‚but de formation
         	--
         		declare @AgeStagiaire int
         		if (@ID_DISPOSITIF = 7) -- dispositif fonction tutorale
         			select @AgeStagiaire = dbo.F_GET_AGE(INDIVIDU.DAT_NAISSANCE, MODULE_PEC.DAT_DEBUT)
         			from INDIVIDU 
         				inner join STAGIAIRE_PEC on (STAGIAIRE_PEC.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU)
         				inner join MODULE_PEC on (MODULE_PEC.ID_MODULE_PEC = STAGIAIRE_PEC.ID_MODULE_PEC)
         			where STAGIAIRE_PEC.ID_STAGIAIRE_PEC = @ID_STAGIAIRE
         		else
         			select @AgeStagiaire = dbo.F_GET_AGE(INDIVIDU.DAT_NAISSANCE, ACTION_PEC.DAT_DEB_ACTION_PEC)
         			from INDIVIDU 
         				inner join STAGIAIRE_PEC on (STAGIAIRE_PEC.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU)
         				inner join MODULE_PEC on (MODULE_PEC.ID_MODULE_PEC = STAGIAIRE_PEC.ID_MODULE_PEC)
         				inner join ACTION_PEC on (ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC)
         			where STAGIAIRE_PEC.ID_STAGIAIRE_PEC = @ID_STAGIAIRE
         
         	---------------------------------------------------------------
         	------------ identification des regles de chiffrage 
         	------------ qui sont definis entre autres pour le dispositif
         	------------ voir 3) dans la specification
         		declare	@TYPE_CALCUL_HTT varchar(2)
         		declare	@TAUX_HORAIRE_HTT decimal(10,5)
         		declare	@POURC_HORAIRE_HTT decimal(10,5)
         		declare	@PLAFOND_TOTAL_HTT decimal(18,2)
         		declare	@TYPE_CALCUL_TT varchar(2)
         		declare	@TAUX_HORAIRE_TT decimal(10,5)
         		declare	@POURC_HORAIRE_TT decimal(10,5)
         		declare	@PLAFOND_TOTAL_TT decimal(18,2)
         		declare @NATURE_HEURES_HTT varchar(3)
         		declare @NATURE_HEURES_TT varchar(3)
         		declare @POURC_AIC decimal(10,5)
         		set @TYPE_CALCUL_HTT = null
         		set @TYPE_CALCUL_TT = null
         		
         		-- initialisation des @..._HTT 
         		SELECT top 1
         			@TYPE_CALCUL_HTT = TYPE_CALCUL,
         			@TAUX_HORAIRE_HTT = TAUX_HORAIRE, -- DSZ 12975 NON :isnull(TAUX_HORAIRE,0),
         			@POURC_HORAIRE_HTT = POURC_HORAIRE, -- isnull(POURC_HORAIRE,0),
         			@PLAFOND_TOTAL_HTT = PLAFOND_TOTAL, --isnull(PLAFOND_TOTAL,0)
         			@NATURE_HEURES_HTT = NATURE_HEURES,
         			@POURC_AIC = POURC_AIC
         		FROM
         			REGLES_CHIFFRAGE_PEC
         		WHERE
         			BLN_ACTIF=1
         			AND (ID_BRANCHE = @ID_BRANCHE or ID_BRANCHE IS NULL)
         			AND (ID_DISPOSITIF = @ID_DISPOSITIF or ID_DISPOSITIF IS NULL)
         			AND (ID_SANCTION = @ID_SANCTION or ID_SANCTION IS NULL)
         			AND (ID_MODALITE_FORMATION = @ID_MOD_FORM or ID_MODALITE_FORMATION IS NULL)
         			AND (NATURE_HEURES = 'HTT' or NATURE_HEURES IS NULL)
         
         			AND (coalesce(AGE_MIN,0)<=@AgeStagiaire or @AgeStagiaire IS NULL)
         			AND (coalesce(AGE_MAX,200)>@AgeStagiaire or @AgeStagiaire IS NULL)
         			-- ajout ASD 20110713
         			AND (coalesce(EFFECTIF_MIN,0)<=@EffectifAdhStagiaire or @EffectifAdhStagiaire IS NULL)
         			AND (coalesce(EFFECTIF_MAX,1000000)>@EffectifAdhStagiaire or @EffectifAdhStagiaire IS NULL)
         			-- fin ajout ASD 20110713
         			--ajout DSZ 13143
         			AND (EXTERNE = @BLN_EXTERNE or EXTERNE is null)
         			AND (CATALOGUE = @BLN_CATALOGUE or CATALOGUE is null)
         			--fin ajout DSZ 13143
         		ORDER BY --ajout DSZ 16/04/2010: nulls … la fin si plusieurs lignes
         			coalesce(ID_BRANCHE, 50),
         			coalesce(ID_SANCTION, 100),
         			coalesce(CATALOGUE, 10),
         			AGE_MIN desc,
         			AGE_MAX desc
         			-- ajout ASD 20110713
         			,
         			EFFECTIF_MIN desc,
         			EFFECTIF_MAX desc ,
         			-- fin ajout ASD 20110713
         			coalesce(EXTERNE, 10)
         		
         		-- puis initialisation des @..._TT 
         		if @TYPE_CALCUL_HTT is null or @TYPE_CALCUL_HTT <> 'PT' --sauf si on a d‚j… trouv‚ PT
         			SELECT top 1
         				@TYPE_CALCUL_TT = TYPE_CALCUL,
         				@TAUX_HORAIRE_TT = TAUX_HORAIRE, -- DSZ 12975 NON :isnull(TAUX_HORAIRE,0),
         				@POURC_HORAIRE_TT = POURC_HORAIRE ,-- isnull(POURC_HORAIRE,0),
         				@PLAFOND_TOTAL_TT = PLAFOND_TOTAL, -- isnull(PLAFOND_TOTAL,0)
         				@NATURE_HEURES_TT = NATURE_HEURES,
         				@POURC_AIC = ISNULL(@POURC_AIC, REGLES_CHIFFRAGE_PEC.POURC_AIC)
         			FROM
         				REGLES_CHIFFRAGE_PEC
         			WHERE
         				BLN_ACTIF=1
         				AND (ID_BRANCHE = @ID_BRANCHE or ID_BRANCHE IS NULL)
         				AND (ID_DISPOSITIF = @ID_DISPOSITIF or ID_DISPOSITIF IS NULL)
         				AND (ID_SANCTION = @ID_SANCTION or ID_SANCTION IS NULL)
         				AND (ID_MODALITE_FORMATION = @ID_MOD_FORM or ID_MODALITE_FORMATION IS NULL)
         				AND (NATURE_HEURES = 'TT' or NATURE_HEURES IS NULL)
         				----------ajout DSZ 23/02/2010 EVOL 12302
         				AND (coalesce(AGE_MIN,0)<=@AgeStagiaire or @AgeStagiaire IS NULL)
         				AND (coalesce(AGE_MAX,200)>@AgeStagiaire or @AgeStagiaire IS NULL)
         				-- ajout ASD 20110713
         				AND (coalesce(EFFECTIF_MIN,0)<=@EffectifAdhStagiaire or @EffectifAdhStagiaire IS NULL)
         				AND (coalesce(EFFECTIF_MAX,1000000)>@EffectifAdhStagiaire or @EffectifAdhStagiaire IS NULL)
         				-- fin ajout ASD 20110713
         				--ajout DSZ 13143
         				AND (EXTERNE = @BLN_EXTERNE or EXTERNE is null)
         				AND (CATALOGUE = @BLN_CATALOGUE or CATALOGUE is null)
         				--fin ajout DSZ 13143
         			ORDER BY --ajout DSZ 16/04/2010: nulls … la fin si plusieurs lignes
         				coalesce(ID_BRANCHE, 50),
         				coalesce(ID_SANCTION, 100),
         				coalesce(CATALOGUE, 10),
         				AGE_MIN desc,
         				AGE_MAX desc
         				-- ajout ASD 20110713
         				, 
         				EFFECTIF_MIN desc,
         				EFFECTIF_MAX desc ,
         				-- fin ajout ASD 20110713
         				coalesce(EXTERNE, 10)
         
         
         	---------------------------------------------------------------
         	-------------application ratio aux plafonds totaux pour la rŠgle PT (DSZ 08/02/2102 13129)
         	
         	if (@TYPE_CALCUL_HTT = 'PT' 
         		and @NATURE_HEURES_HTT = 'HTT' --les valeurs possibles sont null ou HTT; si null, pas de ratio … appliquer donc on ne le prend pas en compte ici
         		and @NB_HEURES_PREVU_TT > 0) --sinon le ratio est ‚gal … 1, calcul inutil
         	begin 
         		set @PLAFOND_TOTAL_HTT = 
         			@PLAFOND_TOTAL_HTT * (@NB_HEURES_PREVU_HTT / (@NB_HEURES_PREVU_HTT + @NB_HEURES_PREVU_TT))
         		set @PLAFOND_TOTAL_TT = @PLAFOND_TOTAL_HTT
         	end
         	else  --v‚rifions si une ligne TT avec la rŠgle PT existe
         		if (@TYPE_CALCUL_HTT is null --il n'a pas de rŠgle HTT
         			and @TYPE_CALCUL_TT = 'PT'
         			and @NATURE_HEURES_TT = 'TT' --donc pas null; si null, pas de ratio
         			and @NB_HEURES_PREVU_HTT > 0) --si 0, le ratio … 1
         		begin	
         			set @PLAFOND_TOTAL_TT = 
         				@PLAFOND_TOTAL_TT * (@NB_HEURES_PREVU_TT / (@NB_HEURES_PREVU_HTT + @NB_HEURES_PREVU_TT))
         			set @PLAFOND_TOTAL_HTT = @PLAFOND_TOTAL_TT	
         		end
         	
         	--ajout DSZ 13377 pour eviter les faux messages d'erreur. si PT pour HTT, PT pour TT aussi
         	if @TYPE_CALCUL_HTT = 'PT'
         	begin
         		set @TYPE_CALCUL_TT = 'PT'
         		set @PLAFOND_TOTAL_TT = @PLAFOND_TOTAL_HTT --ajout DSZ 30/03/2012
         	end 
         	---------------------------------------------------------------
         	--------------Maintenant on prendra en compte chaque sous-type de cout
         	-------------- donc chaque poste_cout engage pour le module qui n'etait pas desengage
         		
         		declare @MNT_DEMANDE decimal(18,2)
         		declare @ID_SOUSTYPE_COUT int
         		declare @COD_SOUS_TYPE_COUT varchar(8)
         		declare @PLAFOND_TT decimal(18,2)
         		declare @PLAFOND_HTT decimal(18,2)
         		declare @ID_POSTE_COUT_ENGAGE int
         
         
         		--on calcule deja les plafonds valables pour tous les sous-types de count
         		--DSZ 12975 on emploie isnull ici
         		set @PLAFOND_TT = isnull(@NB_HEURES_PREVU_TT * @TAUX_HORAIRE_TT * @POURC_HORAIRE_TT/100, 0)
         		set @PLAFOND_HTT = isnull( @NB_HEURES_PREVU_HTT * @TAUX_HORAIRE_HTT * @POURC_HORAIRE_HTT/100,0)
         
         		DECLARE curs_soustype_cout CURSOR FOR 
         		SELECT 
         			POSTE_COUT_ENGAGE.MNT_PREVISIONNEL_HT,
         			POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT,
         			SOUS_TYPE_COUT.COD_SOUS_TYPE_COUT,
         			POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE
         		FROM POSTE_COUT_ENGAGE 
         		left join SOUS_TYPE_COUT on (SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT = POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT)
         		WHERE 
         			POSTE_COUT_ENGAGE.ID_MODULE_PEC = @ID_MODULE_PEC
         			and POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT is null
         			and POSTE_COUT_ENGAGE.ID_ENGAGEMENT is null --ajout DSZ 22/11/2011
         		--au cas o—, mais sans impact car le bouton gris‚ si engag‚
         		order by SOUS_TYPE_COUT.NUM_ORDRE
         
         		OPEN curs_soustype_cout
         
         		FETCH NEXT FROM curs_soustype_cout 
         		INTO @MNT_DEMANDE, @ID_SOUSTYPE_COUT, @COD_SOUS_TYPE_COUT, @ID_POSTE_COUT_ENGAGE
         
         
         
         		WHILE @@FETCH_STATUS = 0
         		BEGIN
         		---------------------------------------------------------------
         		------- calculs preliminaires MNT_PRE_NH. Voir 1) et 2) dans la spec
         
         			declare @MNT_PRE_TT decimal(18,2)
         			declare @MNT_PRE_HTT decimal(18,2)
         			set @MNT_PRE_TT = 0
         			set @MNT_PRE_HTT = 0
         			if @COD_SOUS_TYPE_COUT not in ('REM','AF', 'REMFORM') --prorata temporis, point 1)
         				begin
         					if (@SOMME_HEURES <> 0)
         					begin
         						
         						set @MNT_PRE_TT = @MNT_DEMANDE * @NB_HEURES_PREVU_TT /@SOMME_HEURES
         						set @MNT_PRE_HTT = @MNT_DEMANDE * @NB_HEURES_PREVU_HTT /@SOMME_HEURES
         					
         					end
         					if @COD_SOUS_TYPE_COUT in ('HEBR','REPAS', 'REPHEB') --hebergement, repas - cas particulier
         					begin						
         						set @MNT_PRE_TT = dbo.GetMin(@MNT_PRE_TT,@taux_hebergement * @NB_HEURES_PREVU_TT)
         						set @MNT_PRE_HTT = dbo.GetMin(@MNT_PRE_HTT,@taux_hebergement * @NB_HEURES_PREVU_HTT)
         					end 
         				end
         			else   --'remuneration', 'allocation formation' - calculs preliminaires sur la base du salaire, point 2)
         				if @COD_SOUS_TYPE_COUT = 'REM'					
         					set @MNT_PRE_TT = @SALAIRE_HORAIRE_BRUT_CHARGE * @NB_HEURE_REM
         				else if @COD_SOUS_TYPE_COUT = 'AF'
         					set @MNT_PRE_HTT = (@SALAIRE_HORAIRE_NET * 0.50) * @NB_HEURES_PREVU_HTT
         				else if @COD_SOUS_TYPE_COUT = 'REMFORM' 
         					begin
         					set @MNT_PRE_TT =  @SOMME_FORMA_INTERNES * (@NB_HEURES_PREVU_TT) /@SOMME_HEURES
         					set @MNT_PRE_HTT = @SOMME_FORMA_INTERNES * (@NB_HEURES_PREVU_HTT) /@SOMME_HEURES
         					end
         			set @MNT_PRE_TT = isnull(@MNT_PRE_TT,0)
         			set @MNT_PRE_HTT = isnull(@MNT_PRE_HTT,0)
         
         
         			--------------------------------------------------------
         			-------- INFO UTILISATEUR -> essayons d'identifier quelques soucis ici
         			declare @dispo varchar(8)
         			select @dispo = COD_DISPOSITIF from DISPOSITIF where ID_DISPOSITIF = @ID_DISPOSITIF
         			if (@COD_SOUS_TYPE_COUT = 'AF')
         				if (@SALAIRE_HORAIRE_NET is null or @SALAIRE_HORAIRE_NET = 0)
         					insert into #CHIFFRAGE_LOG (LOG_MSG)
         					values('On ne peut rien calculer pour '+@dispo+', Allocation formation car le salaire horaire net est 0')
         				else if (@NB_HEURES_PREVU_HTT is null or @NB_HEURES_PREVU_HTT = 0)
         					insert into #CHIFFRAGE_LOG (LOG_MSG)
         					values('On ne peut rien calculer pour '+@dispo+', Allocation formation car le nombre d''heures hors temps de travail est 0')
         
         			if (@COD_SOUS_TYPE_COUT = 'REM')
         				if (@SALAIRE_HORAIRE_BRUT_CHARGE is null or @SALAIRE_HORAIRE_BRUT_CHARGE = 0)
         					insert into #CHIFFRAGE_LOG (LOG_MSG)
         					values('On ne peut rien calculer pour '+@dispo+', Remuneration car le salaire horaire brut est 0')
         				else if (@NB_HEURES_PREVU_TT is null or @NB_HEURES_PREVU_TT = 0)
         					insert into #CHIFFRAGE_LOG (LOG_MSG)
         					values('On ne peut rien calculer pour '+@dispo+', Remuneration car le nombre d''heures sur temps de travail est 0')
         			if (@TYPE_CALCUL_TT is null)
         			begin
         				declare @sanction varchar(20), @modform varchar(8)
         				select @sanction = LIBC_SANCTION from SANCTION where ID_SANCTION = @ID_SANCTION
         				select @modform = COD_MODALITE_FORMATION from MODALITE_FORMATION where ID_MODALITE_FORMATION = @ID_MOD_FORM
         				insert into #CHIFFRAGE_LOG (LOG_MSG)
         				values('Aucune rŠgle de chiffrage n''a ‚t‚ trouv‚e pour '+@dispo+', '+@sanction+', '+@modform+ '!')
         			end
         			if (not(@PLAFOND_TOTAL_TT is null) --DSZ 12975 NON: or @PLAFOND_TOTAL_TT =0)
         				and not(@PLAFOND_TOTAL_HTT is null) -- or @PLAFOND_TOTAL_HTT =0)
         				and @PLAFOND_TOTAL_TT <> @PLAFOND_TOTAL_HTT)
         			begin
         				declare @sanctionPT varchar(20), @modformPT varchar(8)
         				select @sanctionPT = LIBC_SANCTION from SANCTION where ID_SANCTION = @ID_SANCTION
         				select @modformPT = COD_MODALITE_FORMATION from MODALITE_FORMATION where ID_MODALITE_FORMATION = @ID_MOD_FORM
         				insert into #CHIFFRAGE_LOG (LOG_MSG)
         				values('Deux rŠgles de plafond (HTT et TT) trouv‚es pour '+@dispo+', '+@sanctionPT+', '+@modformPT+ '!')
         			end
         
         --if ( @ID_DISPOSITIF =1)
         --begin
         --select '************************************* ' 
         --select @COD_SOUS_TYPE_COUT as SousTypeCout, COD_DISPOSITIF as Dispositif, 'STT',
         --@NB_HEURES_PREVU_TT NB_HEURES_PREVU_TT, @MNT_PRE_TT MNT_PRE_TT, @TYPE_CALCUL_TT TypeCalcul
         --from DISPOSITIF where ID_DISPOSITIF = @ID_DISPOSITIF 
         --select @COD_SOUS_TYPE_COUT as SousTypeCout, COD_DISPOSITIF as Dispositif, 'HTT',
         --@NB_HEURES_PREVU_HTT NB_HEURES_PREVU_HTT, @MNT_PRE_HTT MNT_PRE_HTT, @TYPE_CALCUL_HTT TypeCalcul
         --from DISPOSITIF where ID_DISPOSITIF = @ID_DISPOSITIF 
         --end
         			----------------------------------------------------
         			---- Application des regles de chiffrage. Voir point 4) de la spec
         
         			declare @mnt_chiffre decimal(18,2)
         			-- a) REEL	
         			-- 20120330 activation du POURC_HORAIRE sur le cas R‚el.
         			if (@TYPE_CALCUL_HTT = 'R')
         				insert into #CHIFFRAGE_MNT_CHIFFRE (ID_SOUSTYPE_COUT, ID_POSTE_COUT_ENGAGE, NATURE_HEURE, MNT_CHIFFRE,TYPE_CALCUL)
         				values(@ID_SOUSTYPE_COUT, @ID_POSTE_COUT_ENGAGE, 'HTT', @MNT_PRE_HTT * @POURC_HORAIRE_HTT/100, @TYPE_CALCUL_HTT)
         			if (@TYPE_CALCUL_TT = 'R')
         				insert into #CHIFFRAGE_MNT_CHIFFRE (ID_SOUSTYPE_COUT, ID_POSTE_COUT_ENGAGE, NATURE_HEURE, MNT_CHIFFRE,TYPE_CALCUL)
         				values(@ID_SOUSTYPE_COUT, @ID_POSTE_COUT_ENGAGE, 'TT', @MNT_PRE_TT * @POURC_HORAIRE_TT/100, @TYPE_CALCUL_TT)
         
         			-- b)c) Forfait horaire, plafond horaire
         			if (@TYPE_CALCUL_HTT in ('FH', 'PH'))
         			begin
         				-- ici le tx s'applique au MNT_PRE, il a d‚j… ‚t‚ appliqu‚ au PLAFOND
         				set @mnt_chiffre = dbo.GetMin(	@MNT_PRE_HTT * @POURC_HORAIRE_HTT/100, @PLAFOND_HTT  )
         
         				insert into #CHIFFRAGE_MNT_CHIFFRE (ID_SOUSTYPE_COUT, ID_POSTE_COUT_ENGAGE, NATURE_HEURE, MNT_CHIFFRE,TYPE_CALCUL)
         				values(@ID_SOUSTYPE_COUT, @ID_POSTE_COUT_ENGAGE, 'HTT', @mnt_chiffre, @TYPE_CALCUL_HTT)
         				set @PLAFOND_HTT = @PLAFOND_HTT - @mnt_chiffre
         			end
         			if (@TYPE_CALCUL_TT in ('FH', 'PH'))
         			begin
         				set @mnt_chiffre = dbo.GetMin(@MNT_PRE_TT * @POURC_HORAIRE_TT/100, @PLAFOND_TT)
         				insert into #CHIFFRAGE_MNT_CHIFFRE (ID_SOUSTYPE_COUT, ID_POSTE_COUT_ENGAGE, NATURE_HEURE, MNT_CHIFFRE,TYPE_CALCUL)
         				values(@ID_SOUSTYPE_COUT, @ID_POSTE_COUT_ENGAGE, 'TT', @mnt_chiffre, @TYPE_CALCUL_TT)
         				set @PLAFOND_TT = @PLAFOND_TT - @mnt_chiffre
         			end
         
         			-- d) Plafond total
         			if (@TYPE_CALCUL_TT = 'PT' or @TYPE_CALCUL_HTT = 'PT')
         			begin
         					
         				set @mnt_chiffre = dbo.GetMin(@PLAFOND_TOTAL_TT, @MNT_PRE_TT+@MNT_PRE_HTT)
         				if (@mnt_chiffre is null) --DSZ 12975 NON : or @mnt_chiffre =0)
         					begin
         						set @mnt_chiffre = dbo.GetMin(@PLAFOND_TOTAL_HTT, @MNT_PRE_TT+@MNT_PRE_HTT)
         						set @PLAFOND_TOTAL_HTT= @PLAFOND_TOTAL_HTT - @mnt_chiffre
         						insert into #CHIFFRAGE_MNT_CHIFFRE (ID_SOUSTYPE_COUT, ID_POSTE_COUT_ENGAGE, NATURE_HEURE, MNT_CHIFFRE,TYPE_CALCUL)
         						values(@ID_SOUSTYPE_COUT, @ID_POSTE_COUT_ENGAGE, 'HTT', @mnt_chiffre, @TYPE_CALCUL_TT)
         					end
         				else 
         					begin
         						set @PLAFOND_TOTAL_TT = @PLAFOND_TOTAL_TT - @mnt_chiffre
         						if(@mnt_chiffre<=@PLAFOND_TOTAL_HTT)
         							set @PLAFOND_TOTAL_HTT = @PLAFOND_TOTAL_HTT-@mnt_chiffre
         						else
         							set @PLAFOND_TOTAL_HTT = 0
         						insert into #CHIFFRAGE_MNT_CHIFFRE (ID_SOUSTYPE_COUT, ID_POSTE_COUT_ENGAGE, NATURE_HEURE, MNT_CHIFFRE,TYPE_CALCUL)
         						values(@ID_SOUSTYPE_COUT, @ID_POSTE_COUT_ENGAGE, 'TT', @mnt_chiffre, @TYPE_CALCUL_TT)
         					end 
         			end --'PT'
         
         			-- "Puis on somme pour l'ensemble des natures d'heures :"
         			select @mnt_chiffre = sum (isnull(MNT_CHIFFRE,0))
         			from #CHIFFRAGE_MNT_CHIFFRE 
         			where ID_SOUSTYPE_COUT = @ID_SOUSTYPE_COUT
         			and ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE 
         
         			-- Imputation de la diff‚rence entre le montant demand‚ et le chiffr‚ 
         	--20120330 utilisation du POURC_HORAIRE pour le r‚el ‚galement
         	--20120330		if (@TYPE_CALCUL_TT <> 'R')
         			begin 
         				declare @diff decimal(18,2)
         				set @diff = @MNT_PRE_TT+@MNT_PRE_HTT - @mnt_chiffre
         
         				if (@diff >0 and @diff is not null)
         				begin
         					declare @cnt int
         					select @cnt = count(*) from #CHIFFRAGE_DIFF 
         					where ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
         					and ID_DISPOSITIF = @ID_DISPOSITIF
         
         					if (@cnt=0 or @cnt is null)
         					begin
         						INSERT INTO #CHIFFRAGE_DIFF (ID_POSTE_COUT_ENGAGE,ID_DISPOSITIF,DIFF)
         						VALUES (@ID_POSTE_COUT_ENGAGE, @ID_DISPOSITIF, @diff)
         					end
         					else
         					begin
         						UPDATE #CHIFFRAGE_DIFF
         						SET DIFF=DIFF+ @diff
         						WHERE ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
         						and ID_DISPOSITIF = @ID_DISPOSITIF
         					end
         				end
         			end
         			-------NEXT sous-type cout
         			FETCH NEXT FROM curs_soustype_cout 
         			INTO @MNT_DEMANDE, @ID_SOUSTYPE_COUT, @COD_SOUS_TYPE_COUT, @ID_POSTE_COUT_ENGAGE
         		END 
         		----- tous les sous-types de cout ont ‚t‚ trait‚s
         
         		--Lorsque tous les sous-types de co–t ont ‚t‚ trait‚s, si la valeur du disponible n'est pas z‚ro, on augmente d'autant le sous-type de co–t de moindre priorit‚.	
         		if (@TYPE_CALCUL_TT = 'FH' and @PLAFOND_TT >0)
         		begin
         			update #CHIFFRAGE_MNT_CHIFFRE
         			set TYPE_CALCUL = 'Type de calcul: FH; Mnt preliminaire: '+dbo.FloatToVarchar(MNT_CHIFFRE)+' plus le reste du plafond: '+dbo.FloatToVarchar(@PLAFOND_TT),
         			MNT_CHIFFRE = isnull(MNT_CHIFFRE, 0)+@PLAFOND_TT
         			where ID_SOUSTYPE_COUT = @ID_SOUSTYPE_COUT 
         			and NATURE_HEURE = 'TT'
         		end
         		if (@TYPE_CALCUL_HTT = 'FH' and @PLAFOND_HTT >0)
         		begin
         			update #CHIFFRAGE_MNT_CHIFFRE
         			set TYPE_CALCUL = 'Type de calcul: FH; Mnt preliminaire: '+dbo.FloatToVarchar(MNT_CHIFFRE)+' plus le reste du plafond: '+dbo.FloatToVarchar(@PLAFOND_HTT),
         			MNT_CHIFFRE = isnull(MNT_CHIFFRE,0)+@PLAFOND_HTT
         			where ID_SOUSTYPE_COUT = @ID_SOUSTYPE_COUT 
         			and NATURE_HEURE = 'HTT'
         		end
         
         		CLOSE curs_soustype_cout
         		DEALLOCATE curs_soustype_cout
         		--------------- tous les montants chiffr‚s sont calcul‚s dans #CHIFFRAGE_MNT_CHIFFRE
         
         		INSERT INTO #CHIFFRAGE_FINAL
         				(ID_DISPOSITIF ,
         				ID_UNITE_STAGIAIRE ,
         				ID_POSTE_COUT_ENGAGE,
         				MONTANT,
         				COMMENT)
         		SELECT 
         			@ID_DISPOSITIF,
         			@ID_UNITE_STAGIAIRE,
         			ID_POSTE_COUT_ENGAGE,
         			sum(isnull(MNT_CHIFFRE,0)),
         			max(TYPE_CALCUL)
         		FROM #CHIFFRAGE_MNT_CHIFFRE
         		GROUP BY ID_SOUSTYPE_COUT, ID_POSTE_COUT_ENGAGE
         		HAVING sum(MNT_CHIFFRE) is not null
         
         		FETCH NEXT FROM curs_soustype_dispositif 
         		INTO @ID_DISPOSITIF, @NB_HEURES_PREVU_HTT, @NB_HEURES_PREVU_TT,@NB_HEURE_REM, @ID_UNITE_STAGIAIRE
         	END 
         	CLOSE curs_soustype_dispositif
         	DEALLOCATE curs_soustype_dispositif
         	DROP TABLE #CHIFFRAGE_MNT_CHIFFRE
         
         -----------------------------------------
         -----------------------------------------
         --select s.COD_SOUS_TYPE_COUT, d.COD_DISPOSITIF, f.MONTANT
         --FROM #CHIFFRAGE_FINAL f, POSTE_COUT_ENGAGE p, DISPOSITIF d, SOUS_TYPE_COUT s
         --where f.ID_DISPOSITIF = d.ID_DISPOSITIF
         --and f.ID_POSTE_COUT_ENGAGE = p.ID_POSTE_COUT_ENGAGE
         --and p.ID_SOUS_TYPE_COUT = s.ID_SOUS_TYPE_COUT
         --
         --select d.DIFF, s.COD_SOUS_TYPE_COUT
         --from #CHIFFRAGE_DIFF d, POSTE_COUT_ENGAGE p, SOUS_TYPE_COUT s
         --where d.ID_POSTE_COUT_ENGAGE = p.ID_POSTE_COUT_ENGAGE
         --and p.ID_SOUS_TYPE_COUT = s.ID_SOUS_TYPE_COUT
         
         	-------final
         	DECLARE @montant decimal(18,2)
         	DECLARE @comm varchar(250)
         	DECLARE final_cursor CURSOR FOR 
         	SELECT ID_DISPOSITIF, ID_UNITE_STAGIAIRE, ID_POSTE_COUT_ENGAGE, MONTANT, COMMENT
         	FROM #CHIFFRAGE_FINAL
         	OPEN final_cursor
         
         		FETCH NEXT FROM final_cursor 
         		INTO @ID_DISPOSITIF, @ID_UNITE_STAGIAIRE, @ID_POSTE_COUT_ENGAGE, @montant, @comm
         
         
         		WHILE @@FETCH_STATUS = 0
         		BEGIN
         			if (@montant is null)
         			begin
         				FETCH NEXT FROM final_cursor 
         				INTO @ID_DISPOSITIF, @ID_UNITE_STAGIAIRE, @ID_POSTE_COUT_ENGAGE, @montant, @comm
         				continue
         			end
         			if (LEN(@comm)<=2)
         				set @comm = 'Type de calcul '+@comm+'; montant chiffr‚ '+dbo.FloatToVarchar(@montant)
         
         			set @diff = NULL
         			select @diff = DIFF
         			from #CHIFFRAGE_DIFF
         			where ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
         			and ID_DISPOSITIF = @ID_DISPOSITIF
         			
         			
         --***
         --select 'TO plan financement', COD_DISPOSITIF dispo, @ID_DISPOSITIF id_disp,
         --@ID_UNITE_STAGIAIRE unite_st,	@ID_POSTE_COUT_ENGAGE ID_POSTE_COUT_ENGAGE,
         --s.COD_SOUS_TYPE_COUT SousTypeCout, @montant mnt, @diff diff, @comm comment
         --from dispositif d,dbo.POSTE_COUT_ENGAGE p,dbo.SOUS_TYPE_COUT s 
         --where d.id_dispositif = @ID_DISPOSITIF
         --and p.ID_SOUS_TYPE_COUT = s.ID_SOUS_TYPE_COUT
         --and p.ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
         --***
         			declare @comm_diff varchar(255)
         			set @comm_diff = 'difference entre le demand‚ et le chiffr‚: '+dbo.FloatToVarchar(@diff)
         --				insert into #CHIFFRAGE_LOG (LOG_MSG)
         --				values('Exec INSPLANFI '+str(@ID_MODULE_PEC)+', '+str(@ID_DISPOSITIF)+', '+str(@ID_UNITE_STAGIAIRE)+ '!')
         			declare @montant_decimal decimal(18,2)
         			set @montant_decimal = cast (@montant as decimal(18,2))
         			declare @montant_diff decimal(18,2)
         			set @montant_diff = cast (@diff as decimal(18,2))
         			EXEC INS_PLAN_FINANCEMENT_US_CHIFFRAGE 	
         					@ID_MODULE_PEC, 
         					@ID_DISPOSITIF,
         					@ID_UNITE_STAGIAIRE,
         					@ID_POSTE_COUT_ENGAGE,
         					@montant_decimal,
         					@comm,
         					@montant_diff,
         					@comm_diff,
         					@POURC_AIC
         			
         			--l'utilisateur doit obligatoirement clique "approuver" sur l'ecran chiffrage
         			-- ce qui va saisir 1 dans BLN_OK_FINANCEMENT
         			-- donc on y met 0 en ce moment
         			UPDATE POSTE_COUT_ENGAGE
         			SET BLN_OK_FINANCEMENT = 0
         			WHERE ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
         
         			FETCH NEXT FROM final_cursor 
         			INTO @ID_DISPOSITIF, @ID_UNITE_STAGIAIRE, @ID_POSTE_COUT_ENGAGE, @montant, @comm
         		END 
         		CLOSE final_cursor
         		DEALLOCATE final_cursor
         		DROP TABLE #CHIFFRAGE_FINAL
         		DROP TABLE #CHIFFRAGE_DIFF
         END
         
 CREATE PROCEDURE [dbo].[INS_REGLEMENT]
         -- =============================================  
         -- Author:  SV  
         -- Create date: 14 ao–t 2007  
         -- Description: Ajout d'un contrainte sur le BLN_ACTIF du poste co–t r‚gl‚  
         -- =============================================  
         -- Author:  KS  
         -- Modif. date: 14 sept 2007  
         -- Description: Ajout de l'ID AGENCE  
         -- ---------------------------------------------  
         -- Modif. date: 17 sept 2007  
         -- Description: bln actif = 2 + null en date ‚dition  
         -- =============================================  
         -- Author:  SV  
         -- Modif. date: 31 octobre 2007  
         -- Description: Ajout de la prise en compte de l'agence  
         -- =============================================  
         -- Author:  KS  
         -- Modif. date: 29 nov 2007  
         -- Description: MANTIS : 0006221 >> MaJ du PCR selon le num iban  
         -- ---------------------------------------------  
         -- Modif. date: 06 d‚c 2007  
         -- Description: MANTIS : 0006304 >> MaJ des PCR selon mode b‚n‚f. (adh ou ‚tab)  
         -- ---------------------------------------------  
         -- Modif. date: 05 jan 2008  
         -- Description: MANTIS : 0006981 >> MaJ pr‚cise de l'ID Reglement  
         -- ---------------------------------------------  
         -- Modif. date: 05 jan 2008  
         -- Description: MANTIS : 0008005 >> [ADHERENT].ID_AGENCE pour le cas de l'update PCR type ben‚f adh (non 2)  
         -- ---------------------------------------------  
         -- Modif. date: 02 oct 2008 ASD  
         -- Description: modification des jointures sur adherent : pas seulement etablissement principal  
         -- ---------------------------------------------  
         -- Author:  MB  
         -- Modif. date: 05/12/2008  
         -- Description: Dans le cas de la reprise, l'etablissement OF de la transaction peut differe de l'etablissement OF du module  
         -- ---------------------------------------------  
         -- Author:  AMA  
         -- Modif. date: 26/02/2009  
         -- Description: Modification du calcul du nø de virement. On n'utilise plus le compteur  
         -- ---------------------------------------------  
         -- Author:  AMA  
         -- Modif. date: 06/03/2009  
         -- Description: EVOLUTION 289: g‚n‚ration des op‚rations blanches  
         -- ---------------------------------------------  
         -- Author:  AMA  
         -- Modif. date: 13/03/2009  
         -- Description: EVOLUTION 289: Cr‚ation d'un table temporaire @REGLEMENT pour pouvoir  
         --    int‚grer des dates nullables. Evolution ds spec (prise en compte du  
         --    champs DAT_EDITION  
         -- ---------------------------------------------  
         -- Author:  ASD  
         -- Modif. date: 18/03/2009  
         -- Description: EVOLUTION 289: Correction pb de champs manquants ou mal identifi‚s (aliases)  
         -- ---------------------------------------------  
         -- Author:  AMA  
         -- Modif. date: 23/03/2009  
         -- Description: EVOLUTION 289: correction update dans la table session   
         -- ---------------------------------------------  
         -- Author:  AMA  
         -- Modif. date: 01/04/2009  
         -- Description: Correction regression introduite par ASD 18/03/2009  
         --    la double jointure sur les poste de cout r‚gl‚s  
         --    n'a aucun sens et complexifie mortellement la requˆte   
         --    =>(produit cart‚siens de 190 000 de millers de lignes    
         --    36 100 000 000 lignes … traiter...)=>plantage  
         -- ---------------------------------------------  
         -- Author:  ASD  
         -- Modif. date: 07/04/2009  
         -- Description: Correction ‚criture du CASE WHEN en mettant toute la clause aprŠs le WHEN  
         --    Dans le Select into #SESSIONS  
         -- ---------------------------------------------  
         -- Author:  DCL  
         -- Modif. date: 04/05/2009  
         -- Description: ano 11971 : maj id_reglement de PCR pour la transaction @ID_TRANSACTION (dest = OF)  
         -- =============================================  
         -- Author:  MBL
         -- Modif. date: 19/10/2009
         -- Description: Homog‚n‚isation du calcul des montants HT, TVA et TTC sur la chaine de traitement
         -- =============================================  
         -- Author  : APA
         -- Create date : 23/01/2012
         -- Description : Defi Lot 1 - 13139 - Ajout du filtre par Id Utilisateur    
         -- =============================================
         -- 06/03/13 - SBR : l'‚tablissement ADH du PCR peut ˆtre diff‚rent de l'‚tablissemenbt destinataire de la transaction de rŠglement
         -- =============================================
         -- 30/05/13 - OPA : 15236 : UPDATE PCR uniquement si ce sont ceux de l'utilisateur du rŠglement
         -- =============================================
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         -- =============================================
         -- DSZ 13/11/2015 : #1154 : DAT_VALID_REGLEMENT et dat_edition … null pour les montants < 0
         -- =============================================
         -- DSZ 23/02/2016 : #1154 : suppression group by id_transaction, et parametre @id_agence; ajout @NUM_IBAN
         -- =============================================
         
         (  
          @ID_UTILISATEUR_REGLEMENT AS INT,
          @ID_TRANSACTION INT
         )  
           
         AS  
         BEGIN  
          SET NOCOUNT ON  
          DECLARE @COMPTEUR_ORDRE_VIREMENT AS INT  
          DECLARE @COMPTEUR    AS INT  
          DECLARE @COD_REGLEMENT_PREFIXE AS VARCHAR(3)  
          DECLARE @NBLIGNES    AS INT  
          DECLARE @ID_TYPE_DESTINATAIRE AS INT  
          DECLARE @ID_TYPE_BENEFICIAIRE AS INT  
          DECLARE @ID_BENEF    AS INT  
          DECLARE @ID_REGLEMENT   AS INT  
             
          SET @COMPTEUR = 0  
          SET @COD_REGLEMENT_PREFIXE = ''  
          SELECT @COMPTEUR_ORDRE_VIREMENT = COALESCE((SELECT max(NUM_VIREMENT) FROM REGLEMENT),0)  
         
           
          declare @REGLEMENT TABLE   
          (  
           ID_TYPE_DESTINATAIRE INT,  
           ID_TYPE_BENEFICIAIRE INT,  
           ID_BENEF    INT,  
           COD_REGLEMENT   INT,  
           DAT_REGLEMENT   DATETIME,  
           NUM_VIREMENT   INT,  
           DAT_EDITION    DATETIME NULL,  
           MNT_REGLE_TTC   DECIMAL (18, 2),  
           MNT_REGLE_HT   DECIMAL (18, 2),  
           ID_TRANSACTION   INT,  
           BLN_CRITERE    INT,  
           TRAITE     INT
          )  
           
          INSERT INTO @REGLEMENT(ID_TYPE_DESTINATAIRE,  
           ID_TYPE_BENEFICIAIRE,  
           ID_BENEF,  
           COD_REGLEMENT,  
           DAT_REGLEMENT,  
           NUM_VIREMENT,  
           DAT_EDITION,  
           MNT_REGLE_TTC,  
           MNT_REGLE_HT,  
           ID_TRANSACTION,  
           BLN_CRITERE,  
           TRAITE
          )  
          SELECT   
           PCRPR.ID_TYPE_BENEFICIAIRE,  
           PCRPR.ID_TYPE_DESTINATAIRE, 
           PCRPR.ID_DESTINATAIRE ,   
           -1,  --cod reglement
           GETDATE(),  --dat_reglement
           -1,  --num_virement
           NULL,  --DAT_EDITION
           CAST( SUM( CAST (PCRPR.MNT_REGLE_TTC AS DECIMAL(18,2) ) ) AS DECIMAL(18,2)) ,  
           CAST( SUM( CAST (PCRPR.MNT_REGLE_HT AS DECIMAL(18,2) ) ) AS DECIMAL(18,2)) ,   
           min(PCRPR.ID_TRANSACTION) as ID_TRANSACTION,  
           CASE  
            WHEN ( EXISTS(SELECT ID_POSTE_COUT_REGLE FROM POSTE_COUT_REGLE WHERE BLN_CRITERE = 1 AND NUM_IBAN = PCRPR.NUM_IBAN AND 
            min(PCRPR.ID_TRANSACTION) = ID_TRANSACTION) ) THEN 1  
            ELSE 0  
           END ,  
           0
          FROM  
           [dbo].[POSTE_COUT_REGLE_POUR_REGLEMENT](@ID_UTILISATEUR_REGLEMENT) AS PCRPR  
          where PCRPR.ID_TRANSACTION  = @ID_TRANSACTION
          GROUP BY   
           PCRPR.NUM_IBAN,  
           PCRPR.ID_TYPE_DESTINATAIRE,  
           PCRPR.ID_TYPE_BENEFICIAIRE,  
           PCRPR.ID_DESTINATAIRE  
            
          -- en principe il n'y a qu'une ligne, mais mieux vaut pr‚venir ...  
          SELECT TOP 1 @ID_TYPE_DESTINATAIRE = ID_TYPE_DESTINATAIRE, @ID_TYPE_BENEFICIAIRE = ID_TYPE_BENEFICIAIRE, @ID_BENEF = ID_BENEF   
           from @REGLEMENT   
           
          ---------------------------------------------------------------------------------------------------------------------------  
          --        MAJ des lignes REGLEMENT et insertion dans la table REGLEMENT  
          ---------------------------------------------------------------------------------------------------------------------------  
          SET @NBLIGNES = @@ROWCOUNT  
          SET ROWCOUNT 1   
          SET NOCOUNT OFF  
           
          WHILE (@NBLIGNES > 0)  
          BEGIN  
           SET ROWCOUNT 0  
           DELETE FROM @REGLEMENT WHERE TRAITE = 1  
           SET ROWCOUNT 1  
           
           SET @NBLIGNES = (SELECT COUNT(*) FROM @REGLEMENT WHERE TRAITE = 1)  
           
           IF @NBLIGNES > 0  
           BEGIN  
            SET @COMPTEUR = @COMPTEUR + 1  
           END  
             
           UPDATE @REGLEMENT  
           SET   
            NUM_VIREMENT = @COMPTEUR_ORDRE_VIREMENT + @COMPTEUR + 1,  
            COD_REGLEMENT = @COD_REGLEMENT_PREFIXE + CONVERT(VARCHAR,@COMPTEUR),  
            TRAITE = 1  
           WHERE NUM_VIREMENT < 0  
           
           UPDATE @REGLEMENT  
           SET      
             DAT_EDITION = GETDATE()
          WHERE MNT_REGLE_TTC = 0 and MNT_REGLE_HT = 0     
          
          
           SET @NBLIGNES = (SELECT COUNT(*) FROM @REGLEMENT WHERE TRAITE = 1)  
           
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
            DAT_VALID_REGLEMENT,
            ID_UTILISATEUR  
           )  
           SELECT   
            COD_REGLEMENT,  
            DAT_REGLEMENT,  
            NUM_VIREMENT,  
            DAT_EDITION,  
            CAST( MNT_REGLE_TTC AS DECIMAL (18,2) ) ,  
            CAST( MNT_REGLE_HT AS DECIMAL (18,2) ) ,  
            ID_TRANSACTION,  
            1, -- BLN_ACTIF,  
            1, -- BLN_EN_COURS,  
            BLN_CRITERE,  
            NULL, --DAT_VALID_REGLEMENT,
            @ID_UTILISATEUR_REGLEMENT  
           FROM @REGLEMENT  
           WHERE TRAITE = 1  
           SET @NBLIGNES = @@ROWCOUNT  
           
           UPDATE REGLEMENT   
           SET COD_REGLEMENT = ID_REGLEMENT   
           WHERE ID_REGLEMENT = SCOPE_IDENTITY()  
           
           SET @ID_REGLEMENT = SCOPE_IDENTITY()  
           
          END  
          SET ROWCOUNT 0  
          SET NOCOUNT ON  
           
          ---------------------------------------------------------------------------------------------------------------------------  
          --           MAJ de la table POSTE_COUT_REGLE  
          ---------------------------------------------------------------------------------------------------------------------------  
          IF (@ID_TYPE_DESTINATAIRE = 2)  
           BEGIN  
            UPDATE POSTE_COUT_REGLE  
            SET   
             POSTE_COUT_REGLE.ID_REGLEMENT = REGLEMENT.ID_REGLEMENT  
            FROM  
             POSTE_COUT_REGLE  
              INNER JOIN MODULE_PEC   ON MODULE_PEC.ID_MODULE_PEC = POSTE_COUT_REGLE.ID_MODULE_PEC  
              INNER JOIN [TRANSACTION] T ON T.ID_TRANSACTION   = POSTE_COUT_REGLE.ID_TRANSACTION  
              INNER JOIN ETABLISSEMENT_OF  ON ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = T.ID_ETABLISSEMENT_OF_DEST   
              INNER JOIN ACTION_PEC   ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC   
              INNER JOIN SESSION    ON SESSION.ID_SESSION   = POSTE_COUT_REGLE.ID_SESSION   
              INNER JOIN [TRANSACTION] T1 ON T1.NUM_IBAN     = T.NUM_IBAN  
              INNER JOIN REGLEMENT   ON REGLEMENT.ID_TRANSACTION = T1.ID_TRANSACTION 
              AND REGLEMENT.ID_UTILISATEUR = SESSION.ID_UTILISATEUR_REGLEMENT -- 15236
            WHERE   
             REGLEMENT.BLN_ACTIF = 1   
             AND (REGLEMENT.BLN_EN_COURS = 1 OR (REGLEMENT.BLN_EN_COURS = 0 AND REGLEMENT.MNT_REGLE_TTC = 0))  
             AND POSTE_COUT_REGLE.BLN_ACTIF = 1   -- Ne pas modifier les inactifs  
             AND POSTE_COUT_REGLE.DAT_BAP IS NOT NULL  
             AND POSTE_COUT_REGLE.ID_REGLEMENT IS NULL   
             AND SESSION.DAT_PAIEMENT IS NULL   
             AND SESSION.DAT_RECEPTION IS NOT NULL  
             AND T.BLN_ACTIF = 1  
             AND SESSION.ID_SESSION IS NOT NULL --AND SESSION.DAT_PAIEMENT IS NULL AND SESSION.DAT_RECEPTION IS NOT NULL  
             AND POSTE_COUT_REGLE.BLN_FACTURE_DIRECTE = 1  
          AND   
             (  
              @ID_TYPE_BENEFICIAIRE = 1 AND T.ID_ETABLISSEMENT_BENEF = @ID_BENEF  
              OR  
              @ID_TYPE_BENEFICIAIRE = 2 AND T.ID_ETABLISSEMENT_OF_BENEF = @ID_BENEF  
              OR  
              @ID_TYPE_BENEFICIAIRE = 3 AND T.ID_TIERS_BENEF = @ID_BENEF  
             )  
             AND REGLEMENT.ID_REGLEMENT = @ID_REGLEMENT  
           END  
          ELSE  
           BEGIN  
           UPDATE POSTE_COUT_REGLE  
           SET  POSTE_COUT_REGLE.ID_REGLEMENT = REGLEMENT.ID_REGLEMENT  
           FROM POSTE_COUT_REGLE  
           JOIN MODULE_PEC
           ON  MODULE_PEC.ID_MODULE_PEC = POSTE_COUT_REGLE.ID_MODULE_PEC  
           JOIN [TRANSACTION] T
           ON  T.ID_TRANSACTION = POSTE_COUT_REGLE.ID_TRANSACTION 
           JOIN ETABLISSEMENT 
           ON  ETABLISSEMENT.ID_ETABLISSEMENT = POSTE_COUT_REGLE.ID_ETABLISSEMENT  
           JOIN ADHERENT
           ON  ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT  
           JOIN ACTION_PEC  
           ON  MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC   
           JOIN [SESSION]
           ON  [SESSION].ID_SESSION = POSTE_COUT_REGLE.ID_SESSION   
           JOIN [TRANSACTION] T1 
           ON  T1.NUM_IBAN = T.NUM_IBAN
           JOIN REGLEMENT
           ON  REGLEMENT.ID_TRANSACTION = T1.ID_TRANSACTION AND REGLEMENT.ID_UTILISATEUR = SESSION.ID_UTILISATEUR_REGLEMENT -- 15236
           WHERE   REGLEMENT.ID_REGLEMENT = @ID_REGLEMENT  
           AND  REGLEMENT.BLN_ACTIF = 1   
           AND  (REGLEMENT.BLN_EN_COURS = 1 OR (REGLEMENT.BLN_EN_COURS = 0 AND REGLEMENT.MNT_REGLE_TTC = 0))  
           AND  POSTE_COUT_REGLE.BLN_ACTIF = 1   -- Ne pas modifier les inactifs
           AND  POSTE_COUT_REGLE.DAT_BAP IS NOT NULL
           AND  POSTE_COUT_REGLE.ID_REGLEMENT IS NULL
           AND  [SESSION].DAT_PAIEMENT IS NULL
           AND  [SESSION].DAT_RECEPTION IS NOT NULL
           AND  T.BLN_ACTIF = 1
           AND  [SESSION].ID_SESSION IS NOT NULL --AND SESSION.DAT_PAIEMENT IS NULL AND SESSION.DAT_RECEPTION IS NOT NULL  
           AND  POSTE_COUT_REGLE.BLN_FACTURE_DIRECTE = 0  
         
         AND  (  
              @ID_TYPE_BENEFICIAIRE = 1 AND T.ID_ETABLISSEMENT_BENEF = @ID_BENEF
              OR  
              @ID_TYPE_BENEFICIAIRE = 2 AND T.ID_ETABLISSEMENT_OF_BENEF = @ID_BENEF  
              OR  
              @ID_TYPE_BENEFICIAIRE = 3 AND T.ID_TIERS_BENEF = @ID_BENEF  
             )  
           END  
           
         --AMA 13/03/2009 Fermeture des session  
         --  Si la session de ce poste cout r‚gl‚ (PCR.ID_SESSION) est li‚e …   
         --  des rŠglements tous valid‚s (PCR.ID_REGLEMENT et REGLEMENT.DAT_VALID_REGLEMENT not null)   
         --  alors  session.DAT_VALID_REGLEMENT=GetDate().  
           select   
            ses.ID_SESSION as ID_SESSION,  
            SUM(CASE WHEN ses.ID_SESSION is NULL THEN 0 ELSE 1 END) AS NUM_SESSION,  
            SUM(CASE WHEN rgS.DAT_VALID_REGLEMENT is NULL THEN 0 ELSE 1 END)AS NUM_DAT_VALID  
           into #SESSIONS
           from  poste_cout_regle pcrR   
           inner join session ses on ses.ID_SESSION = pcrR.ID_SESSION  
           inner join reglement rgS on rgS.ID_REGLEMENT = pcrR.ID_REGLEMENT  
           group by ses.ID_SESSION  
            
           update SESSION      
            set SESSION.DAT_PAIEMENT = GetDate()       
           from SESSION, #SESSIONS     
           where   
           SESSION.ID_SESSION = #SESSIONS.ID_SESSION    
           AND #SESSIONS.NUM_SESSION = #SESSIONS.NUM_DAT_VALID  
           AND DAT_PAIEMENT IS NULL  
           
         END
         
         ----------------------------------------------  
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         ----------------------------------------------  
         
         CREATE PROCEDURE [dbo].[EDT_LOT_REMISE_BANCAIRE]
         	@IDS_BORDEREAU varchar(500)	-- List of ID_BORDEREAU separated with ',' without spaces - i.e.: 1,2,3,4,5,6
         AS
         BEGIN
         	DECLARE @Item int;
         	DECLARE	@LIB_NOM varchar(35)
         	DECLARE	@LIB_PNM varchar(35)
         	DECLARE	@COD_BORDEREAU int
         	DECLARE	@ID_LOT_REMISE_BANCAIRE int
         	DECLARE	@COD_ADHERENT int
         	DECLARE	@LIB_RAISON_SOCIALE varchar(50)
         	DECLARE	@ID_POSTE_VERSEMENT int
         	DECLARE @ID_VERSEMENT int
         	DECLARE	@LIB_BANQUE varchar(50)
         	DECLARE	@NUM_CHEQUE varchar(10)
         	DECLARE	@DAT_SAISIE datetime
         	DECLARE	@TOTAL_HT decimal(18,2)
         	
         	DECLARE @CHQ_CPT int
         	DECLARE	@OLD_COD_BORDEREAU int
         	DECLARE	@OLD_ID_VERSEMENT int
         	SET @OLD_COD_BORDEREAU = 0
         	SET @OLD_ID_VERSEMENT = 0
         	SET @CHQ_CPT = 0
         	
         	CREATE TABLE #TMP_DATA
         	(
         		LIB_NOM varchar(35),
         		LIB_PNM varchar(35),
         		COD_BORDEREAU int,
         		ID_LOT_REMISE_BANCAIRE int,
         		COD_ADHERENT int,
         		LIB_RAISON_SOCIALE varchar(50),
         		ID_POSTE_VERSEMENT int,
         		LIB_BANQUE varchar(50),
         		NUM_CHEQUE varchar(10),
         		DAT_SAISIE datetime,
         		TOTAL_HT decimal(18,2),
         		NUM_CHEQ int
         	)
         	-- Create a temporary table which contains IDs
         	--DROP TABLE #List
         	CREATE TABLE #List(Item int)
         	DECLARE @Delimiter char
         	SET @Delimiter = ','
         	WHILE CHARINDEX(@Delimiter,@IDS_BORDEREAU,0) <> 0
         	BEGIN
         		SELECT
         			@Item=RTRIM(LTRIM(SUBSTRING(@IDS_BORDEREAU,1,CHARINDEX(@Delimiter,@IDS_BORDEREAU,0)-1))),
         			@IDS_BORDEREAU=RTRIM(LTRIM(SUBSTRING(@IDS_BORDEREAU,CHARINDEX(@Delimiter,@IDS_BORDEREAU,0)+1,LEN(@IDS_BORDEREAU))))
         
         		IF LEN(@Item) > 0
         			INSERT INTO #List SELECT @Item
         	END
         
         	IF LEN(@IDS_BORDEREAU) > 0
         		INSERT INTO #List SELECT @IDS_BORDEREAU -- Put the last item in
         	
         	-- Start the selection
         	DECLARE CURSOR_DATA CURSOR FOR
         	SELECT
         			UTILISATEUR.LIB_NOM,
         			UTILISATEUR.LIB_PNM,
         			BORDEREAU.COD_BORDEREAU,
         			BORDEREAU.ID_LOT_REMISE_BANCAIRE,
         			ADHERENT.COD_ADHERENT,
         			ADHERENT.LIB_RAISON_SOCIALE,
         			POSTE_VERSEMENT.ID_POSTE_VERSEMENT,
         			VERSEMENT.ID_VERSEMENT,
         			VERSEMENT.LIB_BANQUE,
         			VERSEMENT.NUM_CHEQUE,
         			VERSEMENT.DAT_SAISIE,
         			
         			SUM(POSTE_IMPUTATION.MNT_HT) as TOTAL_HT
         	FROM
         			BORDEREAU
         				INNER JOIN UTILISATEUR		ON BORDEREAU.ID_UTILISATEUR = UTILISATEUR.ID_UTILISATEUR
         				INNER JOIN VERSEMENT		ON VERSEMENT.ID_BORDEREAU = BORDEREAU.ID_BORDEREAU
         				INNER JOIN POSTE_VERSEMENT	ON POSTE_VERSEMENT.ID_VERSEMENT = VERSEMENT.ID_VERSEMENT
         				INNER JOIN ADHERENT			ON POSTE_VERSEMENT.ID_ADHERENT_BENEFICIAIRE = ADHERENT.ID_ADHERENT
         				INNER JOIN POSTE_IMPUTATION	ON POSTE_IMPUTATION.ID_POSTE_VERSEMENT = POSTE_VERSEMENT.ID_POSTE_VERSEMENT
         	WHERE
         			BORDEREAU.ID_BORDEREAU in (select Item from #List)
         	GROUP BY
         			UTILISATEUR.LIB_NOM,
         			UTILISATEUR.LIB_PNM,
         			BORDEREAU.COD_BORDEREAU,
         			BORDEREAU.ID_LOT_REMISE_BANCAIRE,
         
         			ADHERENT.COD_ADHERENT,
         			ADHERENT.LIB_RAISON_SOCIALE,
         			VERSEMENT.ID_VERSEMENT,
         			POSTE_VERSEMENT.ID_POSTE_VERSEMENT,
         			VERSEMENT.LIB_BANQUE,
         			VERSEMENT.NUM_CHEQUE,
         			VERSEMENT.DAT_SAISIE
         	ORDER BY
         			VERSEMENT.ID_VERSEMENT,
         			POSTE_VERSEMENT.ID_POSTE_VERSEMENT,
         			ADHERENT.COD_ADHERENT
         	
         
         	
         	-- Parcours des activit?s concern?es pour trouver le montant d?
         	OPEN CURSOR_DATA
         	FETCH NEXT FROM CURSOR_DATA INTO @LIB_NOM, @LIB_PNM, @COD_BORDEREAU, @ID_LOT_REMISE_BANCAIRE, @COD_ADHERENT, @LIB_RAISON_SOCIALE, @ID_POSTE_VERSEMENT, @ID_VERSEMENT, @LIB_BANQUE, @NUM_CHEQUE, @DAT_SAISIE, @TOTAL_HT
         
         	WHILE @@FETCH_STATUS = 0
         	BEGIN
         		IF (@COD_BORDEREAU <> @OLD_COD_BORDEREAU)
         			BEGIN
         				SET @CHQ_CPT = 0
         				SET @OLD_COD_BORDEREAU = @COD_BORDEREAU
         			END
         		
         		IF (@ID_VERSEMENT <> @OLD_ID_VERSEMENT)
         			SET @CHQ_CPT = @CHQ_CPT + 1
         		
         		INSERT INTO #TMP_DATA VALUES
         		(
         			@LIB_NOM,
         			@LIB_PNM,
         			@COD_BORDEREAU,
         			@ID_LOT_REMISE_BANCAIRE,
         			@COD_ADHERENT,
         			@LIB_RAISON_SOCIALE,
         			@ID_POSTE_VERSEMENT, 
         			@LIB_BANQUE,
         			@NUM_CHEQUE,
         			@DAT_SAISIE,
         			@TOTAL_HT,
         			@CHQ_CPT
         		)
         		
         		FETCH NEXT FROM CURSOR_DATA INTO @LIB_NOM, @LIB_PNM, @COD_BORDEREAU, @ID_LOT_REMISE_BANCAIRE, @COD_ADHERENT, @LIB_RAISON_SOCIALE, @ID_POSTE_VERSEMENT, @ID_VERSEMENT, @LIB_BANQUE, @NUM_CHEQUE, @DAT_SAISIE, @TOTAL_HT
         	END
         	CLOSE CURSOR_DATA
         	DEALLOCATE CURSOR_DATA
         	
         	SELECT * FROM #TMP_DATA
         END
         
         
         
         
         -- =============================================
         -- Author:		APA
         -- Create date: 10/02/2012
         -- Description:	Sous types de cout pour edition pour  FICHE_DOSSIER 
         -- =============================================
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         -- =============================================
         
         CREATE PROCEDURE [dbo].[LEC_GRP_SOUS_TYPE_COUT_DEMANDE]
            @ID_MODULE int
         AS
         BEGIN
         	CREATE TABLE #TEMPLIST
         	(
         		ID_SOUS_TYPE_COUT INT,
         		MNT_REGLE DECIMAL(18,2)
         	)
         	
         	CREATE TABLE #TEMP_MNT_CHIFFRE
         	(
         		MNT_CHIFFRE DECIMAL(18,2),
         		ID_POSTE_COUT_ENGAGE INT
         	)
         	
         	INSERT INTO #TEMPLIST (ID_SOUS_TYPE_COUT, MNT_REGLE)
         	SELECT SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT,
         	SUM (case 
         			when REGLEMENT.DAT_VALID_REGLEMENT is null then 0
         			else POSTE_COUT_REGLE.MNT_REGLE_HT
         		end)
         	FROM SOUS_TYPE_COUT
         	LEFT JOIN POSTE_COUT_REGLE ON POSTE_COUT_REGLE.ID_SOUS_TYPE_COUT = SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT
         	AND POSTE_COUT_REGLE.ID_MODULE_PEC = @ID_MODULE
         	AND POSTE_COUT_REGLE.BLN_ACTIF = 1
         	LEFT JOIN REGLEMENT ON REGLEMENT.ID_REGLEMENT = POSTE_COUT_REGLE.ID_REGLEMENT
         	AND REGLEMENT.DAT_VALID_REGLEMENT IS NOT NULL
         	GROUP BY SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT
         
         	INSERT INTO #TEMP_MNT_CHIFFRE
         	SELECT sum(PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US) AS MNT_CHIFFRE,
         		PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE
         	from PLAN_FINANCEMENT_US
         	left join	POSTE_COUT_ENGAGE 
         	on POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE = PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE
         	AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL
         	where POSTE_COUT_ENGAGE.ID_MODULE_PEC = @ID_MODULE
         	group by PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE
         
         	SELECT
         		SOUS_TYPE_COUT.LIBC_SOUS_TYPE_COUT		AS SOUS_TYPE_COUT,
         		POSTE_COUT_ENGAGE.MNT_PREVISIONNEL_HT	AS MNT_DEMANDE
         
         	FROM  SOUS_TYPE_COUT
         		LEFT JOIN POSTE_COUT_ENGAGE ON SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT = POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT
         		AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL
         		LEFT JOIN #TEMPLIST ON #TEMPLIST.ID_SOUS_TYPE_COUT = SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT
         		LEFT JOIN ENGAGEMENT ON ENGAGEMENT.ID_ENGAGEMENT = POSTE_COUT_ENGAGE.ID_ENGAGEMENT
         		LEFT JOIN #TEMP_MNT_CHIFFRE ON #TEMP_MNT_CHIFFRE.ID_POSTE_COUT_ENGAGE = POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE
         	WHERE ID_MODULE_PEC = @ID_MODULE
         	AND (POSTE_COUT_ENGAGE.MNT_PREVISIONNEL_HT > 0)
         			
         	ORDER BY SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT
         END
      
         
         
         --EXEC [LEC_DET_DOSSIERS_CRITERES] 
         --
         --	@ID_AGENCE = NULL,
         --	@ID_ADHERENT = NULL, 
         --	@ID_OF = NULL,
         --	@ID_SOUS_TYPE_COUT = NULL,
         --	@MNT_TTC = NULL,
         --	@BLN_INFERIEUR = 0,
         --	@NOUVEAUX_OF = 0,
         --	@NOMBRE_DOSSIERS = 0
         
         -- =============================================
         -- Author		: XX
         -- Create date	: XX XXXX 2007
         -- Description	: Cr‚ation
         -- =============================================
         -- Author		: SV
         -- Create date	: 27 ao–t 2007
         -- Description	: Modification de la gestion du montant minimum car il ne prennait pas en compte tous les montants par d‚faut (seulement les > 0)
         --				  Modification de la gestion des adh‚rents car la jointure dans la s‚lection principale retournait un nombre major‚ par les participants
         -- ============================================= 
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         -- =============================================
         
         CREATE PROCEDURE [dbo].[LEC_DET_DOSSIERS_CRITERES] 
         	@ID_AGENCE int,
         	@ID_ADHERENT int,
         	@ID_OF int,
         	@ID_SOUS_TYPE_COUT int,
         	@MNT_TTC decimal(18,2),
         	@BLN_INFERIEUR bit,
         	@NOUVEAUX_OF bit,
         	@NOMBRE_DOSSIERS INT OUTPUT
         
         AS
         BEGIN
         
         	IF @MNT_TTC IS NULL
         		SET @MNT_TTC = 0
         
         --	IF @BLN_INFERIEUR IS NULL			-- Sinon, le cas des avoirs ne pourra ˆtre pris en compte
         --		SET @BLN_INFERIEUR = 0
         
         	IF @NOUVEAUX_OF IS NULL
         		SET @NOUVEAUX_OF = 0
         	
         	SELECT
         	--	AGENCE.ID_AGENCE AS ID_AGENCE,
         	--	ADHERENT.ID_ADHERENT AS ID_ADHERENT,
         	--	ORGANISME_FORMATION.ID_OF AS ID_OF,
         	--	SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT AS ID_SOUS_TYPE_COUT,
         	--	POSTE_COUT_REGLE.MNT_REGLE_TTC AS MNT_TTC,
         	--	COUNT(POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE)
         		@NOMBRE_DOSSIERS = COUNT(POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE)
         	FROM 
         		POSTE_COUT_REGLE
         			INNER JOIN SESSION					ON SESSION.ID_SESSION = POSTE_COUT_REGLE.ID_SESSION
         													AND SESSION.DAT_RECEPTION IS NOT NULL
         													AND SESSION.DAT_PAIEMENT IS NULL
         			INNER JOIN AGENCE					ON AGENCE.ID_AGENCE = SESSION.ID_AGENCE
         			INNER JOIN MODULE_PEC				ON MODULE_PEC.ID_MODULE_PEC = POSTE_COUT_REGLE.ID_MODULE_PEC
         	--		INNER JOIN NR140					ON NR140.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
         	--		INNER JOIN ETABLISSEMENT			ON ETABLISSEMENT.ID_ETABLISSEMENT = NR140.ID_ETABLISSEMENT
         	--		INNER JOIN ADHERENT					ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         			LEFT OUTER JOIN ETABLISSEMENT_OF	ON ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = MODULE_PEC.ID_ETABLISSEMENT_OF
         			LEFT OUTER JOIN ORGANISME_FORMATION	ON ORGANISME_FORMATION.ID_OF = ETABLISSEMENT_OF.ID_OF
         			INNER JOIN SOUS_TYPE_COUT			ON SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT = POSTE_COUT_REGLE.ID_SOUS_TYPE_COUT
         			LEFT JOIN 
         				(SELECT EO.ID_OF, COUNT(*) AS COUNTER
         					FROM POSTE_COUT_REGLE PCR
         					INNER JOIN MODULE_PEC M ON M.ID_MODULE_PEC = PCR.ID_MODULE_PEC
         					INNER JOIN REGLEMENT R ON R.ID_REGLEMENT = PCR.ID_REGLEMENT
         						AND R.DAT_VALID_REGLEMENT IS NOT NULL
         					INNER JOIN ETABLISSEMENT_OF EO ON EO.ID_ETABLISSEMENT_OF = M.ID_ETABLISSEMENT_OF
         					GROUP BY EO.ID_OF
         				) AS PCR_NEW ON PCR_NEW.ID_OF = ETABLISSEMENT_OF.ID_OF
         						
         	WHERE
         		POSTE_COUT_REGLE.BLN_ACTIF = 1
         		AND (@ID_AGENCE IS NULL OR AGENCE.ID_AGENCE = @ID_AGENCE)
         		AND
         		(
         			@ID_ADHERENT IS NULL
         			OR	-- On v‚rifie que l'adh‚rent participe … au moins une action
         			(
         				(
         					select
         						count(*)
         					from
         						NR140
         							INNER JOIN ETABLISSEMENT	ON ETABLISSEMENT.ID_ETABLISSEMENT = NR140.ID_ETABLISSEMENT
         							INNER JOIN ADHERENT			ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         					where
         						NR140.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
         						AND ADHERENT.ID_ADHERENT = @ID_ADHERENT
         				) > 0
         			) 
         		)
         		AND (@ID_SOUS_TYPE_COUT IS NULL OR SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT = @ID_SOUS_TYPE_COUT) 
         		AND
         		(
         			(@BLN_INFERIEUR IS NULL)
         			OR
         			(
         				(@BLN_INFERIEUR = 0 AND POSTE_COUT_REGLE.MNT_REGLE_TTC >= @MNT_TTC)
         				OR (@BLN_INFERIEUR = 1 AND POSTE_COUT_REGLE.MNT_REGLE_TTC < @MNT_TTC)
         			)
         		) 
         		AND
         		(
         			(
         				@NOUVEAUX_OF = 0
         				AND (ORGANISME_FORMATION.ID_OF = @ID_OF OR @ID_OF IS NULL)
         			)
         			OR
         			(
         				@NOUVEAUX_OF = 1
         				AND (DATEADD(MM, 3, ORGANISME_FORMATION.DAT_CREATION) >= GETDATE()	
         				OR PCR_NEW.COUNTER < 10)
         			)
         		)
         
         --	GROUP BY
         --		AGENCE.ID_AGENCE,
         --		ADHERENT.ID_ADHERENT,
         --		ORGANISME_FORMATION.ID_OF,
         --		SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT,
         --		POSTE_COUT_REGLE.MNT_REGLE_TTC
         --
         --  PRINT @NOMBRE_DOSSIERS
         
         END
         
         
 CREATE PROCEDURE [dbo].[UPD_R21_FOR_SYNCHRO]
          @ID_ADHERENT int
         AS
         --================================================
         -- DSZ 18/02/2011 12639
         -- refection de la procedure : with au lieu de la table temporaire,
         -- et suppresion du curseur
         --================================================
         -- DSZ 10/11/2015 #1169 ajout masse salariale CDD
         --================================================
         -- DSZ 22/02/2016 #1322 ajout cr‚ation de la ligne si n'existe pas
         --================================================
         BEGIN
         DECLARE @ADHERENT_R21bis TABLE (NUM_EFFECTIF int, MASSE_SALARIALE_REELLE float, MASSE_SALARIALE_CDD int, ID_PERIODE int);
         
         INSERT INTO @ADHERENT_R21bis
          (NUM_EFFECTIF, MASSE_SALARIALE_REELLE , MASSE_SALARIALE_CDD , ID_PERIODE)
          select
            SUM(R21_BIS.NUM_EFFECTIF) AS NUM_EFFECTIF,
            SUM(R21_BIS.MASSE_SALARIALE_REELLE) AS MASSE_SALARIALE_REELLE,
            SUM(R21_BIS.MASSE_SALARIALE_CDD) as MASSE_SALARIALE_CDD,
            R21_BIS.ID_PERIODE
          from ETABLISSEMENT
            INNER JOIN R21_BIS ON ETABLISSEMENT.ID_ETABLISSEMENT = R21_BIS.ID_ETABLISSEMENT
          where  ETABLISSEMENT.ID_ADHERENT = @ID_ADHERENT
          GROUP BY R21_BIS.ID_PERIODE
         
         INSERT INTO R21
                    ([ID_ADHERENT]
                    ,[ID_PERIODE]
                    ,[MASSE_SALARIALE_REELLE]
                    ,[NUM_EFFECTIF]
                    ,[MASSE_SALARIALE_CDD])
         select
          @ID_ADHERENT,
          t.ID_PERIODE,
          T.MASSE_SALARIALE_REELLE,
          T.NUM_EFFECTIF,
          T.MASSE_SALARIALE_CDD
         from 
          @ADHERENT_R21bis T
          LEFT JOIN R21 ON R21.ID_ADHERENT = @ID_ADHERENT AND R21.ID_PERIODE =  T.ID_PERIODE
         where R21.ID_PERIODE IS NULL
         
          
         
         update R21 
         set 
          NUM_EFFECTIF = T.NUM_EFFECTIF ,
          MASSE_SALARIALE_REELLE = T.MASSE_SALARIALE_REELLE ,
          MASSE_SALARIALE_CDD = T.MASSE_SALARIALE_CDD  
         from R21
          inner join @ADHERENT_R21bis T on (R21.ID_PERIODE = T.ID_PERIODE)
         where 
          R21.ID_ADHERENT = @ID_ADHERENT
         END

 CREATE PROCEDURE [dbo].[COPIE_ACTION_PEC]  
          @ID_ACTION_PEC_SOURCE INT,
          @ID_UTILISATEUR INT
         AS
         -- ================================================================
         -- ARI + HBO : #907 : Ajout du dossier PEC dans la table EXPORT_D3R
         -- ================================================================
         BEGIN
          DECLARE @ID_ACTION_PEC INT
          SET @ID_ACTION_PEC = 0
          DECLARE @ANNEE_ACTION_PEC INT
          set @ANNEE_ACTION_PEC = datepart(yyyy, getdate())
         
          --lecture p‚riode en se basant sur @ANNEE_ACTION_PEC
          declare @ID_PERIODE int
          select @ID_PERIODE = ID_PERIODE from PERIODE
          where ID_TYPE_PERIODE = 1
          and PERIODE.BLN_ACTIF = 1
          and PERIODE.NUM_ANNEE =  @ANNEE_ACTION_PEC
         
         
          --create COD_ACTION_PEC
          DECLARE @COD_ACTION_PEC INT
          EXEC @COD_ACTION_PEC = ACTION_COMPTEUR 
            @ACTION_ANNEE = @ANNEE_ACTION_PEC
         
          INSERT INTO ACTION_PEC
               ([COD_ACTION_PEC]
           ,[LIBL_ACTION_PEC]
           ,[DAT_DEB_ACTION_PEC]
           ,[DAT_FIN_ACTION_PEC]
           ,[NUM_DUREE_JOUR]
           ,[NUM_DUREE_HEURE]
           ,[BLN_ACTIF]
           ,[DAT_CREATION]
           ,[DAT_MODIF]
           ,[CIBLE_ACTION]
           ,[ID_SANCTION]
           ,[ID_NIVEAU]
           ,[ID_OPERATION_FINANCIERE] 
           ,[ID_THEME]
           ,[ID_UTILISATEUR] 
           ,[ID_AGENCE]
           ,[BLN_CYCLE_COURT]
           ,[ID_FORMACODE]
           ,[ANNEE_ACTION_PEC]
           ,[DAT_RECU]
           ,[ID_DECISION_ACTION_PEC]
           ,[AXE]
           ,[DOMAINE]
           ,[BLN_ACCORD]
           ,[ID_UTILISATEUR_CREATEUR]
           ,[ID_CHARGEE_MISSION]
           ,[BLN_REPRISE_ADHOC])
         
          SELECT    @COD_ACTION_PEC as COD_ACTION_PEC --[cod action pec] initialis‚ comme dans insert
           ,[LIBL_ACTION_PEC]
           ,[DAT_DEB_ACTION_PEC]
           ,[DAT_FIN_ACTION_PEC]
           ,[NUM_DUREE_JOUR]
           ,[NUM_DUREE_HEURE]
           , 1 --[BLN_ACTIF], La nouvelle action doit toujours ˆtre active 
           ,GETDATE()
           ,GETDATE()
           ,[CIBLE_ACTION]
           ,[ID_SANCTION]
           ,[ID_NIVEAU]
           ,[ID_OPERATION_FINANCIERE] 
           ,[ID_THEME]
           ,@ID_UTILISATEUR
           ,[ID_AGENCE]
           ,[BLN_CYCLE_COURT]
           ,[ID_FORMACODE]
           ,@ANNEE_ACTION_PEC
           ,[DAT_RECU]
           ,null -- [ID_DECISION_ACTION_PEC] La nouvelle action ne doit pas ˆtre refus‚e
           ,[AXE]
           ,[DOMAINE]
           ,1 --[BLN_ACCORD]La nouvelle action ne doit pas ˆtre refus‚e
           ,@ID_UTILISATEUR
           ,[ID_CHARGEE_MISSION]
           ,0
            FROM ACTION_PEC
           WHERE ID_ACTION_PEC = @ID_ACTION_PEC_SOURCE
         
          set @ID_ACTION_PEC = @@IDENTITY
          DECLARE @CODE_ACTION varchar(11)
          set @CODE_ACTION = dbo.GetActionPECCode( @COD_ACTION_PEC,@ANNEE_ACTION_PEC)
          
           EXEC dbo.INS_EXPORT_D3R
            @ID_ACTION_PEC,
            'PEC'
         
          INSERT INTO [MODULE_PEC]
               ([COD_MODULE_PEC]
            ,[DAT_DEBUT]
            ,[DAT_FIN]
            ,[NUM_DUREE_JOUR]
            ,[NUM_DUREE_HEURE]
            ,[BLN_ACTIF]
            ,[DAT_CREATION]
            ,[DAT_MODIF]
            ,[BLN_IMPUTABLE]
            --13527--,[MNT_CONVENTION]
            ,[BLN_EXTERNE]
            ,[ID_ACTION_PEC]
            ,[ID_PERIODE]
            ,[ID_THEME]
            ,[ID_UTILISATEUR]
            ,[ID_ETABLISSEMENT_OF]
            ,[ID_STAGE]
            ,[ID_FORMACODE]
            ,[LIBL_MODULE_PEC]
            ,[NUM_INTERNE]
            ,[ID_DEPARTEMENT]
            ,[BLN_OK_PIECE]
            ,[BLN_DELEGATION_PAIEMENT]
            ,[BLN_INTRA]
            ,[AXE_MODULE]
            ,[DOMAINE_MODULE]
            ,[ID_MODALITE_FORMATION]
            ,[ID_UTILISATEUR_CREATEUR]
            ,[ID_DISPOSITIF_PAR_DEFAUT]
            ,[ID_CRITERE_CHIFFRAGE]
            ,[BLN_CATALOGUE])
               
          SELECT
          right('000000' 
           + CONVERT(VARCHAR(8), @COD_ACTION_PEC)+ ' ' 
           + CONVERT(VARCHAR(2), SUBSTRING([COD_MODULE_PEC],8,2)),9) + '/' 
           + CONVERT(VARCHAR(4),@ANNEE_ACTION_PEC)
             as [COD_MODULE_PEC]
             ,[DAT_DEBUT]
             ,[DAT_FIN]
             ,[NUM_DUREE_JOUR]
             ,[NUM_DUREE_HEURE]
             ,[BLN_ACTIF]
             ,getdate()
             ,getdate()
             ,[BLN_IMPUTABLE]
             --13527--,[MNT_CONVENTION]
             ,[BLN_EXTERNE]
             ,@ID_ACTION_PEC
             ,@ID_PERIODE
             ,[ID_THEME]
             ,@ID_UTILISATEUR
             ,[ID_ETABLISSEMENT_OF]
             ,[ID_STAGE]
             ,[ID_FORMACODE]
             ,[LIBL_MODULE_PEC]
             ,CASE [NUM_INTERNE]
             WHEN null THEN null
             WHEN '' THEN null
             ELSE [NUM_INTERNE] + '_copie'
             END
             ,[ID_DEPARTEMENT]
             ,1
             ,[BLN_DELEGATION_PAIEMENT]
             ,[BLN_INTRA]
             ,[AXE_MODULE]
             ,[DOMAINE_MODULE]
             ,[ID_MODALITE_FORMATION]
             ,@ID_UTILISATEUR
             ,[ID_DISPOSITIF_PAR_DEFAUT]
             ,[ID_CRITERE_CHIFFRAGE]
             ,[BLN_CATALOGUE]
           FROM MODULE_PEC
          WHERE ID_ACTION_PEC = @ID_ACTION_PEC_SOURCE
          order by [COD_MODULE_PEC]
         
          INSERT INTO [NR140]
          (
              [ID_ACTION_PEC]
             ,[ID_ETABLISSEMENT]
             ,[NUM_INTERNE]
          )
          SELECT @ID_ACTION_PEC
             ,[ID_ETABLISSEMENT]
             ,CASE [NUM_INTERNE]
             WHEN null THEN null
             WHEN '' THEN null
             ELSE [NUM_INTERNE] + '_copie'
             END
            FROM [NR140]
            WHERE ID_ACTION_PEC = @ID_ACTION_PEC_SOURCE
         
          select @COD_ACTION_PEC
         END

		          
create procedure CKParser.TheEnd 
as 
begin         
	print 'Everything worked';
end