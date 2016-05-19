         -- =============================================    
         -- Author  : XXX    
         -- Create date : XX XXX XXX    
         -- comment  : # lecture groupe pour les sous type de cout    
         -- ---------------------------------------------    
         -- Author  : Say    
         -- Modified date: 30 juillet 2007    
         -- comment  : # ajout du montant "MNT chiffrÇ¸ "    
         -- ---------------------------------------------    
         -- Author  : Say    
         -- Modified date: 06 aoÇ¯t 2007    
         -- comment  : # montant "MNT chiffrÇ¸ " devient "MNT reste Çÿ engager"    
         -- ---------------------------------------------    
         -- Author  : Say    
         -- Modified date: 08 aoÇ¯t 2007    
         -- comment  : # montant "MNT reste Çÿ engager" redevient "MNT chiffrÇ¸ "    
         -- ---------------------------------------------    
         -- Author  : Say    
         -- Modified date: 08 aoÇ¯t 2007    
         -- comment  : # les mnt des postes de coÇ¯t rÇ¸glÇ¸s ne doivent Ç¦tre affichÇ¸s     
         --     que si les postes de coÇ¯t sont associÇ¸s Çÿ un rÇùglement validÇ¸.    
         -- ---------------------------------------------    
         -- Author  : DSZ    
         -- Modified date: 18/04/2008    
         -- comment  : Modifie calcul MNT_CHIFFRE pour prendre en compte PLAN_FNANCEMENT_US    
         -- ---------------------------------------------    
         -- Author  : DSZ    
         -- Modified date: 24/04/2008    
         -- comment  : "MNT_ENGAGE_HT > 0" remplace par "MNT_PREVISIONNEL_HT >0"    
         --    a ce niveau il y a juste le montant demande qui est > 0    
         -- ---------------------------------------------    
         -- Author  : AMA    
         -- Modified date: 22/09/2008    
         -- comment  : j'ai supprimÇ¸ le filtre sur "MNT_ENGAGE_HT > 0"     
         --      Contrairement Çÿ ce qu'on peut penser, il peut exister des    
         --      montants nÇ¸gatifs Çÿ ce niveau (notamment au niveau de la    
         --      reprise et les montants calculÇ¸s deviennent automatiquement    
         --      faux (problÇùme d'arroni signalÇ¸ dans MANTIS 10680    
         -- =============================================     
         -- 08/12/2009 SAFI ANO ID 12251    
         -- Correction calcul de MNT_CHIFFRE, Filtre sur PCE non dÇ¸sengagÇ¸     
         -- Correction trasmis par SBR    
         -- ---------------------------------------------    
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT    
         ----------------------------------------------      
         --DSZ 14/03/2013 : US 135 PB_ventilation devient BLN_OK_FINANCEMENT    
         ----------------------------------------------      
         -- MB Modif du 21/08/2014 : Les PCE avec montant demand‚ valoris‚ … 0 sont pr‚sent‚s dans l'‚cran de Chiffrage
         ----------------------------------------------      
             
             
         CREATE PROCEDURE [dbo].[LEC_GRP_SOUS_TYPE_COUT_CHIFFRAGE]    
            @ID_MODULE int    
         AS    
         BEGIN    
             
          CREATE TABLE #TEMPLIST    
          (    
           ID_SOUS_TYPE_COUT INT,    
           MNT_REGLE DECIMAL(18,2),    
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
             
          select sum(PLAN_FINANCEMENT_US.MNT_PLAN_FINANCEMENT_US) AS MNT_CHIFFRE,    
           PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE    
          into #TEMP_MNT_CHIFFRE    
          from PLAN_FINANCEMENT_US    
          left join POSTE_COUT_ENGAGE on POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE = PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE    
          --SAFI AJOUTER 08/12/2009      
             AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL    
          --END SAFI 08/12/2009    
                
          where POSTE_COUT_ENGAGE.ID_MODULE_PEC = @ID_MODULE    
          group by PLAN_FINANCEMENT_US.ID_POSTE_COUT_ENGAGE    
             
          Select    
           POSTE_COUT_ENGAGE.ID_MODULE_PEC   AS ID_MODULE,    
           SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT  AS ID_SOUS_TYPE_COUT,    
           SOUS_TYPE_COUT.LIBC_SOUS_TYPE_COUT  AS SOUS_TYPE_COUT,    
           POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE AS ID_POSTE_COUT_ENGAGE,    
             
           CASE    
            WHEN (DAT_DESENGAGEMENT is NULL  AND (ID_TYPE_ENGAGEMENT is null OR ID_TYPE_ENGAGEMENT <> 2) AND     
              ENGAGEMENT.DAT_BAE is NOT NULL) THEN POSTE_COUT_ENGAGE.MNT_ENGAGE_HT    
            ELSE 0    
           END          AS MNT_ENGAGE,    
         --  CASE    
         --   WHEN (DAT_DESENGAGEMENT is NULL  AND (ID_TYPE_ENGAGEMENT is null OR ID_TYPE_ENGAGEMENT <> 2))/* AND     
         --     (ENGAGEMENT.DAT_BAE is NULL OR POSTE_COUT_ENGAGE.ID_ENGAGEMENT IS NULL)) */    
         --             THEN POSTE_COUT_ENGAGE.MNT_ENGAGE_HT    
         --   ELSE 0    
         --  END          AS MNT_CHIFFRE,    
           #TEMP_MNT_CHIFFRE.MNT_CHIFFRE   AS MNT_CHIFFRE,    
             
           POSTE_COUT_ENGAGE.MNT_PREVISIONNEL_HT AS MNT_PREVISIONNEL,    
           CASE     
            WHEN POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NOT NULL THEN 0     
            ELSE 1     
           END          AS MODIFIABLE_MNT,    
           #TEMPLIST.MNT_REGLE      AS MNT_REGLE,    
           POSTE_COUT_ENGAGE.BLN_OK_FINANCEMENT ,    
           ENGAGEMENT.DAT_BAE    
               
          FROM  SOUS_TYPE_COUT    
           LEFT JOIN POSTE_COUT_ENGAGE ON SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT = POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT    
            --AMA 22/12/2008 (voir le commentaire au dÇ¸but)     
            --AND POSTE_COUT_ENGAGE.MNT_ENGAGE_HT > 0     
                
            AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT IS NULL    
           LEFT JOIN #TEMPLIST ON #TEMPLIST.ID_SOUS_TYPE_COUT = SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT    
           LEFT JOIN ENGAGEMENT ON ENGAGEMENT.ID_ENGAGEMENT = POSTE_COUT_ENGAGE.ID_ENGAGEMENT    
           LEFT JOIN #TEMP_MNT_CHIFFRE ON #TEMP_MNT_CHIFFRE.ID_POSTE_COUT_ENGAGE = POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE    
          WHERE ID_MODULE_PEC = @ID_MODULE    
         -- MB Modif du 21/08/2014 : Les PCE avec montant demand‚ valoris‚ … 0 sont pr‚sent‚s dans l'‚cran de Chiffrage
         --AND (POSTE_COUT_ENGAGE.MNT_PREVISIONNEL_HT <> 0  OR MNT_CHIFFRE <> 0)    
         AND (POSTE_COUT_ENGAGE.MNT_PREVISIONNEL_HT >= 0  OR MNT_CHIFFRE <> 0)    
               
               
          ORDER BY SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT    
             
         END  

----------------------------------------------
-- Auteur		: MBL
-- Date			: 15/10/2013
-- Description	: Generation de la LOG associe a un traitement d'import EDI
--				  Les tables EDI_PEC_ST et EDI_LOG auront ‚t‚ pr‚alablement aliment‚es
--     			  et un lot EDI aura ‚t‚ g‚n‚r‚
-- ParamŠtres	: @id_lot_import = identifiant du lot d'import EDI

----------------------------------------------
CREATE PROCEDURE [dbo].[EDI_GENERATION_LOG_EDI]
	@ID_LOT_IMPORT				int,
	@ID_EDI_GROUPE_EDI_PEC		int
	
	
AS
BEGIN
         	DECLARE @cmd									VARCHAR(2000),
         			@COD_TABLE_TAMPON						VARCHAR(50),
         			@nom_fichier_chemin						VARCHAR(100),
         			@MAIL_DESTINATAIRE_PRINCIPAL_LOG_EDI	VARCHAR(500),
         			@MAIL_DESTINATAIRE_CC_LOG_EDI			VARCHAR(500),
         			@BLN_DEBUG								TINYINT
         
         	SET @BLN_DEBUG = 1
         	SET NOCOUNT ON
         
         	if exists (select 1 from sys.objects where type = 'U' and name = 'TMP02_EDI')
         	BEGIN
         		DROP TABLE TMP02_EDI
         	END
         
         	CREATE TABLE TMP02_EDI
         	(data varchar(8000))
         
         
         
         	-- Recherche du lot d'import associe au fichier traite
         	SELECT		@COD_TABLE_TAMPON	= EDI_IMPORT_TABLE.COD_TABLE_TAMPON,
         				@nom_fichier_chemin	= NOM_FICHIER
         	FROM		EDI_LOT_IMPORT
         	INNER JOIN	EDI_IMPORT_TABLE ON EDI_IMPORT_TABLE.ID_TABLE = EDI_LOT_IMPORT.ID_TABLE
         	WHERE		EDI_LOT_IMPORT.ID_LOT_IMPORT = @ID_LOT_IMPORT 
         
         
         	-- Retour pour g‚n‚ration du fichier de log
         	declare @nb_enr_file varchar(10), 
         			@nb_enr_ok varchar(10), 
         			@nb_enr_ko varchar(10)
         
         	IF @COD_TABLE_TAMPON = 'EDI_PEC_ST'
         	BEGIN
         		-- nombre d'enregistrements dans le fichier
         		select	@nb_enr_file = cast(COUNT(*) as varchar)
         		from	EDI_TAMPON_PEC
         
         		-- nombre d'enregistrements trait‚s avec succ‚s
         		select	@nb_enr_ok = cast(COUNT(*) as varchar)
         		from	EDI_PEC_ST
         		where	ID_LOT_IMPORT = @id_lot_import
         		and		BLN_REJET = 0
         	END
         
         	IF @COD_TABLE_TAMPON = 'EDI_PAR_ST'
         	BEGIN
         
         		-- nombre d'enregistrements dans le fichier
         		select	@nb_enr_file = cast(COUNT(*) as varchar)
         		from	EDI_TAMPON_PAR_ST
         
         		-- nombre d'enregistrements trait‚s avec succ‚s
         		select	@nb_enr_ok = cast(COUNT(*) as varchar)
         		from	EDI_PAR_ST
         		where	ID_LOT_IMPORT = @id_lot_import
         		and		BLN_REJET = 0
         	END
         
         	-- nombre d'enregistrements rejet‚s
         	select	@nb_enr_ko = cast(COUNT(distinct NUM_LIGNE) as varchar)
         	from	EDI_LOG
         	where	ID_LOT_IMPORT = @id_lot_import
         
         	-- lignes du fichier de log
         	INSERT INTO TMP02_EDI
         	select	T.LIB
         	from	(
         			select	-4 as NUM_LIGNE, convert(varchar, getdate(), 103) + ' - Traitement du fichier ' + @nom_fichier_chemin as LIB
         			UNION
         			SELECT -3 as NUM_LIGNE, @nb_enr_file + ' lignes d‚tect‚es dans le fichier' 
         			UNION
         			SELECT -2 as NUM_LIGNE, @nb_enr_ok + ' lignes trait‚es avec succ‚s'
         			UNION
         			SELECT -1 as NUM_LIGNE, @nb_enr_ko + ' lignes rejet‚es' 
         			UNION
         			SELECT 0 as NUM_LIGNE, '------------------------------------------------' 
         
         			UNION
         
         			select	t1.NUM_LIGNE, ('Ligne '+cast(t1.NUM_LIGNE as varchar) + ' - ' + t1.LIB_PROBLEME) AS LIB
         			from	EDI_LOG t1
         			where	t1.ID_LOT_IMPORT = @id_lot_import
         			) T
         
         	order by T.NUM_LIGNE
         
         	SELECT *  FROM TMP02_EDI
         
         	SELECT 	@MAIL_DESTINATAIRE_PRINCIPAL_LOG_EDI = MAIL_DESTINATAIRE_PRINCIPAL_LOG_EDI,
         			@MAIL_DESTINATAIRE_CC_LOG_EDI		 = MAIL_DESTINATAIRE_CC_LOG_EDI		
         	FROM	EDI_GROUPE_EDI_PEC
         	WHERE	ID_EDI_GROUPE_EDI_PEC = @ID_EDI_GROUPE_EDI_PEC
         
         	IF @BLN_DEBUG = 1
         	BEGIN
         		SELECT 	@MAIL_DESTINATAIRE_PRINCIPAL_LOG_EDI = 'miguel.blanco@sisyphe-tech.com',
         				@MAIL_DESTINATAIRE_CC_LOG_EDI		 = 'miguel.blanco@sisyphe-tech.com'		
         	END
         
         	-- ENVOI DES MAILS DE COMPTE RENDU AUX DESTINATTAIRES
         	IF LEN(ISNULL(@MAIL_DESTINATAIRE_PRINCIPAL_LOG_EDI, '')) > 0
         	OR LEN(ISNULL(@MAIL_DESTINATAIRE_CC_LOG_EDI, '')) > 0
         	BEGIN
         
         		/* ENVOI DE LA TRACE DE L'IMPORT PAR MAIL */
         		--* Declaration et affectation des parametres d'envoi de mails */
         		declare
         			@p_profile_name				varchar(255),
         			@p_recipients				varchar(255),
         			@p_copy_recipients			varchar(255),
         			@p_body 					varchar(4000),
         			@p_resolution				varchar(2000),
         			@p_subject					varchar(255),
         			@p_subject1					varchar(255),
         			@p_execute_query_database	varchar(255),
         			@p_query					varchar(255),
         			@NB int
         		IF @COD_TABLE_TAMPON = 'EDI_PEC_ST'
         		BEGIN
         			SET @p_subject					= 'OPCA DEFI : TRACE IMPORT EDI PEC'
         		END
         		IF @COD_TABLE_TAMPON = 'EDI_PAR_ST'
         		BEGIN
         			SET @p_subject					= 'OPCA DEFI : TRACE IMPORT EDI PARCOURS'
         		END
         		
         		SELECT    
         			@p_profile_name				= 'Administrateur',
         			@p_recipients				= @MAIL_DESTINATAIRE_PRINCIPAL_LOG_EDI,
         			@p_copy_recipients			= @MAIL_DESTINATAIRE_CC_LOG_EDI,
         			@p_query					= 'SET NOCOUNT ON;SELECT * FROM TMP02_EDI',
         			@p_body						= 'Vous trouverez ci-joint le compte-renu d''import de l''integration EDI du fichier' + @nom_fichier_chemin 
         
         
         			IF @@SERVERNAME = 'DEFISRV03'
         			BEGIN
         				SET @p_execute_query_database =  'C2P_RECETTE_BATCH'
         			END
         			IF @@SERVERNAME = 'DEFISRV06'
         			BEGIN
         				SET @p_execute_query_database =  'C2P_PROD_BO_BATCH'
         			END
         			IF @@SERVERNAME = 'WS5'
         			BEGIN
         				SET @p_execute_query_database =  'C2P_PROD_BATCH'
         			END
         
         
         		SELECT @NB = COUNT(*) FROM TMP02_EDI
         
         		IF @NB > 0
         		BEGIN
         
         			SELECT 'EXEC msdb.dbo.sp_send_dbmail ',
         				profile_name					= @p_profile_name, 
         				recipients						= @p_recipients , 
         				copy_recipients					= @p_copy_recipients,
         				subject							= @p_subject , 
         				body							= @p_body ,	
         				execute_query_database			= @p_execute_query_database, 
         				query							= @p_query,
         				attach_query_result_as_file	= 1,
         				query_attachment_filename		='TRACE_IMPORT_EDI.txt',
         				query_result_header			= 0,
         				query_result_no_padding		= 1,
         				query_result_width				= 1000
         			
         			EXEC msdb.dbo.sp_send_dbmail 
         				@profile_name					= @p_profile_name, 
         				@recipients						= @p_recipients , 
         				@copy_recipients				= @p_copy_recipients,
         				@subject						= @p_subject , 
         				@body							= @p_body ,	
         				@execute_query_database			= @p_execute_query_database, 
         				@query							= @p_query,
         				@attach_query_result_as_file	= 1,
         				@query_attachment_filename		='TRACE_IMPORT_EDI.txt',
         				@query_result_header			= 0,
         				@query_result_no_padding		= 1,
         				@query_result_width				= 1000
         
         		END
         		DELETE FROM TMP02_EDI
         		/* FIN ENVOI DE LA TRACE DE L'IMPORT PAR MAIL */
         
         		
         
         		/* ENVOI DU FICHIER DE RECYCLAGE DE L'IMPORT PAR MAIL */
         		IF @COD_TABLE_TAMPON = 'EDI_PEC_ST'
         		BEGIN
         
         			INSERT INTO TMP02_EDI
         			SELECT 	data 
         			FROM
         			(
         				SELECT DISTINCT NUM_LIGNE FROM EDI_LOG
         				WHERE ID_LOT_IMPORT = @id_lot_import
         			) LIGNE_REJETE
         			INNER JOIN
         			(
         				SELECT row_number() OVER (order by getdate()) as NUM_LIGNE, data = RTRIM(LTRIM(data))
         				FROM EDI_TAMPON_PEC
         			) TAMPON
         			ON TAMPON.NUM_LIGNE = LIGNE_REJETE.NUM_LIGNE
         
         		END
         
         		IF @COD_TABLE_TAMPON = 'EDI_PAR_ST'
         		BEGIN
         			INSERT INTO TMP02_EDI
         			SELECT 	data 
         			FROM
         			(
         			SELECT DISTINCT NUM_LIGNE FROM EDI_LOG
         			WHERE ID_LOT_IMPORT = @id_lot_import
         			) LIGNE_REJETE
         			INNER JOIN
         			(
         			SELECT row_number() OVER (order by getdate()) as NUM_LIGNE, data = RTRIM(LTRIM(data))
         			FROM EDI_TAMPON_PAR_ST
         			) TAMPON
         			ON TAMPON.NUM_LIGNE = LIGNE_REJETE.NUM_LIGNE
         
         		END
         
         		SELECT    
         			@p_profile_name = 'Administrateur',
         			@p_recipients = @MAIL_DESTINATAIRE_PRINCIPAL_LOG_EDI,
         			@p_copy_recipients = @MAIL_DESTINATAIRE_CC_LOG_EDI,
         			--@p_subject = 'OPCA DEFI : REJETS EDI PEC',
         			--@p_execute_query_database = 'C2P_RECETTE_BATCH',
         			@p_query = 'SET NOCOUNT ON;SELECT * FROM TMP02_EDI',
         			@p_body = 'Les salaries contenus dans le fichier ci-joint ont ete rejetes lors de l''EDI lors de l''integration du fichier' + @nom_fichier_chemin 
         
         			IF @COD_TABLE_TAMPON = 'EDI_PEC_ST'
         			BEGIN
         				SET @p_subject = 'OPCA DEFI : REJETS EDI PEC'
         			END
         			IF @COD_TABLE_TAMPON = 'EDI_PAR_ST'
         			BEGIN
         				SET @p_subject = 'OPCA DEFI : REJETS EDI PARCOURS'
         			END
         
         
         			IF @@SERVERNAME = 'DEFISRV03'
         			BEGIN
         				SET @p_execute_query_database =  'C2P_RECETTE_BATCH'
         			END
         			IF @@SERVERNAME = 'DEFISRV06'
         			BEGIN
         				SET @p_execute_query_database =  'C2P_PROD_BO_BATCH'
         			END
         			IF @@SERVERNAME = 'WS5'
         			BEGIN
         				SET @p_execute_query_database =  'C2P_PROD_BATCH'
         			END
         
         
         
         		SELECT @NB = COUNT(*) FROM TMP02_EDI
         
         		IF @NB > 0
         		BEGIN
         
         			SELECT 'EXEC msdb.dbo.sp_send_dbmail ',
         				profile_name					= @p_profile_name, 
         				recipients						= @p_recipients , 
         				copy_recipients				= @p_copy_recipients,
         				subject						= @p_subject , 
         				body							= @p_body ,	
         				execute_query_database			= @p_execute_query_database, 
         				query							= @p_query,
         				attach_query_result_as_file	= 1,
         				query_attachment_filename		='RECYCLAGE_EDI.csv',
         				query_result_header			= 0,
         				query_result_no_padding		= 1,
         				query_result_width				= 1000
         			
         			EXEC msdb.dbo.sp_send_dbmail 
         				@profile_name					= @p_profile_name, 
         				@recipients						= @p_recipients , 
         				@copy_recipients				= @p_copy_recipients,
         				@subject						= @p_subject , 
         				@body							= @p_body ,	
         				@execute_query_database			= @p_execute_query_database, 
         				@query							= @p_query,
         				@attach_query_result_as_file	= 1,
         				@query_attachment_filename		='RECYCLAGE_EDI.csv',
         				@query_result_header			= 0,
         				@query_result_no_padding		= 1,
         				@query_result_width				= 1000
         
         		END
         		/* FIN ENVOI DU FICHIER DE RECYCLAGE DE L'IMPORT PAR MAIL */
         	END
         	SELECT *  FROM TMP02_EDI
         
         	-- suppression des donn‚es de la table de travail
         	set @cmd = 'DELETE ' + @COD_TABLE_TAMPON + ' WHERE ID_LOT_IMPORT = ' + CAST(@ID_LOT_IMPORT as varchar)
         	--EXEC (@cmd)
         
         
         	if exists (select 1 from sys.objects where type = 'U' and name = 'TMP02_EDI')
         	BEGIN
         		DROP TABLE TMP02_EDI
         	END
         
         	RETURN 0
         END
         


         -- =============================================
         -- Author:		SAFIYULLA
         -- Create date: 19-05-2010
         -- Description:	Lettre relance Compl‚ment Etablissement
         -- =============================================
         -- Author:		SAFIYULLA
         -- Create date: 05-07-2010
         -- Description:	Change to Null if IDAgence is -1
         --				Maxdate avec fonctionne Convert
         -- Anno ID :	12442
         -- =============================================
         -- DSZ 12/07/2010 12447
         -- condition erron‚e ETABLISSEMENT.ID_UTILISATEUR = @ID_UTILISATEUR 
         -- remplac‚e par ETABLISSEMENT.ID_CHARGEE_RELATION = @ID_UTILISATEUR 
         -- =============================================
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         -- =============================================
         -- BBL 11/12/2015 : Exclure les clotures
         -- =============================================
         
         
         CREATE PROCEDURE [dbo].[LEC_GRP_RELANCE_COMP_ETAB]
         	@ID_UTILISATEUR INT,
         	@ID_AGENCE INT = null,
         	@COD_CONTRAT_PRO VARCHAR(10)
         AS
         BEGIN
         
         DECLARE @ID_DOCUMENT		INT
         DECLARE @SEUIL_RELANCE_ADH	DECIMAL(18,2)
         DECLARE @DELAI_RELANCE_1	INT
         DECLARE @DELAI_RELANCE_2	INT
         DECLARE @DELAI_RELANCE_3	INT
         DECLARE @TEMP_MAX_DATE		DATETIME
         
         IF @COD_CONTRAT_PRO = ''
         BEGIN 
         SET @COD_CONTRAT_PRO = NULL
         END
         
         IF @ID_AGENCE = -1
         BEGIN 
         SET @ID_AGENCE = NULL
         END
         
         
         
         
         SELECT @ID_DOCUMENT = ID_DOCUMENT  FROM DOCUMENT WHERE COD_DOCUMENT = 'LET_REL_CETAB'
         --SELECT @SEUIL_RELANCE_ADH = SEUIL_RELANCE_ADH  FROM PARAM_EDITION_CPRO WHERE ID_DOCUMENT = @ID_DOCUMENT
         
         SELECT	@DELAI_RELANCE_1	=	DELAI_RELANCE_1,
         		@DELAI_RELANCE_2	=	DELAI_RELANCE_2,
         		@DELAI_RELANCE_3	=	DELAI_RELANCE_3,
         		@SEUIL_RELANCE_ADH	=	SEUIL_RELANCE_ADH
         FROM	PARAM_EDITION_CPRO
         WHERE	ID_DOCUMENT			=	@ID_DOCUMENT	
         
         --
         --SET @DELAI_RELANCE_1 = 0
         --SET @DELAI_RELANCE_2 = 0
         --SET @DELAI_RELANCE_3 = 0 
         --
         
         
         SELECT		
         			ETABLISSEMENT.ID_ETABLISSEMENT, 
         			ADHERENT.ID_ADHERENT,
         			CONTRAT_PRO.ID_CONTRAT_PRO,
         			CONTRAT_PRO.COD_CONTRAT_PRO AS _COD_CONTRAT_PRO,
         			CONTRAT_PRO.ID_AGENCE,	
         			SESSION_PRO.ID_SESSION_PRO,	
         			SESSION_PRO.DAT_BAP_OF,
         			SESSION_PRO.MNT_VERSE_HT_ADH,			
         			EVENEMENT_ADMIN.ID_TYPE_EVENEMENT_ADMIN,
         			EVENEMENT_ADMIN.DAT_EVENEMENT_ADMIN,
         			@TEMP_MAX_DATE AS MAX_DAT_EVENEMENT_ADMIN,
         			@TEMP_MAX_DATE AS MAX_DAT_BAP_OF,
         			0 AS NUM_RELANCE									
         INTO		#TEMP_LIST_ALL			-- without counting the relance already done
         FROM		ETABLISSEMENT
         INNER JOIN	ADHERENT				ON	ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         INNER JOIN	CONTRAT_PRO				ON  CONTRAT_PRO.ID_ETABLISSEMENT = ETABLISSEMENT.ID_ETABLISSEMENT AND
         										CONTRAT_PRO.BLN_REPRISE_ADHOC = 0 								
         INNER JOIN	MODULE_PRO				ON	MODULE_PRO.ID_CONTRAT_PRO = CONTRAT_PRO.ID_CONTRAT_PRO
         INNER JOIN	SESSION_PRO				ON	SESSION_PRO.ID_MODULE_PRO = MODULE_PRO.ID_MODULE_PRO 								
         INNER  JOIN	REGLEMENT_PRO REGOF		ON	(SESSION_PRO.ID_REGLEMENT_PRO_OF = REGOF.ID_REGLEMENT_PRO )-- rglt OF existe obligatoirement
         --LEFT  JOIN  REGLEMENT_PRO REGADH	ON	(SESSION_PRO.ID_REGLEMENT_PRO_ADH = REGADH.ID_REGLEMENT_PRO )-- rglt ADH n'existe pas			
         INNER JOIN	UTILISATEUR				ON	UTILISATEUR.ID_UTILISATEUR = ETABLISSEMENT.ID_CHARGEE_RELATION 
         										-- AND UTILISATEUR.ID_PROFIL = 2 								
         LEFT JOIN	EVENEMENT_ADMIN			ON	EVENEMENT_ADMIN.ID_CONTRAT_PRO = CONTRAT_PRO.ID_CONTRAT_PRO  
         										AND EVENEMENT_ADMIN.ID_TYPE_EVENEMENT_ADMIN in (15, 16, 17)
         WHERE
         	/* BBL 12/10/2015 CLOTURE a exclure */
         	CONTRAT_PRO.DAT_CLOTURE IS NULL AND	
         	MODULE_PRO.DAT_CLOTURE IS NULL AND	
         	/* BBL 12/10/2015*/
         						
         	((@ID_AGENCE IS NULL)		OR	(UTILISATEUR.ID_AGENCE = @ID_AGENCE))				AND
         	-- remplacement de ETABLISSEMENT.ID_UTILISATEUR par ETABLISSEMENT.ID_CHARGEE_RELATION DSZ 12/07/2010
         	((@ID_UTILISATEUR IS NULL )	OR	(ETABLISSEMENT.ID_CHARGEE_RELATION = @ID_UTILISATEUR))	AND
         	((@COD_CONTRAT_PRO IS NULL) OR (@COD_CONTRAT_PRO = '') OR	(CONTRAT_PRO.COD_CONTRAT_PRO like '%'+ @COD_CONTRAT_PRO+'%' ))	AND
         	ETABLISSEMENT.BLN_ACTIF = 1															AND
         	SESSION_PRO.BLN_ACTIF	= 1															AND
         	MODULE_PRO.BLN_ACTIF	= 1															AND
         	CONTRAT_PRO.BLN_ACTIF	= 1															AND
         	SESSION_PRO.ID_FACTURE_OF IS NOT NULL												AND
         	SESSION_PRO.ID_FACTURE_ADHERENT IS NULL												AND
         	REGOF.ID_REGLEMENT_PRO IS NOT NULL													AND								
         	REGOF.DAT_VALID_REGLEMENT IS NOT NULL												AND 	
         	DATEDIFF(DAY, SESSION_PRO.DAT_BAP_OF,GETDATE()) > @DELAI_RELANCE_1					AND	
         	(
         		EVENEMENT_ADMIN.ID_TYPE_EVENEMENT_ADMIN IS NULL 
         		OR 	
         		EVENEMENT_ADMIN.ID_TYPE_EVENEMENT_ADMIN in (15, 16) OR
         		(
         			EVENEMENT_ADMIN.ID_TYPE_EVENEMENT_ADMIN in (17) AND
         			DATEDIFF(DAY, SESSION_PRO.DAT_BAP_OF, EVENEMENT_ADMIN.dat_EVENEMENT_ADMIN) > @DELAI_RELANCE_1
         		)
         	)
         
         	-- Getting the Max date and Max Evenement 
         /*
             SELECT 
         		MAX(DAT_BAP_OF)				AS MAX_DAT_BAP_OF, 
         		MAX(DAT_EVENEMENT_ADMIN)	AS MAX_DAT_EVENEMENT_ADMIN, 
         		ID_CONTRAT_PRO 
         	INTO #MAX_TEMP_LIST_ALL  
         	FROM #TEMP_LIST_ALL 
         	GROUP BY ID_CONTRAT_PRO
         */
         
         	SELECT
         	CONVERT (DATETIME, MAX(DAT_BAP_OF))			AS MAX_DAT_BAP_OF,
         	CONVERT (DATETIME,MAX(DAT_EVENEMENT_ADMIN)) AS MAX_DAT_EVENEMENT_ADMIN,
         	ID_CONTRAT_PRO
         	INTO #MAX_TEMP_LIST_ALL
         	FROM #TEMP_LIST_ALL
         	GROUP BY ID_CONTRAT_PRO
         
         
         
         	UPDATE	#TEMP_LIST_ALL 
         	SET		#TEMP_LIST_ALL.MAX_DAT_BAP_OF			= T2.MAX_DAT_BAP_OF, 
         			#TEMP_LIST_ALL.MAX_DAT_EVENEMENT_ADMIN	= T2.MAX_DAT_EVENEMENT_ADMIN
         	FROM	#MAX_TEMP_LIST_ALL T2 
         	WHERE   #TEMP_LIST_ALL.ID_CONTRAT_PRO = T2.ID_CONTRAT_PRO
         
         	UPDATE	#TEMP_LIST_ALL 
         	SET		NUM_RELANCE = dbo.GetNumRelanceCompEtabPRO(@ID_DOCUMENT, #TEMP_LIST_ALL.ID_CONTRAT_PRO, #TEMP_LIST_ALL.MAX_DAT_BAP_OF, #TEMP_LIST_ALL.MAX_DAT_EVENEMENT_ADMIN) --AS NUM_RELANCE
         	
         
         	-- Delete all the CPs, those were already have 3 'relance'
         	DELETE 	FROM #TEMP_LIST_ALL WHERE ID_CONTRAT_PRO IN (SELECT DISTINCT ID_CONTRAT_PRO FROM #TEMP_LIST_ALL WHERE NUM_RELANCE = 0)
         
         
         -- Find all the contracts that have a sum amount of Adherent is less that the limit and delete it 
         	SELECT DISTINCT ID_CONTRAT_PRO, SUM(MNT_VERSE_HT_ADH) AS SUM_MNT_VERSE_HT_ADH  
         	INTO #TMP_LIST_MNT_MOINS_LIMIT 
         	FROM #TEMP_LIST_ALL 
         	GROUP BY ID_CONTRAT_PRO
         
         -- Delete all the contracts that have the sum amount less then the limit
         	DELETE FROM #TEMP_LIST_ALL WHERE ID_CONTRAT_PRO IN  
         	(SELECT	DISTINCT ID_CONTRAT_PRO FROM #TMP_LIST_MNT_MOINS_LIMIT WHERE  SUM_MNT_VERSE_HT_ADH < @SEUIL_RELANCE_ADH)	
         
         
         
         SELECT 			DISTINCT 
         				ID_ETABLISSEMENT, 
         				ID_ADHERENT,
         				_COD_CONTRAT_PRO,
         				ID_AGENCE	 
         FROM #TEMP_LIST_ALL
         
         END
         
                  
CREATE PROCEDURE [dbo].[MVT_BUDGETAIRE_PEC_ANNULATION]  
          @ID_EVENEMENT  INT,
          @ID_TYPE_MOUVEMENT INT,
          @P_E_R    varchar(1),
          @ID_EVENEMENT_MIN INT = NULL , 
          @ID_EVENEMENT_MAX INT = NULL 
          
         AS
         /* *****************************************************************************************
         -- Auteur   : MBL
         -- Date Creation : 31/05/2012
         
         -- Role    : Generation de Mouvements Budgetaires Previsionnels ou d'Engagement
                  permettant d'annuler remettre a zero le solde Previsionnel/engagement 
               pour le couple module/sous type de cout associe a l'evenement
               
           Les differents cas d'appels de cette procedure sont :
         
           - Generation des Mouvements Budgetaires d'annulation de Pr‚vision lors du Chiffrage 
            permettant de remettre a zero les Montants Previsionnels lies au sous type de cout du PCE associe a l'evenement
            @COD_TYPE_EVENEMENT = 'CHIF_PEC', @ID_TYPE_MOUVEMENT = 19 -- Mouvement d'annulation de Pr‚vision, @P_E_R = 'P'
           
           - Generation des Mouvements Budgetaires d'annulation d'Engagement lors du Desengagement
            permettant de remettre a zero les Montants Engages lies au sous type de cout du PCE associe a l'evenement
            @COD_TYPE_EVENEMENT = 'DESENG', @ID_TYPE_MOUVEMENT = 21 -- Desengagement, @P_E_R = 'E'
         
           - Generation des Mouvements Budgetaires d'annulation de Pr‚vision lors de l'Engagement
            permettant de remettre a zero les Montants Previsionnels lies au sous type de cout du PCE associe a l'evenement
            @COD_TYPE_EVENEMENT = 'ENGAGMT', @ID_TYPE_MOUVEMENT = 19 -- Mouvement d'annulation de Pr‚vision, @P_E_R = 'P'
         
           - Generation des Mouvements Budgetaires d'annulation de Pr‚vision lors de l'Annulation
            permettant de remettre a zero les Montants Previsionnels lies au sous type de cout du PCE associe a l'evenement
            @COD_TYPE_EVENEMENT = 'ANNULATI', @ID_TYPE_MOUVEMENT = 19 -- Mouvement d'annulation de Pr‚vision, @P_E_R = 'P'
         
           - Generation des Mouvements Budgetaires de desengagement lors de l'Annulation d'un module
            permettant de remettre a zero les Montants Engages lies au sous type de cout du PCE associe a l'evenement
            @COD_TYPE_EVENEMENT = 'ANNULATI', @ID_TYPE_MOUVEMENT = 21 -- D‚sengagement, @P_E_R = 'E'
         
         
           - Generation des Mouvements Budgetaires d'annulation de Pr‚vision lors de la cloture d'un module
            permettant de remettre a zero les Montants Previsionnels lies au sous type de cout du PCE associe a l'evenement
            @COD_TYPE_EVENEMENT = 'CLOMOPEC', @ID_TYPE_MOUVEMENT = 19 -- Mouvement d'annulation de Pr‚vision, @P_E_R = 'P'
         
           - Generation des Mouvements Budgetaires de desengagement lors de la cloture d'un module
            permettant de remettre a zero les Montants Engages lies au sous type de cout du PCE associe a l'evenement
            @COD_TYPE_EVENEMENT = 'CLOMOPEC', SET @ID_TYPE_MOUVEMENT = 21 -- D‚sengagement, @P_E_R = 'E'
         
         ------------------------------------------------------------------------------------------------
         -- Modif MB du 26/05/2014 : Rajout des mouvements budgetaires lies a l'evenement de DECLOTURE
         ------------------------------------------------------------------------------------------------
         
           - Generation des Mouvements Budgetaires de reengagement lors de la decloture d'un module
            permettant d'annuler le desengagement effectuer lors de la derniere cloture pour le sous type de cout du PCE associe a l'evenement
            @COD_TYPE_EVENEMENT = 'ANCLOPEC', SET @ID_TYPE_MOUVEMENT = 21 -- D‚sengagement, @P_E_R = 'E'      
         ------------------------------------------------------------------------------------------------
         -- Modif MB du 20/10/2014 : Pour les mouvements d'engagement (P_E_R = 'E'),
                tous les mouvements bugetaires avec un montant = 0 sont generes
         ------------------------------------------------------------------------------------------------
         ***************************************************************************************** */
         -- OPA 29/01/2015 684 Suppression d'ID_Public_FAF dans les PS "PEC3"  
         -- ===========================================================================================================================  
         BEGIN
         
          DECLARE 
          @DAT_EVENEMENT   datetime,
          @ID_POSTE_COUT_ENGAGE int,
          @ID_MODULE_PEC   int,
          @ID_SOUS_TYPE_COUT  int,
          @ID_COMPTE    int, 
          @ID_ENVELOPPE   int, 
          @ID_PERIODE_FISC  int,
          @ID_PERIODE_CPT   int,
          @ID_ACTIVITE   int,
          @ID_ADHERENT   int,
          @ID_GROUPE    int,
          @ID_ETABLISSEMENT  int,
          @ID_TYPE_FINANCEMENT int,
          @ID_DISPOSITIF   int,
          @N_R     varchar(1),
          @MNT_MVT_BUDGETAIRE  decimal(15,2),
          @LIBL_MVT_BUDGETAIRE varchar (60),
          @COD_MODULE_PEC   varchar (14),
          @COD_TYPE_COUT   varchar (08),
          @COD_SOUS_TYPE_COUT  varchar (08),
          @COD_TYPE_EVENEMENT  varchar (08),
          @COD_FACTURE   VARCHAR(50)
          
          SELECT @ID_POSTE_COUT_ENGAGE = ID_POSTE_COUT_ENGAGE,
            @DAT_EVENEMENT   = DAT_EVENEMENT,
            @COD_TYPE_EVENEMENT  = COD_TYPE_EVENEMENT
          FROM EVENEMENT
          INNER JOIN TYPE_EVENEMENT ON EVENEMENT.ID_TYPE_EVENEMENT =TYPE_EVENEMENT.ID_TYPE_EVENEMENT
          WHERE ID_EVENEMENT = @ID_EVENEMENT
         
         
          SELECT @ID_MODULE_PEC   = MODULE_PEC.ID_MODULE_PEC,
            @COD_MODULE_PEC   = MODULE_PEC.COD_MODULE_PEC,
            @ID_SOUS_TYPE_COUT  = SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT,
            @COD_TYPE_COUT   = TYPE_COUT.COD_TYPE_COUT,
            @COD_SOUS_TYPE_COUT  = SOUS_TYPE_COUT.COD_SOUS_TYPE_COUT
          FROM POSTE_COUT_ENGAGE 
          INNER JOIN MODULE_PEC   ON MODULE_PEC.ID_MODULE_PEC = POSTE_COUT_ENGAGE .ID_MODULE_PEC
          INNER JOIN SOUS_TYPE_COUT  ON SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT = POSTE_COUT_ENGAGE .ID_SOUS_TYPE_COUT 
          INNER JOIN TYPE_COUT   ON TYPE_COUT.ID_TYPE_COUT = SOUS_TYPE_COUT.ID_TYPE_COUT 
          WHERE POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE = @ID_POSTE_COUT_ENGAGE
         
         
          IF @COD_TYPE_EVENEMENT  = 'CHIF_PEC' -- Chiffrage PEC
           AND @ID_TYPE_MOUVEMENT = 19   -- Mouvement d'annulation de Pr‚vision
          BEGIN
           SET @LIBL_MVT_BUDGETAIRE = 'Annul pr‚vision suite Chiffrage '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT
          END
         
          ELSE IF @COD_TYPE_EVENEMENT = 'ENGAGMT' -- Engagement
          AND @ID_TYPE_MOUVEMENT  = 19  -- Mouvement d'annulation de Pr‚vision
          BEGIN
           SET @LIBL_MVT_BUDGETAIRE = 'Annul pr‚vision suite Engagement '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT
          END
         
          ELSE IF @COD_TYPE_EVENEMENT = 'ANNULATI' -- Annulation (Module Desactive ou Rendu Non imputable
          AND @ID_TYPE_MOUVEMENT  = 19   -- Mouvement d'annulation de Pr‚vision
          BEGIN
           SET @LIBL_MVT_BUDGETAIRE = 'Annul pr‚vision suite Annulation '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT
          END
         
          ELSE IF @COD_TYPE_EVENEMENT = 'ANNULATI' -- Annulation (Module Desactive ou Rendu Non imputable
          AND @ID_TYPE_MOUVEMENT  = 21   -- Mouvement de desengagement
          BEGIN
           SET @LIBL_MVT_BUDGETAIRE = 'Annul engagement suite Annulation '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT
          END
         
          ELSE IF @COD_TYPE_EVENEMENT = 'CLOMOPEC' -- Cloture Module 
          AND @ID_TYPE_MOUVEMENT  = 19   -- Mouvement d'annulation de Pr‚vision
          BEGIN
           SET @LIBL_MVT_BUDGETAIRE = 'Annul pr‚vision suite Cl“ture '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT
          END
         
          ELSE IF @COD_TYPE_EVENEMENT = 'DESENG' -- D‚sengagement
          AND @ID_TYPE_MOUVEMENT  = 21   -- Mouvement de desengagement
          BEGIN 
           SET @LIBL_MVT_BUDGETAIRE = 'D‚sengagement '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT
          END
         
          ELSE IF @COD_TYPE_EVENEMENT = 'CLOMOPEC' -- Cloture Module
          AND @ID_TYPE_MOUVEMENT  = 21   -- Mouvement de desengagement
          BEGIN
           SET @LIBL_MVT_BUDGETAIRE = 'Annul engagement suite Cl“ture '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT
          END
         
          ELSE IF @COD_TYPE_EVENEMENT = 'ANCLOPEC' -- Decloture Module
          AND @ID_TYPE_MOUVEMENT  = 20   -- Mouvement d'engagement
          BEGIN
           SET @LIBL_MVT_BUDGETAIRE = 'Annul Cl“ture '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT
          END
         
          ELSE IF @COD_TYPE_EVENEMENT = 'ANCLOPEC' -- Decloture Module
          AND @ID_TYPE_MOUVEMENT  = 18   -- Mouvement de Pr‚vision
          BEGIN
           SET @LIBL_MVT_BUDGETAIRE = 'Annul Cl“ture '+ @COD_MODULE_PEC+ ' '+ @COD_SOUS_TYPE_COUT
          END
          
          ELSE
          BEGIN
           SELECT PB = 'Type d evenement / Type de mouvement non definis', COD_TYPE_EVENEMENT = @COD_TYPE_EVENEMENT, ID_TYPE_MOUVEMENT = @ID_TYPE_MOUVEMENT  
           SELECT @COD_TYPE_EVENEMENT = NULL, @ID_TYPE_MOUVEMENT = NULL
          END
         
          -- Recherche de tous les "groupes" de mouvements budgetaires associe au niveau de P_E_R souhaite
          -- associes au module et au sous type de cout lie au module
          -- et pour chaque occurence trouvee generation d'un mouvement budgetaire d'annulation.
          IF NOT (@COD_TYPE_EVENEMENT IS NULL OR @ID_TYPE_MOUVEMENT IS NULL)
          BEGIN
         
           DECLARE cu_annul CURSOR FOR
           SELECT 
            COALESCE(POSTE_COUT_REGLE.ID_SOUS_TYPE_COUT , POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT),
            MVT_BUDGETAIRE.ID_COMPTE, 
            MVT_BUDGETAIRE.ID_ENVELOPPE, 
            MVT_BUDGETAIRE.ID_PERIODE_FISC,
            MVT_BUDGETAIRE.ID_PERIODE_CPT,
            MVT_BUDGETAIRE.ID_ACTIVITE,
            MVT_BUDGETAIRE.ID_ADHERENT ,
            MVT_BUDGETAIRE.ID_GROUPE ,
            MVT_BUDGETAIRE.ID_ETABLISSEMENT ,
            MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT ,
            MVT_BUDGETAIRE.ID_DISPOSITIF ,
            MVT_BUDGETAIRE.N_R,
            SUM(ISNULL(CAST(MNT_MVT_BUDGETAIRE AS DECIMAL(15,2)), 0))
           FROM MVT_BUDGETAIRE      
           INNER JOIN EVENEMENT  on EVENEMENT.ID_EVENEMENT = MVT_BUDGETAIRE.ID_EVENEMENT
           LEFT JOIN POSTE_COUT_REGLE  on EVENEMENT.ID_POSTE_COUT_REGLE=POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE
           LEFT JOIN POSTE_COUT_ENGAGE on EVENEMENT.ID_POSTE_COUT_ENGAGE=POSTE_COUT_ENGAGE.ID_POSTE_COUT_ENGAGE
           WHERE  MVT_BUDGETAIRE.ID_MODULE_PEC               = @ID_MODULE_PEC 
             AND P_E_R                    = @P_E_R 
             AND COALESCE(POSTE_COUT_REGLE.ID_SOUS_TYPE_COUT , POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT) = @ID_SOUS_TYPE_COUT
             AND EVENEMENT.ID_EVENEMENT >= ISNULL(@ID_EVENEMENT_MIN, EVENEMENT.ID_EVENEMENT )
             AND EVENEMENT.ID_EVENEMENT <= ISNULL(@ID_EVENEMENT_MAX, EVENEMENT.ID_EVENEMENT )
         
           GROUP BY  
            COALESCE(POSTE_COUT_REGLE.ID_SOUS_TYPE_COUT , POSTE_COUT_ENGAGE.ID_SOUS_TYPE_COUT),
            MVT_BUDGETAIRE.ID_COMPTE, 
            MVT_BUDGETAIRE.ID_ENVELOPPE, 
            MVT_BUDGETAIRE.ID_PERIODE_FISC,
            MVT_BUDGETAIRE.ID_PERIODE_CPT,
            MVT_BUDGETAIRE.ID_ACTIVITE,
            MVT_BUDGETAIRE.ID_ADHERENT ,
            MVT_BUDGETAIRE.ID_GROUPE ,
            MVT_BUDGETAIRE.ID_ETABLISSEMENT ,
            MVT_BUDGETAIRE.ID_TYPE_FINANCEMENT ,
            MVT_BUDGETAIRE.ID_DISPOSITIF,
            MVT_BUDGETAIRE.N_R
         
           OPEN cu_annul
           FETCH cu_annul INTO
            @ID_SOUS_TYPE_COUT,
            @ID_COMPTE, 
            @ID_ENVELOPPE, 
            @ID_PERIODE_FISC,
            @ID_PERIODE_CPT,
            @ID_ACTIVITE,
            @ID_ADHERENT ,
            @ID_GROUPE ,
            @ID_ETABLISSEMENT ,
            @ID_TYPE_FINANCEMENT ,
            @ID_DISPOSITIF ,
            @N_R,
            @MNT_MVT_BUDGETAIRE
            
           WHILE (@@FETCH_STATUS <> -1)
           BEGIN
         
            -- Les mouvements budgetaires a 0 ne sont generes que pour les Engagements
            -- Modif MB du 20/10/2014 : Pour les mouvements d'engagement (P_E_R = 'E'),tous les mouvements bugetaires avec un montant = 0 sont generes
            -- IF (@MNT_MVT_BUDGETAIRE != 0 OR @ID_TYPE_MOUVEMENT IN (@COD_TYPE_EVENEMENT = 'ENGAGMT')
            IF (
             @MNT_MVT_BUDGETAIRE != 0 
             OR 
             @P_E_R = 'E'
             ) 
         
            BEGIN
             SELECT @COD_TYPE_COUT = COD_TYPE_COUT , @COD_SOUS_TYPE_COUT = COD_SOUS_TYPE_COUT 
             FROM SOUS_TYPE_COUT  
             INNER JOIN TYPE_COUT ON SOUS_TYPE_COUT.ID_TYPE_COUT = TYPE_COUT .ID_TYPE_COUT 
             WHERE ID_SOUS_TYPE_COUT  = @ID_SOUS_TYPE_COUT   
             
             SET @MNT_MVT_BUDGETAIRE = -@MNT_MVT_BUDGETAIRE
             
             EXEC dbo.INS_MVT_BUDGETAIRE
             @ID_TYPE_MOUVEMENT ,  
             @ID_EVENEMENT ,
             @ID_COMPTE ,
             @ID_ENVELOPPE ,
             @MNT_MVT_BUDGETAIRE ,
             @P_E_R ,
             @DAT_EVENEMENT ,
             @N_R ,
             @ID_PERIODE_FISC ,
             @ID_PERIODE_CPT ,
             @LIBL_MVT_BUDGETAIRE ,
             @ID_ACTIVITE ,
             @ID_ADHERENT ,
             @ID_GROUPE ,
             @ID_ETABLISSEMENT ,
             @ID_TYPE_FINANCEMENT ,
             @ID_DISPOSITIF ,
             @ID_MODULE_PEC 
            END
         
            FETCH cu_annul INTO
            @ID_SOUS_TYPE_COUT,
            @ID_COMPTE, 
            @ID_ENVELOPPE, 
            @ID_PERIODE_FISC,
            @ID_PERIODE_CPT,
            @ID_ACTIVITE,
            @ID_ADHERENT ,
            @ID_GROUPE ,
            @ID_ETABLISSEMENT ,
            @ID_TYPE_FINANCEMENT ,
            @ID_DISPOSITIF ,
            @N_R,
            @MNT_MVT_BUDGETAIRE
         
           END
         
          CLOSE cu_annul
          DEALLOCATE cu_annul
          END
         END



         -- =======================================================================================================================================
         -- Author		: SV
         -- Date			: 14 ao–t 2007
         -- Description	: Ajout des piŠces modules (non adh‚rent et non stagiaire)
         --				  Ajout d'un filtre sur les piŠces adh‚rent pour ne prendre en compte que les piŠces de l'adh‚rent en cours
         -- =======================================================================================================================================
         -- Author		: MBL
         -- Date			: 24 octobre 2007
         -- Description	: Correction mauvais lien entre ID_OF et ID_ETABLISSEMENT_OF
         -- =======================================================================================================================================
         -- Author		: SV
         -- Date			: 18 d‚cembre 2007
         -- Description	: Ouverture de la jointure sur l'‚tablissement O.F. et l'organisme de formation
         -- =======================================================================================================================================
         -- Author		: GiO
         -- Date			: 09 Avril 2008
         -- Description	: Module ACtif
         -- =======================================================================================================================================
         -- Author		: SBRU
         -- Date			: 02/06/2008
         -- Description	: La partie Emetteur est renseign‚e … partir de la fonction GetXmlAdrChargeRelation (au lieu de GetXmlAgenceContact)
         --				  Le paramŠtre en entr‚e @ID_ACTION_PEC est remplac‚ par @CODE_ACTION_PEC, concat‚nation de COD_ACTION_PEC et ANNEE_ACTION_PEC
         -- =======================================================================================================================================
         -- Author		: AMA
         -- Date			: 09/06/2008
         -- Description	: La table temporaire #TEMP_REFERENCE avait ‚t‚ remplac‚e par #TMP_REFERRENCE
         --				  dans la version pr‚c‚dente
         -- =======================================================================================================================================
         -- Le 26/02/09 par SBR - Adaptation gabarit suite au groupe de travail sur les ‚ditions
         -- =======================================================================================================================================
         -- Le 23/05/2012 par DSZ - 13443 : refection totale suite … l'‚vol
         -- =======================================================================================================================================
         -- Le 18/04/2013 DSZ - 15026 utiliser la DAT_RELANCE_ENGAGE_x
         -- =======================================================================================================================================
         -- LDE/OPA 23/05/2014 : #213 En tant qu'utilisateur OPTIFORM, 
         -- lorsque j'‚dite un courrier (PEC, PRO) … destination d'un Adh‚rent ou d'un OF, je veux que, 
         -- l'adresse … afficher dans la zone "correspondance" soit l'adresse TSA propre … ce type de courrier 
         -- si le mode "TSA" s'applique sinon qu'elle soit l'adresse actuelle
         -- =======================================================================================================================================
         
         CREATE PROCEDURE [dbo].[EDT_LETTRE_PEC_RELANCE_INSTRUCTION_ADH]
         	@ID_ETABLISSEMENT INT,
         	@ID_BENEFICIAIRE INT,
         	@TYPE_BENEFICIAIRE INT,
         	@ID_ADRESSE INT,
         	@ID_CONTACT INT,
         	@CODE_ACTION_PEC VARCHAR(11)
         AS
         
         BEGIN
         		/**** R‚cup‚ration des infos pour la partie r‚f‚rence ****/
         
         		-- r‚cup‚ration de l'ID_ACTION_PEC en fonction de @CODE_ACTION_PEC
         		DECLARE @ID_ACTION_PEC INT
         		SELECT	@ID_ACTION_PEC = dbo.GetActionPECId(@CODE_ACTION_PEC)
         		
         		-- Si action individuelle: CR de l'‚tablissement et CM de l'‚tablissement
         		-- Si action collective: CR = CM = responsable des actions collectives renseign‚ dans le champ CM au niveau de l'action
         		SELECT	ETABLISSEMENT.ID_ETABLISSEMENT,
         				ETABLISSEMENT.NUM_SIRET,
         				ADHERENT.ID_ADHERENT,
         				ADHERENT.COD_ADHERENT,
         				ADHERENT.LIB_RAISON_SOCIALE,
         				CM.LIB_NOM			AS LIB_NOM_CONSEILLER,
         				CM.LIB_PNM			AS LIB_PNM_CONSEILLER,
         				CR.ID_UTILISATEUR	AS ID_UTIL,
         				CR.LIB_PNM			AS LIB_PRENOM_CHARGE_RELATION,
         				CR.LIB_NOM			AS LIB_NOM_CHARGE_RELATION,
         				CR.EMAIL			AS EMAIL_CHARGE_RELATION,
         				CR.LIB_VILLE
         		INTO	#TMP_REFERENCE
         		FROM	ADHERENT
         		JOIN	ETABLISSEMENT 
         		ON		ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         		AND		ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
         		JOIN	NR140
         		ON		ETABLISSEMENT.ID_ETABLISSEMENT = NR140.ID_ETABLISSEMENT
         		AND		NR140.ID_ACTION_PEC = @ID_ACTION_PEC
         		JOIN	ACTION_PEC
         		ON		ACTION_PEC.ID_ACTION_PEC = NR140.ID_ACTION_PEC
         		LEFT JOIN
         				UTILISATEUR CR -- charg‚ de relation ou responsable des actions collectives
         		ON		CASE WHEN (ACTION_PEC.CIBLE_ACTION = 1 OR ACTION_PEC.BLN_REPRISE_ADHOC = 1)
         					 THEN ETABLISSEMENT.ID_CHARGEE_RELATION
         					 ELSE ACTION_PEC.ID_CHARGEE_MISSION 
         				END = CR.ID_UTILISATEUR
         		LEFT JOIN
         				UTILISATEUR CM -- charg‚ de mission ou responsable des actions collectives
         		ON		CASE WHEN (ACTION_PEC.CIBLE_ACTION = 1 OR ACTION_PEC.BLN_REPRISE_ADHOC = 1)
         					 THEN ETABLISSEMENT.ID_CHARGEE_MISSION
         					 ELSE ACTION_PEC.ID_CHARGEE_MISSION
         				END = CM.ID_UTILISATEUR
         
         
         		/**** Liste des modules de l'action qui ne sont pas complŠtes ****/
         		SELECT		
         					MODULE_PEC.ID_MODULE_PEC,
         					MODULE_PEC.COD_MODULE_PEC, 
         					MODULE_PEC.LIBL_MODULE_PEC, 
         					MODULE_PEC.BLN_OK_PIECE,
         					CONVERT(VARCHAR(8),MODULE_PEC.DAT_DEBUT, 3)  AS DAT_DEBUT,
         					CONVERT(VARCHAR(8),MODULE_PEC.DAT_FIN, 3)  AS DAT_FIN,
         					CASE MODULE_PEC.BLN_EXTERNE
         						 WHEN 1 THEN ORGANISME_FORMATION.LIB_RAISON_SOCIALE
         						 ELSE 'Votre soci‚t‚'
         					END AS ORGANISME_FORMATION,
         					PIECE_PEC.ID_PIECE_PEC,
         					PIECE_PEC.LIBL_PIECE_PEC,
         					--ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC,
         					--ARRIVEE_PIECE_PEC.BLN_ACTIF,
         					--min( case 
         					--	when ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1 is null then 1
         					--	when ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2 is null then 2
         					--	when ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3 is null then 3 end) as NUM_RELANCE, --sera null si 3 relances d‚j…
         					--max(ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_1) as DERNIERE_RELANCE_1,
         					--max(ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_2) as DERNIERE_RELANCE_2,
         					--max(ARRIVEE_PIECE_PEC.DAT_RELANCE_ENGAGE_3) as DERNIERE_RELANCE_3,
         					MOTIF_NON_CONFORM_PIECE.LIB_EDT_DEB_MOTIF_NON_CONFORM_PIECE,
         					MOTIF_NON_CONFORM_PIECE.LIB_EDT_FIN_MOTIF_NON_CONFORM_PIECE, 
         					MIN(ARRIVEE_PIECE_PEC.BLN_ACTIF) AS BLN_ACTIF,
         					MODULE_PEC.ID_UTILISATEUR,
         					PIECE_PEC.LIB_EDT_DEB_PIECE_PEC,
         					PIECE_PEC.LIB_EDT_FIN_PIECE_PEC
         		INTO		#TMP_PIECES
         
         		FROM		ACTION_PEC 
         		JOIN		MODULE_PEC
         			ON			MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         		LEFT JOIN	ETABLISSEMENT_OF
         			ON			ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = MODULE_PEC.ID_ETABLISSEMENT_OF 
         		LEFT JOIN	ORGANISME_FORMATION	
         			ON			ORGANISME_FORMATION.ID_OF = ETABLISSEMENT_OF.ID_OF 
         		JOIN		ARRIVEE_PIECE_PEC	
         		ON			ARRIVEE_PIECE_PEC.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         		AND			(
         						ARRIVEE_PIECE_PEC.ID_ADHERENT = (select ETABLISSEMENT.ID_ADHERENT from ETABLISSEMENT where id_ETABLISSEMENT = @ID_ETABLISSEMENT) 	-- PiŠces adh‚rent
         						OR ARRIVEE_PIECE_PEC.ID_ADHERENT is null					-- PiŠces module
         					)
         		JOIN		PIECE_PEC
         		ON			ARRIVEE_PIECE_PEC.ID_PIECE_PEC = PIECE_PEC.ID_PIECE_PEC
         		LEFT JOIN	NR210	
         		ON			ARRIVEE_PIECE_PEC.ID_ARRIVEE_PIECE_PEC = NR210.ID_ARRIVEE_PIECE_PEC
         		LEFT JOIN	MOTIF_NON_CONFORM_PIECE
         		ON			NR210.ID_MOTIF_NON_CONFORM_PIECE = MOTIF_NON_CONFORM_PIECE.ID_MOTIF_NON_CONFORM_PIECE
         	
         		WHERE		
         			ACTION_PEC.ID_ACTION_PEC = @ID_ACTION_PEC
         			AND			MODULE_PEC.BLN_OK_PIECE = 0
         			AND			PIECE_PEC.BLN_BLOQUANT_ENGAGEMENT = 1
         			AND			(
         							PIECE_PEC.BLN_ADHERENT =1				-- PiŠces adh‚rent
         							OR
         							PIECE_PEC.BLN_MODULE = 1				-- PiŠces module
         							OR
         							PIECE_PEC.BLN_STAGIAIRE = 1				-- PiŠces stagiaire
         						)
         			AND			ACTION_PEC.BLN_ACTIF = 1
         			AND			MODULE_PEC.BLN_ACTIF = 1
         			AND			PIECE_PEC.BLN_ACTIF=1
         			-- piŠce manquante ou  non conforme
         			AND			(ARRIVEE_PIECE_PEC.BLN_ACTIF = 0 OR ARRIVEE_PIECE_PEC.BLN_CONFORME  = 0)
         			GROUP BY
         					MODULE_PEC.ID_MODULE_PEC,
         					MODULE_PEC.COD_MODULE_PEC, 
         					MODULE_PEC.LIBL_MODULE_PEC, 
         					MODULE_PEC.BLN_OK_PIECE,
         					CONVERT(VARCHAR(8),MODULE_PEC.DAT_DEBUT, 3) ,
         					CONVERT(VARCHAR(8),MODULE_PEC.DAT_FIN, 3) ,
         					CASE MODULE_PEC.BLN_EXTERNE
         						 WHEN 1 THEN ORGANISME_FORMATION.LIB_RAISON_SOCIALE
         						 ELSE 'Votre soci‚t‚'
         					END ,
         					PIECE_PEC.ID_PIECE_PEC,
         					PIECE_PEC.LIBL_PIECE_PEC,
         					MODULE_PEC.ID_UTILISATEUR,
         					PIECE_PEC.LIB_EDT_DEB_PIECE_PEC,
         					PIECE_PEC.LIB_EDT_FIN_PIECE_PEC,
         					MOTIF_NON_CONFORM_PIECE.LIB_EDT_DEB_MOTIF_NON_CONFORM_PIECE,
         					MOTIF_NON_CONFORM_PIECE.LIB_EDT_FIN_MOTIF_NON_CONFORM_PIECE
         
         		SELECT DISTINCT
         			ID_MODULE_PEC,
         			COD_MODULE_PEC, 
         			CAST(NULL AS VARCHAR(100)) AS NOM_STAGIARE,
         			LIBL_MODULE_PEC,
         			DAT_DEBUT,
         			DAT_FIN,
         			ORGANISME_FORMATION,
         			CASE UTILISATEUR.ID_PROFIL 
         				WHEN 1 THEN 'Conseiller Formation' 
         				WHEN 2 THEN 'Charg‚ d''Instruction'
         				ELSE '' END AS FONCTION, 
         			UTILISATEUR.LIB_PNM +' '+UTILISATEUR.LIB_NOM AS NOM_CONSEILLER
         		INTO #TMP_MODULES
         		FROM #TMP_PIECES	
         			INNER JOIN UTILISATEUR ON (UTILISATEUR.ID_UTILISATEUR = #TMP_PIECES.ID_UTILISATEUR)
         		ORDER BY COD_MODULE_PEC;
         		
         		UPDATE #TMP_MODULES
         			SET NOM_STAGIARE = PRENOM_INDIVIDU +' '+ NOM_INDIVIDU 
         		FROM #TMP_MODULES
         			INNER JOIN STAGIAIRE_PEC ON STAGIAIRE_PEC.ID_MODULE_PEC = #TMP_MODULES.ID_MODULE_PEC
         			INNER JOIN INDIVIDU ON INDIVIDU.ID_INDIVIDU = STAGIAIRE_PEC.ID_INDIVIDU
         		WHERE STAGIAIRE_PEC.ID_ETABLISSEMENT = @ID_ETABLISSEMENT;
         
         
         	/*****  Creation du flux XML *****************************************************************************/
         	WITH XMLNAMESPACES (
         		DEFAULT 'EDT_LETTRE_PEC_RELANCE_INSTRUCTION_ADH'
         	)
         	/**********************************************************************************************************/
         		
         	SELECT
         	(		
         	SELECT
         		-- R‚cup‚ration des informations sur le contact et le b‚n‚ficiaire
         		dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) as BENEFICIAIRE,
         
         		/**********************************************************************************************/
         		/*****************	ENTETE DE LA LETTRE *******************************************************/
         		/**********************************************************************************************/
         		(
         			SELECT	              	
         
         				/******************* Recuperation des informations de l'entete ****************************/
         				ISNULL(COD_ADHERENT, '')				AS COD_ADHERENT,
         				ISNULL(LIB_PRENOM_CHARGE_RELATION, '')	AS LIB_PRENOM_CHARGE_RELATION,
         				ISNULL(LIB_NOM_CHARGE_RELATION, '')		AS LIB_NOM_CHARGE_RELATION,
         				ISNULL(EMAIL_CHARGE_RELATION, '')		AS EMAIL,
         
         				-- R‚cup‚ration des informations sur l'‚metteur
         				dbo.GetXmlAdrUtilAvecTel(ENTETE.ID_UTIL, (SELECT TOP 1 BLN_TSA FROM ETABLISSEMENT WHERE ID_ETABLISSEMENT = @ID_BENEFICIAIRE), 1) AS EMETTEUR,
         
         				RTRIM(CASE WHEN PATINDEX('%CEDEX%', LIB_VILLE) <> 0 THEN LEFT(LIB_VILLE, PATINDEX('%CEDEX%', LIB_VILLE)-1) ELSE LIB_VILLE END) AS LIB_VILLE, -- retraitement de la ville au cas o— de la forme COMMUNE CEDEX 999
         				dbo.GetFullDate(GETDATE()) AS DATE
         			FROM #TMP_REFERENCE AS ENTETE     
         			FOR XML AUTO, ELEMENTS, TYPE
         		),
         		#TMP_MODULES.COD_MODULE_PEC,
         		#TMP_MODULES.NOM_STAGIARE,
         		(
         			SELECT TOP 1
         				dbo.GetContactSalutation(@ID_CONTACT, 1)
         			FROM CIVILITE
         			FOR XML PATH('POLITESSE_HAUT'), TYPE
         		),
         		#TMP_MODULES.LIBL_MODULE_PEC AS TITRE_MODULE,
         		DAT_DEBUT,
         		DAT_FIN,
         		ORGANISME_FORMATION,
         			
         		(
         		SELECT A.LIB_EDT_DEB_PIECE_PEC, A.LIB_EDT_FIN_PIECE_PEC
         		FROM 
         		(
         			SELECT	
         					LIB_EDT_DEB_PIECE_PEC,
         					LIB_EDT_FIN_PIECE_PEC, 
         					BLN_ACTIF
         			FROM    #TMP_PIECES
         			WHERE  ID_MODULE_PEC = #TMP_MODULES.ID_MODULE_PEC
         				AND BLN_ACTIF = 0 --absentes
         			UNION 
         				SELECT 
         					LIB_EDT_DEB_MOTIF_NON_CONFORM_PIECE AS LIB_EDT_DEB_PIECE_PEC,
         					LIB_EDT_FIN_MOTIF_NON_CONFORM_PIECE AS LIB_EDT_FIN_PIECE_PEC,
         					BLN_ACTIF
         				FROM    #TMP_PIECES
         				WHERE  ID_MODULE_PEC = #TMP_MODULES.ID_MODULE_PEC
         				AND  BLN_ACTIF =1 --presentes mais non conformes
         		) AS A
         			
         		WHERE 	(LIB_EDT_DEB_PIECE_PEC IS NOT NULL OR LIB_EDT_FIN_PIECE_PEC IS NOT NULL)
         		ORDER BY A.BLN_ACTIF 
         		FOR XML RAW('PIECES_MANQUANTES'), ELEMENTS, TYPE, ROOT('PIECES_MANQUANTE')
         		),
         		(
         			SELECT	TOP 1
         					dbo.GetContactSalutation(@ID_CONTACT, 0)
         			FROM CIVILITE
         			FOR XML PATH('POLITESSE_BAS'), TYPE
         		),
         						
         		/**********************************************************************************************/
         		/*****************	SIGNATURE DE LA LETTRE ****************************************************/
         		/**********************************************************************************************/
         		NOM_CONSEILLER,
         		FONCTION
         
         		/**************************************************************************************************/
         		FROM	#TMP_MODULES
         		FOR XML RAW('MODULES'), ELEMENTS, TYPE
         	)
         	FROM #TMP_REFERENCE
         	FOR XML RAW('LETTRE'), ELEMENTS, TYPE
         END

CREATE PROCEDURE dbo.sp_dropdiagram
         	(
         		@diagramname 	sysname,
         		@owner_id	int	= null
         	)
         	WITH EXECUTE AS 'dbo'
         	AS
         	BEGIN
         		set nocount on
         		declare @theId 			int
         		declare @IsDbo 			int
         		
         		declare @UIDFound 		int
         		declare @DiagId			int
         	
         		if(@diagramname is null)
         		begin
         			RAISERROR ('Invalid value', 16, 1);
         			return -1
         		end
         	
         		EXECUTE AS CALLER;
         		select @theId = DATABASE_PRINCIPAL_ID();
         		select @IsDbo = IS_MEMBER(N'db_owner'); 
         		if(@owner_id is null)
         			select @owner_id = @theId;
         		REVERT; 
         		
         		select @DiagId = diagram_id, @UIDFound = principal_id from dbo.sysdiagrams where principal_id = @owner_id and name = @diagramname 
         		if(@DiagId IS NULL or (@IsDbo = 0 and @UIDFound <> @theId))
         		begin
         			RAISERROR ('Diagram does not exist or you do not have permission.', 16, 1)
         			return -3
         		end
         	
         		delete from dbo.sysdiagrams where diagram_id = @DiagId;
         	
         		return 0;
         	END
         	
		 -- =============================================
         -- Author:		
         -- Create date: 
         -- Description:	
         -- =============================================
         -- Modified by:	ASD
         -- Modif date:  04/01/2011
         -- Description:	12503 Correction erreur de copier coller pour l'OF 
         -- =============================================
         -- EOU 03/04/2012 ajout LIBL_TRANSACTION et BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE
         -- 13323 
         -- =============================================
         -- DSZ 13579 12/06/2012 BLN_ACTIF ne doit pas ˆtre null
         -- =============================================
         -- TLE 13/07/2013 : 15143 : Modification taille NUM_IBAN (de 27 … 34)
         -- ====================================================
         -- HBO - 141113 - M16371: Lot 1 - Modification structure de donn‚es / proc‚dures stock‚es
         -- =============================================
         CREATE PROCEDURE INS_TRANSACTION
         	@ID_ETABLISSEMENT_DEST int,
         	@ID_ETABLISSEMENT_OF_DEST int,
         	@ID_ETABLISSEMENT_BENEF int,
         	@ID_ETABLISSEMENT_OF_BENEF int,
         	@ID_TIERS_BENEF int,
         	@ID_ADRESSE int,
         	@ID_CONTACT int,
         	@ID_TYPE_TRANSACTION int,
         	@ID_SOUS_TYPE_TRANSACTION int,
         	@ID_ACTIVITE int,
         	@BLN_ACTIF tinyint,
         	@ID_MODE_ENVOI_DOC int,
         	@NUM_IBAN varchar(34),
         	@ID_UTILISATEUR int,
         	@BLN_TRANSACTION_REGLEMENT_PRINCIPAL tinyint,
         	@BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE tinyint,
         	@LIBL_TRANSACTION varchar(50),
         	@BIC varchar(11)
         AS
         	BEGIN
         		IF @BLN_TRANSACTION_REGLEMENT_PRINCIPAL=1
         			BEGIN
         				IF @ID_ETABLISSEMENT_DEST IS NOT NULL
         			BEGIN
         				exec [dbo].[UPD_TRANSACTION_REG_PRINCIPAL_RAZ] @ID_ETABLISSEMENT_DEST,null,1
         			END
         
         		--IF @ID_ETABLISSEMENT_OF_BENEF IS NOT NULL
         		IF @ID_ETABLISSEMENT_OF_DEST IS NOT NULL
         			BEGIN
         				--20110104 ASD ajout "_OF"
         				exec [dbo].[UPD_TRANSACTION_REG_PRINCIPAL_RAZ] @ID_ETABLISSEMENT_OF_DEST,null,2
         			END
         	END
         
         	INSERT INTO [TRANSACTION]
         		(
         
         			ID_ETABLISSEMENT_DEST,
         			ID_ETABLISSEMENT_OF_DEST,
         			ID_ETABLISSEMENT_BENEF,
         			ID_ETABLISSEMENT_OF_BENEF,
         			ID_TIERS_BENEF,
         			ID_ADRESSE,
         			ID_CONTACT,
         			ID_TYPE_TRANSACTION,
         			ID_SOUS_TYPE_TRANSACTION,
         			ID_ACTIVITE,
         			BLN_ACTIF,
         			ID_MODE_ENVOI_DOC,
         			NUM_IBAN,
         			ID_UTILISATEUR,
         			BLN_TRANSACTION_REGLEMENT_PRINCIPAL,
         			BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE,
         			LIBL_TRANSACTION,
         			BIC
         		)
         		VALUES
         		(
         			@ID_ETABLISSEMENT_DEST,
         			@ID_ETABLISSEMENT_OF_DEST,
         			@ID_ETABLISSEMENT_BENEF,
         			@ID_ETABLISSEMENT_OF_BENEF,
         			@ID_TIERS_BENEF,
         			@ID_ADRESSE,
         			@ID_CONTACT,
         			@ID_TYPE_TRANSACTION,
         			@ID_SOUS_TYPE_TRANSACTION,
         			@ID_ACTIVITE,
         			isnull(@BLN_ACTIF,0),
         			@ID_MODE_ENVOI_DOC,
         			@NUM_IBAN,
         			@ID_UTILISATEUR,
         			@BLN_TRANSACTION_REGLEMENT_PRINCIPAL,
         			@BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE,
         			@LIBL_TRANSACTION,
         			@BIC
         		)
         RETURN SCOPE_IDENTITY()
         END


         -- =======================================================================================================================================
         -- Author		: WOOLLAMS
         -- Create date	: 8 f‚vrier 2008
         -- Description	: Lettre d'engagement pour l'adh‚rent
         -- Modif du 27/11/08 par SBR - adaptation au gabarit C2P
         -- Modif du 06/04/09 par SBR - adaptation au gabarit C2P suite au groupe de travail sur les ‚ditions
         -- Modif du 05/12/11 par BBL - modification du contenu
         -- Modif du 02/08/12 par DSZ - 13799 : heure -> h
         -- Modif du 18/09/12 par DSZ - 13697 : Ajout num tel de l'emetteur
         -- Modif du 18/02/13 par DSZ - 14832 Suppression Signature
         -- =======================================================================================================================================
         -- LDE/OPA 23/05/2014 : #213 En tant qu'utilisateur OPTIFORM, 
         -- lorsque j'‚dite un courrier (PEC, PRO) … destination d'un Adh‚rent ou d'un OF, je veux que, 
         -- l'adresse … afficher dans la zone "correspondance" soit l'adresse TSA propre … ce type de courrier 
         -- si le mode "TSA" s'applique sinon qu'elle soit l'adresse actuelle
         -- =======================================================================================================================================
         
         CREATE PROCEDURE [dbo].[EDT_LETTRE_ENGAGEMENT_CONTRAT_PRO_ADH]
         	@ID_ETABLISSEMENT	INT,
         	@ID_BENEFICIAIRE	INT,
         	@TYPE_BENEFICIAIRE	INT,
         	@ID_ADRESSE			INT,
         	@ID_CONTACT			INT,
         	@COD_CONTRAT_PRO	VARCHAR(10)
         AS
         BEGIN
         
         	-- Contact principal par d‚faut mais pas de "."
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
         			CR.ID_UTILISATEUR	AS ID_UTIL,
         			CR.LIB_PNM			AS LIB_PRENOM_CHARGE_RELATION,
         			CR.LIB_NOM			AS LIB_NOM_CHARGE_RELATION,
         			CR.LIB_VILLE,
         			CR.EMAIL			AS EMAIL_CHARGE_RELATION
         	INTO	#TEMP_REFERENCE
         	FROM	ADHERENT
         	JOIN	ETABLISSEMENT	
         	ON		ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         	JOIN	ADRESSE
         	ON		ETABLISSEMENT.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE
         	LEFT JOIN
         			UTILISATEUR CR -- charg‚ de relation
         	ON		ETABLISSEMENT.ID_CHARGEE_RELATION = CR.ID_UTILISATEUR
         	LEFT JOIN
         			UTILISATEUR CM -- charg‚ de mission
         	ON		ETABLISSEMENT.ID_CHARGEE_MISSION = CM.ID_UTILISATEUR
         	WHERE	ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT;
         
         	
         	SELECT	CONTRAT_PRO.ID_CONTRAT_PRO,
         			CONTRAT_PRO.COD_CONTRAT_PRO,
         			CONTRAT_PRO.LIBL_CONTRAT_PRO,
         			CONTRAT_PRO.LIB_LIEU_FORMATION,
         			CONTRAT_PRO.DAT_DEB_CONTRAT,
         			CONTRAT_PRO.DAT_FIN_CONTRAT,
         			CONTRAT_PRO.ID_TUTEUR,
         			dbo.GetFrenchCurrencyFormat(CAST(CONTRAT_PRO.NUM_DUREE_CONTRAT AS MONEY)) AS NUM_DUREE_CONTRAT,
         			INDIVIDU.NOM_INDIVIDU, 
         			INDIVIDU.PRENOM_INDIVIDU,
         			CASE WHEN INDIVIDU.BLN_MASCULIN  = 0 THEN
         					'Madame'
         				ELSE
         					'Monsieur'
         			END AS CIVILITE
         	INTO	#TMP_CONTRAT_PRO 
         	FROM	CONTRAT_PRO
         	JOIN	SALARIE_PRO
         	ON		CONTRAT_PRO.ID_SALARIE_PRO = SALARIE_PRO.ID_SALARIE_PRO
         	JOIN	INDIVIDU
         	ON		SALARIE_PRO.ID_INDIVIDU = INDIVIDU.ID_INDIVIDU
         	WHERE   CONTRAT_PRO.COD_CONTRAT_PRO = @COD_CONTRAT_PRO;
         	
         	SELECT DISTINCT #TMP_CONTRAT_PRO.ID_CONTRAT_PRO,
         					CONVERT(VARCHAR(8),MODULE_PRO.DAT_DEBUT, 3) AS DAT_DEBUT,
         					MODULE_PRO.ID_MODULE_PRO,
         					MODULE_PRO.COD_MODULE_PRO,
         					ORGANISME_FORMATION.LIB_RAISON_SOCIALE,
         					CASE WHEN MODULE_PRO.BLN_SUBROGE = 1 THEN
         							'Oui'
         						ELSE
         							'Non'
         					END AS SUBROGE,
         					MODULE_PRO.LIBL_MODULE_PRO,
         
         					MODULE_PRO.NB_UNITE_FORMATION,
         					MODULE_PRO.NB_UNITE_TOTALE,
         					CASE WHEN TYPE_FORMATION.COD_TYPE_FORMATION = 'FTUT' THEN
         							CAST(MODULE_PRO.NB_UNITE_TOTALE AS VARCHAR(15))+' mois'
         						 ELSE
         							CAST(MODULE_PRO.NB_UNITE_TOTALE AS VARCHAR(15))+' h'
         					END AS NB_UNITE_FORMATION_AFFICHAGE,
         
         					CASE WHEN TYPE_FORMATION.COD_TYPE_FORMATION = 'FTUT' THEN
         							'0 ?/mois'
         						ELSE
         							CAST(COALESCE(MODULE_PRO.TAUX_HORAIRE_RETENU_OF,0) AS VARCHAR(15)) +' ?/h'
         					END AS TAUX_OF,
         					
         					CASE WHEN TYPE_FORMATION.COD_TYPE_FORMATION = 'FTUT' THEN
         							CAST(COALESCE(MODULE_PRO.TAUX_HORAIRE_RETENU_ADH,0) AS VARCHAR(15)) +' ?/mois'
         						ELSE
         							CAST(COALESCE(MODULE_PRO.TAUX_HORAIRE_RETENU_ADH,0) AS VARCHAR(15)) +' ?/h'
         					END AS TAUX_PEC,
         					
         					MODULE_PRO.NB_UNITE_TOTALE * MODULE_PRO.TAUX_HORAIRE_MODULE AS MONTANT,
         					CAST((MODULE_PRO.NB_UNITE_TOTALE * MODULE_PRO.TAUX_HORAIRE_MODULE) AS VARCHAR(15) )+' ?' AS MONTANT_AFFICHAGE,
         					TYPE_FORMATION.ID_TYPE_FORMATION,
         					TYPE_FORMATION.NB_UNITE_MAX,
         					TYPE_FORMATION.COD_TYPE_FORMATION,
         					
         					CASE WHEN INDIVIDU.BLN_MASCULIN  = 0 THEN
         							'Madame'
         						ELSE
         							'Monsieur'
         					END AS CIVILITE_TUTEUR,
         					INDIVIDU.PRENOM_INDIVIDU AS PRENOM_TUTEUR,
         					INDIVIDU.NOM_INDIVIDU	 AS NOM_TUTEUR
         	INTO			#TMP_MODULE_PRO	
         	FROM			#TMP_CONTRAT_PRO
         	JOIN			MODULE_PRO 
         	ON				#TMP_CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         	AND				MODULE_PRO.BLN_ACTIF = 1
         	LEFT JOIN		ETABLISSEMENT_OF 
         	ON				MODULE_PRO.ID_ETABLISSEMENT_OF = ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF 
         	LEFT JOIN		ORGANISME_FORMATION 
         	ON				ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF
         	JOIN			TYPE_FORMATION 
         	ON				MODULE_PRO.ID_TYPE_FORMATION = TYPE_FORMATION.ID_TYPE_FORMATION
         	LEFT JOIN		INDIVIDU 
         	ON				INDIVIDU.ID_INDIVIDU = #TMP_CONTRAT_PRO.ID_TUTEUR
         	AND				INDIVIDU.BLN_ACTIF = 1;
         
         	
         	WITH XMLNAMESPACES (
         		DEFAULT 'EDT_LETTRE_ENGAGEMENT_CONTRAT_PRO_ADH'
         	)
         	
         	SELECT
         		-- R‚cup‚ration des informations sur le contact et le b‚n‚ficiaire
         		dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) AS BENEFICIAIRE,
         		(
         			SELECT
         					
         				ISNULL(COD_ADHERENT, '')				AS COD_ADHERENT,
         				ISNULL(LIB_PRENOM_CHARGE_RELATION, '')	AS LIB_PRENOM_CONTACT,
         				ISNULL(LIB_NOM_CHARGE_RELATION, '')		AS LIB_NOM_CONTACT,
         				ISNULL(EMAIL_CHARGE_RELATION, '')		AS EMAIL,
         
         				-- R‚cup‚ration des informations sur l'‚metteur
         				dbo.GetXmlAdrUtilAvecTel(ENTETE.ID_UTIL, (SELECT TOP 1 BLN_TSA FROM ETABLISSEMENT WHERE ID_ETABLISSEMENT = @ID_BENEFICIAIRE), 1) AS EMETTEUR,
         
         				-- r‚f‚rence du contrat pro
         				(
         					SELECT	#TMP_CONTRAT_PRO.COD_CONTRAT_PRO AS REF_CONTRAT
         					FROM	#TMP_CONTRAT_PRO
         					FOR XML PATH(''), TYPE 
         				),
         				ENTETE.NUM_SIRET AS SIRET,
         				RTRIM(CASE WHEN PATINDEX('%CEDEX%', LIB_VILLE) <> 0 THEN LEFT(LIB_VILLE, PATINDEX('%CEDEX%', LIB_VILLE)-1) ELSE LIB_VILLE END) AS LIB_VILLE, -- retraitement de la ville au cas o— de la forme COMMUNE CEDEX 999
         				dbo.GetFullDate(getDate()) AS DATE,				
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
         			SELECT	' ' + CORPS.LIBL_CONTRAT_PRO + ' ' AS LIBL_CPRO,
         					' ' + CORPS.CIVILITE + ' ' AS CIVILITE_CPRO,
         					CORPS.NOM_INDIVIDU + ' ' AS NOM_INDIVIDU_CPRO,
         					CORPS.PRENOM_INDIVIDU + ' ' AS PNM_INDIVIDU_CPRO,
         					CONVERT(VARCHAR, CORPS.DAT_DEB_CONTRAT, 103) AS DEB_CPRO,
         					CONVERT(VARCHAR, CORPS.DAT_FIN_CONTRAT, 103) AS FIN_CPRO,
         --						CORPS.LIBL_CONTRAT_PRO,
         --						CORPS.LIB_LIEU_FORMATION,
         --						CONVERT(VARCHAR, CORPS.DAT_DEB_CONTRAT, 103) AS DAT_DEB_CONTRAT,
         --						CONVERT(VARCHAR, CORPS.DAT_FIN_CONTRAT, 103) AS DAT_FIN_CONTRAT,
         --						CORPS.CIVILITE,
         --						CORPS.PRENOM_INDIVIDU,
         --						CORPS.NOM_INDIVIDU,
         					' ' + CONVERT(VARCHAR, CORPS.NUM_DUREE_CONTRAT, 0) + ' ' AS NUM_DUREE_CONTRAT,
         					(
         						SELECT 								
         							(
         								SELECT	 CONVERT(VARCHAR, #TMP_MODULE_PRO.DAT_DEBUT, 103) AS DAT_DEBUT,
         											#TMP_MODULE_PRO.COD_MODULE_PRO, 
         											COALESCE(#TMP_MODULE_PRO.LIB_RAISON_SOCIALE, '') AS LIB_RAISON_SOCIALE,
         											COALESCE(#TMP_MODULE_PRO.SUBROGE, '') AS SUBROGE,
         											CASE WHEN #TMP_MODULE_PRO.COD_TYPE_FORMATION <> 'FTUT' THEN
         													COALESCE(#TMP_MODULE_PRO.LIBL_MODULE_PRO, '')
         											ELSE
         													'Fonction tutorale'
         											END AS LIBL_MODULE_PRO,
         											#TMP_MODULE_PRO.NB_UNITE_FORMATION_AFFICHAGE,
         											#TMP_MODULE_PRO.TAUX_OF,
         											#TMP_MODULE_PRO.TAUX_PEC,
         											#TMP_MODULE_PRO.MONTANT_AFFICHAGE
         								FROM     #TMP_MODULE_PRO
         								ORDER BY #TMP_MODULE_PRO.DAT_DEBUT
         								FOR XML RAW('MODULE'), ELEMENTS, TYPE
         							),
         							(
         								SELECT	  --CAST((SUM(CASE WHEN #TMP_MODULE_PRO.COD_TYPE_FORMATION <> 'FTUT' THEN #TMP_MODULE_PRO.NB_UNITE_TOTALE ELSE 0 END)) AS VARCHAR(15))+' h'	AS TOTAL_UNITE_FORMATION,
         											CAST((SUM(#TMP_MODULE_PRO.MONTANT)) AS VARCHAR(15)) +' ?'	AS TOTAL_MONTANT
         								FROM	  #TMP_MODULE_PRO
         								GROUP BY  #TMP_MODULE_PRO.ID_CONTRAT_PRO
         								FOR XML RAW('TOTAL'),ELEMENTS, TYPE
         							)
         						FOR XML RAW('MODULES'),ELEMENTS, TYPE, ROOT('TABLEAU')
         					)
         
         --						CASE WHEN ((SELECT  COUNT(*) FROM #TMP_MODULE_PRO WHERE #TMP_MODULE_PRO.COD_TYPE_FORMATION = 'FTUT')  > 0 ) THEN (
         --							SELECT	
         --									 dbo.GetFrenchCurrencyFormat(CAST((	SELECT TOP(1) PARAM_TAUX_CONTRAT_PRO.NUM_TAUX_HORAIRE_FORFAIT_HT 
         --																		FROM   PARAM_TAUX_CONTRAT_PRO
         --																		WHERE  PARAM_TAUX_CONTRAT_PRO.ID_TYPE_FORMATION = #TMP_MODULE_PRO.ID_TYPE_FORMATION) * #TMP_MODULE_PRO.NB_UNITE_MAX AS MONEY)
         --																) AS MONTANT_AFFICHAGE_TUT,
         --									 dbo.GetFrenchCurrencyFormat(CAST(#TMP_MODULE_PRO.NB_UNITE_MAX AS MONEY)) AS NB_UNITE_MAX,
         --									 #TMP_MODULE_PRO.CIVILITE_TUTEUR,
         --									 #TMP_MODULE_PRO.PRENOM_TUTEUR,
         --									 #TMP_MODULE_PRO.NOM_TUTEUR
         --							FROM     #TMP_MODULE_PRO
         --							WHERE	 #TMP_MODULE_PRO.COD_TYPE_FORMATION = 'FTUT'
         --							FOR XML RAW('ASTERISQUE'),ELEMENTS, TYPE
         --						)END
         
         			FROM #TMP_CONTRAT_PRO AS CORPS     
         			FOR XML AUTO, ELEMENTS, TYPE
         		),
         
         		(
         			SELECT	TOP 1 DBO.GETCONTACTSALUTATION(@ID_CONTACT, 1)
         			FROM CIVILITE
         			FOR XML PATH('POLITESSE_BAS'), TYPE
         		)			
         
         	FROM	#TEMP_REFERENCE as LETTRE
         	FOR XML AUTO, ELEMENTS
         END



CREATE PROCEDURE [BATCH_TRANSFERT_PRESCRIPTION_COMPTE_PLAN_OBLIGATOIRE]
         /*
         =============================================  
         Author  : MBL
         Create date : 03/03/2015
         Description : Proc‚dure permettant de lancer des tranferts de prescription du compte Plan Obligatoire
         (@COD_TYPE_EVENEMENT = 'PRESCPOB')
         
         Le traitement fait appel a la fonction de table F_TRANSFERT_PRESCRIPTION_COMPTE_PLAN_OBLIGATOIRE constituant un outil d'aide … la d‚cision afin de g‚n‚rer ces transferts.
         
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
         
         	SET @COD_TYPE_EVENEMENT	 = 'PRESCPOB'
         	
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
         		SELECT @ID_TYPE_FINANCEMENT = 4 -- Compte Plan Obligatoire
         		
         		SELECT t.*, ID_ACTIVITE_PLAN_N = ACTIVITE_ADH.ID_ACTIVITE
         		INTO #TMP_TRANSFERT 
         		FROM F_TRANSFERT_PRESCRIPTION_COMPTE_PLAN_OBLIGATOIRE(@NUM_ANNEE_N, @ID_GROUPE_TRAITE) t
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
         
         
         		SET @LIBL_EVENEMENT		= 'Prescription Cpt Plan Obligatoire '	+ CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
         		SET @LIBL_MVT			= 'Prescription Cpt Plan Obligatoire '	+ CAST(@NUM_ANNEE_N AS VARCHAR(4)) 
         
         
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
         

         -- =============================================
         -- Author			: XX
         -- Create date		: XX XXXX 2007
         -- Description		: Cr‚ation
         -- =============================================
         -- Author			: SV
         -- Modified date	: 20 novembre 2007
         -- Description		: Ajout d'une contrainte sur la date de d‚sengagement du P.C.E.
         -- =============================================
         -- Author			: DSZ
         -- Modified date	: 24/06/2009
         -- Description		: ajout return pour pouvoir utiliser dans UPD_DISPOSITIF_STAGIAIRE. Liee a Mantis 12078
         -- =============================================
         -- DSZ 15/10/2010 reecriture totale en vue de performance  (12423)
         -- ATTENTION: renommage de colonne de resultat! + revu ASD 20101018
         -- =============================================
         
         CREATE PROCEDURE [dbo].[ACTION_ENGAGED]
         	@ID_ACTION int,
         	@ID_MODULE int
         AS
         BEGIN
         
         	-- 1 ENGAGED
         	-- 0 NOT ENGAGED
         	DECLARE @CNT int
         	if (@ID_MODULE is not null)
         		SELECT @CNT = COUNT(*) 
         		FROM
         			POSTE_COUT_ENGAGE
         				INNER JOIN ENGAGEMENT			ON POSTE_COUT_ENGAGE.ID_ENGAGEMENT = ENGAGEMENT.ID_ENGAGEMENT
         												AND ENGAGEMENT.ID_TYPE_ENGAGEMENT <> 2
         												AND ENGAGEMENT.DAT_BAE IS NOT NULL
         		WHERE  
         			POSTE_COUT_ENGAGE.ID_MODULE_PEC = @ID_MODULE
         			AND POSTE_COUT_ENGAGE.MNT_ENGAGE_HT >= 0
         			AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT is null
         	else if (@ID_ACTION is not null)
         		SELECT @CNT = COUNT(*) 
         		FROM
         			MODULE_PEC
         				INNER JOIN ENGAGEMENT			ON ENGAGEMENT.ID_ACTION_PEC = @ID_ACTION
         													AND ENGAGEMENT.ID_TYPE_ENGAGEMENT <> 2
         													AND ENGAGEMENT.DAT_BAE IS NOT NULL
         				INNER JOIN POSTE_COUT_ENGAGE	ON POSTE_COUT_ENGAGE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         													AND POSTE_COUT_ENGAGE.ID_ENGAGEMENT = ENGAGEMENT.ID_ENGAGEMENT
         													AND POSTE_COUT_ENGAGE.MNT_ENGAGE_HT >= 0
         													AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT is null
         		WHERE 
         			(MODULE_PEC.ID_ACTION_PEC = @ID_ACTION)
         	else
         		SELECT @CNT = COUNT(*) 
         		FROM
         			MODULE_PEC
         				INNER JOIN ENGAGEMENT			ON ENGAGEMENT.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
         													AND ENGAGEMENT.ID_TYPE_ENGAGEMENT <> 2
         													AND ENGAGEMENT.DAT_BAE IS NOT NULL
         				INNER JOIN POSTE_COUT_ENGAGE	ON POSTE_COUT_ENGAGE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         													AND POSTE_COUT_ENGAGE.ID_ENGAGEMENT = ENGAGEMENT.ID_ENGAGEMENT
         													AND POSTE_COUT_ENGAGE.MNT_ENGAGE_HT >= 0
         													AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT is null
         			
         	/*
         	SELECT
         		@CNT = COUNT(*) 
         	FROM
         		ACTION_PEC
         			INNER JOIN ENGAGEMENT			ON ENGAGEMENT.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         												AND ENGAGEMENT.ID_TYPE_ENGAGEMENT <> 2
         												AND ENGAGEMENT.DAT_BAE IS NOT NULL
         			INNER JOIN MODULE_PEC			ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         												AND
         												(
         													(ACTION_PEC.ID_ACTION_PEC = @ID_ACTION OR @ID_ACTION IS NULL)
         													AND	(MODULE_PEC.ID_MODULE_PEC = @ID_MODULE OR @ID_MODULE IS NULL)
         												)
         			INNER JOIN POSTE_COUT_ENGAGE	ON POSTE_COUT_ENGAGE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         												AND POSTE_COUT_ENGAGE.ID_ENGAGEMENT = ENGAGEMENT.ID_ENGAGEMENT
         												AND POSTE_COUT_ENGAGE.MNT_ENGAGE_HT >= 0
         												AND POSTE_COUT_ENGAGE.DAT_DESENGAGEMENT is null
         	
         	select @CNT AS MODIFIABLE
         	*/
         	select @CNT AS ENGAGED
         	return @CNT
         END
                 
         
         
         -- =============================================
         -- Author		: SAFI
         -- Create date	: 23-06-2010
         -- Description	: Insertion d'‚venement administratif pour un 
         -- Relance Edition comp. etablissemnet 
         -- =============================================
         -- 07/07/2010 DSZ
         -- @ID_TYPE_EVENEMENT_ADMIN calcul‚ dans la procedure, supprim‚ des parametres
         -- ajout insert  EDITION_DOSSIERS
         -- =============================================
         -- 20/07/2010 ASD
         -- Modif libell‚ Evenement administratif pour r‚cup‚rer le libell‚ du type d'‚vŠnement administratif
         -- Gestion comparaison MAX DAT BAP OF et DAT EVENEMENT ADMIN
         -- =============================================
         
         CREATE PROCEDURE [dbo].[INS_EVENEMENT_ADMIN_RELANCE_COMP_ETAB_PRO]
         	@COD_CONTRAT_PRO	VARCHAR(10),
         	@ID_UTILISATEUR INT,
         	@ID_EDITION INT
         AS 
         BEGIN
         	DECLARE @ID_TYPE_EVENEMENT_ADMIN INT
         	DECLARE @LIB_MOTIF VARCHAR(250)
         	DECLARE @ID_CONTRAT_PRO INT
         	DECLARE @ID_EVENEMENT_ADMIN INT
         	--20100720 ASD ADD declare
         	DECLARE @MAX_DAT_BAP_SESSION DATETIME
         
         	SELECT @ID_CONTRAT_PRO = ID_CONTRAT_PRO
         	FROM CONTRAT_PRO WHERE COD_CONTRAT_PRO = @COD_CONTRAT_PRO
         
         
         	--20100720 ASD ADD select
         	SELECT @MAX_DAT_BAP_SESSION = max(coalesce(SESSION_PRO.DAT_BAP_OF, '01/01/1900')) 
         	from SESSION_PRO 
         	INNER JOIN MODULE_PRO ON MODULE_PRO.ID_CONTRAT_PRO = @ID_CONTRAT_PRO
         	AND MODULE_PRO.ID_MODULE_PRO = SESSION_PRO.ID_MODULE_PRO
         
         	---ajout DSZ 07/07/2010
         	--20100720 ASD add SET et modif SELECT pour exploiter @MAX_DAT_BAP_SESSION
         	SET @ID_TYPE_EVENEMENT_ADMIN = 15
         	SELECT top 1 @ID_TYPE_EVENEMENT_ADMIN = coalesce(EVENEMENT_ADMIN.ID_TYPE_EVENEMENT_ADMIN+1,15)
         	FROM EVENEMENT_ADMIN 
         	WHERE 
         		ID_CONTRAT_PRO = @ID_CONTRAT_PRO 
         		AND EVENEMENT_ADMIN.ID_TYPE_EVENEMENT_ADMIN IN (15,16,17) 
         		AND datediff(d, DAT_EVENEMENT_ADMIN, @MAX_DAT_BAP_SESSION)<=0
         	ORDER BY DAT_EVENEMENT_ADMIN DESC
         --select @ID_TYPE_EVENEMENT_ADMIN
         
         	if (@ID_TYPE_EVENEMENT_ADMIN  in (15,16,17) ) 
         	begin
         	--<- fin ajout DSZ 07/07/2010
         	-- 20100720 ASD add r‚cup‚ration libell‚ ‚venement
         	SET @LIB_MOTIF = 'RELANCE COMP ETAB '
         	SELECT @LIB_MOTIF = coalesce(LIBL_TYPE_EVENEMENT_ADMIN, @LIB_MOTIF)
         	FROM TYPE_EVENEMENT_ADMIN
         	WHERE TYPE_EVENEMENT_ADMIN.ID_TYPE_EVENEMENT_ADMIN = @ID_TYPE_EVENEMENT_ADMIN
         	-- Cr‚ation ou MaJ d'un contrat
         
         			INSERT INTO EVENEMENT_ADMIN
         				(
         					COD_EVENEMENT_ADMIN, 
         					ID_UTILISATEUR, 
         					BLN_ACTIF, 
         					ID_TYPE_EVENEMENT_ADMIN,	
         					LIBL_EVENEMENT_ADMIN,	
         					DAT_EVENEMENT_ADMIN,		
         					ID_CONTRAT_PRO
         				)
         			VALUES
         				(	'',
         					@ID_UTILISATEUR, 
         					1,
         					@ID_TYPE_EVENEMENT_ADMIN, 
         					isnull(@LIB_MOTIF, ''), 
         					GETDATE(),
         					@ID_CONTRAT_PRO)
         
         			SET @ID_EVENEMENT_ADMIN = @@IDENTITY
         		
         			UPDATE EVENEMENT_ADMIN 
         			SET COD_EVENEMENT_ADMIN = ID_EVENEMENT_ADMIN 
         			WHERE 
         				ID_EVENEMENT_ADMIN = @@IDENTITY AND 
         				ID_TYPE_EVENEMENT_ADMIN = @ID_TYPE_EVENEMENT_ADMIN
         
         
         			INSERT INTO EDITION_DOSSIERS 
         				(ID_EDITION, ID_EVENEMENT_ADMIN)
         			VALUES (@ID_EDITION, @ID_EVENEMENT_ADMIN)
         		end
         
         END
         ------------------------------------------------
         
         
         
         
         -- modif DSZ 29/09/2010 12503
         -- lorsqu'on desactive la transaction, elle ne doit pas ˆtre principale
         -- =============================================
         -- EOU 03/04/2012 ajout LIBL_TRANSACTION et BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE
         -- 13323 
         -- =============================================
         -- DSZ 13579 12/06/2012 BLN_ACTIF ne doit pas ˆtre null
         -- =============================================
         -- TLE 13/07/2013 : 15143 : Modification taille NUM_IBAN (de 27 … 34)
         -- ====================================================
         -- HBO - 141113 - M16371: Lot 1 - Modification structure de donn‚es / proc‚dures stock‚es
         -- =============================================
         -- HBO - 20140905 - #442 Exclure les rŠglements
         -- =============================================
         CREATE PROCEDURE [dbo].[UPD_TRANSACTION]
         	@ID_TRANSACTION								INT,
         	@TIME_STAMP									TIMESTAMP,
         	@ID_ETABLISSEMENT_DEST						INT,
         	@ID_ETABLISSEMENT_OF_DEST					INT,	
         	@ID_ETABLISSEMENT_BENEF						INT,
         	@ID_ETABLISSEMENT_OF_BENEF					INT,
         	@ID_TIERS_BENEF								INT,
         	@ID_ADRESSE									INT,
         	@ID_CONTACT									INT,
         	@ID_TYPE_TRANSACTION						INT,
         	@ID_SOUS_TYPE_TRANSACTION					INT,
         	@ID_ACTIVITE								INT,
         	@BLN_ACTIF									TINYINT,
         	@ID_MODE_ENVOI_DOC							INT,
         	@NUM_IBAN									VARCHAR(34),
         	@BLN_TRANSACTION_REGLEMENT_PRINCIPAL		TINYINT,
         	@ID_UTILISATEUR								INT,
         	@BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE	TINYINT,
         	@LIBL_TRANSACTION							VARCHAR(50),
         	@BIC										VARCHAR(11)
         AS
         BEGIN
         	--SELECT
         	--	@BIC = ISNULL(@BIC,BIC),
         	--	@NUM_IBAN = ISNULL(@NUM_IBAN,NUM_IBAN)
         	--FROM
         	--	[TRANSACTION]
         	--WHERE
         	--	ID_TRANSACTION = @ID_TRANSACTION
         
         	IF (ISNULL(@BLN_ACTIF,0) = 0)
         	BEGIN
         		SET @BLN_TRANSACTION_REGLEMENT_PRINCIPAL = 0
         	END
         
         	IF (@BLN_TRANSACTION_REGLEMENT_PRINCIPAL = 1)
         	BEGIN
         		IF (@ID_ETABLISSEMENT_DEST IS NOT NULL)
         		BEGIN
         			EXEC [dbo].[UPD_TRANSACTION_REG_PRINCIPAL_RAZ] @ID_ETABLISSEMENT_DEST, @ID_TRANSACTION, 1
         		END
         
         		IF (@ID_ETABLISSEMENT_OF_DEST IS NOT NULL)
         		BEGIN
         			EXEC [dbo].[UPD_TRANSACTION_REG_PRINCIPAL_RAZ] @ID_ETABLISSEMENT_OF_DEST, @ID_TRANSACTION, 2
         		END
         	END
         
         	UPDATE
         		[TRANSACTION]
         	SET
         		ID_ETABLISSEMENT_DEST = @ID_ETABLISSEMENT_DEST,
         		ID_ETABLISSEMENT_OF_DEST = @ID_ETABLISSEMENT_OF_DEST,
         		ID_ETABLISSEMENT_BENEF = @ID_ETABLISSEMENT_BENEF,
         		ID_ETABLISSEMENT_OF_BENEF = @ID_ETABLISSEMENT_OF_BENEF,
         		ID_TIERS_BENEF = @ID_TIERS_BENEF,
         		ID_ADRESSE = @ID_ADRESSE,
         		ID_CONTACT = @ID_CONTACT,
         		ID_TYPE_TRANSACTION = @ID_TYPE_TRANSACTION,
         		ID_SOUS_TYPE_TRANSACTION = @ID_SOUS_TYPE_TRANSACTION,
         		ID_ACTIVITE = @ID_ACTIVITE,
         		BLN_ACTIF = isnull(@BLN_ACTIF,0),
         		ID_MODE_ENVOI_DOC = @ID_MODE_ENVOI_DOC,
         		NUM_IBAN = @NUM_IBAN,
         		BLN_TRANSACTION_REGLEMENT_PRINCIPAL = @BLN_TRANSACTION_REGLEMENT_PRINCIPAL,
         		BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE = @BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE,
         		LIBL_TRANSACTION = @LIBL_TRANSACTION,
         		ID_UTILISATEUR = @ID_UTILISATEUR,
         		DAT_MODIF = getDate(),
         		BIC = @BIC
         	WHERE
         		ID_TRANSACTION = @ID_TRANSACTION
         
         	IF (@@ROWCOUNT = 0)
         	BEGIN
         		IF EXISTS(SELECT * FROM [TRANSACTION] WHERE ID_TRANSACTION  = @ID_TRANSACTION)
         		BEGIN
         			/* ProblŠme de Concurrence d'accŠs */
         			RAISERROR(50001, 16, 1)
         		END
         		ELSE
         		BEGIN
         			/* L'enregistrement a ‚t‚ d‚truit */
         			RAISERROR(50002, 16, 1)
         		END
         	END
         END

         
         -- =============================================================
         -- Author		: WOOLLAMS
         -- Create date	: 8 f‚vrier 2008
         -- Description	: Lettre d'engagement pour l'adh‚rent
         -- Modif du 28/11/08 par SBR - adaptation au gabarit C2P
         -- Modif du 06/04/09 par SBR - adaptation au gabarit C2P suite au groupe de travail sur les ‚ditions
         -- Modif du 17/12/11 par BBL - modification contenu 
         -- Modif du 18/09/12 par DSZ - 13697 Ajout num tel de l'emetteur
         -- Modif du 18/02/13 par DSZ - 14832 Suppression Signature
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         -- =============================================================
         -- LDE 23/05/2014 #213
         -- =============================================================
         CREATE PROCEDURE [dbo].[EDT_LETTRE_ENGAGEMENT_CONTRAT_PRO_OF]
         	@ID_ETABLISSEMENT	INT,
         	@ID_BENEFICIAIRE	INT,
         	@TYPE_BENEFICIAIRE	INT,
         	@ID_ADRESSE			INT,
         	@ID_CONTACT			INT,
         	@COD_CONTRAT_PRO	VARCHAR(10)
         AS
         BEGIN
         
         	DECLARE @NUM_DUREE_CONTRAT decimal(18,2)
         
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
         			CONTRAT_PRO.LIBL_CONTRAT_PRO,
         			CONTRAT_PRO.DAT_DEB_CONTRAT,
         			CONTRAT_PRO.DAT_FIN_CONTRAT,
         			ADHERENT.COD_ADHERENT,
         			coalesce(ETABLISSEMENT.LIB_ENSEIGNE, ADHERENT.LIB_RAISON_SOCIALE) as LIB_RAISON_SOCIALE,
         			ORGANISME_FORMATION.COD_OF,
         			CONTRAT_PRO.ID_TUTEUR,
         			CR.ID_UTILISATEUR	as ID_UTIL,
         			CR.LIB_PNM			as LIB_PRENOM_CHARGE_RELATION,
         			CR.LIB_NOM			as LIB_NOM_CHARGE_RELATION,
         			CR.LIB_VILLE,
         			CR.EMAIL			as EMAIL_CHARGE_RELATION,
         			INDIVIDU.NOM_INDIVIDU, 
         			INDIVIDU.PRENOM_INDIVIDU,
         			CASE WHEN INDIVIDU.BLN_MASCULIN  = 0 THEN
         					'Madame'
         				ELSE
         					'Monsieur'
         			END AS CIVILITE
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
         
         
         	SELECT @NUM_DUREE_CONTRAT =  SUM(MODULE_PRO.NB_UNITE_TOTALE)
         	FROM    #TMP_CONTRAT_PRO 
         			INNER JOIN MODULE_PRO ON #TMP_CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO 
         						AND MODULE_PRO.BLN_ACTIF = 1 AND MODULE_PRO.BLN_SUBROGE = 1
         			INNER JOIN ENGAGEMENT_PRO ON MODULE_PRO.ID_ENGAGEMENT_PRO = ENGAGEMENT_PRO.ID_ENGAGEMENT_PRO
         			LEFT JOIN ETABLISSEMENT_OF ON MODULE_PRO.ID_ETABLISSEMENT_OF = ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF
         	WHERE		ENGAGEMENT_PRO.DAT_BAE IS NOT NULL AND ENGAGEMENT_PRO.DAT_LETTRE_ENGAGEMENT_PRO_OF IS NULL
         			AND ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_ETABLISSEMENT;
         
         	
         	SELECT DISTINCT #TMP_CONTRAT_PRO.ID_CONTRAT_PRO,
         					CONVERT(VARCHAR(8),MODULE_PRO.DAT_DEBUT, 3) AS DAT_DEBUT,
         					MODULE_PRO.COD_MODULE_PRO,
         					ORGANISME_FORMATION.LIB_RAISON_SOCIALE,
         					MODULE_PRO.ID_MODULE_PRO,
         					MODULE_PRO.LIBL_MODULE_PRO,
         					MODULE_PRO.NB_UNITE_FORMATION,
         					MODULE_PRO.NB_UNITE_TOTALE,
         					CAST(MODULE_PRO.NB_UNITE_TOTALE AS VARCHAR(15))+' h' AS NB_UNITE_FORMATION_AFFICHAGE,
         					CAST(MODULE_PRO.TAUX_HORAIRE_RETENU_OF AS VARCHAR(15)) +' ?/heure'AS TAUX_HORAIRE_MODULE_AFFICHAGE,
         					MODULE_PRO.NB_UNITE_TOTALE * MODULE_PRO.TAUX_HORAIRE_RETENU_OF AS MONTANT,
         					CAST((MODULE_PRO.NB_UNITE_TOTALE * MODULE_PRO.TAUX_HORAIRE_RETENU_OF) AS VARCHAR(15))+' ?' AS MONTANT_AFFICHAGE
         	INTO			#TMP_MODULE_PRO	
         	FROM			#TMP_CONTRAT_PRO
         					INNER JOIN MODULE_PRO ON #TMP_CONTRAT_PRO.ID_CONTRAT_PRO = MODULE_PRO.ID_CONTRAT_PRO
         												AND MODULE_PRO.BLN_ACTIF = 1 AND MODULE_PRO.BLN_SUBROGE = 1
         					LEFT JOIN ETABLISSEMENT_OF ON MODULE_PRO.ID_ETABLISSEMENT_OF = ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF
         					INNER JOIN ORGANISME_FORMATION ON ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF
         	WHERE			ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_ETABLISSEMENT;
         
         	
         	SELECT  DISTINCT PIECE_PRO.ID_PIECE_PRO
         	INTO	#TMP_SUBRO	
         	FROM	#TMP_MODULE_PRO 
         			INNER JOIN ARRIVEE_PIECE_PRO ON #TMP_MODULE_PRO.ID_MODULE_PRO = ARRIVEE_PIECE_PRO.ID_MODULE_PRO 
         			INNER JOIN PIECE_PRO ON ARRIVEE_PIECE_PRO.ID_PIECE_PRO = PIECE_PRO.ID_PIECE_PRO
         	WHERE		PIECE_PRO.COD_PIECE_PRO = 'SUBRO'
         			AND ARRIVEE_PIECE_PRO.BLN_ACTIF = 0;
         
         	
         	WITH XMLNAMESPACES (
         		DEFAULT 'EDT_LETTRE_CONTRAT_PRO_ENGAGEMENT_OF'
         	)
         	
         			
         	SELECT
         			-- R‚cup‚ration des informations sur le contact et le b‚n‚ficiaire
         			dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) as BENEFICIAIRE,
         			(
         				
         				
         				
         				SELECT	              	
         					
         						isnull(COD_OF, '')						as COD_OF,
         						isnull(LIB_PRENOM_CHARGE_RELATION, '')	as LIB_PRENOM_CHARGE_RELATION,
         						isnull(LIB_NOM_CHARGE_RELATION, '')		as LIB_NOM_CHARGE_RELATION,
         						isnull(EMAIL_CHARGE_RELATION, '')		as EMAIL,
         
         						-- R‚cup‚ration des informations sur l'‚metteur
         						dbo.GetXmlAdrUtilAvecTel(ENTETE.ID_UTIL, dbo.DossierEstTSA(@COD_CONTRAT_PRO, 0), 0)		as EMETTEUR, -- LDE 23/05/2014 #213
         
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
         									dbo.GetContactSalutationOF(@ID_CONTACT, 1)
         							FROM CIVILITE
         							FOR XML PATH('POLITESSE_HAUT'), TYPE
         						)
         
         				FROM #TMP_CONTRAT_PRO AS ENTETE     
         				FOR XML AUTO, ELEMENTS, TYPE
         			),
         
         			(
         				
         				
         				
         				SELECT	
         						' '+CORPS.LIBL_CONTRAT_PRO+' ' as LIBL_CPRO,
         						' '+CORPS.LIB_RAISON_SOCIALE+' ' as LIB_RAISON_SOCIALE,
         						' '+CORPS.CIVILITE+' ' as CIVILITE_CPRO,
         						CORPS.NOM_INDIVIDU+' ' as NOM_INDIVIDU_CPRO,
         						CORPS.PRENOM_INDIVIDU+' ' as PNM_INDIVIDU_CPRO,
         						convert(varchar, CORPS.DAT_DEB_CONTRAT, 103) as DEB_CPRO,
         						convert(varchar, CORPS.DAT_FIN_CONTRAT, 103) as FIN_CPRO,
         
         --						CORPS.LIBL_CONTRAT_PRO + ' sign‚ entre ' + CORPS.LIB_RAISON_SOCIALE +
         --						' et ' + CORPS.CIVILITE + ' ' + CORPS.NOM_INDIVIDU + ' ' + CORPS.PRENOM_INDIVIDU +
         --						' pour la p‚riode du ' + convert(varchar, CORPS.DAT_DEB_CONTRAT, 103) + 
         --						' au ' + convert(varchar, CORPS.DAT_FIN_CONTRAT, 103) AS INFO_CPRO,
         --
         						dbo.GetFrenchCurrencyFormat(CAST(@NUM_DUREE_CONTRAT AS MONEY)) AS NUM_DUREE_CONTRAT,
         						(
         							SELECT 								
         								(
         									SELECT	 #TMP_MODULE_PRO.DAT_DEBUT,
         											 #TMP_MODULE_PRO.COD_MODULE_PRO,			
         											 #TMP_MODULE_PRO.LIBL_MODULE_PRO, 
         											 #TMP_MODULE_PRO.NB_UNITE_FORMATION_AFFICHAGE,
         											 #TMP_MODULE_PRO.TAUX_HORAIRE_MODULE_AFFICHAGE,
         											 #TMP_MODULE_PRO.MONTANT_AFFICHAGE
         									FROM     #TMP_MODULE_PRO
         									ORDER BY #TMP_MODULE_PRO.DAT_DEBUT
         									FOR XML RAW('MODULE'), ELEMENTS, TYPE
         								),
         								(
         									SELECT	  CAST((SUM(#TMP_MODULE_PRO.NB_UNITE_TOTALE)) AS VARCHAR(15))+' h'	AS TOTAL_UNITE_FORMATION,
         											  CAST((SUM(#TMP_MODULE_PRO.MONTANT)) AS VARCHAR(15))+' ?'				AS TOTAL_MONTANT
         									FROM	  #TMP_MODULE_PRO
         									GROUP BY  #TMP_MODULE_PRO.ID_CONTRAT_PRO
         									FOR XML RAW('TOTAL'),ELEMENTS, TYPE
         								)
         							FOR XML RAW('MODULES'),ELEMENTS, TYPE, ROOT('TABLEAU')
         						),
         --						CASE WHEN((SELECT  COUNT(*) FROM #TMP_SUBRO) > 0 ) THEN (	
         --							 
         --							SELECT	' mais veuillez nous faire parvenir le document'
         --							FOR XML PATH('ABSENCE_SUBRO'), TYPE	
         --
         --						) END,	
         						(
         							SELECT	top 1 dbo.GetContactSalutationOF(@ID_CONTACT, 1)
         							FROM CIVILITE
         							FOR XML PATH('POLITESSE_BAS'), TYPE
         						)
         				FROM #TMP_CONTRAT_PRO AS CORPS     
         				FOR XML AUTO, ELEMENTS, TYPE
         			)
         			
         
         	FROM	#TMP_CONTRAT_PRO as LETTRE
         	FOR XML AUTO, ELEMENTS
         END
         
         -- =============================================
         -- Author       : SBRU
         -- Create date  : 03/03/2008
         -- Description  : Relance des contributions partiellement pay‚es
         -- Modification	: Le XX/XX/XXXX par XXXX
         -- =============================================
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         -- =============================================
         -- DSZ 25/09/2013 : 16177 : tau tva en decimal(10,5)
         -- =============================================
         CREATE PROCEDURE [dbo].[EDT_RELANCE_PARTIELLEMENT_PAYE]
         	@ID_ETABLISSEMENT int,
         	@ID_BENEFICIAIRE int,
         	@TYPE_BENEFICIAIRE int,
         	@ID_ADRESSE int,
         	@ID_CONTACT int,
         	@ID_PERIODE int,
         	@BLN_SECRETAIRE_GENERAL tinyint
         AS
         	DECLARE @LibelleFonction varchar(50)
         	DECLARE @RaisonSociale varchar(50)
         	DECLARE @annee varchar(4)
         
         	select @annee = cast(num_annee as varchar(4)) from periode where id_periode = @ID_PERIODE
         
         	SELECT
         			VISU_GLOBALE_ADHERENT_PERIODE.id_adherent, 
         			VISU_GLOBALE_ADHERENT_PERIODE.lib_raison_sociale, 
         			VISU_GLOBALE_ADHERENT_PERIODE.ID_ETABLISSEMENT_BENEFICIAIRE, 
         			VISU_GLOBALE_ADHERENT_PERIODE.id_periode, 
         			VISU_GLOBALE_ADHERENT_PERIODE.id_activite, 
         			VISU_GLOBALE_ADHERENT_PERIODE.libl_activite, 
         			VISU_GLOBALE_ADHERENT_PERIODE.cod_periode, 
         			VISU_GLOBALE_ADHERENT_PERIODE.num_annee, 
         			VISU_GLOBALE_ADHERENT_PERIODE.masse_salariale, 
         			VISU_GLOBALE_ADHERENT_PERIODE.TAU_GLO_APPEL, 
         			VISU_GLOBALE_ADHERENT_PERIODE.DEDUCTION_MNT_HT, 
         			VISU_GLOBALE_ADHERENT_PERIODE.VERSE_HT, 
         			VISU_GLOBALE_ADHERENT_PERIODE.REVERSE_HT, 
         			VISU_GLOBALE_ADHERENT_PERIODE.TAU_TVA,
         			(MASSE_SALARIALE * TAU_GLO_APPEL/100) - DEDUCTION_MNT_HT + REVERSE_HT							as TOTAL_DU_HT,
         			(MASSE_SALARIALE * TAU_GLO_APPEL/100) - DEDUCTION_MNT_HT - VERSE_HT + REVERSE_HT				as RESTANT_DU_HT,
         			((MASSE_SALARIALE * TAU_GLO_APPEL/100) - DEDUCTION_MNT_HT - VERSE_HT + REVERSE_HT) * TAU_TVA	as RESTANT_DU_TVA,
         			(MASSE_SALARIALE * TAU_GLO_APPEL/100 - DEDUCTION_MNT_HT - VERSE_HT + REVERSE_HT) +
         			((MASSE_SALARIALE * TAU_GLO_APPEL/100 - DEDUCTION_MNT_HT - VERSE_HT + REVERSE_HT) * TAU_TVA)	as RESTANT_DU_TTC
         
         	INTO	#TMP_VISU_GLOBALE
         	FROM
         			VISU_GLOBALE_ADHERENT_PERIODE
         	JOIN	ADHERENT 
         	ON		ADHERENT.ID_ADHERENT = VISU_GLOBALE_ADHERENT_PERIODE.ID_ADHERENT
         	AND		ADHERENT.ID_ETABLISSEMENT_PRINCIPAL = @ID_ETABLISSEMENT
         	WHERE
         			ID_PERIODE = @ID_PERIODE
         --	AND		VISU_GLOBALE_ADHERENT_PERIODE.TAU_APPELE > 0 
         	AND		TAU_GLO_APPEL > 0
         	GROUP BY
         			VISU_GLOBALE_ADHERENT_PERIODE.id_adherent, 
         			VISU_GLOBALE_ADHERENT_PERIODE.lib_raison_sociale, 
         			VISU_GLOBALE_ADHERENT_PERIODE.ID_ETABLISSEMENT_BENEFICIAIRE, 
         			VISU_GLOBALE_ADHERENT_PERIODE.id_periode, 
         			VISU_GLOBALE_ADHERENT_PERIODE.id_activite, 
         			VISU_GLOBALE_ADHERENT_PERIODE.libl_activite, 
         			VISU_GLOBALE_ADHERENT_PERIODE.cod_periode, 
         			VISU_GLOBALE_ADHERENT_PERIODE.num_annee, 
         			VISU_GLOBALE_ADHERENT_PERIODE.masse_salariale, 
         			VISU_GLOBALE_ADHERENT_PERIODE.TAU_GLO_APPEL, 
         --			VISU_GLOBALE_ADHERENT_PERIODE.TAU_APPELE, 
         			VISU_GLOBALE_ADHERENT_PERIODE.DEDUCTION_MNT_HT, 
         			VISU_GLOBALE_ADHERENT_PERIODE.VERSE_HT, 
         			VISU_GLOBALE_ADHERENT_PERIODE.REVERSE_HT, 
         			VISU_GLOBALE_ADHERENT_PERIODE.TAU_TVA
         	
         	DECLARE @TAU_TVA decimal(10,5);
         
         	select top 1 @TAU_TVA=TAU_TVA from #TMP_VISU_GLOBALE;
         
         
         	-- GENERATION XML
         	WITH XMLNAMESPACES (
         		DEFAULT 'RELANCE_PARTIELLEMENT_PAYE'
         	)
         	-- Construction du flux XML
         	SELECT
         		
         			 --R‚cup‚ration des informations sur le contact
         			 dbo.GetXmlBenefiaireContact(@ID_BENEFICIAIRE, @TYPE_BENEFICIAIRE, @ID_ADRESSE, @ID_CONTACT) as BENEFICIAIRE,
         
         			(
         				SELECT 
         					ADHERENT.COD_ADHERENT	as COD_ADHERENT,
         					dbo.GetFullDate(getDate()) as DATE,
         			(
         				SELECT top 1
         						dbo.GetContactSalutation(@ID_CONTACT, 1)
         				FROM	CIVILITE
         				FOR XML PATH('POLITESSE_HAUT'), TYPE
         			)
         				FROM
         					ADHERENT
         				WHERE
         					ADHERENT.ID_ETABLISSEMENT_PRINCIPAL = @ID_ETABLISSEMENT
         				FOR XML RAW('ENTETE'), ELEMENTS, TYPE
         			),
         	
         			(
         				SELECT
         						@annee as ANNEE,
         					
         				-- R‚cup‚ration des sommes dues par activit‚
         				(
         					SELECT
         							VENTILATION.LIBL_ACTIVITE as LIB_ACTIVITE,
         							dbo.GetFrenchCurrencyFormat(VENTILATION.TOTAL_DU_HT)	as TOTAL_DU_HT,						
         							dbo.GetFrenchCurrencyFormat(VENTILATION.VERSE_HT)		as MONTANT_VERSE_HT
         					FROM
         							#TMP_VISU_GLOBALE as VENTILATION
         					FOR XML AUTO, ELEMENTS, TYPE
         				),
         				-- R‚cup‚ration des sommes dues au total
         				(
         					SELECT
         							dbo.GetFrenchCurrencyFormat(cast(sum(RESTANT_DU_HT) as money))	as TOTAL_DU_HT,
         							dbo.GetFrenchCurrencyFormat(cast((@TAU_TVA * 100) as money))	as TVA,
         							dbo.GetFrenchCurrencyFormat(cast(sum(RESTANT_DU_TVA) as money))	as MONTANT_TVA,
         							dbo.GetFrenchCurrencyFormat(cast(sum(RESTANT_DU_TTC) as money))	as MONTANT_TTC						
         					FROM
         							#TMP_VISU_GLOBALE as TOTAL_DU
         					FOR XML AUTO, ELEMENTS, TYPE
         				),
         				(
         					SELECT top 1
         							dbo.GetContactSalutation(@ID_CONTACT, 1)
         					FROM	CIVILITE
         					FOR XML PATH('POLITESSE_BAS'), TYPE
         				)
         
         				FROM
         					ADHERENT
         				WHERE
         					ADHERENT.ID_ETABLISSEMENT_PRINCIPAL = @ID_ETABLISSEMENT
         				FOR XML RAW('CORPS'), ELEMENTS, TYPE
         			),
         			(
         				SELECT top 1
         						NOM_PNM_DIRECTEUR
         				FROM	PARAMETRES
         				FOR XML RAW('SIGNATURE'), ELEMENTS, TYPE
         			)
         
         	FROM
         			ADHERENT
         	JOIN	ETABLISSEMENT			
         	ON		ADHERENT.ID_ETABLISSEMENT_PRINCIPAL = ETABLISSEMENT.ID_ETABLISSEMENT
         	JOIN	ADRESSE					
         	ON		ADHERENT.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE
         	JOIN	AGENCE					
         	ON		AGENCE.ID_AGENCE = ADHERENT.ID_AGENCE
         	WHERE
         			ADHERENT.ID_ETABLISSEMENT_PRINCIPAL = @ID_ETABLISSEMENT
         	FOR XML RAW('LETTRE'), ELEMENTS
         

CREATE PROCEDURE [dbo].[MVT_BUDGETAIRE_COLLECTE_IMPAYE]
          @ID_EVENEMENT INT
         AS
         /* *****************************************************************************************
         -- Auteur   : MBL
         -- Date Creation : 11/12/2014
         
         -- Role    : Cr‚ation de mouvements Budg‚taires d'impay‚ permettant d'annuler le versement initial
         ***************************************************************************************** */
         BEGIN
          DECLARE
           @ID_COMPTE    INT,
           @ID_ENVELOPPE   INT,
           @MNT_MVT_BUDGETAIRE  DECIMAL(18,2),
           @P_E_R     VARCHAR,
           @N_R     VARCHAR,
           @ID_PERIODE_FISC  INT,
           @ID_PERIODE_CPT   INT,
           @LIBL_MVT_BUDGETAIRE VARCHAR (60),
           @ID_ACTIVITE   INT,
           @ID_ADHERENT   INT,
           @ID_GROUPE    INT,
           @ID_ETABLISSEMENT  INT,
           @ID_TYPE_FINANCEMENT INT,
           @ID_TYPE_MOUVEMENT  INT,
           @ID_DISPOSITIF   INT = NULL,
           @ID_MODULE_PEC   INT = NULL,
           @ID_VERSEMENT   INT,
           @DAT_EVENEMENT   DATETIME,
           @COD_TYPE_EVENEMENT  VARCHAR(8),
           @ID_EVENEMENT_VERSEMENT INT 
         
          SELECT
           @ID_VERSEMENT = ID_VERSEMENT , 
           @DAT_EVENEMENT = DAT_EVENEMENT
          FROM
           EVENEMENT
          WHERE
           ID_EVENEMENT = @ID_EVENEMENT
         
          -- Recherche de l'evenement initialement associe … l'impay‚
          SELECT
           @ID_EVENEMENT_VERSEMENT = ID_EVENEMENT
          FROM
           EVENEMENT
           INNER JOIN TYPE_EVENEMENT
            ON TYPE_EVENEMENT.ID_TYPE_EVENEMENT = EVENEMENT.ID_TYPE_EVENEMENT
          WHERE
           ID_VERSEMENT = @ID_VERSEMENT
           AND COD_TYPE_EVENEMENT = 'VERSEMEN'
          
          DECLARE CU_MVT_VERSEMENT CURSOR FOR
          SELECT
           ID_COMPTE,
           ID_ENVELOPPE,
           MNT_MVT_BUDGETAIRE,
           P_E_R,
           N_R,
           ID_PERIODE_FISC,
           ID_PERIODE_CPT,
           LIBL_MVT_BUDGETAIRE,
           ID_ACTIVITE,
           ID_ADHERENT,
           ID_GROUPE,
           ID_ETABLISSEMENT,
           ID_TYPE_FINANCEMENT
          FROM
           MVT_BUDGETAIRE
          WHERE
           ID_EVENEMENT = @ID_EVENEMENT_VERSEMENT
          
          OPEN
           CU_MVT_VERSEMENT 
          FETCH
           CU_MVT_VERSEMENT 
          INTO
           @ID_COMPTE,
           @ID_ENVELOPPE,
           @MNT_MVT_BUDGETAIRE,
           @P_E_R,
           @N_R,
           @ID_PERIODE_FISC,
           @ID_PERIODE_CPT,
           @LIBL_MVT_BUDGETAIRE,
           @ID_ACTIVITE,
           @ID_ADHERENT,
           @ID_GROUPE,
           @ID_ETABLISSEMENT,
           @ID_TYPE_FINANCEMENT
          
          WHILE (@@FETCH_STATUS <> -1)
          BEGIN
           SET @ID_TYPE_MOUVEMENT = 10 -- Mouvement d'alimentation du Compte Adh‚rent
           SET @LIBL_MVT_BUDGETAIRE = REPLACE(@LIBL_MVT_BUDGETAIRE, 'Versement', 'Impay‚')
           
           IF (PATINDEX('%Impay‚%', @LIBL_MVT_BUDGETAIRE) =0)
           BEGIN
            SET @LIBL_MVT_BUDGETAIRE = LEFT('Impay‚ ' + @LIBL_MVT_BUDGETAIRE, 60)
           END
           
           SET @MNT_MVT_BUDGETAIRE = - @MNT_MVT_BUDGETAIRE
           
           EXEC dbo.INS_MVT_BUDGETAIRE
            @ID_TYPE_MOUVEMENT = @ID_TYPE_MOUVEMENT,
            @ID_EVENEMENT = @ID_EVENEMENT,
            @ID_COMPTE = @ID_COMPTE,
            @ID_ENVELOPPE   = @ID_ENVELOPPE,
            @MNT_MVT_BUDGETAIRE = @MNT_MVT_BUDGETAIRE,
            @P_E_R = @P_E_R,
            @DAT_MVT_BUDGETAIRE = @DAT_EVENEMENT,
            @N_R = @N_R,
            @ID_PERIODE_FISC = @ID_PERIODE_FISC,
            @ID_PERIODE_CPT = @ID_PERIODE_CPT,
            @LIBL_MVT_BUDGETAIRE = @LIBL_MVT_BUDGETAIRE,
            @ID_ACTIVITE = @ID_ACTIVITE,
            @ID_ADHERENT = @ID_ADHERENT,
            @ID_GROUPE = @ID_GROUPE,
            @ID_ETABLISSEMENT = @ID_ETABLISSEMENT,
            @ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT,
            @ID_DISPOSITIF = NULL,
            @ID_MODULE_PEC = NULL
           
           FETCH
            CU_MVT_VERSEMENT
           INTO
            @ID_COMPTE,
            @ID_ENVELOPPE,
            @MNT_MVT_BUDGETAIRE,
            @P_E_R,
            @N_R,
            @ID_PERIODE_FISC,
            @ID_PERIODE_CPT,
            @LIBL_MVT_BUDGETAIRE,
            @ID_ACTIVITE,
            @ID_ADHERENT,
            @ID_GROUPE,
            @ID_ETABLISSEMENT,
            @ID_TYPE_FINANCEMENT
          END
          CLOSE CU_MVT_VERSEMENT
          DEALLOCATE CU_MVT_VERSEMENT
         END


         CREATE PROCEDURE [dbo].[LEC_DET_CONTACT_ADH_MBL]
         --=======================================================
         -- Author		: MBL 
         -- Modif. date	: 10/11/2015
         -- commentaire  : Refonte PS suite … pr‚conisation DBA
         --=======================================================
         	@ID_CONTACT INT,
         	@ID_ADRESSE INT,
         	@ID_ETABLISSEMENT INT,
         	@BLN_PRINCIPAL TINYINT = 0,
         	@BLN_ACTIF TINYINT = 0
         AS
         	DECLARE @SQLQuery AS NVARCHAR(4000)
         	DECLARE @ParameterDefinition AS NVARCHAR(1500) 
         	
         	BEGIN
         		SET @SQLQuery = ''
         
         		SET @SQLQuery += 'SELECT '+ CHAR(13)
         		SET @SQLQuery += ' CT.ID_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CT.ID_CIVILITE, '+ CHAR(13)
         		SET @SQLQuery += ' CT.COD_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CT.LIB_NOM_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CT.LIB_PNM_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CT.COM_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CV.LIBC_CIVILITE, '+ CHAR(13)
         		SET @SQLQuery += ' AD.LIB_ADR, '+ CHAR(13)
         		SET @SQLQuery += ' FN.LIBL_FONCTION, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.NUM_TEL, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.NUM_FAX, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.NUM_PORT, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.LIB_TITRE, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.EMAIL_PERS, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.EMAIL_PRO '+ CHAR(13)
         		SET @SQLQuery += ' FROM CONTACT CT '+ CHAR(13)
         		SET @SQLQuery += ' INNER JOIN NR31 ON NR31.ID_CONTACT = CT.ID_CONTACT '+ CHAR(13)
         		SET @SQLQuery += ' INNER JOIN CIVILITE CV ON CV.ID_CIVILITE = CT.ID_CIVILITE '+ CHAR(13)
         		SET @SQLQuery += ' INNER JOIN ADRESSE AD ON NR31.ID_ADRESSE = AD.ID_ADRESSE '+ CHAR(13)
         		SET @SQLQuery += ' LEFT OUTER JOIN FONCTION FN ON FN.ID_FONCTION = NR31.ID_FONCTION '+ CHAR(13)
         		SET @SQLQuery += ' WHERE 1=1'+ CHAR(13)
         		IF @ID_CONTACT IS NOT NULL 
         		BEGIN
         			SET @SQLQuery += ' AND CT.ID_CONTACT = @ID_CONTACT '+ CHAR(13)
         		END
         		IF @ID_ETABLISSEMENT IS NOT NULL 
         		BEGIN
         			SET @SQLQuery += ' AND AD.ID_ETABLISSEMENT  = @ID_ETABLISSEMENT '+ CHAR(13)
         		END
         		IF @ID_ADRESSE IS NOT NULL 
         		BEGIN
         			SET @SQLQuery += ' AND AD.ID_ADRESSE  = @ID_ADRESSE '+ CHAR(13)
         		END
         		SET @SQLQuery += ' AND NR31.BLN_PRINCIPAL >= @BLN_PRINCIPAL '+ CHAR(13)
         		SET @SQLQuery += ' AND NR31.BLN_ACTIF >= @BLN_ACTIF '+ CHAR(13)
         		SET @SQLQuery += ' ORDER BY NR31.BLN_PRINCIPAL DESC'
         		
         		SET @ParameterDefinition = '@ID_CONTACT INT,@ID_ADRESSE INT,@ID_ETABLISSEMENT INT,@BLN_PRINCIPAL TINYINT = 0,@BLN_ACTIF TINYINT = 0'		
         		EXECUTE sp_executesql @SQLQuery, @ParameterDefinition, 	@ID_CONTACT, @ID_ADRESSE, @ID_ETABLISSEMENT, @BLN_PRINCIPAL, @BLN_ACTIF
         
         	END
         	/* Code Supprime suite Refonte 
         	BEGIN
         		SELECT
         			CT.ID_CONTACT,
         			CT.ID_CIVILITE,
         			CT.COD_CONTACT,
         			CT.LIB_NOM_CONTACT,
         			CT.LIB_PNM_CONTACT,
         			CT.COM_CONTACT,
         			
         			CV.LIBC_CIVILITE,
         			
         			AD.LIB_ADR,
         			
         			FN.LIBL_FONCTION,
         			
         			NR31.NUM_TEL,
         			NR31.NUM_FAX,
         			NR31.NUM_PORT,
         			NR31.LIB_TITRE,
         			NR31.EMAIL_PERS,
         			NR31.EMAIL_PRO
         		FROM CONTACT CT
         			INNER JOIN NR31 ON NR31.ID_CONTACT = CT.ID_CONTACT
         			INNER JOIN ADRESSE AD ON NR31.ID_ADRESSE = AD.ID_ADRESSE
         			LEFT OUTER JOIN FONCTION FN ON FN.ID_FONCTION = NR31.ID_FONCTION
         			INNER JOIN CIVILITE CV ON CV.ID_CIVILITE = CT.ID_CIVILITE
         		WHERE
         			--ADRESSE.BLN_PRINCIPAL = 1 AND 
         			(@ID_CONTACT IS NULL OR CT.ID_CONTACT = @ID_CONTACT) AND 
         			(@ID_ADRESSE IS NULL OR AD.ID_ADRESSE = @ID_ADRESSE) AND
         			(@ID_ETABLISSEMENT IS NULL OR NR31.ID_ETABLISSEMENT = @ID_ETABLISSEMENT) AND
         			(NR31.BLN_PRINCIPAL >= @BLN_PRINCIPAL) AND (NR31.BLN_ACTIF >= @BLN_ACTIF)
         		ORDER BY NR31.BLN_PRINCIPAL DESC
         	END
         	*/
         
         CREATE PROCEDURE [dbo].[MAJ_ENVELOPPES]
          @ANNEE INT
         AS
         -- =============================================
         -- Author   : ???
         -- Date    : ??/??/????
         -- Description  : Initialiser les enveloppes de l'ann‚e pass‚e en paramŠtre … partir des types enveloppe existants.
         -- 17/11/09 - SBR : R‚entrance de la proc‚dure - pour une ann‚e donn‚e si de nouveaux types enveloppe ont ‚t‚ cr‚‚s, les enveloppes li‚es sont cr‚‚es
         -- 04/12/12 - SBR : Remplacement de la clause WHERE COD_ENVELOPPE = 0 par WHERE COD_ENVELOPPE = '0'
         -- 27/11/14 - MBL : Les enveloppes de l'annee ne sont cr‚‚es que pour les types d'enveloppe ACTIFS
         -- 10/09/15- MBL : Modificatoion regle de generation du code enveloppe pour ‚viter les cl‚s en double
         -- =============================================
         BEGIN
          DECLARE @ID_PERIODE_ANNEE AS INT,
            @ANNEE_TEXTE AS VARCHAR(4)
         
          SELECT @ID_PERIODE_ANNEE = ID_PERIODE
          FROM PERIODE
          WHERE ID_TYPE_PERIODE = 1
          AND  NUM_ANNEE = @ANNEE
         
          SELECT @ANNEE_TEXTE = convert(varchar(4), @ANNEE)
         
          -- est-ce qu'il existe des enveloppes inexistantes pour l'ann‚e pass‚e en paramŠtre et un type enveloppe existant?
          IF NOT EXISTS ( 
              SELECT 1
              FROM ENVELOPPE 
              RIGHT JOIN 
                TYPE_ENVELOPPE 
              ON  ENVELOPPE.ID_TYPE_ENVELOPPE = TYPE_ENVELOPPE.ID_TYPE_ENVELOPPE 
              AND  ENVELOPPE.ID_PERIODE = @ID_PERIODE_ANNEE
              WHERE ENVELOPPE.ID_ENVELOPPE IS NULL
              AND  TYPE_ENVELOPPE .BLN_ACTIF = 1
              )
          BEGIN
           Print 'Toutes les enveloppes existent d‚j… pour l''ann‚e ' + @ANNEE_TEXTE
           RETURN
          END
         
          INSERT INTO ENVELOPPE
          (COD_ENVELOPPE
          ,LIBC_ENVELOPPE
          ,LIBL_ENVELOPPE
          ,DAT_DEBUT
          ,DAT_FIN
          ,ID_PERIODE
          ,BLN_ACTIF
          ,COM_ENVELOPPE
          ,DAT_CREATION
          ,DAT_MODIF
          ,ID_TYPE_ENVELOPPE
          ,ID_UTILISATEUR)
         
          SELECT '9000' +CAST(t1.ID_TYPE_ENVELOPPE AS VARCHAR)
            ,t1.LIBC_TYPE_ENVELOPPE
            ,LEFT(@ANNEE_TEXTE + t1.LIBL_TYPE_ENVELOPPE, 50)
            ,convert(datetime, '01/01/' + @ANNEE_TEXTE, 103)
            ,convert(datetime, '31/12/' + @ANNEE_TEXTE, 103)
            ,@ID_PERIODE_ANNEE
            ,1
            ,NULL
            ,getdate()
            ,NULL
            ,t1.ID_TYPE_ENVELOPPE
            ,82
          FROM TYPE_ENVELOPPE t1
          WHERE t1.BLN_ACTIF  = 1
          AND  not exists (
               select 1 
               from enveloppe 
               where id_type_enveloppe = t1.id_type_enveloppe 
               and id_periode = @ID_PERIODE_ANNEE
               )
         
          UPDATE ENVELOPPE
          SET  COD_ENVELOPPE = ID_ENVELOPPE
          WHERE COD_ENVELOPPE LIKE '9000%'
         END
         
         
		 -- =============================================  
         -- Author    : XX  
         -- Create date   : XX XXXX 2007  
         -- Description   : Cr‚ation  
         -- =============================================  
         -- Author    : SV  
         -- Modification date : 26 septembre 2007  
         -- Description   : Correction de l'erreur sur la restriction sur le champ ETABLISSEMENT_OF.ID_OF dans le cas de la recherche avec un ‚tablissement destinataire de type O.F.  
         -- =============================================  
         -- Author    : EOU  
         -- Modification date : 04 fevrier 2012  
         -- Description   : 13323  
         -- =============================================  
         -- DSZ 25/04/2012 13421   
         -- ORGANISME_FORMATION.LIB_RAISON_SOCIALE est varchar (64) et non pas 50  
         -- et aussi refection de la procedure pour virer le curseur   
         -- =============================================  
         -- DSZ 01/06/2012 13557  
         -- gerer les nulls dans nom prenom de contact  
         -- =============================================  
         -- ASD 08/10/2012 14062  
         -- recherche sur transaction OF avec destinataire et b‚n‚ficiaire renseign‚  
         -- =============================================  
         -- EOU 05/12/2012 14234 Recherche sur transaction b‚n‚f adh‚rent correction de copier coller  
         --- =============================================  
         -- SBR 16/01/2013 le champ #FINAL.ADRESSE_DESTINATAIRE passe de varchar(100) … varchar(200)
         --- =============================================  
         CREATE PROCEDURE [dbo].[LEC_GRP_TRANSACTION_COMPLEXE]  
          @ID_ETABLISSEMENT_DESTINATAIRE int,   -- Identifiant de l'‚tablissement (adh‚rent ou O.F.) destinataire  
          @ID_ADHERENT_DESTINATAIRE  int,   -- Identifiant de l'adh‚rent destinataire  
          @ID_OF_DESTINATAIRE    int,   -- Identifiant de l'organisme de formation destinataire  
           
          @ID_BENEFICIAIRE    int,   -- Identifiant de l'‚tablissement (adh‚rent ou O.F.) b‚n‚ficiaire  
          @ID_TIERS_BENEFICIAIRE   int,   -- Identifiant du tiers b‚n‚ficiaire   
          @ID_ADHERENT_BENEFICIAIRE  int,   -- Identifiant de l'adh‚rent b‚n‚ficiaire  
          @ID_OF_BENEFICIAIRE    int,   -- Identifiant de l'organisme de formation b‚n‚ficiaire  
            
          @ID_TYPE_TRANSACTION   int,   -- Type de la transaction  
          @ID_SOUS_TYPE_TRANSACTION  int,   -- Sous type de la transaction  
          @ID_ACTIVITE     int,   -- Activit‚ de la transaction  
            
          @CONTACT_NOM     varchar(35), -- Nom du contact  
            
          @BLN_ACTIF      tinyint = 1, -- Etat de la transaction  
          @ONLY_NUM_IBAN_NOT_NULL   bit = 0,  -- Only NUM_IBAN not null  
          @ID_CONTACT  INT,  
          @ID_GROUPE  INT      
         AS  
         begin  
          -- R‚cup‚ration de(des) destinataire(s) dans une tables temporaire  
          CREATE TABLE #DESTINATAIRES  
           (  
            ID_DESTINATAIRE int  
           )  
           
          if (@ID_ETABLISSEMENT_DESTINATAIRE is null)-- ADH ou OF  
           begin  
            if (@ID_ADHERENT_DESTINATAIRE is not null)  
             -- Recherche des ‚tablissements d'un adh‚rent  
             begin  
              INSERT INTO #DESTINATAIRES  
              SELECT  
                ETABLISSEMENT.ID_ETABLISSEMENT as ID_DESTINATAIRE  
              FROM  
                ETABLISSEMENT  
              WHERE  
                ETABLISSEMENT.ID_ADHERENT = @ID_ADHERENT_DESTINATAIRE  
             end  
            else if (@ID_OF_DESTINATAIRE is not null)  
             -- Recherche des ‚tablissements d'un O.F.  
             begin  
              INSERT INTO #DESTINATAIRES  
              SELECT  
                ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF as ID_DESTINATAIRE  
              FROM  
                ETABLISSEMENT_OF  
              WHERE  
                ETABLISSEMENT_OF.ID_OF = @ID_OF_DESTINATAIRE  
                 
             end  
           end  
          else  
           begin  
             -- Le destinataire est un O.F.  
            if (@ID_ADHERENT_DESTINATAIRE is null)  
             begin  
              INSERT INTO #DESTINATAIRES  
              SELECT  
                ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF as ID_DESTINATAIRE  
              FROM  
                ORGANISME_FORMATION inner join ETABLISSEMENT_OF  
                ON ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF  
              WHERE  
               ORGANISME_FORMATION.ID_OF = @ID_OF_DESTINATAIRE  
               and ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_ETABLISSEMENT_DESTINATAIRE  
                 
             end  
            else  
             -- Le destinataire est un adh‚rent  
             begin  
              INSERT INTO #DESTINATAIRES  
              SELECT  
                ETABLISSEMENT.ID_ETABLISSEMENT as ID_DESTINATAIRE  
              FROM  
                ADHERENT INNER JOIN ETABLISSEMENT  
                ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT  
              WHERE  
                ADHERENT.ID_ADHERENT = @ID_ADHERENT_DESTINATAIRE  
               and ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT_DESTINATAIRE  
             end  
           end  
           
          -- R‚cup‚ration de(des) b‚n‚ficiaires(s) dans une table temporaire  
          CREATE TABLE #BENEFICIAIRES  
           (  
            ID_BENEFICIAIRE int  
           )  
            
           if (@ID_TIERS_BENEFICIAIRE is not null)  
            begin  
             INSERT INTO #BENEFICIAIRES  
             SELECT  
               TIERS.ID_TIERS as ID_BENEFICIAIRE  
             FROM  
               TIERS  
             WHERE  
               TIERS.ID_TIERS = @ID_TIERS_BENEFICIAIRE  
            end  
           --else @ID_TIERS_BENEFICIAIRE is null et si  
           else if (@ID_BENEFICIAIRE is null)  
            begin  
             if (@ID_ADHERENT_BENEFICIAIRE is not null)  
              -- Recherche des ‚tablissements d'un adh‚rent  
              begin  
               INSERT INTO #BENEFICIAIRES  
               SELECT  
                 ETABLISSEMENT.ID_ETABLISSEMENT as ID_BENEFICIAIRE  
               FROM  
                 ETABLISSEMENT  
               WHERE  
                 ETABLISSEMENT.ID_ADHERENT = @ID_ADHERENT_BENEFICIAIRE  
              end  
             else if (@ID_OF_BENEFICIAIRE is not null)  
              -- Recherche des ‚tablissements d'un O.F.  
              begin  
               INSERT INTO #BENEFICIAIRES  
               SELECT  
                 ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF as ID_BENEFICIAIRE  
               FROM  
                 ETABLISSEMENT_OF  
               WHERE  
                 ETABLISSEMENT_OF.ID_OF = @ID_OF_BENEFICIAIRE  
              end  
             end  
           else    
            begin  
             if (@ID_ADHERENT_BENEFICIAIRE is null)  
              -- Le destinataire est un O.F.  
              begin  
               INSERT INTO #BENEFICIAIRES  
               SELECT  
                 ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF as ID_BENEFICIAIRE  
               FROM  
                 ORGANISME_FORMATION INNER JOIN ETABLISSEMENT_OF  
                 ON ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF  
               WHERE  
                 ORGANISME_FORMATION.ID_OF = @ID_OF_BENEFICIAIRE  
                and ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_BENEFICIAIRE  
             end  
             else  
              -- Le destinataire est un adh‚rent  
              begin  
               INSERT INTO #BENEFICIAIRES  
               SELECT  
                 ETABLISSEMENT.ID_ETABLISSEMENT as ID_BENEFICIAIRE  
               FROM  
                 ADHERENT INNER JOIN ETABLISSEMENT  
                 ON ADHERENT.ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT  
               WHERE  
                 ADHERENT.ID_ADHERENT = @ID_ADHERENT_BENEFICIAIRE  
                and ETABLISSEMENT.ID_ETABLISSEMENT = @ID_BENEFICIAIRE  
              end  
            end  
           
          -- R‚cup‚ration des transactions dans #FINAL pour en ajouter les informations sur le destinataire par la suite  
          IF(@ID_GROUPE is NULL)  
          BEGIN  
           SELECT DISTINCT  
            "TRANSACTION".ID_ADRESSE,  
            "TRANSACTION".ID_ETABLISSEMENT_OF_BENEF,  
            "TRANSACTION".ID_TIERS_BENEF,  
            "TRANSACTION".ID_ETABLISSEMENT_BENEF,  
            "TRANSACTION".ID_CONTACT,  
            "TRANSACTION".ID_ETABLISSEMENT_OF_DEST,  
            "TRANSACTION".ID_ETABLISSEMENT_DEST,  
            "TRANSACTION".ID_TRANSACTION,  
            "TRANSACTION".ID_TYPE_TRANSACTION,  
            "TRANSACTION".ID_SOUS_TYPE_TRANSACTION,  
            "TRANSACTION".ID_ACTIVITE,  
            "TRANSACTION".BLN_ACTIF,  
            "TRANSACTION".NUM_IBAN,  
            "TRANSACTION".BLN_TRANSACTION_REGLEMENT_PRINCIPAL,  
            "TRANSACTION".ID_MODE_ENVOI_DOC,  
            cast (null as  varchar(64)) as LIB_RAISON_SOCIALE_DEST,  
            cast (null as  varchar(100)) as LIB_ADR_DEST ,  
            cast (null as  varchar(5)) as LIB_CP_CEDEX_DEST,  
            cast (null as  varchar(50)) as LIB_VIL_CEDEX_DEST,  
            cast (null as  varchar(200)) as ADRESSE_DESTINATAIRE ,  -- 16/01/2013 SBR
              
            CASE  
            when "TRANSACTION".BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE = 0 then  ADRESSE.LIB_ADR +' '+ ADRESSE.LIB_CP_CEDEX+' '+ADRESSE.LIB_VIL_CEDEX   
            else null end as ADRESSE_BENEFICIAIRE,  
            isnull(CONTACT.LIB_NOM_CONTACT,'')+' '+isnull(CONTACT.LIB_PNM_CONTACT,'') as NOM_PRENOM,  
            "TRANSACTION".LIBL_TRANSACTION   
          INTO #FINAL    
          FROM  
            "TRANSACTION"   
            left outer join CONTACT on CONTACT.ID_CONTACT = "TRANSACTION".ID_CONTACT  
            left join ADRESSE on ADRESSE.ID_ADRESSE = "TRANSACTION".ID_ADRESSE  
              
          WHERE  
            ((@ID_TYPE_TRANSACTION is null) or (ID_TYPE_TRANSACTION = @ID_TYPE_TRANSACTION))  
           and ((@ID_SOUS_TYPE_TRANSACTION is null) or (ID_SOUS_TYPE_TRANSACTION = @ID_SOUS_TYPE_TRANSACTION))  
           and ((@ID_ACTIVITE is null) or (ID_ACTIVITE = @ID_ACTIVITE))  
           and "TRANSACTION".BLN_ACTIF >= @BLN_ACTIF  
           -- Restrictions sur les destinataires  
           and ((@ID_ADHERENT_DESTINATAIRE is null) or (ID_ETABLISSEMENT_DEST in (select ID_DESTINATAIRE from #DESTINATAIRES)))  
           and ((@ID_OF_DESTINATAIRE is null) or (ID_ETABLISSEMENT_OF_DEST in (select ID_DESTINATAIRE from #DESTINATAIRES)))  
           -- Restrictions sur les b‚n‚ficiaires  
           and ((@ID_TIERS_BENEFICIAIRE is null) or (ID_TIERS_BENEF in (select ID_BENEFICIAIRE from #BENEFICIAIRES)))  
           and ((@ID_ADHERENT_BENEFICIAIRE is null) or (ID_ETABLISSEMENT_BENEF in (select ID_BENEFICIAIRE from #BENEFICIAIRES)))  
           and ((@ID_OF_BENEFICIAIRE is null) or (ID_ETABLISSEMENT_OF_BENEF in (select ID_BENEFICIAIRE from #BENEFICIAIRES)))  
           and ((@CONTACT_NOM is null) or (CONTACT.LIB_NOM_CONTACT like @CONTACT_NOM))  
           and ((@ID_CONTACT is null) or (CONTACT.ID_CONTACT = @ID_CONTACT))  
             
          -- Agr‚gation des informations finales  
           update #FINAL  
           set   
            LIB_RAISON_SOCIALE_DEST = ORGANISME_FORMATION.LIB_RAISON_SOCIALE,  
            LIB_ADR_DEST   = ADRESSE.LIB_ADR ,  
            LIB_CP_CEDEX_DEST  = ADRESSE.LIB_CP_CEDEX,  
            LIB_VIL_CEDEX_DEST  = ADRESSE.LIB_VIL_CEDEX,  
            ADRESSE_DESTINATAIRE = ADRESSE.LIB_ADR + ' ' + ADRESSE.LIB_CP_CEDEX+' '+ADRESSE.LIB_VIL_CEDEX   
           from #FINAL  
           inner join ETABLISSEMENT_OF on (ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = #FINAL.ID_ETABLISSEMENT_OF_DEST)  
           inner join ORGANISME_FORMATION on (ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF)  
           inner join  ADRESSE on (ETABLISSEMENT_OF.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE)  
           where #FINAL.ID_ETABLISSEMENT_OF_DEST is not null  
           
           
           update #FINAL  
           set   
            LIB_RAISON_SOCIALE_DEST = ADHERENT.LIB_RAISON_SOCIALE,  
            LIB_ADR_DEST   = ADRESSE.LIB_ADR ,  
            LIB_CP_CEDEX_DEST  = ADRESSE.LIB_CP_CEDEX,  
            LIB_VIL_CEDEX_DEST  = ADRESSE.LIB_VIL_CEDEX,  
            ADRESSE_DESTINATAIRE = ADRESSE.LIB_ADR + ' ' + ADRESSE.LIB_CP_CEDEX+' '+ADRESSE.LIB_VIL_CEDEX   
           from #FINAL  
           inner join ETABLISSEMENT on (ETABLISSEMENT.ID_ETABLISSEMENT = #FINAL.ID_ETABLISSEMENT_DEST)  
           inner join ADHERENT on (ETABLISSEMENT.ID_ADHERENT = ADHERENT.ID_ADHERENT)  
           inner join  ADRESSE on (ETABLISSEMENT.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE)  
           where #FINAL.ID_ETABLISSEMENT_OF_DEST is null  
           and #FINAL.ID_ETABLISSEMENT_DEST is not null  
           
            
          SELECT * from #FINAL  
           WHERE ((NUM_IBAN IS NOT NULL AND LTRIM(RTRIM(NUM_IBAN)) <> '') OR @ONLY_NUM_IBAN_NOT_NULL = 0)  
          END  
          ELSE  
          BEGIN  
          SELECT DISTINCT  
            "TRANSACTION".ID_ADRESSE,  
            "TRANSACTION".ID_ETABLISSEMENT_OF_BENEF,  
            "TRANSACTION".ID_TIERS_BENEF,  
            "TRANSACTION".ID_ETABLISSEMENT_BENEF,  
            "TRANSACTION".ID_CONTACT,  
            "TRANSACTION".ID_ETABLISSEMENT_OF_DEST,  
            "TRANSACTION".ID_ETABLISSEMENT_DEST,  
            "TRANSACTION".ID_TRANSACTION,  
            "TRANSACTION".ID_TYPE_TRANSACTION,  
            "TRANSACTION".ID_SOUS_TYPE_TRANSACTION,  
            "TRANSACTION".ID_ACTIVITE,  
            "TRANSACTION".BLN_ACTIF,  
            "TRANSACTION".NUM_IBAN,  
            "TRANSACTION".BLN_TRANSACTION_REGLEMENT_PRINCIPAL,  
            "TRANSACTION".ID_MODE_ENVOI_DOC,  
            cast (null as  varchar(64)) as LIB_RAISON_SOCIALE_DEST,  
            cast (null as  varchar(100)) as LIB_ADR_DEST ,  
            cast (null as  varchar(5)) as LIB_CP_CEDEX_DEST,  
            cast (null as  varchar(50)) as LIB_VIL_CEDEX_DEST,  
            cast (null as  varchar(100)) as ADRESSE_DESTINATAIRE ,  
              
            CASE  
            when "TRANSACTION".BLN_BENEFICIAIRE_IDENTIQUE_DESTINATAIRE = 0 then  ADRESSE.LIB_ADR +' '+ ADRESSE.LIB_CP_CEDEX+' '+ADRESSE.LIB_VIL_CEDEX   
            else null end as ADRESSE_BENEFICIAIRE,  
            isnull(CONTACT.LIB_NOM_CONTACT,'')+' '+isnull(CONTACT.LIB_PNM_CONTACT,'') as NOM_PRENOM,  
            "TRANSACTION".LIBL_TRANSACTION   
          INTO #FINAL2    
          FROM  
            "TRANSACTION"   
            INNER join ETABLISSEMENT on (ETABLISSEMENT.ID_ETABLISSEMENT = "TRANSACTION".ID_ETABLISSEMENT_DEST)  
            left outer join CONTACT on CONTACT.ID_CONTACT = "TRANSACTION".ID_CONTACT  
            left join ADRESSE on ADRESSE.ID_ADRESSE = "TRANSACTION".ID_ADRESSE  
              
          WHERE  
            ((@ID_TYPE_TRANSACTION is null) or (ID_TYPE_TRANSACTION = @ID_TYPE_TRANSACTION))  
           and ((@ID_SOUS_TYPE_TRANSACTION is null) or (ID_SOUS_TYPE_TRANSACTION = @ID_SOUS_TYPE_TRANSACTION))  
           and ((@ID_ACTIVITE is null) or (ID_ACTIVITE = @ID_ACTIVITE))  
           and "TRANSACTION".BLN_ACTIF >= @BLN_ACTIF  
           -- Restrictions sur les destinataires  
           and ((@ID_ADHERENT_DESTINATAIRE is null) or (ID_ETABLISSEMENT_DEST in (select ID_DESTINATAIRE from #DESTINATAIRES)))  
           and ((@ID_OF_DESTINATAIRE is null) or (ID_ETABLISSEMENT_OF_DEST in (select ID_DESTINATAIRE from #DESTINATAIRES)))  
           -- Restrictions sur les b‚n‚ficiaires  
           and ((@ID_TIERS_BENEFICIAIRE is null) or (ID_TIERS_BENEF in (select ID_BENEFICIAIRE from #BENEFICIAIRES)))  
           and ((@ID_ADHERENT_BENEFICIAIRE is null) or (ID_ETABLISSEMENT_BENEF in (select ID_BENEFICIAIRE from #BENEFICIAIRES)))  
           and ((@ID_OF_BENEFICIAIRE is null) or (ID_ETABLISSEMENT_OF_BENEF in (select ID_BENEFICIAIRE from #BENEFICIAIRES)))  
           and ((@CONTACT_NOM is null) or (CONTACT.LIB_NOM_CONTACT like @CONTACT_NOM))  
           and ((@ID_CONTACT is null) or (CONTACT.ID_CONTACT = @ID_CONTACT))  
           --AND ((@ID_GROUPE is null) or (SELECT ID_GROUPE FROM ETABLISSEMENT WHERE ETABLISSEMENT.ID_ETABLISSEMENT = "TRANSACTION".ID_ETABLISSEMENT_DEST) = @ID_GROUPE)  
           AND (ETABLISSEMENT.ID_GROUPE = @ID_GROUPE)  
           -- Agr‚gation des informations finales  
           update #FINAL2  
           set   
            LIB_RAISON_SOCIALE_DEST = ORGANISME_FORMATION.LIB_RAISON_SOCIALE,  
            LIB_ADR_DEST   = ADRESSE.LIB_ADR ,  
            LIB_CP_CEDEX_DEST  = ADRESSE.LIB_CP_CEDEX,  
            LIB_VIL_CEDEX_DEST  = ADRESSE.LIB_VIL_CEDEX,  
            ADRESSE_DESTINATAIRE = ADRESSE.LIB_ADR + ' ' + ADRESSE.LIB_CP_CEDEX+' '+ADRESSE.LIB_VIL_CEDEX   
           from #FINAL2  
           inner join ETABLISSEMENT_OF on (ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = #FINAL2.ID_ETABLISSEMENT_OF_DEST)  
           inner join ORGANISME_FORMATION on (ETABLISSEMENT_OF.ID_OF = ORGANISME_FORMATION.ID_OF)  
           inner join  ADRESSE on (ETABLISSEMENT_OF.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE)  
           where #FINAL2.ID_ETABLISSEMENT_OF_DEST is not null  
           
           
           update #FINAL2  
           set   
            LIB_RAISON_SOCIALE_DEST = ADHERENT.LIB_RAISON_SOCIALE,  
            LIB_ADR_DEST   = ADRESSE.LIB_ADR ,  
            LIB_CP_CEDEX_DEST  = ADRESSE.LIB_CP_CEDEX,  
            LIB_VIL_CEDEX_DEST  = ADRESSE.LIB_VIL_CEDEX,  
            ADRESSE_DESTINATAIRE = ADRESSE.LIB_ADR + ' ' + ADRESSE.LIB_CP_CEDEX+' '+ADRESSE.LIB_VIL_CEDEX   
           from #FINAL2  
           inner join ETABLISSEMENT on (ETABLISSEMENT.ID_ETABLISSEMENT = #FINAL2.ID_ETABLISSEMENT_DEST)  
           inner join ADHERENT on (ETABLISSEMENT.ID_ADHERENT = ADHERENT.ID_ADHERENT)  
           inner join  ADRESSE on (ETABLISSEMENT.ID_ADRESSE_PRINCIPALE = ADRESSE.ID_ADRESSE)  
           where #FINAL2.ID_ETABLISSEMENT_OF_DEST is null  
           and #FINAL2.ID_ETABLISSEMENT_DEST is not null  
           
            
          SELECT * from #FINAL2  
           WHERE ((NUM_IBAN IS NOT NULL AND LTRIM(RTRIM(NUM_IBAN)) <> '') OR @ONLY_NUM_IBAN_NOT_NULL = 0)  
          END  
           
         end
         

         CREATE PROCEDURE [dbo].[LEC_DET_CONTACT_ADH]
         --=======================================================
         -- Author		: MBL 
         -- Modif. date	: 10/11/2015
         -- commentaire  : Refonte PS suite … pr‚conisation DBA
         --=======================================================
         	@ID_CONTACT INT,
         	@ID_ADRESSE INT,
         	@ID_ETABLISSEMENT INT,
         	@BLN_PRINCIPAL TINYINT = 0,
         	@BLN_ACTIF TINYINT = 0
         AS
         	DECLARE @SQLQuery AS NVARCHAR(4000)
         	DECLARE @ParameterDefinition AS NVARCHAR(1500) 
         	
         	BEGIN
         		SET @SQLQuery = ''
         
         		SET @SQLQuery += 'SELECT '+ CHAR(13)
         		SET @SQLQuery += ' CT.ID_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CT.ID_CIVILITE, '+ CHAR(13)
         		SET @SQLQuery += ' CT.COD_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CT.LIB_NOM_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CT.LIB_PNM_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CT.COM_CONTACT, '+ CHAR(13)
         		SET @SQLQuery += ' CV.LIBC_CIVILITE, '+ CHAR(13)
         		SET @SQLQuery += ' AD.LIB_ADR, '+ CHAR(13)
         		SET @SQLQuery += ' FN.LIBL_FONCTION, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.NUM_TEL, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.NUM_FAX, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.NUM_PORT, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.LIB_TITRE, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.EMAIL_PERS, '+ CHAR(13)
         		SET @SQLQuery += ' NR31.EMAIL_PRO '+ CHAR(13)
         		SET @SQLQuery += ' FROM CONTACT CT '+ CHAR(13)
         		SET @SQLQuery += ' INNER JOIN NR31 ON NR31.ID_CONTACT = CT.ID_CONTACT '+ CHAR(13)
         		SET @SQLQuery += ' INNER JOIN CIVILITE CV ON CV.ID_CIVILITE = CT.ID_CIVILITE '+ CHAR(13)
         		SET @SQLQuery += ' INNER JOIN ADRESSE AD ON NR31.ID_ADRESSE = AD.ID_ADRESSE '+ CHAR(13)
         		SET @SQLQuery += ' LEFT OUTER JOIN FONCTION FN ON FN.ID_FONCTION = NR31.ID_FONCTION '+ CHAR(13)
         		SET @SQLQuery += ' WHERE 1=1'+ CHAR(13)
         		IF @ID_CONTACT IS NOT NULL 
         		BEGIN
         			SET @SQLQuery += ' AND CT.ID_CONTACT = @ID_CONTACT '+ CHAR(13)
         		END
         		IF @ID_ETABLISSEMENT IS NOT NULL 
         		BEGIN
         			SET @SQLQuery += ' AND AD.ID_ETABLISSEMENT  = @ID_ETABLISSEMENT '+ CHAR(13)
         		END
         		IF @ID_ADRESSE IS NOT NULL 
         		BEGIN
         			SET @SQLQuery += ' AND AD.ID_ADRESSE  = @ID_ADRESSE '+ CHAR(13)
         		END
         		SET @SQLQuery += ' AND NR31.BLN_PRINCIPAL >= @BLN_PRINCIPAL '+ CHAR(13)
         		SET @SQLQuery += ' AND NR31.BLN_ACTIF >= @BLN_ACTIF '+ CHAR(13)
         		SET @SQLQuery += ' ORDER BY NR31.BLN_PRINCIPAL DESC'
         		
         		SET @ParameterDefinition = '@ID_CONTACT INT,@ID_ADRESSE INT,@ID_ETABLISSEMENT INT,@BLN_PRINCIPAL TINYINT = 0,@BLN_ACTIF TINYINT = 0'		
         		EXECUTE sp_executesql @SQLQuery, @ParameterDefinition, 	@ID_CONTACT, @ID_ADRESSE, @ID_ETABLISSEMENT, @BLN_PRINCIPAL, @BLN_ACTIF
         
         	END
         	/* Code Supprime suite Refonte 
         	BEGIN
         		SELECT
         			CT.ID_CONTACT,
         			CT.ID_CIVILITE,
         			CT.COD_CONTACT,
         			CT.LIB_NOM_CONTACT,
         			CT.LIB_PNM_CONTACT,
         			CT.COM_CONTACT,
         			
         			CV.LIBC_CIVILITE,
         			
         			AD.LIB_ADR,
         			
         			FN.LIBL_FONCTION,
         			
         			NR31.NUM_TEL,
         			NR31.NUM_FAX,
         			NR31.NUM_PORT,
         			NR31.LIB_TITRE,
         			NR31.EMAIL_PERS,
         			NR31.EMAIL_PRO
         		FROM CONTACT CT
         			INNER JOIN NR31 ON NR31.ID_CONTACT = CT.ID_CONTACT
         			INNER JOIN ADRESSE AD ON NR31.ID_ADRESSE = AD.ID_ADRESSE
         			LEFT OUTER JOIN FONCTION FN ON FN.ID_FONCTION = NR31.ID_FONCTION
         			INNER JOIN CIVILITE CV ON CV.ID_CIVILITE = CT.ID_CIVILITE
         		WHERE
         			--ADRESSE.BLN_PRINCIPAL = 1 AND 
         			(@ID_CONTACT IS NULL OR CT.ID_CONTACT = @ID_CONTACT) AND 
         			(@ID_ADRESSE IS NULL OR AD.ID_ADRESSE = @ID_ADRESSE) AND
         			(@ID_ETABLISSEMENT IS NULL OR NR31.ID_ETABLISSEMENT = @ID_ETABLISSEMENT) AND
         			(NR31.BLN_PRINCIPAL >= @BLN_PRINCIPAL) AND (NR31.BLN_ACTIF >= @BLN_ACTIF)
         		ORDER BY NR31.BLN_PRINCIPAL DESC
         	END
         	*/
         


         -- =============================================
         -- Author:		XX
         -- Create date: XX XX 2007
         -- Description:	Cr‚ation
         -- ----------------------------------------------
         -- Author:		SV
         -- Create date: 17 ao–t 2007
         -- Description:	Ajout de la r‚cup‚ration de ID_SESSION_PEC
         -- ----------------------------------------------
         -- Author:		DSZ
         -- Create date: 30 avril 2008
         -- Description:	 Ajout ID_ETABLISSEMENT_D_ADHERENT au select
         -- -----------------------------------------------
         -- Author:		SAFI
         -- Create date: 27 AVRIL 2009
         -- Description:	Ajout d'une colonne dans la selection (ID_TYPE_TVA)
         -- -----------------------------------------------
         -- Author:		DSZ
         -- Create date: 01/03/2012
         -- Description:	13187 : Ajout de la valeur A_RECHIFFRER dans la selection
         -- -----------------------------------------------
         -- Author:		LDE
         -- Create date: 26/10/2012
         -- Description:	14165: DNE - Evolution pour reprise DR : chŠque et Num_iban vide
         -- Ajout du num‚ro de chŠque
         -- -----------------------------------------------
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         -- --------------------------------------------  
         -- TLE 16/10/2013 : 16240 : Modification taille NUM_IBAN (de 27 … 34)
         -- ----------------------------------------------------------  
         -- DSZ 03/03/2014 : #114 : ajout bln_paiement_partiel
         -- ===============================================================
         -- OPA 03/03/2014 : U#123 S#2 : Ajout BLN_DEMANDE_CLOTURE_MODULE
         -- ===============================================================
         
         
         CREATE  PROCEDURE [dbo].[LEC_DET_DEMANDES_REGLEMENT]
         	 @ID_POSTE_COUT_REGLE AS INT,
         	 @ID_MODULE_PEC AS INT
         
         AS
         
         BEGIN
         
         	------------------------------------------------------
             -- Calculate ID_TRANSACTION and NUM_IBAN
             ------------------------------------------------------
         
         	DECLARE @ID_TRANSACTION AS INT
         	DECLARE @NUM_IBAN AS VARCHAR(34)
         
         	SET @ID_TRANSACTION = NULL
         	SET @NUM_IBAN = 0
         
         	SELECT
         		@ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION,
         		@NUM_IBAN = NUM_IBAN
         	FROM [TRANSACTION]
         		INNER JOIN MODULE_PEC ON [TRANSACTION].ID_ETABLISSEMENT_OF_DEST = MODULE_PEC. ID_ETABLISSEMENT_OF
         											AND MODULE_PEC.ID_MODULE_PEC = @ID_MODULE_PEC
         	WHERE
         	    [TRANSACTION].BLN_TRANSACTION_REGLEMENT_PRINCIPAL = 1
         
         	
         	------------------------------------------------
         	-- Block's destinataires du r‚glement
         	------------------------------------------------	
         
         	DECLARE @ID_ETABLISSEMENT_OF AS INT
         	DECLARE @ID_TIERS AS INT
         	DECLARE @ID_ETABLISSEMENT AS INT
         	DECLARE @ID_ADHERENT AS INT
         	DECLARE @ID_OF AS INT
         
         	SET @ID_ETABLISSEMENT_OF = NULL
         	SET @ID_TIERS = NULL
         	SET @ID_ETABLISSEMENT = NULL
         	SET @ID_ADHERENT = NULL
         	SET @ID_OF = NULL
         
         	SELECT
         		@ID_ETABLISSEMENT_OF = [TRANSACTION].ID_ETABLISSEMENT_OF_BENEF,
         		@ID_TIERS = [TRANSACTION].ID_TIERS_BENEF,
         		@ID_ETABLISSEMENT = [TRANSACTION].ID_ETABLISSEMENT_BENEF
         	FROM
         		[TRANSACTION]
         		INNER JOIN POSTE_COUT_REGLE ON POSTE_COUT_REGLE.ID_TRANSACTION = [TRANSACTION].ID_TRANSACTION
         						AND POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE =  @ID_POSTE_COUT_REGLE
         	
         	IF @ID_ETABLISSEMENT IS NOT NULL
         	BEGIN
         		SELECT 
         			@ID_ADHERENT = ETABLISSEMENT.ID_ADHERENT
         		FROM 
         			ETABLISSEMENT
         		WHERE 
         			ETABLISSEMENT.ID_ETABLISSEMENT = @ID_ETABLISSEMENT
         	END
         
         	IF @ID_ETABLISSEMENT_OF IS NOT NULL
         	BEGIN
         		SELECT 
         			@ID_OF = ETABLISSEMENT_OF.ID_OF
         		FROM 
         			ETABLISSEMENT_OF
         		WHERE 
         			ETABLISSEMENT_OF.ID_ETABLISSEMENT_OF = @ID_ETABLISSEMENT_OF
         	END
         
         
         	------------------------------------------------
         	-- Select Demande Detail
         	------------------------------------------------
         	
         	SELECT
         		DISTINCT
         		ISNULL(P.ID_POSTE_COUT_REGLE,0) AS ID_POSTE_COUT_REGLE,
         		MODULE_PEC.ID_MODULE_PEC AS ID_MODULE_PEC,
         		MODULE_PEC.COD_MODULE_PEC AS COD_MODULE_PEC,
         		MODULE_PEC.LIBL_MODULE_PEC AS LIBL_MODULE_PEC,
         		MODULE_PEC.ID_ACTION_PEC AS ID_ACTION_PEC,
         		ACTION_PEC.COD_ACTION_PEC AS COD_ACTION_PEC,
         		ACTION_PEC.LIBL_ACTION_PEC AS LIBL_ACTION_PEC,
         		ACTION_PEC.BLN_CYCLE_COURT AS BLN_CYCLE_COURT,
         		ACTION_PEC.CIBLE_ACTION,
         		CASE
         			WHEN MODULE_PEC.BLN_EXTERNE = 1 
         				THEN ORGANISME_FORMATION.ID_OF
         			WHEN  MODULE_PEC.BLN_EXTERNE = 0
         				THEN NULL		
         		END
         		AS ID_OF,
         		ISNULL(P.ID_TRANSACTION,@ID_TRANSACTION) AS ID_TRANSACTION,
         		MODULE_PEC.ID_ETABLISSEMENT_OF AS ID_ETABLISSEMENT_OF,
         		PERIODE.ID_PERIODE AS ID_PERIODE,
         		P.ID_SOUS_TYPE_COUT AS ID_SOUS_TYPE_COUT,
         		P.BLN_ACTIF AS BLN_ACTIF,
         		P.DAT_BAP AS DAT_BAP,
         		CASE 
         			WHEN @ID_POSTE_COUT_REGLE IS NULL THEN
         				NULL
         			ELSE
         				(SELECT ET.ID_ADHERENT
         				FROM ETABLISSEMENT ET
         				WHERE ET.ID_ETABLISSEMENT = P.ID_ETABLISSEMENT)
         		END
         		AS ID_ADHERENT,
         		P.BLN_FACTURE_DIRECTE AS BLN_FACTURE_DIRECTE,
         		P.NUM_FACTURE AS NUM_FACTURE,
         		P.ID_TYPE_FACTURE AS ID_TYPE_FACTURE,
         		P.NUM_FACTURE_AVOIR AS NUM_FACTURE_AVOIR,
         		P.MNT_DEMANDE_HT AS MNT_DEMANDE_HT,
         		P.MNT_DEMANDE_TVA AS MNT_DEMANDE_TVA,
         		P.MNT_DEMANDE_TTC AS MNT_DEMANDE_TTC,
         		P.MNT_REGLE_HT AS MNT_ACCORDE_HT,
         		P.MNT_REGLE_TVA AS MNT_ACCORDE_TVA,
         		P.MNT_REGLE_TTC AS MNT_ACCORDE_TTC,
         		P.ID_FACTURE,
         		P.ID_TYPE_TVA,
         		Coalesce(P.TAU_TVA,R26.TAU_TVA ) * 100 AS TAU_TVA,
         		P.DAT_CREATION AS DAT_CREATION,
         		P.DAT_MODIF AS DAT_MODIF,
         		P.ID_UTILISATEUR AS ID_UTILISATEUR,
         		P.BLN_OK_FINANCEMENT,
         		P.BLN_OK_PIECE,
         		P.ID_SESSION_PEC,
         		(SELECT COD_UTIL
         		FROM UTILISATEUR
         		WHERE ID_UTILISATEUR = P.ID_UTILISATEUR) AS PAR,
         		@NUM_IBAN AS NUM_IBAN_DESTINATAIRE,
         		@ID_TIERS AS ID_TIERS_DESTINATAIRE,
         		@ID_ADHERENT AS ID_ADHERENT_DESTINATAIRE,
         		@ID_OF AS ID_OF_DESTINATAIRE,
         		@ID_ETABLISSEMENT_OF AS ID_ETABLISSEMENT_OF_DESTINATAIRE,
         		@ID_ETABLISSEMENT AS ID_ETABLISSEMENT_DESTINAIRE,
         		P.ID_ETABLISSEMENT AS ID_ETABLISSEMENT_D_ADHERENT,
         		case P.ID_ETAT_PEC 	when 14 then 1 else 0 end as A_RECHIFFRER,
         		r.NUM_CHEQUE,												-- LDE 2012/10/26 14165
         		coalesce(p.BLN_PAIEMENT_PARTIEL,0) as BLN_PAIEMENT_PARTIEL,
         		COALESCE(p.BLN_DEMANDE_CLOTURE_MODULE,0) as BLN_DEMANDE_CLOTURE_MODULE
         	FROM
         		MODULE_PEC
         		LEFT JOIN POSTE_COUT_REGLE AS P ON P.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         			AND P.ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE
         		LEFT JOIN ACTION_PEC ON ACTION_PEC.ID_ACTION_PEC = MODULE_PEC.ID_ACTION_PEC
         		LEFT JOIN ORGANISME_FORMATION ON ORGANISME_FORMATION.ID_ETABLISSEMENT_OF_PRINCIPAL = MODULE_PEC.ID_ETABLISSEMENT_OF
         		LEFT JOIN PERIODE ON PERIODE.ID_PERIODE = MODULE_PEC.ID_PERIODE
         		LEFT JOIN REGLEMENT AS r ON p.ID_REGLEMENT = r.ID_REGLEMENT	-- LDE 2012/10/26 14165
         		LEFT JOIN R26 on R26.ID_PERIODE = MODULE_PEC.ID_PERIODE 
         	WHERE 
         		MODULE_PEC.ID_MODULE_PEC = @ID_MODULE_PEC
         
         END

		 ----------------------------------------------  
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         ----------------------------------------------  
         -- HBO - #316
         -- =============================================
         CREATE PROCEDURE [dbo].[LEC_COMPARE_MNT_DEMANDES_CONVENTION]
         	@ID_POSTE_COUT_REGLE	INT,
         	@ID_SOUS_TYPE_COUT		INT,
         	@ID_MODULE_PEC			INT,
             @MNT_CONVENTION			DECIMAL(18,2) OUTPUT,
         	@SUM_DEMANDE_HT			DECIMAL(18,2) OUTPUT
         AS
         BEGIN
         	DECLARE
         		@ID_TYPE_COUT	INT,
         		@TOTAL			DECIMAL(18,2),
         		@DEMANDE_HT		DECIMAL(18,2)
         
         	SELECT
         		@ID_TYPE_COUT = TYPE_COUT.ID_TYPE_COUT
         	FROM 
         		TYPE_COUT
         		INNER JOIN SOUS_TYPE_COUT
         			ON SOUS_TYPE_COUT.ID_TYPE_COUT = TYPE_COUT.ID_TYPE_COUT
         	WHERE
         		SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT = @ID_SOUS_TYPE_COUT
         
         	IF @ID_TYPE_COUT = 1
         	BEGIN
         		SELECT 
         			@SUM_DEMANDE_HT = SUM(POSTE_COUT_REGLE.MNT_DEMANDE_HT)
         		FROM
         			POSTE_COUT_REGLE
         			LEFT JOIN SOUS_TYPE_COUT
         				ON SOUS_TYPE_COUT.ID_SOUS_TYPE_COUT = POSTE_COUT_REGLE.ID_SOUS_TYPE_COUT
         			LEFT JOIN TYPE_COUT
         				ON TYPE_COUT.ID_TYPE_COUT = SOUS_TYPE_COUT.ID_TYPE_COUT
         		WHERE
         			TYPE_COUT.ID_TYPE_COUT = 1 
         			AND POSTE_COUT_REGLE.ID_MODULE_PEC = @ID_MODULE_PEC
         			AND POSTE_COUT_REGLE.BLN_ACTIF = 1
         
         		SELECT
         			@MNT_CONVENTION = MNT_CONVENTION
         		FROM 
         			MODULE_PEC
         		WHERE
         			MODULE_PEC.ID_MODULE_PEC = @ID_MODULE_PEC
         
         		IF @MNT_CONVENTION IS NULL
         		BEGIN
         			SET @MNT_CONVENTION = 0
         		END
         
         		IF @SUM_DEMANDE_HT IS NULL
         		BEGIN
         			SET @SUM_DEMANDE_HT = 0
         		END
         
         		SELECT
         			@DEMANDE_HT = MNT_DEMANDE_HT
         		FROM
         			POSTE_COUT_REGLE
         		WHERE
         			ID_POSTE_COUT_REGLE = @ID_POSTE_COUT_REGLE
         			AND POSTE_COUT_REGLE.BLN_ACTIF = 1
         
         		SET @SUM_DEMANDE_HT = @SUM_DEMANDE_HT - ISNULL(@DEMANDE_HT,0)
         	END
         	ELSE
         	BEGIN
         		SET @SUM_DEMANDE_HT = NULL
         		SET @MNT_CONVENTION = NULL
         	END
         END
         
         
		 -- =============================================
         -- Author:		DSZ
         -- Create date: 31/01/2011
         -- Description:	Donn‚es pour l'init de la creation de convention
         -- =============================================
         CREATE PROCEDURE INS_ECHEANCE_REELLE
         	@ID_TYPE_ECHEANCIER int,
         	@DAT_ECHEANCE_REELLE datetime,
         	@ID_CONV_GRP int,
         	@MNT_ECHEANCE_REELLE_HT decimal (18,2),
         	@MNT_ECHEANCE_REELLE_TTC decimal (18,2)
         AS
         BEGIN
         	INSERT INTO ECHEANCE_REELLE
         	(
         		ID_TYPE_ECHEANCIER,
         		DAT_CREATION,
         		DAT_ECHEANCE_REELLE,
         		ID_CONV_GRP,
         		MNT_ECHEANCE_REELLE_HT,
         		MNT_ECHEANCE_REELLE_TTC
         	)
         	VALUES 
         	(
         		@ID_TYPE_ECHEANCIER,
         		getdate(),
         		@DAT_ECHEANCE_REELLE,
         		@ID_CONV_GRP,
         		@MNT_ECHEANCE_REELLE_HT,
         		@MNT_ECHEANCE_REELLE_TTC
         	)
         
         return @@IDENTITY
         END
         
  CREATE PROCEDURE [dbo].[MVT_BUDGETAIRE_COLLECTE_ALIM_ENVELOPPE_COLLECTE]
          @ID_EVENEMENT  INT
         AS
         /* *****************************************************************************************
         -- Auteur   : MBL
         -- Date Creation : 11/12/2014
         
         -- Role    : Cr‚ation de mouvements Budg‚taires d'Alimentation des enveloppes de collecte 
         ---          pour tous les postes d'imputation associes a l'evenement, quelle que soit l'activit‚
         ***************************************************************************************** */
         BEGIN
          DECLARE 
           @ID_VERSEMENT   INT,
           @ID_ENVELOPPE   INT,
           @ID_PERIODE    INT,
           @ID_ACTIVITE   INT,
           @ID_BRANCHE    INT,
           @MNT_HT     DECIMAL(18,2),
           @DAT_EVENEMENT   DATETIME,
           @LIBL_MVT_BUDGETAIRE VARCHAR(60),
           @ID_ETABLISSEMENT  INT,
           @ID_ADHERENT   INT,
           @ID_GROUPE    INT,
           @NUM_ANNEE    INT,
           @ID_TYPE_MOUVEMENT  INT, 
           @GRP_TYPE_VERSEMENT  VARCHAR,
           @P_E_R     VARCHAR,
           @COD_TYPE_EVENEMENT  VARCHAR(8)
         
          SELECT
           @ID_VERSEMENT = ID_VERSEMENT,
           @DAT_EVENEMENT = DAT_EVENEMENT,
           @COD_TYPE_EVENEMENT = TYPE_EVENEMENT .COD_TYPE_EVENEMENT
          FROM
           EVENEMENT
           INNER JOIN TYPE_EVENEMENT
            ON TYPE_EVENEMENT.ID_TYPE_EVENEMENT = EVENEMENT.ID_TYPE_EVENEMENT
          WHERE
           ID_EVENEMENT = @ID_EVENEMENT
         
          DECLARE CU_POSTE_IMPUTATION SCROLL CURSOR FOR
          SELECT
           ETABLISSEMENT.ID_ETABLISSEMENT,
           ETABLISSEMENT.ID_ADHERENT,
           ETABLISSEMENT.ID_GROUPE,
           ETABLISSEMENT.ID_BRANCHE,
           POSTE_IMPUTATION.ID_ACTIVITE,
           POSTE_IMPUTATION.MNT_HT,
           PERIODE.ID_PERIODE,
           PERIODE.NUM_ANNEE,
           TYPE_VERSEMENT.GRP_TYPE_VERSEMENT
          FROM
           VERSEMENT
           INNER JOIN POSTE_VERSEMENT
            ON VERSEMENT.ID_VERSEMENT = POSTE_VERSEMENT.ID_VERSEMENT
           INNER JOIN ETABLISSEMENT
            ON ETABLISSEMENT.ID_ETABLISSEMENT = POSTE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE
           INNER JOIN POSTE_IMPUTATION
            ON POSTE_VERSEMENT.ID_POSTE_VERSEMENT = POSTE_IMPUTATION.ID_POSTE_VERSEMENT
           INNER JOIN PERIODE
            ON PERIODE.ID_PERIODE = POSTE_VERSEMENT.ID_PERIODE
           INNER JOIN TYPE_VERSEMENT
            ON POSTE_VERSEMENT.ID_TYPE_VERSEMENT = TYPE_VERSEMENT.ID_TYPE_VERSEMENT
          WHERE
           VERSEMENT.ID_VERSEMENT = @ID_VERSEMENT
           AND ABS(MNT_HT) > 0
          
          OPEN
           CU_POSTE_IMPUTATION 
          FETCH
           CU_POSTE_IMPUTATION
          INTO
           @ID_ETABLISSEMENT, 
           @ID_ADHERENT, 
           @ID_GROUPE, 
           @ID_BRANCHE, 
           @ID_ACTIVITE, 
           @MNT_HT, 
           @ID_PERIODE, 
           @NUM_ANNEE,
           @GRP_TYPE_VERSEMENT
          
          WHILE (@@FETCH_STATUS <> -1)
          BEGIN
           SET @ID_ENVELOPPE  = NULL
         
           SELECT
            @LIBL_MVT_BUDGETAIRE = ''
         
           IF (@COD_TYPE_EVENEMENT = 'VERSEMEN')
           BEGIN
            SET @LIBL_MVT_BUDGETAIRE = 'Versement '
           END
         
           IF (@COD_TYPE_EVENEMENT = 'REGUL')
           BEGIN
            SET @LIBL_MVT_BUDGETAIRE = 'Regul. Versement '
           END
         
           IF (@COD_TYPE_EVENEMENT = 'REVERSE')
           BEGIN
            SET @LIBL_MVT_BUDGETAIRE = 'Reversement '
           END
         
           SELECT
            @LIBL_MVT_BUDGETAIRE =
             @LIBL_MVT_BUDGETAIRE
             + CASE
              WHEN  @GRP_TYPE_VERSEMENT = 'O'
               THEN 'Obligatoire'
              ELSE  'Volontaire'
             END
             + ' '
             + CAST(@NUM_ANNEE AS VARCHAR)
           
           SET @ID_TYPE_MOUVEMENT = 6 -- Mouvement de Versement de Collecte
           SET @P_E_R = 'R'
         
           SELECT
            @ID_ENVELOPPE = ENVELOPPE.ID_ENVELOPPE
           FROM
            ENVELOPPE
            INNER JOIN TYPE_ENVELOPPE ON ENVELOPPE.ID_TYPE_ENVELOPPE  = TYPE_ENVELOPPE .ID_TYPE_ENVELOPPE 
           WHERE
            TYPE_ENVELOPPE.BLN_COLLECTE = 1
            AND TYPE_ENVELOPPE.ID_ACTIVITE = @ID_ACTIVITE
            AND TYPE_ENVELOPPE.ID_BRANCHE = @ID_BRANCHE
            AND ENVELOPPE.ID_PERIODE = @ID_PERIODE
         
           IF (@ID_ENVELOPPE IS NOT NULL)
           BEGIN
            EXEC dbo.INS_MVT_BUDGETAIRE
             @ID_TYPE_MOUVEMENT  = @ID_TYPE_MOUVEMENT,  
             @ID_EVENEMENT   = @ID_EVENEMENT,
             @ID_COMPTE    = NULL,
             @ID_ENVELOPPE   = @ID_ENVELOPPE,
             @MNT_MVT_BUDGETAIRE  = @MNT_HT,
             @P_E_R     = 'R',
             @DAT_MVT_BUDGETAIRE  = @DAT_EVENEMENT,
             @N_R     = 'N',
             @ID_PERIODE_FISC  = @ID_PERIODE,
             @ID_PERIODE_CPT   = @ID_PERIODE,
             @LIBL_MVT_BUDGETAIRE = @LIBL_MVT_BUDGETAIRE,
             @ID_ACTIVITE   = @ID_ACTIVITE ,
             @ID_ADHERENT   = @ID_ADHERENT,
             @ID_GROUPE    = @ID_GROUPE ,
             @ID_ETABLISSEMENT  = @ID_ETABLISSEMENT,
             @ID_TYPE_FINANCEMENT = NULL,
             @ID_DISPOSITIF   = NULL,
             @ID_MODULE_PEC   = NULL
           END
           ELSE
           BEGIN
            IF (@NUM_ANNEE >= 2012)
            BEGIN
             SELECT 'Enveloppe Collecte non trouvee POUR :', ID_ACTIVITE  = @ID_ACTIVITE , MNT_HT  = @MNT_HT  , ID_PERIODE  =@ID_PERIODE , ID_BRANCHE  = @ID_BRANCHE
         
             INSERT INTO
              PB_EVENEMENT_MVT_BUDGETAIRE_COLLECTE
             (
              ID_EVENEMENT   ,
              LIB_PB_EVENEMENT  , 
              LIBL_EVENEMENT   , 
              DAT_EVENEMENT   , 
              ID_TYPE_EVENEMENT  , 
              ID_VERSEMENT   ,
              COM_EVENEMENT   
             )
             SELECT
              ID_EVENEMENT   ,
              LIB_PB_EVENEMENT  = 'Enveloppe Collecte non trouv‚e', 
              LIBL_EVENEMENT   , 
              DAT_EVENEMENT   , 
              ID_TYPE_EVENEMENT  , 
              ID_VERSEMENT   ,
              COM_EVENEMENT   = 'Batch Regeneration MVT BUDGETAIRE (PS MVT_BUDGETAIRE_COLLECTE_ALIM_ENVELOPPE_COLLECTE) : ENveloppe Collecte non trouvee POUR : ' 
              + ' ID_ACTIVITE  = ' + CAST(@ID_ACTIVITE AS VARCHAR) 
              + ', MNT_HT  = ' + CAST(@MNT_HT AS VARCHAR) 
              + ', ID_PERIODE  = ' + CAST(@ID_PERIODE AS VARCHAR) 
              + ', ID_BRANCHE    = ' + CAST(@ID_BRANCHE AS VARCHAR) 
             FROM
              EVENEMENT
             WHERE
              ID_EVENEMENT = @ID_EVENEMENT
            END
           END  
         
           FETCH
            cu_poste_imputation
           INTO
            @ID_ETABLISSEMENT, 
            @ID_ADHERENT, 
            @ID_GROUPE, 
            @ID_BRANCHE, 
            @ID_ACTIVITE, 
            @MNT_HT, 
            @ID_PERIODE, 
            @NUM_ANNEE,
            @GRP_TYPE_VERSEMENT
          END
         
          CLOSE CU_POSTE_IMPUTATION 
          DEALLOCATE CU_POSTE_IMPUTATION 
         END

		 -- ============================================= 
         -- Author:		DSZ
         -- Create date: 14/11/2012
         -- Description:	14282 : UPDATE frequence Offre de service pour le groupe, descendue de l'Extranet
         -- ============================================= 
         CREATE PROCEDURE UPD_FREQUENCE_OFFRE_SERVICE 
         	@ID_GROUPE int, 
         	@ID_FREQUENCE int
         AS
         BEGIN
         	SET NOCOUNT ON;
         	
         	DECLARE @ID_OFFRE_SERVICE int
         	
         	SELECT @ID_OFFRE_SERVICE = ID_OFFRE_SERVICE
         	from OFFRE_SERVICE where OFFRE_SERVICE.ID_GROUPE = @ID_GROUPE
         	
         	IF @ID_OFFRE_SERVICE is not null --existe offre service
         		UPDATE OFFRE_SERVICE
         		set 
         			ID_FREQUENCE_DIFFUSION_AUTOMATIQUE = @ID_FREQUENCE,
         			DAT_MODIF = GETDATE(),
         			ID_UTILISATEUR = 82 --admin; peut ˆtre … passer en utilisateur special Extranet
         		where 
         			ID_OFFRE_SERVICE = @ID_OFFRE_SERVICE
         			and OFFRE_SERVICE.ID_FREQUENCE_DIFFUSION_AUTOMATIQUE <> @ID_FREQUENCE
         	ELSE
         	begin
         		if exists (SELECT 1 from groupe where id_groupe = @ID_GROUPE)
         			insert into OFFRE_SERVICE 
         				(ID_FREQUENCE_DIFFUSION_AUTOMATIQUE, 
         				ID_UTILISATEUR, 
         				ID_GROUPE,
         				DAT_MODIF, 
         				DAT_CREATION)
         			values
         				(@ID_FREQUENCE,
         				82,
         				@ID_GROUPE, 
         				GETDATE(), 
         				GETDATE())
         	end
         
         END
         
         -- =============================================
         -- AUTHOR				: EOU
         -- MODIFICATION DATE	: 02 MAI 2012
         -- DESCRIPTION			: 13191
         -- ---------------------------------------------
         -- DSZ 18/06/2012 13605
         -- SI NB_JOURS_VALIDATION_ADP =0,VALIDATION IMMDIATE : ENVOI · LA BANQUE. PASSE EN AUTORISATION VALIDE 
         -- =============================================
         -- WKH, 18/12/2013, #16610, L'TAT "AUTORISATION CONFORME" PASSE EN TAT "AUTORISATION VALIDE" 
         -- SI GETDATE() > PARAMETRES.NB_JOURS_VALIDATION_ADP + AUTORISATION_PRELEVEMENT.DAT_SIGNATURE
         -- =============================================
         CREATE PROCEDURE dbo.UPD_AUTORISATION_PRELEVEMENT_ETAT
         	@ID_ETAT INT,
         	@ID_AUTORISATION_PRELEVEMENT_AUTOMATIQUE INT,
         	@DATE DATETIME
         AS
         BEGIN
         	DECLARE
         		@NB_JOURS INT,
         		@DATE_SIG DATETIME
         
         	SELECT TOP 1 
         		@NB_JOURS = NB_JOURS_BORD_P 
         	FROM
         		PARAMETRES
         
         	SELECT TOP 1
         		@DATE_SIG  = DAT_SIGNATURE
         	FROM
         		AUTORISATION_PRELEVEMENT
         	WHERE
         		ID_AUTORISATION_PRELEVEMENT = @ID_AUTORISATION_PRELEVEMENT_AUTOMATIQUE
         
         	IF (@ID_ETAT = 2) AND EXISTS(SELECT 1 FROM PARAMETRES WHERE NB_JOURS_VALIDATION_ADP = 0)
         	AND (DATEDIFF(DAY, GETDATE(), DATEADD(DAY, @NB_JOURS, @DATE_SIG) ) > 0) -- WKH, 18/12/2013, #16610, L'TAT "AUTORISATION CONFORME" 
         																			-- PASSE EN TAT "AUTORISATION VALIDE" 
         																			-- SI GETDATE() > PARAMETRES.NB_JOURS_VALIDATION_ADP + AUTORISATION_PRELEVEMENT.DAT_SIGNATURE
         		SET @ID_ETAT  = 3
         		
         	IF (@ID_ETAT = 2 ) --ENVOYE · LA BANQUE(AUTORISATION CONFORME)
         	BEGIN
         		UPDATE
         			AUTORISATION_PRELEVEMENT 
         		SET
         			ID_ETAT = @ID_ETAT,
         			DAT_RECEPTION_CONFORME = @DATE 
         		FROM
         			AUTORISATION_PRELEVEMENT 
         			CROSS JOIN PARAMETRES 
         		WHERE
         			ID_AUTORISATION_PRELEVEMENT = @ID_AUTORISATION_PRELEVEMENT_AUTOMATIQUE
         	END
         	
         	IF (@ID_ETAT = 3 ) --AUTORISATION VALIDE
         	BEGIN
         		UPDATE
         			AUTORISATION_PRELEVEMENT  
         		SET
         			DAT_VALIDATION = @DATE ,
         			ID_ETAT = @ID_ETAT
         		WHERE
         			ID_AUTORISATION_PRELEVEMENT = @ID_AUTORISATION_PRELEVEMENT_AUTOMATIQUE
         	 END
         
         	 IF (@ID_ETAT IN (4,5,6))
         	 BEGIN
         		UPDATE
         			AUTORISATION_PRELEVEMENT 
         		SET
         			DAT_ANNULATION = @DATE ,
         			ID_ETAT = @ID_ETAT
         		WHERE
         			ID_AUTORISATION_PRELEVEMENT = @ID_AUTORISATION_PRELEVEMENT_AUTOMATIQUE 
         	 END
         END


         -- =============================================
         -- Author:		AMA
         -- Create date: 02/04/2009
         -- Description:	FICHE 331: lettre une facture et ses avoirs
         --				retourne le nombre de facture lettrees 
         -- =============================================
         -- Author:		ASD
         -- Create date: 19/06/2009
         -- Description:	Correction large (non comment‚e car n‚cessaire … chaque paragraphe) 
         -- =============================================
         -- Author:		SAFI
         -- Create date: 31/03/2010
         -- Description:	Added a variable @CONTROL, to track the successive steps of this procedure, 
         --				and know exactly what happens, There are two things have to be checked before 
         --				proceeding with LETTREing a FACTURE.
         --				The Tolerance Percentage is getting table PARAMETRES.FACT_TOLERANCE
         -- =============================================
         -- Author:		ASD
         -- Create date: 15/07/2010
         -- Description:	Correction faute de copier-coller, jointure superflue, suppression code inutile
         --				Am‚lioration comparaison pourcentage
         -- *****  A NETTOYER POUR LA PROCHAINE VERSION  / VERIFIER UTILITE GESTION DES CONTROLES***** --
         -- =============================================
         -- OPA 31/05/2013 : 15031 : SBR - suppression du type FLOAT et REAL dans le SQL : 2- FLOAT
         -- =============================================
         
         CREATE PROCEDURE [dbo].[LETTRAGE_POTENTIELLEMENT_AUTORISE]	
         	@ID_FACTURE INT
         
         AS
         BEGIN
         
         	IF EXISTS(SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#MA_FACTURE_LETTRABLE'))
         	BEGIN
         		DROP TABLE #MA_FACTURE_LETTRABLE
         	END
         -- <SAFI AJOUTER 1334 : 31/03/2010 >
         -- Added a variable @CONTROL, to track the successive steps of this procedure, and know exactly what happens!
         -- There are two things have to be checked before proceeding with LETTREing a FACTURE.
         -- the error codes are below:
         -- 10 -- First control failed
         -- 11 -- First control suceed
         -- 20 -- Second control failed
         -- 21 -- Second control suceed
         -- 30 -- Update not suceed
         DECLARE @CONTROL INT 
         -- GETTING THE TOLERANCE PERECENTAGE
         DECLARE @FACT_TOLERANCE decimal(18,2)
         SELECT TOP 1 @FACT_TOLERANCE = FACT_TOLERANCE FROM PARAMETRES
         -- </SAFI AJOUTER 1334 : 31/03/2010 >
         
         
         	create TABLE #MA_FACTURE_LETTRABLE
         	(
         		ID_FACTURE	int,
         		MNT_HT		decimal(18,2),
         		SUM_PCR		decimal(18,2)
         	)
         
         	insert into #MA_FACTURE_LETTRABLE
         -- insert factures sans PCR, sans session_pro ou avec PCR et session_pro toutes BAP‚es et r‚gl‚es
         	select f.ID_FACTURE, 
         					f.MNT_HT, 
         					SUM(
         						coalesce(pcr.MNT_REGLE_HT,0) + 
         						case 
         							when sp.ID_FACTURE_OF =f.id_facture then
         							coalesce(sp.MNT_VERSE_HT_OF,0)
         							when sp.ID_FACTURE_ADHERENT =f.id_facture then
         -- correction ASD 20100715
         --							coalesce(sp.MNT_VERSE_HT_OF,0)
         							coalesce(sp.MNT_VERSE_HT_ADH,0)
         							else
         							0
         						end
         					)
         				as SUM_PCR											
         	from FACTURE f
         	left JOIN POSTE_COUT_REGLE pcr on pcr.ID_FACTURE = f.ID_FACTURE
         -- correction ASD 20100715
         --	left JOIN REGLEMENT r on r.ID_REGLEMENT = pcr.ID_REGLEMENT 		
         	left JOIN SESSION_PRO sp on sp.ID_FACTURE_OF = f.ID_FACTURE or sp.ID_FACTURE_ADHERENT = f.ID_FACTURE
         	where 
         		 f.BLN_ACTIF = 1
         		AND f.ID_TYPE_FACTURE = 1
         		AND (f.BLN_LETTRE IS NULL or f.BLN_LETTRE =0)
         		AND f.ID_FACTURE = @ID_FACTURE
         		and not exists(	
         			select * from POSTE_COUT_REGLE 
         				left JOIN REGLEMENT  on REGLEMENT.ID_REGLEMENT = POSTE_COUT_REGLE.ID_REGLEMENT 		
         				where
         					(POSTE_COUT_REGLE.DAT_BAP is null
         					or REGLEMENT.DAT_VALID_REGLEMENT is null)
         					and POSTE_COUT_REGLE.ID_FACTURE = f.ID_FACTURE
         		) 		
         		and not exists(	
         			select * from SESSION_PRO 
         				left JOIN REGLEMENT_PRO  on REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_ADH		
         				where
         					(SESSION_PRO.DAT_BAP_ADH is null
         					or REGLEMENT_PRO.DAT_VALID_REGLEMENT is null)
         					and SESSION_PRO.ID_FACTURE_ADHERENT = f.ID_FACTURE
         		) 		
         		and not exists(	
         			select * from SESSION_PRO 
         				left JOIN REGLEMENT_PRO  on REGLEMENT_PRO.ID_REGLEMENT_PRO = SESSION_PRO.ID_REGLEMENT_PRO_OF		
         				where
         					(SESSION_PRO.DAT_BAP_OF is null
         					or REGLEMENT_PRO.DAT_VALID_REGLEMENT is null)
         					and SESSION_PRO.ID_FACTURE_OF = f.ID_FACTURE
         		) 		
         	group by f.ID_FACTURE,f.MNT_HT
         
         	--Traitement des avoirs associ‚s aux factures
         	IF EXISTS(SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#LETTRABLES_AVEC_AVOIR'))
         		BEGIN
         			DROP TABLE #LETTRABLES_AVEC_AVOIR
         		END
         
         
         	--R‚cuperer la liste des avoirs associ‚s … la facture pass‚e en paramŠtre
         	select	fav.ID_FACTURE, 
         			fav.ID_FACTURE_ASSOCIEE_AVOIR,
         			fav.MNT_HT, 
         			SUM(coalesce(pcrav.MNT_REGLE_HT,0)) as SUM_PCR
         	into #LETTRABLES_AVEC_AVOIR
         	from facture fav
         		left JOIN POSTE_COUT_REGLE pcrav on pcrav.ID_FACTURE = fav.ID_FACTURE
         --ASD 20100715 Suppr
         --		left JOIN REGLEMENT rav on rav.ID_REGLEMENT = pcrav.ID_REGLEMENT 		
         	where 
         		fav.BLN_ACTIF = 1
         		and fav.ID_TYPE_FACTURE = 2
         		AND fav.ID_FACTURE_ASSOCIEE_AVOIR is not null
         		and fav.ID_FACTURE_ASSOCIEE_AVOIR IN (select ID_FACTURE from #MA_FACTURE_LETTRABLE)-- voire @ID_FACTURE
         		and not exists(	--puisqu'on ne peut utiliser les avoirs qu'en PEC :
         			select * from POSTE_COUT_REGLE 
         				left JOIN REGLEMENT  on REGLEMENT.ID_REGLEMENT = POSTE_COUT_REGLE.ID_REGLEMENT 		
         				where
         					(POSTE_COUT_REGLE.DAT_BAP is null
         					or REGLEMENT.DAT_VALID_REGLEMENT is null)
         					and POSTE_COUT_REGLE.ID_FACTURE = fav.ID_FACTURE
         		) 		
         	group by fav.ID_FACTURE, fav.MNT_HT, fav.ID_FACTURE_ASSOCIEE_AVOIR
         
         	IF EXISTS(SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#MES_FACTURES_LETTRABLES_AVEC_AVOIR'))
         		BEGIN
         			DROP TABLE #MES_FACTURES_LETTRABLES_AVEC_AVOIR
         		END
         
         	select	F1.ID_FACTURE  as ID_FACTURE,
         			A1.ID_FACTURE  as ID_AVOIR,
         			F1.MNT_HT as MNT_FACTURE,
         			A1.MNT_HT as MNT_AVOIR,
         			F1.SUM_PCR	   as CUMUL_FACTURE,
         			A1.SUM_PCR	   as CUMUL_AVOIR
         --ASD			--,round(ABS(F1.MNT_HT + A1.MNT_HT - F1.SUM_PCR - A1.SUM_PCR),2) AS DIFFERENTIEL
         	 into #MES_FACTURES_LETTRABLES_AVEC_AVOIR
         --20100715 ASD : mise … niveau !!
         --	 from	#MA_FACTURE_LETTRABLE	F1, 
         --			#LETTRABLES_AVEC_AVOIR		A1
         --	 where A1.ID_FACTURE_ASSOCIEE_AVOIR = F1.ID_FACTURE
         	from #MA_FACTURE_LETTRABLE	F1 
         		inner join		#LETTRABLES_AVEC_AVOIR		A1
         		on A1.ID_FACTURE_ASSOCIEE_AVOIR = F1.ID_FACTURE
         
         -- <SAFI AJOUTER 12334 30/03/2010>
         -- First Control as described on the specification:
         -- 1 Controle-les Demandes de RŠglement et sessions PRO (actives) associ‚es … la facture ou ses avoirs sont r‚gl‚es ET bap‚es.
         -- If the first control is suceeded, we have atleaset one line on any one of the following tables 
         	-- #MA_FACTURE_LETTRABLE OR #MES_FACTURES_LETTRABLES_AVEC_AVOIR.
         	-- Indicates the facture is BAPE and REGLE.
         
         IF((SELECT COUNT(*) FROM #MA_FACTURE_LETTRABLE)> 0 
         --20100715 ASD 
         --OR
         AND
            (SELECT COUNT(*) FROM #MES_FACTURES_LETTRABLES_AVEC_AVOIR)> 0)
         	BEGIN
         		SET @CONTROL = 11	
         		-- 2 Controle as described on the specification :	V‚rifie ensuite si la facture est lettrable. 		
         		--													Parmi les factures lettrables, il faut supprimer celles
         		--													qui ont des avoirs et dont le diff‚rentiel excŠde 5% du montant
         		--													total de la facture
         		delete t1 from #MA_FACTURE_LETTRABLE t1
         		where t1.ID_FACTURE in
         		(
         			select t2.ID_FACTURE
         			from #MES_FACTURES_LETTRABLES_AVEC_AVOIR t2		
         --ASD			--where ABS(t2.DIFFERENTIEL/t2.MNT_FACTURE) > @FACT_TOLERANCE -- 0.05 
         where round(ABS((t2.MNT_FACTURE + t2.MNT_AVOIR - t2.CUMUL_FACTURE - t2.CUMUL_AVOIR)/ (t2.MNT_FACTURE+ t2.MNT_AVOIR)),3) > @FACT_TOLERANCE -- 0.05 
         			AND t2.MNT_FACTURE <> 0
         		) 
         --20100715 ASD inutile
         		-- il faut supprimer ‚galement celles qui n'ont pas d'avoir et qui ne sont pas 100% BAP‚es/R‚gl‚es
         --		delete t1 from #MA_FACTURE_LETTRABLE t1
         --		where t1.ID_FACTURE in 
         --		(select ID_FACTURE from #MA_FACTURE_LETTRABLE
         --			WHERE MNT_HT<>0
         --			AND (abs((MNT_HT-SUM_PCR) / MNT_HT) > @FACT_TOLERANCE) -- 0.05)
         --			AND (round(abs(SUM_PCR),2)) < round(abs(MNT_HT),2) )
         --		and t1.ID_FACTURE not in 
         --		(
         --			select t2.ID_FACTURE
         --			from #MES_FACTURES_LETTRABLES_AVEC_AVOIR t2		
         --		)
         --		
         		
         
         		IF((SELECT COUNT(*) FROM #MA_FACTURE_LETTRABLE)> 0 
         --20100715 ASD 
         --OR
         AND
         	   (SELECT COUNT(*) FROM #MES_FACTURES_LETTRABLES_AVEC_AVOIR)> 0)		
         			BEGIN	
         				-- <SAFI AJOUTER 1334 : 31/03/2010 >				
         				DECLARE @UDATED INT 
         				SET @UDATED = 0
         				-- </SAFI AJOUTER 1334 : 31/03/2010 >
         				--Mise … jour du champ lettre dans les factures
         				update FACTURE
         				SET BLN_LETTRE = 1	
         				where ID_FACTURE in		
         				(
         					select t2.ID_FACTURE
         					from #MA_FACTURE_LETTRABLE t2						
         				)
         				-- <SAFI AJOUTER 1334 : 31/03/2010 >
         				SET @UDATED = @UDATED + @@ROWCOUNT
         				-- </SAFI AJOUTER 1334 : 31/03/2010 >
         				--Mise … jour du champ lettre dans les avoirs
         				update FACTURE
         				SET BLN_LETTRE = 1		
         				where ID_FACTURE in		
         				(
         					select t2.ID_AVOIR
         					from #MES_FACTURES_LETTRABLES_AVEC_AVOIR t2		
         --ASD			--where ABS(t2.DIFFERENTIEL/t2.MNT_FACTURE) < @FACT_TOLERANCE -- 0.05 
         where round(ABS((t2.MNT_FACTURE + t2.MNT_AVOIR - t2.CUMUL_FACTURE - t2.CUMUL_AVOIR)/ (t2.MNT_FACTURE+ t2.MNT_AVOIR)),3) <= @FACT_TOLERANCE -- 0.05 
         					AND t2.MNT_FACTURE <> 0
         				)
         				-- <SAFI AJOUTER 1334 : 31/03/2010 >
         				SET @UDATED = @UDATED + @@ROWCOUNT		
         
         						
         						IF( @UDATED > 0) -- lignes sont updat‚
         						BEGIN
         							SET @CONTROL  = 21 -- donc les ‚taps sont success
         						END
         						ELSE
         						BEGIN
         							SET @CONTROL = 30 -- update non success
         						END
         				-- <SAFI AJOUTER 1334 : 31/03/2010 >
         				--retourne le nombre de factures lettrees		
         			END
         		ELSE
         			BEGIN		
         			-- <SAFI AJOUTER 1334 : 31/03/2010 >
         			SET @CONTROL  = 20
         			-- </SAFI AJOUTER 1334 : 31/03/2010 >
         			END	
           END      
         ELSE -- Premier Control non succes.
         		BEGIN	
         			-- <SAFI AJOUTER 1334 : 31/03/2010 >	
         			SET @CONTROL = 10 
         			-- </SAFI AJOUTER 1334 : 31/03/2010 >
         		END
         -- <SAFI AJOUTER 1334 : 31/03/2010 >
         SELECT @CONTROL
         -- </SAFI AJOUTER 1334 : 31/03/2010 >
         	
         END
         
         
         
         
CREATE PROCEDURE [dbo].[MVT_BUDGETAIRE_COLLECTE_ABONDEMENT_COMPTE]
          @ID_EVENEMENT  INT
         AS
         /* *****************************************************************************************
         -- Auteur   : MBL
         -- Date Creation : 11/12/2014
         
         -- Role    : Cr‚ation de mouvements Budg‚taires permettant :
             : Pour les VO, d'imputer l'enveloppe FPSPP     (Type de Mouvement 23 : Alim Pot Mut FPSPP
             : Pour les VO, d'imputer l'enveloppe Pot de Mutualisation (Type de Mouvement 11 : Alim Pot Mut )
             : Pour les VV, d'imputer l'enveloppe de Frais de Gestion (Type de Mouvement 9 : Alim Frais Gestion )
             : d'imputer le compte Groupe        (Type de Mouvement 10 : Alim Compte courant )
         
         Les VO ne sont alim‚nt‚s que lorsque :
          - le Groupe Type de versement = 'O' 
          - et existence param‚trage d'un abondement … verser sur un compte adh‚rent 
            pour l'activit‚, la p‚riode et la branche collect‚e (PARAM_ABONDEMENT_COMPTE.PRC_ABONDEMENT)    
         ***************************************************************************************** */
         BEGIN
          DECLARE 
           @ID_VERSEMENT    INT,
           @ID_ENVELOPPE_POT_MUT  INT,
           @ID_PERIODE     INT,
           @ID_ACTIVITE    INT,
           @ID_BRANCHE     INT,
           @DAT_EVENEMENT    DATETIME,
           @LIBL_MVT_BUDGETAIRE  VARCHAR(60),
           @ID_ETABLISSEMENT   INT,
           @ID_ADHERENT    INT,
           @ID_GROUPE     INT,
           @NUM_ANNEE     INT,
           @DAT_CONSTITUTION_POT_MUT DATETIME,
           @ID_PARAM_ABONDEMENT_COMPTE INT,
           @PRC_ABONDEMENT    DECIMAL(18,3),
           @ID_TYPE_FINANCEMENT  INT,
           @MNT_MVT_BUDGETAIRE   DECIMAL(18,3),
           @GRP_TYPE_VERSEMENT   VARCHAR,
           @MNT_HT      DECIMAL(18,2),
           @MNT_DONT_REGLE_FPSPP  DECIMAL(18,2),
           @MNT_POT_MUTUALISATION  DECIMAL(18,2),
           @MNT_ABONDEMENT_COMPTE  DECIMAL(18,2),
           @ID_COMPTE     INT,
           @PRC_FRAIS_GESTION   DECIMAL(18,2),
           @ID_ENVELOPPE_FRAIS_GESTION INT,
           @MNT_FRAIS_GESTION   DECIMAL(18,2),
           @REF_VERSEMENT    VARCHAR(35),
           @COD_TYPE_EVENEMENT   VARCHAR(8),
           @ID_TYPE_MOUVEMENT   INT,
           @P_E_R      VARCHAR
         
           
          SELECT
           @ID_VERSEMENT = ID_VERSEMENT , 
           @DAT_EVENEMENT = DAT_EVENEMENT,
           @COD_TYPE_EVENEMENT = TYPE_EVENEMENT .COD_TYPE_EVENEMENT 
          FROM
           EVENEMENT
           INNER JOIN TYPE_EVENEMENT ON TYPE_EVENEMENT.ID_TYPE_EVENEMENT = EVENEMENT.ID_TYPE_EVENEMENT 
          WHERE
           ID_EVENEMENT = @ID_EVENEMENT
          
          DECLARE CU_POSTE_IMPUTATION SCROLL CURSOR FOR
          SELECT
           ETABLISSEMENT.ID_ETABLISSEMENT, 
           ETABLISSEMENT.ID_ADHERENT, 
           ETABLISSEMENT.ID_GROUPE, 
           ETABLISSEMENT.ID_BRANCHE, 
           POSTE_IMPUTATION.ID_ACTIVITE, 
           MNT_HT    = CAST(ISNULL(POSTE_IMPUTATION .MNT_HT, 0) AS DECIMAL(15, 2)), 
           DONT_REGLE_FPSPP = CAST(ISNULL(POSTE_IMPUTATION .DONT_REGLE_FPSPP, 0) AS DECIMAL(15, 2)), 
           PERIODE.ID_PERIODE, 
           PERIODE.NUM_ANNEE,
           PERIODE.DAT_CONSTITUTION_POT_MUT,
           TYPE_VERSEMENT.GRP_TYPE_VERSEMENT,
           REF_VERSEMENT=
           CASE 
           WHEN LEN(LTRIM(VERSEMENT.NUM_CHEQUE)) > 0 
           THEN '-Chq Nø' + VERSEMENT.NUM_CHEQUE
           WHEN LEN(LTRIM(VERSEMENT.REF_EXTRANET)) > 0 
           THEN '-Ref Extranet ' + VERSEMENT.REF_EXTRANET
           ELSE
           '-Virt du ' + CONVERT(VARCHAR(10), COALESCE(VERSEMENT.DAT_VIREMENT, VERSEMENT.DAT_SAISIE, VERSEMENT.DAT_MODIF), 103)
           END
          FROM
           VERSEMENT
           INNER JOIN POSTE_VERSEMENT
            ON VERSEMENT.ID_VERSEMENT = POSTE_VERSEMENT.ID_VERSEMENT
           INNER JOIN ETABLISSEMENT
            ON ETABLISSEMENT.ID_ETABLISSEMENT = POSTE_VERSEMENT.ID_ETABLISSEMENT_BENEFICIAIRE
           INNER JOIN POSTE_IMPUTATION
            ON POSTE_VERSEMENT.ID_POSTE_VERSEMENT = POSTE_IMPUTATION .ID_POSTE_VERSEMENT 
           INNER JOIN PERIODE
            ON PERIODE.ID_PERIODE = POSTE_VERSEMENT.ID_PERIODE 
           INNER JOIN TYPE_VERSEMENT
            ON POSTE_VERSEMENT.ID_TYPE_VERSEMENT  = TYPE_VERSEMENT.ID_TYPE_VERSEMENT   
          WHERE VERSEMENT.ID_VERSEMENT= @ID_VERSEMENT
          AND ABS(POSTE_IMPUTATION.MNT_HT) > 0
         
          OPEN
           CU_POSTE_IMPUTATION 
          FETCH
           CU_POSTE_IMPUTATION
          INTO
           @ID_ETABLISSEMENT, 
           @ID_ADHERENT, 
           @ID_GROUPE, 
           @ID_BRANCHE, 
           @ID_ACTIVITE, 
           @MNT_HT, 
           @MNT_DONT_REGLE_FPSPP,
           @ID_PERIODE, 
           @NUM_ANNEE,
           @DAT_CONSTITUTION_POT_MUT,
           @GRP_TYPE_VERSEMENT,
           @REF_VERSEMENT
         
          WHILE (@@FETCH_STATUS <> -1)
          BEGIN
         
           IF @GRP_TYPE_VERSEMENT = 'O' -- Versement de Type Obligatoire
           BEGIN
            SET @ID_PARAM_ABONDEMENT_COMPTE = NULL
         
            -- Recherche si un abondement est parametre pour le compte pour l'activite/periode/branche
            SELECT  @ID_PARAM_ABONDEMENT_COMPTE = PARAM_ABONDEMENT_COMPTE .ID_PARAM_ABONDEMENT_COMPTE,
            @PRC_ABONDEMENT    = PARAM_ABONDEMENT_COMPTE .PRC_ABONDEMENT,
            @ID_TYPE_FINANCEMENT  = PARAM_ABONDEMENT_COMPTE .ID_TYPE_FINANCEMENT
            FROM PARAM_ABONDEMENT_COMPTE 
            WHERE PARAM_ABONDEMENT_COMPTE.ID_PERIODE  = @ID_PERIODE
            AND  PARAM_ABONDEMENT_COMPTE.ID_BRANCHE  = @ID_BRANCHE
            AND  PARAM_ABONDEMENT_COMPTE.ID_ACTIVITE  = @ID_ACTIVITE
         
            -- SI un abondement est prevu sur un compte adherent
            IF @ID_PARAM_ABONDEMENT_COMPTE IS NOT NULL 
            BEGIN
         
             SET @ID_ENVELOPPE_POT_MUT  = NULL
         
             -- Recherche enveloppe de Pot de Mut associee a l'activite/periode/branche
             SELECT @ID_ENVELOPPE_POT_MUT = ENVELOPPE.ID_ENVELOPPE
             FROM ENVELOPPE
             INNER JOIN TYPE_ENVELOPPE ON ENVELOPPE.ID_TYPE_ENVELOPPE  = TYPE_ENVELOPPE .ID_TYPE_ENVELOPPE 
             WHERE TYPE_ENVELOPPE .BLN_POT_MUTUALISATION_COLLECTE = 1
             AND  TYPE_ENVELOPPE .ID_ACTIVITE = @ID_ACTIVITE
             AND  TYPE_ENVELOPPE .ID_BRANCHE  = @ID_BRANCHE
             AND  ENVELOPPE.ID_PERIODE  = @ID_PERIODE
         
             IF  @ID_ENVELOPPE_POT_MUT IS NOT NULL 
             BEGIN
         
          -- Alimentation du pot de Mut Alim. Pot Mut FPSPP
          IF ABS (@MNT_DONT_REGLE_FPSPP ) > 0
          BEGIN
          --SELECT ' Enveloppe Pot Mut FPSPP TROUVEE :', LIBL_ENVELOPPE, MNT_DONT_REGLE_FPSPP = @MNT_DONT_REGLE_FPSPP
          --FROM ENVELOPPE
          --WHERE ID_ENVELOPPE = @ID_ENVELOPPE 
         
          SELECT @LIBL_MVT_BUDGETAIRE = 'VO Alim. Pot MUT FPSPP '
          IF @COD_TYPE_EVENEMENT = 'REGUL' SET @LIBL_MVT_BUDGETAIRE = 'Regul. ' + @LIBL_MVT_BUDGETAIRE 
          IF @COD_TYPE_EVENEMENT = 'REVERSE' SET @LIBL_MVT_BUDGETAIRE = 'Reversement ' + @LIBL_MVT_BUDGETAIRE 
         
          SELECT @LIBL_MVT_BUDGETAIRE  = @LIBL_MVT_BUDGETAIRE + CAST(@NUM_ANNEE AS VARCHAR)
         
          SET @ID_TYPE_MOUVEMENT = 23 -- Mouvement d'alimentation du pot de Mut Alim. Pot Mut FPSPP
          SET @P_E_R = 'R'
         
          EXEC dbo.INS_MVT_BUDGETAIRE
          @ID_TYPE_MOUVEMENT  = @ID_TYPE_MOUVEMENT,  
          @ID_EVENEMENT   = @ID_EVENEMENT,
          @ID_COMPTE    = NULL,
          @ID_ENVELOPPE   = @ID_ENVELOPPE_POT_MUT,
          @MNT_MVT_BUDGETAIRE  = @MNT_DONT_REGLE_FPSPP,
          @P_E_R     = 'R',
          @DAT_MVT_BUDGETAIRE  = @DAT_EVENEMENT,
          @N_R     = 'N',
          @ID_PERIODE_FISC  = @ID_PERIODE,
          @ID_PERIODE_CPT   = @ID_PERIODE,
          @LIBL_MVT_BUDGETAIRE = @LIBL_MVT_BUDGETAIRE,
          @ID_ACTIVITE   = @ID_ACTIVITE ,
          @ID_ADHERENT   = @ID_ADHERENT,
          @ID_GROUPE    = @ID_GROUPE ,
          @ID_ETABLISSEMENT  = @ID_ETABLISSEMENT,
          @ID_TYPE_FINANCEMENT = NULL,
          @ID_DISPOSITIF   = NULL,
          @ID_MODULE_PEC   = NULL
          END
         
          -- Alimentation du Pot de Mut (Hors FPSPP)
          IF @DAT_EVENEMENT >= @DAT_CONSTITUTION_POT_MUT
          BEGIN
          -- L'evenement a lieu a date Revolue, apres la constitution du pot de MUT., => 100% des montants sont mutualises (hors FPSPP)
          SELECT @LIBL_MVT_BUDGETAIRE  = 'VO DATE REVOLUE Alim. Pot MUT '
         
          SELECT @MNT_POT_MUTUALISATION  = @MNT_HT - @MNT_DONT_REGLE_FPSPP
         
          END
          ELSE
          BEGIN
          -- L'evenement a lieu avant la constitution du pot de MUT. 
          -- => Le montant mutualis‚ est determine a partir de PARAM_ABONDEMENT_COMPTE.PRC_ABONDEMENT 
          --    = (MNT_HT - MNT_FPSPP)* (1 -PRC_ABONDEMENT/100)
         
          SELECT @LIBL_MVT_BUDGETAIRE  = 'VO Alim. Pot MUT '
         
          SELECT @MNT_POT_MUTUALISATION  = CAST((@MNT_HT - @MNT_DONT_REGLE_FPSPP) * (1 - @PRC_ABONDEMENT) AS DECIMAL(15, 2))    
          END
         
          IF @COD_TYPE_EVENEMENT = 'REGUL' SET @LIBL_MVT_BUDGETAIRE = 'Regul ' + @LIBL_MVT_BUDGETAIRE 
          IF @COD_TYPE_EVENEMENT = 'REVERSE' SET @LIBL_MVT_BUDGETAIRE = 'Reversement ' + @LIBL_MVT_BUDGETAIRE 
          SELECT @LIBL_MVT_BUDGETAIRE  = @LIBL_MVT_BUDGETAIRE + CAST(@NUM_ANNEE AS VARCHAR)
         
          IF ABS (@MNT_POT_MUTUALISATION ) > 0
          BEGIN
          --SELECT ' Enveloppe Pot Mut FPSPP TROUVEE :', LIBL_ENVELOPPE, MNT_DONT_REGLE_FPSPP = @MNT_DONT_REGLE_FPSPP
          --FROM ENVELOPPE
          --WHERE ID_ENVELOPPE = @ID_ENVELOPPE 
         
          SET @ID_TYPE_MOUVEMENT = 11 -- Mouvement d'alimentation du pot de Mut Alim. Pot Mut FPSPP
          SET @P_E_R = 'R'
         
          EXEC dbo.INS_MVT_BUDGETAIRE
          @ID_TYPE_MOUVEMENT  = @ID_TYPE_MOUVEMENT,  
          @ID_EVENEMENT   = @ID_EVENEMENT,
          @ID_COMPTE    = NULL,
          @ID_ENVELOPPE   = @ID_ENVELOPPE_POT_MUT,
          @MNT_MVT_BUDGETAIRE  = @MNT_POT_MUTUALISATION,
          @P_E_R     = 'R',
          @DAT_MVT_BUDGETAIRE  = @DAT_EVENEMENT,
          @N_R     = 'N',
          @ID_PERIODE_FISC  = @ID_PERIODE,
          @ID_PERIODE_CPT   = @ID_PERIODE,
          @LIBL_MVT_BUDGETAIRE = @LIBL_MVT_BUDGETAIRE,
          @ID_ACTIVITE   = @ID_ACTIVITE ,
          @ID_ADHERENT   = @ID_ADHERENT,
          @ID_GROUPE    = @ID_GROUPE ,
          @ID_ETABLISSEMENT  = @ID_ETABLISSEMENT,
          @ID_TYPE_FINANCEMENT = NULL,
          @ID_DISPOSITIF   = NULL,
          @ID_MODULE_PEC   = NULL
          END
         
         
          -- Abondement du compte Adherent     
          SELECT @MNT_ABONDEMENT_COMPTE = CAST(@MNT_HT - @MNT_DONT_REGLE_FPSPP - @MNT_POT_MUTUALISATION AS DECIMAL(15, 2))
          --IF ABS(@MNT_ABONDEMENT_COMPTE) > 0
          BEGIN
          SELECT @LIBL_MVT_BUDGETAIRE = ''
          IF @COD_TYPE_EVENEMENT = 'VERSEMEN' SET @LIBL_MVT_BUDGETAIRE = 'Versement ' 
          IF @COD_TYPE_EVENEMENT = 'REGUL' SET @LIBL_MVT_BUDGETAIRE = 'Regul. ' 
          IF @COD_TYPE_EVENEMENT = 'REVERSE' SET @LIBL_MVT_BUDGETAIRE = 'Revers. '
         
          SELECT @LIBL_MVT_BUDGETAIRE  = LEFT(@LIBL_MVT_BUDGETAIRE + 'obligatoire ' + CAST(@NUM_ANNEE AS VARCHAR) + @REF_VERSEMENT, 60)
         
          EXEC @ID_COMPTE = LEC_COMPTE
           @ID_GROUPE    = @ID_GROUPE , 
           @ID_PERIODE_FISC   = @ID_PERIODE , 
           @ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT, 
           @ID_ACTIVITE   = @ID_ACTIVITE,  
           @COM_COMPTE    = 'Cr‚ation par Transfert'  
         
          IF @ID_COMPTE IS NOT NULL
          BEGIN
          SET @ID_TYPE_MOUVEMENT = 10 -- Mouvement d'alimentation du Compte Adh‚rent
          SET @P_E_R = 'R'
         
         
          EXEC dbo.INS_MVT_BUDGETAIRE
          @ID_TYPE_MOUVEMENT  = @ID_TYPE_MOUVEMENT,  
          @ID_EVENEMENT   = @ID_EVENEMENT,
          @ID_COMPTE    = @ID_COMPTE,
          @ID_ENVELOPPE   = NULL,
          @MNT_MVT_BUDGETAIRE  = @MNT_ABONDEMENT_COMPTE,
          @P_E_R     = 'R',
          @DAT_MVT_BUDGETAIRE  = @DAT_EVENEMENT,
          @N_R     = 'N',
          @ID_PERIODE_FISC  = @ID_PERIODE,
          @ID_PERIODE_CPT   = @ID_PERIODE,
          @LIBL_MVT_BUDGETAIRE = @LIBL_MVT_BUDGETAIRE,
          @ID_ACTIVITE   = @ID_ACTIVITE ,
          @ID_ADHERENT   = @ID_ADHERENT,
          @ID_GROUPE    = @ID_GROUPE ,
          @ID_ETABLISSEMENT  = @ID_ETABLISSEMENT,
          @ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT,
          @ID_DISPOSITIF   = NULL,
          @ID_MODULE_PEC   = NULL
          END
          ELSE
          BEGIN
          SELECT 'Compte non trouvee POUR :', 
          ID_GROUPE    = @ID_GROUPE , 
          ID_PERIODE_FISC   = @ID_PERIODE , 
          ID_TYPE_FINANCEMENT  = @ID_TYPE_FINANCEMENT, 
          ID_ACTIVITE    = @ID_ACTIVITE
          END
          END
         
          END
          ELSE
          BEGIN
          SELECT 'Enveloppe Pot de MUT non trouvee POUR :', ID_ACTIVITE  = @ID_ACTIVITE , MNT_HT  = @MNT_HT  , MNT_DONT_REGLE_FPSPP = @MNT_DONT_REGLE_FPSPP, ID_PERIODE  =@ID_PERIODE , ID_BRANCHE  = @ID_BRANCHE
         
          INSERT INTO PB_EVENEMENT_MVT_BUDGETAIRE_COLLECTE
          (
          ID_EVENEMENT   ,
          LIB_PB_EVENEMENT  , 
          LIBL_EVENEMENT   , 
          DAT_EVENEMENT   , 
          ID_TYPE_EVENEMENT  , 
          ID_VERSEMENT   ,
          COM_EVENEMENT   
          )
          SELECT
          ID_EVENEMENT   ,
          LIB_PB_EVENEMENT  = 'Enveloppe Pot de MUT non trouv‚e', 
          LIBL_EVENEMENT   , 
          DAT_EVENEMENT   , 
          ID_TYPE_EVENEMENT  , 
          ID_VERSEMENT   ,
          COM_EVENEMENT   = 'Batch Regeneration MVT BUDGETAIRE (PS MVT_BUDGETAIRE_COLLECTE_ALIM_ENVELOPPE_COLLECTE) : ENveloppe Pot de MUT non trouvee POUR : ' 
          + ' ID_ACTIVITE  = ' + CAST(@ID_ACTIVITE AS VARCHAR) 
          + ', MNT_HT  = ' + CAST(@MNT_HT AS VARCHAR) 
          + ', ID_PERIODE  = ' + CAST(@ID_PERIODE AS VARCHAR) 
          + ', ID_BRANCHE    = ' + CAST(@ID_BRANCHE AS VARCHAR) 
          FROM EVENEMENT
          WHERE ID_EVENEMENT = @ID_EVENEMENT 
         
          END      
         
          END
          END
          ELSE IF @GRP_TYPE_VERSEMENT = 'V' -- Versement de Type Volontaire
          BEGIN
          -- Recherche du taux des frais de gestion du groupe
          SET @PRC_FRAIS_GESTION = NULL
          SELECT @PRC_FRAIS_GESTION = PRC_FRAIS_GESTION
          FROM R20bis
          WHERE ID_GROUPE = @ID_GROUPE
          AND  ID_PERIODE = @ID_PERIODE
         
          IF @PRC_FRAIS_GESTION IS NOT NULL
          BEGIN
          --SELECT 'Frais Gestion du groupe :', ID_GROUPE  = @ID_GROUPE , ID_PERIODE  =@ID_PERIODE , PRC_FRAIS_GESTION = @PRC_FRAIS_GESTION 
         
          SET @ID_ENVELOPPE_FRAIS_GESTION = NULL
         
          -- Recherche enveloppe de Pot de Mut associee a l'activite/periode/branche
          SELECT @ID_ENVELOPPE_FRAIS_GESTION = ENVELOPPE.ID_ENVELOPPE
          FROM ENVELOPPE
          INNER JOIN TYPE_ENVELOPPE ON ENVELOPPE.ID_TYPE_ENVELOPPE  = TYPE_ENVELOPPE .ID_TYPE_ENVELOPPE 
          WHERE TYPE_ENVELOPPE .BLN_FRAIS_GESTION_COLLECTE= 1
          AND  TYPE_ENVELOPPE .ID_BRANCHE  = @ID_BRANCHE
          AND  ENVELOPPE.ID_PERIODE  = @ID_PERIODE
         
          END
         
         
          IF  @ID_ENVELOPPE_FRAIS_GESTION IS NOT NULL 
          BEGIN
         
          SELECT @MNT_FRAIS_GESTION = CAST((@MNT_HT) * (@PRC_FRAIS_GESTION/100) AS DECIMAL(15, 2))    
         
          IF @COD_TYPE_EVENEMENT = 'VERSEMEN' SET @LIBL_MVT_BUDGETAIRE = 'Versement ' 
          IF @COD_TYPE_EVENEMENT = 'REGUL' SET @LIBL_MVT_BUDGETAIRE = 'Regul Versement ' 
          IF @COD_TYPE_EVENEMENT = 'REVERSE' SET @LIBL_MVT_BUDGETAIRE = 'Reversement '
          SELECT @LIBL_MVT_BUDGETAIRE  = 'Frais Gestion '+ @LIBL_MVT_BUDGETAIRE + 'volontaire ' + CAST(@NUM_ANNEE AS VARCHAR)
         
          IF ABS (@MNT_FRAIS_GESTION ) > 0
          BEGIN
         
          SET @ID_TYPE_MOUVEMENT = 9 -- Mouvement d'alimentation de l'enveloppe de frais de gestion
          SET @P_E_R = 'R'
         
          EXEC dbo.INS_MVT_BUDGETAIRE
          @ID_TYPE_MOUVEMENT  = @ID_TYPE_MOUVEMENT,  
          @ID_EVENEMENT   = @ID_EVENEMENT,
          @ID_COMPTE    = NULL,
          @ID_ENVELOPPE   = @ID_ENVELOPPE_FRAIS_GESTION,
          @MNT_MVT_BUDGETAIRE  = @MNT_FRAIS_GESTION,
          @P_E_R     = 'R',
          @DAT_MVT_BUDGETAIRE  = @DAT_EVENEMENT,
          @N_R     = 'N',
          @ID_PERIODE_FISC  = @ID_PERIODE,
          @ID_PERIODE_CPT   = @ID_PERIODE,
          @LIBL_MVT_BUDGETAIRE = @LIBL_MVT_BUDGETAIRE,
          @ID_ACTIVITE   = @ID_ACTIVITE ,
          @ID_ADHERENT   = @ID_ADHERENT,
          @ID_GROUPE    = @ID_GROUPE ,
          @ID_ETABLISSEMENT  = @ID_ETABLISSEMENT,
          @ID_TYPE_FINANCEMENT = NULL,
          @ID_DISPOSITIF   = NULL,
          @ID_MODULE_PEC   = NULL
          END
         
         
          -- Abondement du compte Adherent     
          SELECT @MNT_ABONDEMENT_COMPTE = CAST(@MNT_HT - @MNT_FRAIS_GESTION AS DECIMAL(15, 2))
          IF ABS(@MNT_ABONDEMENT_COMPTE) > 0
          BEGIN
          --SELECT 'Abondement Compte = ', @MNT_ABONDEMENT_COMPTE , ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT 
         
          SELECT @ID_TYPE_FINANCEMENT = ID_TYPE_FINANCEMENT
          FROM PARAM_SOURCE_FINANCEMENT_INDIVIDUALISEE
          WHERE ID_ACTIVITE = @ID_ACTIVITE
          AND  ID_PERIODE = @ID_PERIODE
         
          IF @COD_TYPE_EVENEMENT = 'VERSEMEN' SET @LIBL_MVT_BUDGETAIRE = 'Versement ' 
          IF @COD_TYPE_EVENEMENT = 'REGUL' SET @LIBL_MVT_BUDGETAIRE = 'Regul. ' 
          IF @COD_TYPE_EVENEMENT = 'REVERSE' SET @LIBL_MVT_BUDGETAIRE = 'Revers. '
         
          SELECT @LIBL_MVT_BUDGETAIRE  = LEFT(@LIBL_MVT_BUDGETAIRE + 'volontaire ' + CAST(@NUM_ANNEE AS VARCHAR) + @REF_VERSEMENT, 60)
         
          EXEC @ID_COMPTE = LEC_COMPTE @ID_GROUPE    = @ID_GROUPE , 
          @ID_PERIODE_FISC   = @ID_PERIODE , 
          @ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT, 
          @ID_ACTIVITE   = @ID_ACTIVITE,  
          @COM_COMPTE    = 'Cr‚ation par Transfert'  
         
          IF @ID_COMPTE IS NOT NULL
          BEGIN
          SET @ID_TYPE_MOUVEMENT = 10 -- Mouvement d'alimentation du Compte Adh‚rent
          SET @P_E_R = 'R'
         
         
          EXEC dbo.INS_MVT_BUDGETAIRE
          @ID_TYPE_MOUVEMENT  = @ID_TYPE_MOUVEMENT,  
          @ID_EVENEMENT   = @ID_EVENEMENT,
          @ID_COMPTE    = @ID_COMPTE,
          @ID_ENVELOPPE   = NULL,
          @MNT_MVT_BUDGETAIRE  = @MNT_ABONDEMENT_COMPTE,
          @P_E_R     = 'R',
          @DAT_MVT_BUDGETAIRE  = @DAT_EVENEMENT,
          @N_R     = 'N',
          @ID_PERIODE_FISC  = @ID_PERIODE,
          @ID_PERIODE_CPT   = @ID_PERIODE,
          @LIBL_MVT_BUDGETAIRE = @LIBL_MVT_BUDGETAIRE,
          @ID_ACTIVITE   = @ID_ACTIVITE ,
          @ID_ADHERENT   = @ID_ADHERENT,
          @ID_GROUPE    = @ID_GROUPE ,
          @ID_ETABLISSEMENT  = @ID_ETABLISSEMENT,
          @ID_TYPE_FINANCEMENT = @ID_TYPE_FINANCEMENT,
          @ID_DISPOSITIF   = NULL,
          @ID_MODULE_PEC   = NULL
          END
          ELSE
          BEGIN
          SELECT 'Compte non trouvee POUR :', 
          ID_GROUPE    = @ID_GROUPE , 
          ID_PERIODE_FISC   = @ID_PERIODE , 
          ID_TYPE_FINANCEMENT  = @ID_TYPE_FINANCEMENT, 
          ID_ACTIVITE    = @ID_ACTIVITE
          END
          END
         
          END
          ELSE
          BEGIN
          SELECT 'Frais Gestion du groupe non param‚tr‚s POUR :', ID_GROUPE  = @ID_GROUPE , ID_PERIODE  =@ID_PERIODE 
         
          INSERT INTO PB_EVENEMENT_MVT_BUDGETAIRE_COLLECTE
          (
          ID_EVENEMENT   ,
          LIB_PB_EVENEMENT  , 
          LIBL_EVENEMENT   , 
          DAT_EVENEMENT   , 
          ID_TYPE_EVENEMENT  , 
          ID_VERSEMENT   ,
          COM_EVENEMENT   
          )
          SELECT
          ID_EVENEMENT   ,
          LIB_PB_EVENEMENT  = 'Frais Gestion du groupe non param‚tr‚s', 
          LIBL_EVENEMENT   , 
          DAT_EVENEMENT   , 
          ID_TYPE_EVENEMENT  , 
          ID_VERSEMENT   ,
          COM_EVENEMENT   = 'Batch Regeneration MVT BUDGETAIRE (PS MVT_BUDGETAIRE_COLLECTE_ABONDEMENT_COMPTE) : Frais Gestion du groupe non param‚tr‚s POUR ' 
          + ' ID_GROUPE = ' + CAST(@ID_GROUPE AS VARCHAR) 
          + ', ID_PERIODE  = ' + CAST(@ID_PERIODE AS VARCHAR) 
          FROM EVENEMENT
          WHERE ID_EVENEMENT = @ID_EVENEMENT 
          END
         
         
          END
         
          --SELECT 'ENveloppe Pot de MUT CHERCHEE POUR :', ID_ACTIVITE  = @ID_ACTIVITE , MNT_HT  = @MNT_HT  , MNT_DONT_REGLE_FPSPP = @MNT_DONT_REGLE_FPSPP, ID_PERIODE  =@ID_PERIODE , ID_BRANCHE  = @ID_BRANCHE
         
          FETCH cu_poste_imputation INTO @ID_ETABLISSEMENT, 
          @ID_ADHERENT, 
          @ID_GROUPE, 
          @ID_BRANCHE, 
          @ID_ACTIVITE, 
          @MNT_HT, 
          @MNT_DONT_REGLE_FPSPP,
          @ID_PERIODE, 
          @NUM_ANNEE,
          @DAT_CONSTITUTION_POT_MUT,
          @GRP_TYPE_VERSEMENT,
          @REF_VERSEMENT
          END
         
          CLOSE cu_poste_imputation 
          DEALLOCATE cu_poste_imputation 
         
         END
         
         
         
         -- =============================================
         -- Author:		KW
         -- Create date: 29 novembre 2007
         -- Description:	Recupere le nombre de reglements actifs
         -- pour une session pec un module pec ou un action pec
         -- =============================================
         create PROCEDURE [dbo].[LEC_NB_REGLEMENT_ACTIF]
         	@ID		INT,
         	@TYPE	INT --TYPE 1 = action pec, TYPE 2 = module pec, TYPE 3 = session pec
         AS
         BEGIN
         
         	DECLARE @NB AS INT
         	SET @NB = 0
         
         	IF @TYPE = 1 
         		BEGIN
         			SELECT  @NB =  COUNT(DISTINCT POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE) 
         			FROM	ACTION_PEC
         					INNER JOIN MODULE_PEC ON MODULE_PEC.ID_ACTION_PEC = ACTION_PEC.ID_ACTION_PEC
         					INNER JOIN POSTE_COUT_REGLE ON POSTE_COUT_REGLE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         			WHERE		POSTE_COUT_REGLE.BLN_ACTIF = 1
         					AND MODULE_PEC.BLN_ACTIF = 1
         					AND ACTION_PEC.ID_ACTION_PEC= @ID
         					AND POSTE_COUT_REGLE.DAT_BAP IS NOT NULL
         		END
         
         
         	IF @TYPE = 2 
         		BEGIN
         			SELECT  @NB =  COUNT(DISTINCT POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE) 
         			FROM	MODULE_PEC 
         					INNER JOIN POSTE_COUT_REGLE ON POSTE_COUT_REGLE.ID_MODULE_PEC = MODULE_PEC.ID_MODULE_PEC
         			WHERE		POSTE_COUT_REGLE.BLN_ACTIF = 1
         					AND MODULE_PEC.ID_MODULE_PEC = @ID
         					AND POSTE_COUT_REGLE.DAT_BAP IS NOT NULL
         		END
         
         
         	IF @TYPE = 3 
         		BEGIN
         			SELECT  @NB =  COUNT(DISTINCT POSTE_COUT_REGLE.ID_POSTE_COUT_REGLE) 
         			FROM	SESSION_PEC 
         					INNER JOIN POSTE_COUT_REGLE ON POSTE_COUT_REGLE.ID_SESSION_PEC = SESSION_PEC.ID_SESSION_PEC
         			WHERE		POSTE_COUT_REGLE.BLN_ACTIF = 1
         					AND SESSION_PEC.ID_SESSION_PEC = @ID
         					AND POSTE_COUT_REGLE.DAT_BAP IS NOT NULL
         		END
         
         
         
         	SELECT 'Return Value' = COALESCE(@NB,0)
         END
         
         