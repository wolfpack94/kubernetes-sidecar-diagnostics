FROM fluent/fluentd:v1.1.2-debian

MAINTAINER Azure Diagnostics Tooling Core <ctdiagcore@microsoft.com>
USER root

RUN buildDeps="sudo make gcc g++ libc-dev ruby-dev libffi-dev" \
     && apt-get update \
     && apt-get upgrade -y \
     && apt-get install \
     -y --no-install-recommends \
     $buildDeps \
    && echo 'gem: --no-document' >> /etc/gemrc \
    && gem install fluent-plugin-secure-forward \
    # && gem install fluent-plugin-kubernetes_metadata_filter \
    && gem install fluent-plugin-application-insights \
    && gem install fluent-plugin-script \
    && gem install ffi \
    && gem install fluent-plugin-systemd 
    # \
    # && SUDO_FORCE_REMOVE=yes \
    # apt-get purge -y --auto-remove \
    #               -o APT::AutoRemove::RecommendsImportant=false \
    #               $buildDeps \
    # && rm -rf /var/lib/apt/lists/* \
    # && gem sources --clear-all \
    # && rm -rf /tmp/* /var/tmp/* /usr/lib/ruby/gems/*/cache/*.gem

# TODO: Change to the official ruby gem once the PR is merged (https://github.com/fabric8io/fluent-plugin-kubernetes_metadata_filter/pull/133)
COPY ./fluent-plugin-kubernetes_metadata_filter-2.1.2.gem /fluentd/fluent-plugin-kubernetes_metadata_filter-2.1.2.gem
RUN gem install /fluentd/fluent-plugin-kubernetes_metadata_filter-2.1.2.gem

# TODO: Change to the official ruby gem once it's published
COPY ./fluent-plugin-application-insights-parser-0.1.0.gem /fluentd/fluent-plugin-application-insights-parser-0.1.0.gem
RUN gem install /fluentd/fluent-plugin-application-insights-parser-0.1.0.gem

# TODO: Remove this hack once the fix for http input plugin (https://github.com/fluent/fluentd/commit/f560017ce78331278c2e6a0aff0fd30bca4ee015) is released
COPY ./in_http.rb /var/lib/gems/2.3.0/gems/fluentd-1.1.2/lib/fluent/plugin/in_http.rb

# Copy the dedicated script to flatten kubernetes metadata
COPY ./flatten_kubernetes_metadata.rb /fluentd/etc/flatten_kubernetes_metadata.rb

# Copy configuration files
COPY ./conf/ /fluentd/etc/

# Environment variables
ENV FLUENT_UID=0
ENV FLUENTD_OPT=""
ENV APP_INSIGHTS_HTTP_CHANNEL_PORT=8887
ENV TAIL_INPUT_POS_FILE_DIR="/var/log"
ENV SEND_SIDECAR_LOG=false
ENV SIDECAR_CONTAINER_NAME="fluentdsidecar"

# Run Fluentd
COPY entrypoint.sh /fluentd/entrypoint.sh
CMD ["/fluentd/entrypoint.sh"]
