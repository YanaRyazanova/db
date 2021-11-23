using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace Game.Domain
{
    // TODO Сделать по аналогии с MongoUserRepository
    public class MongoGameRepository : IGameRepository
    {
        private readonly IMongoCollection<GameEntity> gameCollection;
        public const string CollectionName = "games";

        public MongoGameRepository(IMongoDatabase db)
        {
            gameCollection = db.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            gameCollection.InsertOne(game);
            return FindById(game.Id);
        }

        public GameEntity FindById(Guid gameId)
        {
            return gameCollection
                .Find(x => x.Id == gameId)
                .FirstOrDefault();
        }

        public void Update(GameEntity game)
        {
            gameCollection.ReplaceOne(x => x.Id == game.Id, game);
        }

        // Возвращает не более чем limit игр со статусом GameStatus.WaitingToStart
        public IList<GameEntity> FindWaitingToStart(int limit)
        {
            return gameCollection
                .Find(x => x.Status == GameStatus.WaitingToStart)
                .Limit(limit)
                .ToList();
        }

        // Обновляет игру, если она находится в статусе GameStatus.WaitingToStart
        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            var foundedGame = FindById(game.Id);
            if (foundedGame == null || foundedGame.Status != GameStatus.WaitingToStart) return false;
            var newGame = new GameEntity(game.Id, GameStatus.Playing, game.TurnsCount, game.CurrentTurnIndex, game.Players.ToList());
            gameCollection.ReplaceOne(x => x.Id == game.Id, newGame);
            return true;
            //TODO: Для проверки успешности используй IsAcknowledged и ModifiedCount из результата
        }
    }
}