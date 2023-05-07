function Handlers($, logging) {
    
    $('#logVerbose').click(function (e) {
        e.preventDefault();
        logging.verbose();
    });

    $('#logInfo').click(function (e) {
        e.preventDefault();
        logging.information();
    });

    $('#logWarning').click(function (e) {
        e.preventDefault();
        logging.warning();
    });

    $('#logError').click(function (e) {
        e.preventDefault();
        logging.error();
    });

    $('#logCritical').click(function (e) {
        e.preventDefault();
        logging.critical();
    });

    $('#logWrite').click(function (e) {
        e.preventDefault();
        logging.write();
    });

    $('#unhandledException').click(function (e) {
        e.preventDefault();
        logging.unhandledException();
    });
    
    $('#ajaxCall').click(function (e) {
        e.preventDefault();
        logging.ajaxCall();
    });
 
    $('#login').click(function (e) {
        e.preventDefault();
        logging.login();
    });
}