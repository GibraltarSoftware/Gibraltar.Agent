function Handlers($, logging) {
    
    $('#logVerbose').click(function (e) {
        logging.verbose();
    });

    $('#logInfo').click(function (e) {
        logging.information();
    });

    $('#logWarning').click(function (e) {
        logging.warning();
    });

    $('#logError').click(function (e) {
        logging.error();
    });

    $('#logCritical').click(function (e) {
        logging.critical();
    });

    $('#logWrite').click(function (e) {
        logging.write();
    });

    $('#unhandledException').click(function (e) {
        logging.unhandledException();
    });
    
    $('#ajaxCall').click(function (e) {
        logging.ajaxCall();
    });

    $('#login').click(function(e) {
        logging.login();
    });
}