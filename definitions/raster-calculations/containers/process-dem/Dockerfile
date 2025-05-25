FROM function AS function
FROM python:3.11

COPY --from=function /function_runner/function_runner /usr/bin/function_runner
RUN chmod +x /usr/bin/function_runner

WORKDIR /function/
COPY requirements.txt   .

# Install other requirements
RUN pip install -r requirements.txt

COPY index.py    .

ENV FUNCTION_COMMAND="python /function/index.py"

HEALTHCHECK --interval=5s CMD [ -e /tmp/.lock ] || exit 1

CMD ["function_runner"]