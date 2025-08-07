package org.huex.liarbarback.managers;

import java.util.Map;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;

import org.huex.liarbarback.events.RoomUpdatedEvent;
import org.huex.liarbarback.models.Player;
import org.huex.liarbarback.models.Room;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.stereotype.Component;

@Component
public class PlayerManager {
    // @Autowired private ApplicationEventPublisher eventPublisher;

    private final Map<String, Player> players = new ConcurrentHashMap<>();

    public void addPlayer(Player player) {
        players.put(player.getUserId(), player);
        // eventPublisher.publishEvent(new RoomUpdatedEvent(this, player.getRoomId()));
    }

    public Optional<Player> getPlayer(String userId) {
        return Optional.ofNullable(players.get(userId));
    }
    
    public boolean removePlayer(String userId) {
        String roomId = getPlayer(userId).get().getRoomId();
        boolean success = players.remove(userId)!= null;
        // eventPublisher.publishEvent(new RoomUpdatedEvent(this, roomId));
        return success;
    }
}
