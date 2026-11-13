(function () {
    var mensajes = [
        ["Idioma", "Language", "Idioma", "Langue"],
        ["Tema", "Theme", "Tema", "Thème"],
        ["Tema claro", "Light theme", "Tema claro", "Thème clair"],
        ["Oscuro", "Dark", "Escuro", "Sombre"],
        ["Claro", "Light", "Claro", "Clair"],
        ["Bloqueado", "Locked", "Bloqueado", "Verrouillé"],
        ["Solo lectura", "Read-only", "Somente leitura", "Lecture seule"],
        ["Almacenamiento local que recuerda la preferencia de tema claro u oscuro en este navegador.", "Local storage that remembers the light or dark theme preference in this browser.", "Armazenamento local que lembra a preferência pelo tema claro ou escuro neste navegador.", "Stockage local qui mémorise la préférence de thème clair ou sombre dans ce navigateur."],
        ["Información legal", "Legal information", "Informações legais", "Informations légales"],
        ["versión", "version", "versão", "version"],
        ["Gestión Hotelera", "Hotel Management", "Gestão Hoteleira", "Gestion Hôtelière"],
        ["Buscar módulo", "Search module", "Buscar módulo", "Rechercher un module"],
        ["Gestión", "Management", "Gestão", "Gestion"],
        ["Solicitud", "Request", "Solicitação", "Demande"],
        ["Hay filas de la importación pendientes de revisión.", "Some import rows need review.", "Há linhas da importação pendentes de revisão.", "Certaines lignes importées doivent être vérifiées."],
        ["Asigne una habitación a todos los pasajeros antes de generar reservas.", "Assign a room to every passenger before generating reservations.", "Atribua um quarto a todos os passageiros antes de gerar reservas.", "Attribuez une chambre à tous les passagers avant de générer les réservations."],
        ["Asignar automático", "Assign automatically", "Atribuir automaticamente", "Attribuer automatiquement"],
        ["camas disponibles", "beds available", "camas disponíveis", "lits disponibles"],
        ["de", "of", "de", "sur"],
        ["Siguiente pasajero:", "Next passenger:", "Próximo passageiro:", "Passager suivant :"],
        ["Indique la cantidad de camas por habitación. Las camas se asignan a los pasajeros en el orden seleccionado.", "Enter the number of beds per room. Beds are assigned to passengers in the selected order.", "Informe a quantidade de camas por quarto. As camas são atribuídas aos passageiros na ordem selecionada.", "Indiquez le nombre de lits par chambre. Les lits sont attribués aux passagers dans l'ordre sélectionné."],
        ["La cantidad no es válida o supera las camas disponibles. Revise las selecciones anteriores.", "The quantity is invalid or exceeds the available beds. Review the previous selections.", "A quantidade é inválida ou excede as camas disponíveis. Revise as seleções anteriores.", "La quantité est invalide ou dépasse le nombre de lits disponibles. Vérifiez les sélections précédentes."],
        ["Seleccionar", "Select", "Selecionar", "Sélectionner"],
        ["Quitar última selección", "Undo last selection", "Desfazer última seleção", "Annuler la dernière sélection"],
        ["Seleccione una cama para cada pasajero, en el orden indicado. Puede repetir una habitación si tiene camas disponibles.", "Select a bed for each passenger in the indicated order. You can select a room more than once if beds are available.", "Selecione uma cama para cada passageiro na ordem indicada. Você pode repetir um quarto se houver camas disponíveis.", "Sélectionnez un lit pour chaque passager dans l'ordre indiqué. Vous pouvez choisir une chambre plusieurs fois si des lits sont disponibles."],
        ["Asignación Servicio", "Service assignment", "Atribuição de serviço", "Affectation de service"],
        ["Gestión OT", "Work order management", "Gestão de ordens de trabalho", "Gestion des ordres de travail"],
        ["Registro Servicios", "Service log", "Registro de serviços", "Journal des services"],
        ["Solicitud Empresa", "Company request", "Solicitação da empresa", "Demande d'entreprise"],
        ["Control de valijas", "Dispatch bag control", "Controle de malotes", "Gestion des valises"],
        ["Comentarios", "Comments", "Comentários", "Commentaires"],
        ["Asignación EPP", "PPE assignment", "Atribuição de EPI", "Affectation des EPI"],
        ["Asignación Casilleros", "Locker assignment", "Atribuição de armários", "Affectation des casiers"],
        ["Registro Desalojos", "Eviction log", "Registro de desocupações", "Journal des expulsions"],
        ["No Conformidades", "Non-conformities", "Não conformidades", "Non-conformités"],
        ["Planificación Proyectos", "Project planning", "Planejamento de projetos", "Planification des projets"],
        ["Reserva Ubicaciones", "Location reservation", "Reserva de locais", "Réservation d'emplacements"],
        ["No disponible", "Not available", "Indisponível", "Indisponible"],
        ["Alojamiento", "Accommodation", "Alojamento", "Hébergement"],
        ["Solicitud alojamiento", "Accommodation request", "Solicitação de alojamento", "Demande d'hébergement"],
        ["Pasajeros de solicitud", "Request passengers", "Passageiros da solicitação", "Passagers de la demande"],
        ["Reserva", "Reservation", "Reserva", "Réservation"],
        ["Nueva reserva", "New reservation", "Nova reserva", "Nouvelle réservation"],
        ["Reserva por solicitud", "Reservation by request", "Reserva por solicitação", "Réservation par demande"],
        ["Reserva de instalaciones", "Facility reservation", "Reserva de instalações", "Réservation d'installations"],
        ["Asignación", "Assignment", "Atribuição", "Attribution"],
        ["Liberación", "Release", "Liberação", "Libération"],
        ["Check In", "Check-in", "Check-in", "Arrivée"],
        ["Check Out", "Check-out", "Check-out", "Départ"],
        ["Censo", "Occupancy census", "Censo", "Recensement"],
        ["Censo QR", "QR census", "Censo QR", "Recensement QR"],
        ["Mis Censos", "My censuses", "Meus censos", "Mes recensements"],
        ["Desalojos", "Evictions", "Desocupações", "Expulsions"],
        ["Informes", "Reports", "Relatórios", "Rapports"],
        ["Desviaciones", "Deviations", "Desvios", "Écarts"],
        ["Habitaciones", "Rooms", "Quartos", "Chambres"],
        ["Ocupabilidad", "Occupancy", "Ocupação", "Occupation"],
        ["Pabellones", "Buildings", "Pavilhões", "Pavillons"],
        ["Disponibilidad", "Availability", "Disponibilidade", "Disponibilité"],
        ["Inventario", "Inventory", "Inventário", "Inventaire"],
        ["Artículos", "Items", "Artigos", "Articles"],
        ["Grupos de artículos", "Item groups", "Grupos de artigos", "Groupes d'articles"],
        ["Estados de artículo", "Item statuses", "Estados do artigo", "États des articles"],
        ["Precios", "Prices", "Preços", "Prix"],
        ["Registro de artículos", "Item log", "Registro de artigos", "Journal des articles"],
        ["Registro por habitación", "Room log", "Registro por quarto", "Journal par chambre"],
        ["Inventarios de habitación", "Room inventories", "Inventários de quartos", "Inventaires des chambres"],
        ["Ítems de inventario", "Inventory items", "Itens de inventário", "Articles d'inventaire"],
        ["Activos", "Assets", "Ativos", "Actifs"],
        ["Aseo", "Housekeeping", "Limpeza", "Entretien"],
        ["Grupos y actividades de aseo", "Housekeeping groups and activities", "Grupos e atividades de limpeza", "Groupes et activités d'entretien"],
        ["Aseo y checklists", "Housekeeping and checklists", "Limpeza e listas de verificação", "Entretien et listes de contrôle"],
        ["Supervisión de aseo", "Housekeeping supervision", "Supervisão da limpeza", "Supervision de l'entretien"],
        ["Estándares de habitación", "Room standards", "Padrões de quarto", "Normes des chambres"],
        ["EPP", "PPE", "EPI", "EPI"],
        ["Casilleros", "Lockers", "Armários", "Casiers"],
        ["Áreas de casilleros", "Locker areas", "Áreas de armários", "Zones de casiers"],
        ["Secciones de casilleros", "Locker sections", "Seções de armários", "Sections de casiers"],
        ["Casilleros EPP", "PPE lockers", "Armários de EPI", "Casiers EPI"],
        ["Inventario EPP", "PPE inventory", "Inventário de EPI", "Inventaire EPI"],
        ["Operación", "Operations", "Operação", "Opérations"],
        ["Servicio", "Service", "Serviço", "Service"],
        ["Mantenciones", "Maintenance", "Manutenções", "Maintenance"],
        ["Órdenes de servicio", "Service orders", "Ordens de serviço", "Ordres de service"],
        ["Órdenes de trabajo", "Work orders", "Ordens de trabalho", "Ordres de travail"],
        ["No conformidad", "Non-conformity", "Não conformidade", "Non-conformité"],
        ["Propuestas de no conformidad", "Non-conformity proposals", "Propostas de não conformidade", "Propositions de non-conformité"],
        ["Seguimiento de no conformidad", "Non-conformity tracking", "Acompanhamento de não conformidade", "Suivi de non-conformité"],
        ["ESS comentarios", "ESS comments", "Comentários ESS", "Commentaires ESS"],
        ["Valijas", "Dispatch bags", "Malotes", "Valises"],
        ["Entregas de valija", "Dispatch deliveries", "Entregas de malote", "Livraisons de valises"],
        ["Campamento", "Camp", "Acampamento", "Campement"],
        ["Empresas", "Companies", "Empresas", "Entreprises"],
        ["Departamentos, divisiones y UN", "Departments, divisions and BU", "Departamentos, divisões e unidades de negócio", "Départements, divisions et unités commerciales"],
        ["Listas", "Lists", "Listas", "Listes"],
        ["Proyectos, grupos y actividades", "Projects, groups and activities", "Projetos, grupos e atividades", "Projets, groupes et activités"],
        ["Tipos de módulo", "Module types", "Tipos de módulo", "Types de module"],
        ["Módulos", "Modules", "Módulos", "Modules"],
        ["Ubicación y acceso", "Location and access", "Localização e acesso", "Emplacement et accès"],
        ["Tipos de instalación", "Facility types", "Tipos de instalação", "Types d'installation"],
        ["Plantillas de ubicación", "Location templates", "Modelos de localização", "Modèles d'emplacement"],
        ["Trabajadores", "Workers", "Trabalhadores", "Travailleurs"],
        ["Códigos QR", "QR codes", "Códigos QR", "Codes QR"],
        ["Usuarios", "Users", "Usuários", "Utilisateurs"],
        ["Perfiles", "Profiles", "Perfis", "Profils"],
        ["Turnos", "Shifts", "Turnos", "Équipes"],
        ["Configuración", "Settings", "Configurações", "Paramètres"],
        ["Salir", "Sign out", "Sair", "Déconnexion"],
        ["Cuenta", "Account", "Conta", "Compte"],
        ["Filtros", "Filters", "Filtros", "Filtres"],
        ["Agregar", "Add", "Adicionar", "Ajouter"],
        ["Importar Excel", "Import Excel", "Importar Excel", "Importer Excel"],
        ["Importación masiva de reservas", "Bulk reservation import", "Importação em massa de reservas", "Importation groupée de réservations"],
        ["Complete la plantilla Excel. Las filas con errores se informarán sin impedir la importación de las demás.", "Complete the Excel template. Rows with errors will be reported without preventing the other rows from being imported.", "Preencha o modelo Excel. As linhas com erros serão informadas sem impedir a importação das demais.", "Remplissez le modèle Excel. Les lignes en erreur seront signalées sans empêcher l'importation des autres lignes."],
        ["Use los nombres exactos de Grupo Empresa y Turno del catálogo. Ingrese las fechas como AAAA-MM-DD HH:mm.", "Use the exact Company Group and Shift names from the catalog. Enter dates as YYYY-MM-DD HH:mm.", "Use os nomes exatos de Grupo Empresa e Turno do catálogo. Informe as datas como AAAA-MM-DD HH:mm.", "Utilisez les noms exacts du groupe d'entreprise et de l'équipe du catalogue. Saisissez les dates au format AAAA-MM-JJ HH:mm."],
        ["Descargar plantilla", "Download template", "Baixar modelo", "Télécharger le modèle"],
        ["Planilla Excel (.xlsx)", "Excel file (.xlsx)", "Planilha Excel (.xlsx)", "Fichier Excel (.xlsx)"],
        ["Importar reservas", "Import reservations", "Importar reservas", "Importer les réservations"],
        ["Reservas creadas:", "Reservations created:", "Reservas criadas:", "Réservations créées :"],
        ["Filas rechazadas:", "Rejected rows:", "Linhas rejeitadas:", "Lignes rejetées :"],
        ["Fila", "Row", "Linha", "Ligne"],
        ["Cancelar", "Cancel", "Cancelar", "Annuler"],
        ["No se pueden modificar los pasajeros de una solicitud reservada o anulada.", "Passengers cannot be modified on a reserved or cancelled request.", "Não é possível alterar os passageiros de uma solicitação reservada ou cancelada.", "Les passagers d'une demande réservée ou annulée ne peuvent pas être modifiés."],
        ["Resultado de la importación", "Import result", "Resultado da importação", "Résultat de l'importation"],
        ["No hay habitaciones disponibles para los filtros seleccionados.", "There are no rooms available for the selected filters.", "Não há quartos disponíveis para os filtros selecionados.", "Aucune chambre n'est disponible pour les filtres sélectionnés."],
        ["Buscar habitaciones", "Search rooms", "Buscar quartos", "Rechercher des chambres"],
        ["Al completar la selección, elegir otra habitación reemplaza la última seleccionada.", "Once the selection is complete, choosing another room replaces the last one selected.", "Quando a seleção estiver completa, escolher outro quarto substituirá o último selecionado.", "Une fois la sélection complète, choisir une autre chambre remplace la dernière sélectionnée."],
        ["Total de filas", "Total rows", "Total de linhas", "Nombre total de lignes"],
        ["Procesadas", "Processed", "Processadas", "Traitées"],
        ["No procesadas", "Not processed", "Não processadas", "Non traitées"],
        ["Detalle de filas no procesadas", "Details of unprocessed rows", "Detalhes das linhas não processadas", "Détails des lignes non traitées"],
        ["No hay filas pendientes de revisión.", "There are no rows to review.", "Não há linhas pendentes de revisão.", "Aucune ligne ne nécessite de vérification."],
        ["Aún no hay pasajeros ni cupos. Agregue uno o importe una planilla.", "There are no passengers or slots yet. Add one or import a spreadsheet.", "Ainda não há passageiros nem vagas. Adicione um ou importe uma planilha.", "Il n'y a pas encore de passagers ni de places. Ajoutez-en un ou importez une feuille de calcul."],
        ["No hay pasajeros ni cupos para esta solicitud.", "There are no passengers or slots for this request.", "Não há passageiros nem vagas para esta solicitação.", "Il n'y a pas de passagers ni de places pour cette demande."],
        ["Guardar", "Save", "Guardar", "Enregistrer"],
        ["Continuar", "Continue", "Continuar", "Continuer"],
        ["Aplicar", "Apply", "Aplicar", "Appliquer"],
        ["Seleccione", "Select", "Selecione", "Sélectionner"],
        ["Todos", "All", "Todos", "Tous"],
        ["Ahora", "Now", "Agora", "Maintenant"],
        ["Hoy", "Today", "Hoje", "Aujourd'hui"],
        ["Sin resultados", "No results", "Sem resultados", "Aucun résultat"],
        ["Buscando...", "Searching...", "Pesquisando...", "Recherche..."],
        ["Buscar:", "Search:", "Pesquisar:", "Rechercher :"],
        ["Mostrar _MENU_", "Show _MENU_", "Mostrar _MENU_", "Afficher _MENU_"],
        ["_START_ a _END_ de _TOTAL_", "_START_ to _END_ of _TOTAL_", "_START_ a _END_ de _TOTAL_", "_START_ à _END_ sur _TOTAL_"],
        ["0 registros", "0 records", "0 registros", "0 enregistrement"],
        ["No hay registros para mostrar", "No records to display", "Não há registros para exibir", "Aucun enregistrement à afficher"],
        ["Anterior", "Previous", "Anterior", "Précédent"],
        ["Siguiente", "Next", "Próximo", "Suivant"],
        ["Datos", "Details", "Dados", "Données"],
        ["Estadía", "Stay", "Estadia", "Séjour"],
        ["Asignación", "Assignment", "Atribuição", "Attribution"],
        ["Estado", "Status", "Estado", "État"],
        ["Criterios", "Criteria", "Critérios", "Critères"],
        ["Número", "Number", "Número", "Numéro"],
        ["Fecha Reserva", "Reservation date", "Data da reserva", "Date de réservation"],
        ["Fecha Llegada", "Arrival date", "Data de chegada", "Date d'arrivée"],
        ["Fecha Salida", "Departure date", "Data de saída", "Date de départ"],
        ["Fecha de censo", "Census date", "Data do censo", "Date du recensement"],
        ["Fecha Censo", "Census date", "Data do censo", "Date du recensement"],
        ["Fecha", "Date", "Data", "Date"],
        ["Grupo Empresa", "Company Group", "Grupo Empresa", "Groupe d'entreprise"],
        ["Origen reserva", "Reservation source", "Origem da reserva", "Origine de la réservation"],
        ["Pasajero", "Guest", "Hóspede", "Voyageur"],
        ["Habitación", "Room", "Quarto", "Chambre"],
        ["Turno", "Shift", "Turno", "Équipe"],
        ["Tipo Turno", "Shift type", "Tipo de turno", "Type d'équipe"],
        ["Llegada", "Arrival", "Chegada", "Arrivée"],
        ["Salida", "Departure", "Saída", "Départ"],
        ["Observaciones", "Notes", "Observações", "Remarques"],
        ["Observación", "Note", "Observação", "Remarque"],
        ["Camas ocupadas", "Occupied beds", "Camas ocupadas", "Lits occupés"],
        ["Camas ocupadas noche", "Night occupied beds", "Camas ocupadas à noite", "Lits occupés la nuit"],
        ["Módulo", "Module", "Módulo", "Module"],
        ["Piso", "Floor", "Andar", "Étage"],
        ["Campamento", "Camp", "Acampamento", "Campement"],
        ["N°", "No.", "Nº", "N°"],
        ["Nº", "No.", "Nº", "N°"],
        ["Acciones", "Actions", "Ações", "Actions"],
        ["Activo", "Active", "Ativo", "Actif"],
        ["Pendiente", "Pending", "Pendente", "En attente"],
        ["Anulada", "Cancelled", "Cancelada", "Annulée"],
        ["Finalizada", "Completed", "Concluída", "Terminée"],
        ["Asignada", "Assigned", "Atribuída", "Attribuée"],
        ["Pre-reserva", "Pre-booking", "Pré-reserva", "Pré-réservation"],
        ["Camas ocupadas (CheckIn)", "Occupied beds (check-in)", "Camas ocupadas (check-in)", "Lits occupés (arrivée)"],
        ["Estado Censo", "Census status", "Estado do censo", "État du recensement"],
        ["Estado Habitación", "Room status", "Estado do quarto", "État de la chambre"],
        ["N° Camas", "No. of beds", "Nº de camas", "N° de lits"],
        ["Ingresar", "Sign in", "Entrar", "Connexion"],
        ["Rut/Dni", "ID number", "CPF/RG", "N° d'identité"],
        ["Contraseña", "Password", "Senha", "Mot de passe"],
        ["Contraseña actual", "Current password", "Senha atual", "Mot de passe actuel"],
        ["Contraseña nueva", "New password", "Nova senha", "Nouveau mot de passe"],
        ["Repetir contraseña", "Repeat password", "Repetir senha", "Répéter le mot de passe"],
        ["Seleccione Campamento/Perfil", "Select camp/profile", "Selecione acampamento/perfil", "Sélectionnez le camp/profil"],
        ["Cambiar Contraseña", "Change password", "Alterar senha", "Changer le mot de passe"],
        ["Mostrar contraseña", "Show password", "Mostrar senha", "Afficher le mot de passe"],
        ["Ocultar contraseña", "Hide password", "Ocultar senha", "Masquer le mot de passe"],
        ["Política de privacidad", "Privacy policy", "Política de privacidade", "Politique de confidentialité"],
        ["Cookies", "Cookies", "Cookies", "Cookies"],
        ["Preferencias de cookies", "Cookie preferences", "Preferências de cookies", "Préférences des cookies"],
        ["Preferencias", "Preferences", "Preferências", "Préférences"],
        ["Aceptar", "Accept", "Aceitar", "Accepter"],
        ["Rechazar", "Reject", "Rejeitar", "Refuser"],
        ["Configurar", "Configure", "Configurar", "Configurer"],
        ["Necesarias", "Necessary", "Necessárias", "Nécessaires"],
        ["Analíticas", "Analytics", "Analíticas", "Analytiques"],
        ["Publicidad y perfilamiento", "Advertising and profiling", "Publicidade e criação de perfil", "Publicité et profilage"],
        ["Detalle de cookies", "Cookie details", "Detalhes dos cookies", "Détails des cookies"],
        ["Cambiar preferencias", "Change preferences", "Alterar preferências", "Modifier les préférences"],
        ["Volver al ingreso", "Back to sign in", "Voltar ao login", "Retour à la connexion"],
        ["Acceso denegado", "Access denied", "Acesso negado", "Accès refusé"],
        ["Tu perfil no puede abrir esta pantalla.", "Your profile cannot open this page.", "Seu perfil não pode abrir esta tela.", "Votre profil ne peut pas ouvrir cette page."],
        ["Volver al inicio", "Back to home", "Voltar ao início", "Retour à l'accueil"],
        ["Página no encontrada", "Page not found", "Página não encontrada", "Page introuvable"],
        ["Esa dirección no corresponde a una pantalla de Gestión Hotelera.", "This address does not match a Hotel Management page.", "Esse endereço não corresponde a uma tela da Gestão Hoteleira.", "Cette adresse ne correspond pas à une page de Gestion Hôtelière."],
        ["Gestión Hotelera", "Hotel Management", "Gestão Hoteleira", "Gestion Hôtelière"],
        ["Censo QR guardado correctamente.", "QR census saved successfully.", "Censo QR salvo com sucesso.", "Recensement QR enregistré avec succès."],
        ["Reserva creada N° ", "Reservation created No. ", "Reserva criada Nº ", "Réservation créée n° "],
        ["Cargando datos...", "Loading data...", "Carregando dados...", "Chargement des données..."],
        ["No se pudo completar la operación.", "The operation could not be completed.", "Não foi possível concluir a operação.", "L'opération n'a pas pu être terminée."],
        ["No fue posible acceder a la cámara.", "Could not access the camera.", "Não foi possível acessar a câmera.", "Impossible d'accéder à la caméra."],
        ["El código QR no contiene una habitación.", "The QR code does not contain a room.", "O código QR não contém um quarto.", "Le code QR ne contient pas de chambre."],
        ["Usar cámara", "Use camera", "Usar câmera", "Utiliser la caméra"],
        ["Seleccionar Fecha", "Select date", "Selecionar data", "Sélectionner une date"],
        ["Editar", "Edit", "Editar", "Modifier"],
        ["Mostrar", "Show", "Mostrar", "Afficher"],
        ["Solicitud alojamiento", "Accommodation request", "Solicitação de alojamento", "Demande d'hébergement"],
        ["Tipo", "Type", "Tipo", "Type"],
        ["Subtipo", "Subtype", "Subtipo", "Sous-type"],
        ["Nivel", "Level", "Nível", "Niveau"],
        ["Desde", "From", "De", "Du"],
        ["Hasta", "To", "Até", "Au"],
        ["Minera", "Mining company", "Mineradora", "Compagnie minière"],
        ["Consultar", "Search", "Consultar", "Consulter"],
        ["Consultar ocupabilidad", "Check occupancy", "Consultar ocupação", "Consulter l'occupation"],
        ["Gestión diaria", "Daily operations", "Gestão diária", "Gestion quotidienne"],
        ["Descargar informe general (Excel)", "Download general report (Excel)", "Baixar relatório geral (Excel)", "Télécharger le rapport général (Excel)"],
        ["Estado de pago", "Payment status", "Status do pagamento", "État du paiement"],
        ["Consulte el estado de pago consolidado o por servicio.", "View consolidated or per-service payment status.", "Consulte o status de pagamento consolidado ou por serviço.", "Consultez l'état de paiement consolidé ou par service."],
        ["Estado de pago por servicio", "Payment status by service", "Status de pagamento por serviço", "État de paiement par service"],
        ["Servicio", "Service", "Serviço", "Service"],
        ["Informes de Censo", "Census reports", "Relatórios do Censo", "Rapports de recensement"],
        ["Use los servicios de la pantalla para consultar la gestión diaria o descargar el informe general en Excel.", "Use the page tools to view daily operations or download the general Excel report.", "Use as ferramentas da tela para consultar a gestão diária ou baixar o relatório geral em Excel.", "Utilisez les outils de la page pour consulter la gestion quotidienne ou télécharger le rapport général Excel."],
        ["Ocupabilidad de censo", "Census occupancy", "Ocupação do censo", "Occupation du recensement"],
        ["Descargar Excel", "Download Excel", "Baixar Excel", "Télécharger Excel"],
        ["Seleccione Campamento/Perfil", "Select camp/profile", "Selecione acampamento/perfil", "Sélectionnez le camp/profil"],
        ["No se pudo consultar el catálogo.", "Could not load the catalog.", "Não foi possível consultar o catálogo.", "Impossible de consulter le catalogue."],
        ["No se pudo consultar las reservas.", "Could not load reservations.", "Não foi possível consultar as reservas.", "Impossible de consulter les réservations."],
        ["La reserva no llegó completa.", "The reservation data is incomplete.", "Os dados da reserva estão incompletos.", "Les données de réservation sont incomplètes."],
        ["Ingrese la fecha de reserva y la fecha de llegada.", "Enter the reservation and arrival dates.", "Informe as datas da reserva e da chegada.", "Saisissez les dates de réservation et d'arrivée."],
        ["No se pudo crear la reserva.", "Could not create the reservation.", "Não foi possível criar a reserva.", "Impossible de créer la réservation."],
        ["Seleccione una planilla Excel para importar.", "Select an Excel file to import.", "Selecione uma planilha Excel para importar.", "Sélectionnez un fichier Excel à importer."],
        ["El archivo debe tener formato .xlsx.", "The file must be in .xlsx format.", "O arquivo deve estar no formato .xlsx.", "Le fichier doit être au format .xlsx."],
        ["El archivo no es una planilla Excel .xlsx válida.", "The file is not a valid .xlsx workbook.", "O arquivo não é uma planilha .xlsx válida.", "Le fichier n'est pas un classeur .xlsx valide."],
        ["La planilla no contiene hojas.", "The workbook contains no sheets.", "A planilha não contém abas.", "Le classeur ne contient aucune feuille."],
        ["La planilla no contiene reservas.", "The workbook contains no reservations.", "A planilha não contém reservas.", "Le classeur ne contient aucune réservation."],
        ["Grupo Empresa es obligatorio.", "Company Group is required.", "Grupo Empresa é obrigatório.", "Le groupe d'entreprise est obligatoire."],
        ["Turno es obligatorio.", "Shift is required.", "Turno é obrigatório.", "L'équipe est obligatoire."],
        ["Grupo Empresa no existe en el catálogo del campamento.", "Company Group is not in the camp catalog.", "Grupo Empresa não existe no catálogo do acampamento.", "Le groupe d'entreprise n'existe pas dans le catalogue du camp."],
        ["Turno no existe en el catálogo de la empresa.", "Shift is not in the company catalog.", "Turno não existe no catálogo da empresa.", "L'équipe n'existe pas dans le catalogue de l'entreprise."],
        ["Fecha Reserva es obligatoria o no tiene un formato válido.", "Reservation date is required or has an invalid format.", "A data da reserva é obrigatória ou tem formato inválido.", "La date de réservation est obligatoire ou son format est invalide."],
        ["Fecha Llegada es obligatoria o no tiene un formato válido.", "Arrival date is required or has an invalid format.", "A data de chegada é obrigatória ou tem formato inválido.", "La date d'arrivée est obligatoire ou son format est invalide."],
        ["Fecha Reserva no tiene un formato de fecha válido.", "Reservation date has an invalid format.", "A data da reserva tem formato inválido.", "Le format de la date de réservation est invalide."],
        ["Fecha Llegada no tiene un formato de fecha válido.", "Arrival date has an invalid format.", "A data de chegada tem formato inválido.", "Le format de la date d'arrivée est invalide."],
        ["Fecha Salida no tiene un formato de fecha válido.", "Departure date has an invalid format.", "A data de saída tem formato inválido.", "Le format de la date de départ est invalide."],
        ["No fue posible crear esta reserva.", "Could not create this reservation.", "Não foi possível criar esta reserva.", "Impossible de créer cette réservation."],
        ["No fue posible consultar los grupos y turnos disponibles.", "Could not load available company groups and shifts.", "Não foi possível consultar os grupos e turnos disponíveis.", "Impossible de consulter les groupes et équipes disponibles."],
        ["No se pudo completar la operación.", "The operation could not be completed.", "Não foi possível concluir a operação.", "L'opération n'a pas pu être terminée."],
        ["Debe ingresar una fecha y un turno válido.", "Enter a valid date and shift.", "Informe uma data e um turno válidos.", "Saisissez une date et une équipe valides."],
        ["Debe ingresar un período válido.", "Enter a valid date range.", "Informe um período válido.", "Saisissez une période valide."],
        ["Debe ingresar la fecha de censo.", "Enter the census date.", "Informe a data do censo.", "Saisissez la date du recensement."],
        ["Seleccione un turno válido.", "Select a valid shift.", "Selecione um turno válido.", "Sélectionnez une équipe valide."],
        ["La cantidad de camas no es válida.", "The bed count is invalid.", "A quantidade de camas é inválida.", "Le nombre de lits est invalide."],
        ["La cantidad de camas nocturnas no es válida.", "The night bed count is invalid.", "A quantidade de camas noturnas é inválida.", "Le nombre de lits de nuit est invalide."],
        ["El código QR no contiene una habitación válida.", "The QR code does not contain a valid room.", "O código QR não contém um quarto válido.", "Le code QR ne contient pas de chambre valide."],
        ["No se pudo guardar la reserva.", "Could not save the reservation.", "Não foi possível salvar a reserva.", "Impossible d'enregistrer la réservation."],
        ["No se pudo registrar el movimiento.", "Could not register the movement.", "Não foi possível registrar o movimento.", "Impossible d'enregistrer le mouvement."],
        ["No se pudo actualizar la reserva.", "Could not update the reservation.", "Não foi possível atualizar a reserva.", "Impossible de mettre à jour la réservation."],
        ["Censo QR guardado correctamente.", "QR census saved successfully.", "Censo QR salvo com sucesso.", "Recensement QR enregistré avec succès."],
        ["No fue posible acceder a la cámara.", "Could not access the camera.", "Não foi possível acessar a câmera.", "Impossible d'accéder à la caméra."],
        ["El código QR no contiene una habitación.", "The QR code does not contain a room.", "O código QR não contém um quarto.", "Le code QR ne contient pas de chambre."],
        ["Camas ocupadas noche", "Night occupied beds", "Camas ocupadas à noite", "Lits occupés la nuit"],
        ["Fecha reserva", "Reservation date", "Data da reserva", "Date de réservation"],
        ["Cámaras", "Cameras", "Câmeras", "Caméras"],
        ["No se pudieron consultar mis censos.", "Could not load your censuses.", "Não foi possível consultar seus censos.", "Impossible de consulter vos recensements."],
        ["No se pudo guardar el censo.", "Could not save the census.", "Não foi possível salvar o censo.", "Impossible d'enregistrer le recensement."],
        ["Error", "Error", "Erro", "Erreur"],
        ["Acceso denegado", "Access denied", "Acesso negado", "Accès refusé"],
        ["An error occurred while processing your request.", "An error occurred while processing your request.", "Ocorreu um erro ao processar sua solicitação.", "Une erreur s'est produite lors du traitement de votre demande."],
        ["Request ID:", "Request ID:", "ID da solicitação:", "ID de la requête :"],
        ["Development Mode", "Development Mode", "Modo de desenvolvimento", "Mode développement"],
        ["Ir al ingreso", "Go to sign in", "Ir para o login", "Aller à la connexion"],
        ["Volver al inicio", "Back to home", "Voltar ao início", "Retour à l'accueil"],
        ["Entrar", "Sign in", "Entrar", "Se connecter"],
        ["Ingreso", "Sign in", "Login", "Connexion"],
        ["Solo la cookie de sesión es necesaria para entrar. Las demás categorías permanecen apagadas hasta una aceptación previa, específica y revocable. Seguir navegando no las activa.", "Only the session cookie is necessary to sign in. Other categories remain disabled until you give prior, specific and revocable consent. Continuing to browse does not enable them.", "Apenas o cookie de sessão é necessário para entrar. As demais categorias permanecem desativadas até uma aceitação prévia, específica e revogável. Continuar navegando não as ativa.", "Seul le cookie de session est nécessaire pour se connecter. Les autres catégories restent désactivées jusqu'à un consentement préalable, spécifique et révocable. La poursuite de la navigation ne les active pas."],
        ["Cookie de autenticación. Mantiene el ingreso. Caduca al cerrar el navegador o al cumplirse el tiempo del perfil. Un script de la página no puede leerla.", "Authentication cookie. Keeps you signed in. Expires when the browser closes or when the profile session limit is reached. Page scripts cannot read it.", "Cookie de autenticação. Mantém o login. Expira ao fechar o navegador ou ao atingir o tempo limite do perfil. Scripts da página não podem lê-lo.", "Cookie d'authentification. Maintient la connexion. Expire à la fermeture du navigateur ou à la fin de la durée du profil. Les scripts de la page ne peuvent pas le lire."],
        ["Recuerda la elección. Dura un año. Es necesaria para no volver a preguntar y para poder cambiarla.", "Remembers your choice for one year. It is necessary to avoid asking again and to let you change your choice.", "Lembra sua escolha por um ano. É necessário para não perguntar novamente e para permitir alterá-la.", "Mémorise votre choix pendant un an. Il est nécessaire pour ne pas vous le redemander et pour vous permettre de le modifier."],
        ["Servirían para medir el uso del sitio. Hoy no hay ninguna herramienta de medición cargada. Si más adelante se agrega, solo parte después de aceptarla.", "These would measure site usage. No analytics tool is currently loaded. If one is added later, it will only start after you accept.", "Serviriam para medir o uso do site. Atualmente não há nenhuma ferramenta de medição carregada. Se uma for adicionada, só começará após sua aceitação.", "Ils serviraient à mesurer l'utilisation du site. Aucun outil de mesure n'est actuellement chargé. Si un outil est ajouté, il ne démarrera qu'après votre consentement."],
        ["Hoy no se usan. Rechazarlas no impide usar Gestión Hotelera. No hay cesión de estas cookies fuera de Chile.", "These are not currently used. Rejecting them does not prevent you from using Hotel Management. These cookies are not transferred outside Chile.", "Atualmente não são usadas. Recusá-las não impede o uso da Gestão Hoteleira. Esses cookies não são transferidos para fora do Chile.", "Ils ne sont pas utilisés actuellement. Les refuser n'empêche pas l'utilisation de Gestion Hôtelière. Ces cookies ne sont pas transférés hors du Chili."],
        ["Sesión y el recuerdo de esta elección. Siempre activas, porque el ingreso no funciona sin ellas.", "Session and remembering this choice. Always active because sign-in requires them.", "Sessão e memorização desta escolha. Sempre ativos porque o login depende deles.", "Session et mémorisation de ce choix. Toujours actifs, car la connexion en dépend."],
        ["Medir el uso del sitio. Hoy no hay una herramienta de medición cargada.", "Measure site usage. No analytics tool is currently loaded.", "Medir o uso do site. Atualmente não há nenhuma ferramenta de medição carregada.", "Mesurer l'utilisation du site. Aucun outil de mesure n'est actuellement chargé."],
        ["Puede rechazarlas y seguir usando el sitio.", "You can reject them and continue using the site.", "Você pode recusá-los e continuar usando o site.", "Vous pouvez les refuser et continuer à utiliser le site."],
        ["Guardar selección", "Save selection", "Salvar seleção", "Enregistrer la sélection"],
        ["El responsable del tratamiento es Gestión Hotelera. Los datos de la cuenta se usan para identificar a la persona y abrir su sesión. No se elaboran perfiles publicitarios ni se envían estas cookies a otro país.", "Hotel Management is the data controller. Account data is used to identify the person and start their session. Advertising profiles are not created, and these cookies are not sent to another country.", "A Gestão Hoteleira é responsável pelo tratamento. Os dados da conta são usados para identificar a pessoa e iniciar sua sessão. Não são criados perfis publicitários nem esses cookies são enviados a outro país.", "Gestion Hôtelière est responsable du traitement. Les données du compte servent à identifier la personne et à ouvrir sa session. Aucun profil publicitaire n'est créé et ces cookies ne sont pas envoyés dans un autre pays."],
        ["Qué datos se usan", "What data is used", "Quais dados são usados", "Quelles données sont utilisées"],
        ["El Rut o Dni y la contraseña, para comprobar el ingreso.", "The ID number and password, to verify sign-in.", "O CPF/RG e a senha, para verificar o login.", "Le numéro d'identité et le mot de passe, pour vérifier la connexion."],
        ["El campamento y el perfil elegidos, para armar la sesión.", "The selected camp and profile, to establish the session.", "O acampamento e o perfil escolhidos, para iniciar a sessão.", "Le camp et le profil sélectionnés, pour établir la session."],
        ["La dirección IP y el navegador, cuando corresponda registrar una aceptación.", "The IP address and browser, when consent must be recorded.", "O endereço IP e o navegador, quando for necessário registrar uma aceitação.", "L'adresse IP et le navigateur, lorsqu'il faut enregistrer un consentement."],
        ["Cómo se guarda la contraseña", "How the password is stored", "Como a senha é armazenada", "Comment le mot de passe est stocké"],
        ["La contraseña no se conserva en claro. Se compara con el hash guardado. Si ese hash todavía es el legado, se reemplaza por uno nuevo en el mismo ingreso.", "The password is never stored in plain text. It is checked against the stored hash. If that hash is still legacy, it is replaced with a new one during sign-in.", "A senha não é armazenada em texto simples. Ela é comparada com o hash salvo. Se o hash ainda for legado, será substituído por um novo durante o login.", "Le mot de passe n'est jamais conservé en clair. Il est comparé au hachage enregistré. Si ce hachage est encore hérité, il est remplacé par un nouveau lors de la connexion."],
        ["Cuánto dura la sesión", "How long the session lasts", "Quanto tempo dura a sessão", "Durée de la session"],
        ["La sesión vive en una cookie necesaria del sitio. Caduca al cerrar el navegador o al cumplirse el tiempo del perfil. La elección sobre las demás cookies se conserva un año y se puede cambiar con la misma facilidad con la que se otorgó, desde Preferencias.", "The session uses a necessary site cookie. It expires when the browser closes or when the profile session limit is reached. Your choice for other cookies is kept for one year and can be changed at any time in Preferences.", "A sessão usa um cookie necessário do site. Ele expira ao fechar o navegador ou ao atingir o tempo limite do perfil. Sua escolha sobre os demais cookies é mantida por um ano e pode ser alterada a qualquer momento em Preferências.", "La session utilise un cookie nécessaire du site. Il expire à la fermeture du navigateur ou à la fin de la durée du profil. Votre choix concernant les autres cookies est conservé un an et peut être modifié à tout moment dans Préférences."],
        ["Ver el detalle de las cookies", "View cookie details", "Ver detalhes dos cookies", "Voir le détail des cookies"],
        ["Necesarias.", "Necessary.", "Necessárias.", "Nécessaires."],
        ["Analíticas.", "Analytics.", "Analíticas.", "Analytiques."],
        ["Publicidad y perfilamiento.", "Advertising and profiling.", "Publicidade e criação de perfil.", "Publicité et profilage."],
        ["Gestión Hotelera usa cookies necesarias para la sesión. Las analíticas y las de publicidad o perfilamiento siguen apagadas hasta que usted las acepte. Seguir en el sitio no cuenta como aceptación.", "Hotel Management uses necessary cookies for your session. Analytics and advertising or profiling cookies remain disabled until you accept them. Continuing to use the site does not count as consent.", "A Gestão Hoteleira usa cookies necessários para a sessão. Os cookies analíticos e de publicidade ou criação de perfil permanecem desativados até que você os aceite. Continuar no site não significa consentimento.", "Gestion Hôtelière utilise des cookies nécessaires à votre session. Les cookies analytiques, publicitaires ou de profilage restent désactivés jusqu'à votre consentement. La poursuite de la navigation ne vaut pas consentement."],
        ["Seleccione Área", "Select a company group", "Selecione uma área", "Sélectionnez un groupe"],
        ["Seleccione Turno", "Select a shift", "Selecione um turno", "Sélectionnez une équipe"],
        ["Fecha Salida debe ser mayor o igual a la fecha de Llegada", "Departure date must be on or after the arrival date", "A data de saída deve ser igual ou posterior à data de chegada", "La date de départ doit être égale ou postérieure à la date d'arrivée"],
        ["La reserva no tiene datos de sesión válidos.", "The reservation has invalid session data.", "A reserva não tem dados de sessão válidos.", "La réservation contient des données de session invalides."],
        ["El estado de la reserva no es válido.", "The reservation status is invalid.", "O status da reserva é inválido.", "L'état de la réservation est invalide."],
        ["La observación admite hasta 200 caracteres.", "The note can contain up to 200 characters.", "A observação pode ter até 200 caracteres.", "La remarque peut contenir jusqu'à 200 caractères."],
        ["La reserva no corresponde al campamento de la sesión.", "The reservation does not belong to the session camp.", "A reserva não pertence ao acampamento da sessão.", "La réservation n'appartient pas au camp de la session."],
        ["Seleccione una reserva.", "Select a reservation.", "Selecione uma reserva.", "Sélectionnez une réservation."],
        ["La operación no corresponde a un check-in ni a un check-out.", "The operation is neither check-in nor check-out.", "A operação não corresponde a check-in nem check-out.", "L'opération ne correspond ni à une arrivée ni à un départ."],
        ["La sesión no tiene un usuario.", "The session has no user.", "A sessão não tem um usuário.", "La session ne contient aucun utilisateur."],
        ["La sesión no tiene un campamento.", "The session has no camp.", "A sessão não tem um acampamento.", "La session ne contient pas de camp."],
        ["Faltan columnas obligatorias:", "Missing required columns:", "Faltam colunas obrigatórias:", "Colonnes obligatoires manquantes :"]
    ];

    var idiomas = { en: {}, pt: {}, fr: {} };
    var inversos = { en: {}, pt: {}, fr: {} };
    var clavesEspanol = {};
    mensajes.forEach(function (fila) {
        clavesEspanol[fila[0]] = true;
        idiomas.en[fila[0]] = fila[1];
        idiomas.pt[fila[0]] = fila[2];
        idiomas.fr[fila[0]] = fila[3];
        inversos.en[fila[1]] = fila[0];
        inversos.pt[fila[2]] = fila[0];
        inversos.fr[fila[3]] = fila[0];
    });

    function idiomaDelNavegador() {
        var lista = navigator.languages || [navigator.language || "es"];
        for (var i = 0; i < lista.length; i++) {
            var codigo = String(lista[i]).slice(0, 2).toLowerCase();
            if (idiomas[codigo]) return codigo;
        }
        return "es";
    }

    function idiomaGuardado() {
        var valor = localStorage.getItem("sgh.idioma");
        return valor === "es" || valor === "en" || valor === "pt" || valor === "fr"
            ? valor
            : idiomaDelNavegador();
    }

    var actual = idiomaGuardado();

    function traducirTexto(texto) {
        var origen = String(texto || "");
        var limpio = origen.trim();
        if (!limpio) return origen;

        var base = limpio;
        if (!clavesEspanol[limpio]) {
            ["en", "pt", "fr"].some(function (codigo) {
                if (!inversos[codigo][limpio]) return false;
                base = inversos[codigo][limpio];
                return true;
            });
        }
        if (base === limpio) {
            var filaTraducida = limpio.match(/^(?:Row|Linha|Ligne) (\d+): (.+)$/);
            if (filaTraducida) {
                var textoError = filaTraducida[2];
                ["en", "pt", "fr"].some(function (codigo) {
                    if (!inversos[codigo][textoError]) return false;
                    textoError = inversos[codigo][textoError];
                    return true;
                });
                base = "Fila " + filaTraducida[1] + ": " + textoError;
            }
            var reservaCreada = limpio.match(/^(?:Reservation created No\. |Reserva criada Nº |Réservation créée n° )(.+)\.$/);
            if (reservaCreada) base = "Reserva creada N° " + reservaCreada[1] + ".";
            var resumenTraducido = limpio.match(/^(?:Reservations created: |Reservas criadas: |Réservations créées : )(\d+)(?: of | de | sur )(\d+)\.$/);
            if (resumenTraducido)
                base = "Reservas creadas: " + resumenTraducido[1] + " de " + resumenTraducido[2] + ".";
            var rangoTraducido = limpio.match(/^(\d+)(?: to | a | à )(\d+)(?: of | de | sur )(\d+)$/);
            if (rangoTraducido)
                base = rangoTraducido[1] + " a " + rangoTraducido[2] + " de " + rangoTraducido[3];
            var pendientesTraducidos = limpio.match(/^(?:Automatic assignment could not be completed\. Passengers remaining: |Não foi possível concluir a atribuição automática\. Passageiros pendentes: |L'attribution automatique n'a pas pu être terminée\. Passagers restants : )(\d+)(?:\. Check the rooms and filters\.|\. Revise os quartos e filtros\.|\. Vérifiez les chambres et les filtres\.)$/);
            if (pendientesTraducidos)
                base = "No se pudo completar la asignación automática. Pasajeros pendientes: " + pendientesTraducidos[1] + ". Revise las habitaciones y filtros.";
            var tituloTraducido = limpio.match(/^(.+) - (?:Hotel Management|Gestão Hoteleira|Gestion Hôtelière)$/);
            if (tituloTraducido) {
                var textoTitulo = tituloTraducido[1];
                ["en", "pt", "fr"].some(function (codigo) {
                    if (!inversos[codigo][textoTitulo]) return false;
                    textoTitulo = inversos[codigo][textoTitulo];
                    return true;
                });
                base = textoTitulo + " - Gestión Hotelera";
            }
            var marcaTraducida = limpio.match(/^(?:Hotel Management|Gestão Hoteleira|Gestion Hôtelière) - (.+)$/);
            if (marcaTraducida) base = "Gestión Hotelera - " + marcaTraducida[1];
        }
        if (actual === "es") {
            if (base === limpio) return origen;
            return origen.slice(0, origen.indexOf(limpio)) + base
                + origen.slice(origen.indexOf(limpio) + limpio.length);
        }

        var traduccion = idiomas[actual][base];
        if (!traduccion) {
            var pendientes = base.match(/^No se pudo completar la asignación automática\. Pasajeros pendientes: (\d+)\. Revise las habitaciones y filtros\.$/);
            if (pendientes) {
                traduccion = actual === "en" ? "Automatic assignment could not be completed. Passengers remaining: " + pendientes[1] + ". Check the rooms and filters."
                    : actual === "pt" ? "Não foi possível concluir a atribuição automática. Passageiros pendentes: " + pendientes[1] + ". Revise os quartos e filtros."
                    : "L'attribution automatique n'a pas pu être terminée. Passagers restants : " + pendientes[1] + ". Vérifiez les chambres et les filtres.";
            }
        }
        if (!traduccion) {
            var creado = base.match(/^Reserva creada N° (.+)\.$/);
            if (creado) {
                var prefijo = actual === "en" ? "Reservation created No. " : actual === "pt" ? "Reserva criada Nº " : "Réservation créée n° ";
                traduccion = prefijo + creado[1] + ".";
            }
        }
        if (!traduccion) {
            var rango = base.match(/^(\d+) a (\d+) de (\d+)$/);
            if (rango) {
                traduccion = actual === "en"
                    ? rango[1] + " to " + rango[2] + " of " + rango[3]
                    : actual === "pt"
                        ? rango[1] + " a " + rango[2] + " de " + rango[3]
                        : rango[1] + " à " + rango[2] + " sur " + rango[3];
            }
        }
        if (!traduccion) {
            var columnas = base.match(/^Faltan columnas obligatorias: (.+)\.$/);
            if (columnas) {
                var prefijoColumnas = actual === "en" ? "Missing required columns: "
                    : actual === "pt" ? "Faltam colunas obrigatórias: " : "Colonnes obligatoires manquantes : ";
                traduccion = prefijoColumnas + columnas[1].split(", ").map(traducirTexto).join(", ") + ".";
            }
        }
        if (!traduccion) {
            var resumen = base.match(/^Reservas creadas: (\d+) de (\d+)\.$/);
            if (resumen) {
                var prefijoResumen = actual === "en" ? "Reservations created: "
                    : actual === "pt" ? "Reservas criadas: " : "Réservations créées : ";
                var conector = actual === "en" ? " of " : actual === "fr" ? " sur " : " de ";
                traduccion = prefijoResumen + resumen[1] + conector + resumen[2] + ".";
            }
        }
        if (!traduccion) {
            var titulo = base.match(/^(.+) - Gestión Hotelera$/);
            if (titulo)
                traduccion = traducirTexto(titulo[1]) + " - " + idiomas[actual]["Gestión Hotelera"];
        }
        if (!traduccion) {
            var marca = base.match(/^Gestión Hotelera - (.+)$/);
            if (marca)
                traduccion = idiomas[actual]["Gestión Hotelera"] + " - " + marca[1];
        }
        if (!traduccion) {
            var fila = base.match(/^Fila (\d+): (.+)$/);
            if (fila) {
                var prefijoFila = actual === "en" ? "Row " : actual === "pt" ? "Linha " : "Ligne ";
                traduccion = prefijoFila + fila[1] + ": " + traducirTexto(fila[2]);
            }
        }
        if (!traduccion) return origen;
        var inicio = origen.indexOf(limpio);
        return origen.slice(0, inicio) + traduccion + origen.slice(inicio + limpio.length);
    }

    function traducirElemento(elemento) {
        if (elemento.nodeType === Node.TEXT_NODE) {
            if (elemento.parentElement && /^(SCRIPT|STYLE|TEXTAREA)$/.test(elemento.parentElement.tagName)) return;
            var texto = traducirTexto(elemento.nodeValue);
            if (texto !== elemento.nodeValue) elemento.nodeValue = texto;
            return;
        }
        if (elemento.nodeType !== Node.ELEMENT_NODE) return;
        if (elemento.matches("[data-selector-idioma]")) {
            if (elemento.hasAttribute("aria-label")) {
                var etiquetaActual = elemento.getAttribute("aria-label");
                var etiquetaTraducida = traducirTexto(etiquetaActual);
                if (etiquetaTraducida !== etiquetaActual)
                    elemento.setAttribute("aria-label", etiquetaTraducida);
            }
            return;
        }
        ["placeholder", "title", "aria-label"].forEach(function (atributo) {
            if (elemento.hasAttribute(atributo)) {
                var actualAtributo = elemento.getAttribute(atributo);
                var traduccionAtributo = traducirTexto(actualAtributo);
                if (traduccionAtributo !== actualAtributo)
                    elemento.setAttribute(atributo, traduccionAtributo);
            }
        });
        Array.prototype.forEach.call(elemento.childNodes, traducirElemento);
    }

    function aplicar() {
        document.documentElement.lang = actual === "pt" ? "pt-BR" : actual;
        document.querySelectorAll("[data-selector-idioma]").forEach(function (selector) {
            selector.value = actual;
        });
        var titulo = document.querySelector("title");
        if (titulo) traducirElemento(titulo);
        traducirElemento(document.body);
        document.dispatchEvent(new CustomEvent("sgh:idioma-cambiado", { detail: { idioma: actual } }));
    }

    document.addEventListener("change", function (evento) {
        if (!evento.target.matches("[data-selector-idioma]")) return;
        actual = evento.target.value;
        localStorage.setItem("sgh.idioma", actual);
        var cultura = actual === "es" ? "es-CL"
            : actual === "en" ? "en-US"
                : actual === "pt" ? "pt-BR" : "fr-FR";
        var cookie = ".AspNetCore.Culture=" + encodeURIComponent("c=" + cultura + "|uic=" + cultura)
            + "; path=/; max-age=31536000; samesite=lax";
        if (location.protocol === "https:") cookie += "; secure";
        document.cookie = cookie;
        aplicar();
    });

    aplicar();
    new MutationObserver(function (cambios) {
        cambios.forEach(function (cambio) {
            if (cambio.type === "characterData") traducirElemento(cambio.target);
            if (cambio.type === "attributes") traducirElemento(cambio.target);
            cambio.addedNodes.forEach(traducirElemento);
        });
    }).observe(document.body, {
        childList: true,
        characterData: true,
        attributes: true,
        attributeFilter: ["placeholder", "title", "aria-label"],
        subtree: true
    });

    window.SGHI18n = {
        idioma: function () { return actual; },
        traducir: traducirTexto
    };
})();
