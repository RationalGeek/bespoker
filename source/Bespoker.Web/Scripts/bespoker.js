var bespoker = function (sessionName) {
    $(document).ready(function () {
        var hub = $.connection.bespokerHub;
        var viewModel = null;

        hub.client.initSession = function (session) {
            // Init viewmodel based on session
            viewModel = ko.mapping.fromJS(session);
            viewModel.CurrentConnectionId = ko.observable(hub.connection.id);
            ko.applyBindings(viewModel);
        };

        hub.client.playerJoined = function (player) {
            viewModel.Players.push(ko.mapping.fromJS(player));
        };

        hub.client.cardSelected = function (playerId, cardValue) {
            // Find player by Id
            var players = viewModel.Players();
            for (var i = 0; i < players.length; i++) {
                var player = players[i];
                if (player.Id() == playerId) {
                    // Update selected card value
                    player.SelectedCard(cardValue);
                    break;
                }
            }
        };

        hub.client.showCards = function () {
            viewModel.CardsRevealed(true);
        };

        hub.client.hideCards = function () {
            viewModel.CardsRevealed(false);
        };

        hub.client.reset = function (session) {
            // Update viewmodel based on new session data
            viewModel = ko.mapping.fromJS(session, viewModel);

            // UI should show no cards selected
            $('#cardChooser .card').removeClass('selected');
        };

        // Initialize connection to SignalR
        $.connection.hub.start().done(function () {
            hub.server.registerForSession(sessionName);
        });

        var cards = $('#cardChooser .card');
        cards.click(function () {
            // Is this card already selected?
            var card = $(this);
            if (card.hasClass('selected')) {
                card.removeClass('selected');
                hub.server.selectCard(null);
            }
            else {
                $(cards).removeClass('selected');
                card.addClass('selected');
                hub.server.selectCard(card.text());
            }
        });

        $('#showCards').click(function () {
            hub.server.showCards();
        });

        $('#hideCards').click(function () {
            hub.server.hideCards();
        });

        $('#reset').click(function () {
            hub.server.reset();
        });

        $('#changeName').click(function () {
            var newName = prompt('Change name to what?');
            hub.server.changeName(newName);
        });

        hub.client.newPlayerName = function (playerId, newName) {
            // Loop through all the players, find the one in question, and update name
            var players = viewModel.Players();
            for (var i = 0; i < players.length; i++) {
                var player = players[i];
                if (player.Id() == playerId) {
                    // Update selected card value
                    player.Name(newName);
                    break;
                }
            }
        };

        hub.client.removePlayer = function (playerId) {
            // Loop through all the players, find the one in question, and update name
            var players = viewModel.Players();
            for (var i = 0; i < players.length; i++) {
                var player = players[i];
                if (player.Id() == playerId) {
                    viewModel.Players.remove(player);
                    break;
                }
            }
        };
    });
}