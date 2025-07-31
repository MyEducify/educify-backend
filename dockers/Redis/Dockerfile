FROM redis:7.2-alpine

# Optional: Copy your custom redis.conf if needed
COPY redis.conf /usr/local/etc/redis/redis.conf

EXPOSE 6379

CMD ["redis-server"]