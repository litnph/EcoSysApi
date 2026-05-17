CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE audit_log_retentions (
        id uuid NOT NULL,
        entity_type character varying(128),
        retain_days integer NOT NULL,
        archive_before_delete boolean NOT NULL,
        archive_storage_key_prefix character varying(512),
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_audit_log_retentions PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE locales (
        id uuid NOT NULL,
        code character varying(20) NOT NULL,
        name character varying(64) NOT NULL,
        english_name character varying(64) NOT NULL,
        direction text NOT NULL,
        is_default boolean NOT NULL,
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_locales PRIMARY KEY (id),
        CONSTRAINT ak_locales_code UNIQUE (code)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE system_event_logs (
        id uuid NOT NULL,
        event_type character varying(128) NOT NULL,
        entity_type character varying(128),
        entity_id uuid,
        job_name character varying(255),
        job_id character varying(64),
        payload jsonb,
        status character varying(16) NOT NULL,
        error_message character varying(2048),
        stack_trace text,
        duration_ms bigint,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_system_event_logs PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE users (
        id uuid NOT NULL,
        email character varying(320) NOT NULL,
        password_hash character varying(255),
        full_name character varying(255) NOT NULL,
        is_email_verified boolean NOT NULL,
        last_login_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        CONSTRAINT pk_users PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE translation_fallbacks (
        id uuid NOT NULL,
        locale_code character varying(20) NOT NULL,
        fallback_locale_code character varying(20) NOT NULL,
        priority integer NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_translation_fallbacks PRIMARY KEY (id),
        CONSTRAINT fk_translation_fallbacks_locales_fallback_locale_code FOREIGN KEY (fallback_locale_code) REFERENCES locales (code) ON DELETE RESTRICT,
        CONSTRAINT fk_translation_fallbacks_locales_locale_code FOREIGN KEY (locale_code) REFERENCES locales (code) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE translations (
        id uuid NOT NULL,
        entity_type character varying(128) NOT NULL,
        entity_id uuid NOT NULL,
        field character varying(64) NOT NULL,
        locale_code character varying(20) NOT NULL,
        value text NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_translations PRIMARY KEY (id),
        CONSTRAINT fk_translations_locales_locale_code FOREIGN KEY (locale_code) REFERENCES locales (code) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE ui_strings (
        id uuid NOT NULL,
        key character varying(255) NOT NULL,
        locale_code character varying(20) NOT NULL,
        value text NOT NULL,
        description character varying(512),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_ui_strings PRIMARY KEY (id),
        CONSTRAINT fk_ui_strings_locales_locale_code FOREIGN KEY (locale_code) REFERENCES locales (code) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE audit_logs (
        id uuid NOT NULL,
        user_id uuid,
        session_id uuid,
        entity_type character varying(128) NOT NULL,
        entity_id uuid NOT NULL,
        action text NOT NULL,
        before_snapshot jsonb,
        after_snapshot jsonb,
        changed_fields jsonb,
        ip_address character varying(64),
        user_agent character varying(512),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_audit_logs PRIMARY KEY (id),
        CONSTRAINT fk_audit_logs_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE organizations (
        id uuid NOT NULL,
        is_personal boolean NOT NULL,
        slug character varying(64) NOT NULL,
        name character varying(255) NOT NULL,
        owner_id uuid NOT NULL,
        default_currency character varying(8) NOT NULL,
        description character varying(1024),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        version integer NOT NULL,
        updated_by uuid,
        last_session_id uuid,
        CONSTRAINT pk_organizations PRIMARY KEY (id),
        CONSTRAINT fk_organizations_users_owner_id FOREIGN KEY (owner_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_auth_providers (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        provider text NOT NULL,
        provider_user_id character varying(255),
        provider_email character varying(320),
        is_active boolean NOT NULL,
        linked_at timestamp with time zone NOT NULL,
        last_used_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_auth_providers PRIMARY KEY (id),
        CONSTRAINT fk_user_auth_providers_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_avatar_uploads (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        storage_key character varying(512) NOT NULL,
        storage_url character varying(1024),
        content_type character varying(64) NOT NULL,
        size_bytes bigint NOT NULL,
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_avatar_uploads PRIMARY KEY (id),
        CONSTRAINT fk_user_avatar_uploads_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_data_exports (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        status text NOT NULL,
        processed_at timestamp with time zone,
        ready_at timestamp with time zone,
        expires_at timestamp with time zone,
        storage_key character varying(512),
        download_url character varying(2048),
        size_bytes bigint,
        error_message character varying(2048),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_data_exports PRIMARY KEY (id),
        CONSTRAINT fk_user_data_exports_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_deletion_requests (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        status text NOT NULL,
        confirmed_at timestamp with time zone,
        scheduled_execution_at timestamp with time zone NOT NULL,
        cancelled_at timestamp with time zone,
        executed_at timestamp with time zone,
        reason character varying(2048),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_deletion_requests PRIMARY KEY (id),
        CONSTRAINT fk_user_deletion_requests_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_email_verifications (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        type text NOT NULL,
        token_hash character varying(128) NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        verified_at timestamp with time zone,
        new_email character varying(320),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_email_verifications PRIMARY KEY (id),
        CONSTRAINT fk_user_email_verifications_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_login_attempts (
        id uuid NOT NULL,
        user_id uuid,
        attempted_email character varying(320) NOT NULL,
        is_success boolean NOT NULL,
        failure_reason character varying(64),
        ip_address character varying(64),
        user_agent character varying(512),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_login_attempts PRIMARY KEY (id),
        CONSTRAINT fk_user_login_attempts_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_notification_prefs (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        module_code text NOT NULL,
        channel text NOT NULL,
        event_type character varying(64) NOT NULL,
        is_enabled boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_notification_prefs PRIMARY KEY (id),
        CONSTRAINT fk_user_notification_prefs_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_password_resets (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        token_hash character varying(128) NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        used_at timestamp with time zone,
        request_ip_address character varying(64),
        used_ip_address character varying(64),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_password_resets PRIMARY KEY (id),
        CONSTRAINT fk_user_password_resets_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_profiles (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        language_code character varying(20) NOT NULL,
        timezone character varying(64) NOT NULL,
        date_format character varying(32) NOT NULL,
        theme character varying(16) NOT NULL,
        display_name character varying(255),
        phone_number character varying(32),
        date_of_birth date,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_profiles PRIMARY KEY (id),
        CONSTRAINT fk_user_profiles_locales_language_code FOREIGN KEY (language_code) REFERENCES locales (code) ON DELETE RESTRICT,
        CONSTRAINT fk_user_profiles_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE user_sessions (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        token_hash character varying(128) NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        revoked_at timestamp with time zone,
        last_used_at timestamp with time zone NOT NULL,
        device_name character varying(255),
        device_type character varying(32),
        user_agent character varying(512),
        ip_address character varying(64),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_sessions PRIMARY KEY (id),
        CONSTRAINT fk_user_sessions_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE org_members (
        id uuid NOT NULL,
        org_id uuid NOT NULL,
        user_id uuid NOT NULL,
        role text NOT NULL,
        is_active boolean NOT NULL,
        joined_at timestamp with time zone,
        left_at timestamp with time zone,
        invited_by uuid,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        version integer NOT NULL,
        updated_by uuid,
        last_session_id uuid,
        CONSTRAINT pk_org_members PRIMARY KEY (id),
        CONSTRAINT fk_org_members_organizations_org_id FOREIGN KEY (org_id) REFERENCES organizations (id) ON DELETE CASCADE,
        CONSTRAINT fk_org_members_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE organization_history (
        id uuid NOT NULL,
        entity_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        version integer NOT NULL,
        changed_by uuid,
        session_id uuid,
        change_type text NOT NULL,
        changed_fields jsonb,
        snapshot jsonb,
        change_reason character varying(1024),
        CONSTRAINT pk_organization_history PRIMARY KEY (id),
        CONSTRAINT fk_organization_history_organizations_entity_id FOREIGN KEY (entity_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE spaces (
        id uuid NOT NULL,
        org_id uuid NOT NULL,
        parent_id uuid,
        name character varying(255) NOT NULL,
        description character varying(1024),
        type text NOT NULL,
        path character varying(2048) NOT NULL,
        depth integer NOT NULL,
        sort_order integer NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        version integer NOT NULL,
        updated_by uuid,
        last_session_id uuid,
        CONSTRAINT pk_spaces PRIMARY KEY (id),
        CONSTRAINT fk_spaces_organizations_org_id FOREIGN KEY (org_id) REFERENCES organizations (id) ON DELETE CASCADE,
        CONSTRAINT fk_spaces_spaces_parent_id FOREIGN KEY (parent_id) REFERENCES spaces (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE org_member_history (
        id uuid NOT NULL,
        entity_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        version integer NOT NULL,
        changed_by uuid,
        session_id uuid,
        change_type text NOT NULL,
        changed_fields jsonb,
        snapshot jsonb,
        change_reason character varying(1024),
        CONSTRAINT pk_org_member_history PRIMARY KEY (id),
        CONSTRAINT fk_org_member_history_org_members_entity_id FOREIGN KEY (entity_id) REFERENCES org_members (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE space_history (
        id uuid NOT NULL,
        entity_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        version integer NOT NULL,
        changed_by uuid,
        session_id uuid,
        change_type text NOT NULL,
        changed_fields jsonb,
        snapshot jsonb,
        change_reason character varying(1024),
        CONSTRAINT pk_space_history PRIMARY KEY (id),
        CONSTRAINT fk_space_history_spaces_entity_id FOREIGN KEY (entity_id) REFERENCES spaces (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE space_members (
        id uuid NOT NULL,
        space_id uuid NOT NULL,
        user_id uuid NOT NULL,
        role text NOT NULL,
        inherited boolean NOT NULL,
        inherited_from_space_id uuid,
        invited_by uuid,
        joined_at timestamp with time zone,
        left_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        version integer NOT NULL,
        updated_by uuid,
        last_session_id uuid,
        CONSTRAINT pk_space_members PRIMARY KEY (id),
        CONSTRAINT fk_space_members_spaces_inherited_from_space_id FOREIGN KEY (inherited_from_space_id) REFERENCES spaces (id) ON DELETE RESTRICT,
        CONSTRAINT fk_space_members_spaces_space_id FOREIGN KEY (space_id) REFERENCES spaces (id) ON DELETE CASCADE,
        CONSTRAINT fk_space_members_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE space_modules (
        id uuid NOT NULL,
        space_id uuid NOT NULL,
        module_code text NOT NULL,
        is_enabled boolean NOT NULL,
        settings jsonb,
        enabled_at timestamp with time zone NOT NULL,
        disabled_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        CONSTRAINT pk_space_modules PRIMARY KEY (id),
        CONSTRAINT fk_space_modules_spaces_space_id FOREIGN KEY (space_id) REFERENCES spaces (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE space_member_history (
        id uuid NOT NULL,
        entity_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        version integer NOT NULL,
        changed_by uuid,
        session_id uuid,
        change_type text NOT NULL,
        changed_fields jsonb,
        snapshot jsonb,
        change_reason character varying(1024),
        CONSTRAINT pk_space_member_history PRIMARY KEY (id),
        CONSTRAINT fk_space_member_history_space_members_entity_id FOREIGN KEY (entity_id) REFERENCES space_members (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_categories (
        id uuid NOT NULL,
        smodule_id uuid NOT NULL,
        name character varying(128) NOT NULL,
        code character varying(64) NOT NULL,
        kind text NOT NULL,
        icon character varying(64),
        color character varying(16),
        parent_id uuid,
        path character varying(2048),
        depth integer NOT NULL,
        sort_order integer NOT NULL,
        is_system boolean NOT NULL,
        description character varying(1024),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        CONSTRAINT pk_fin_categories PRIMARY KEY (id),
        CONSTRAINT fk_fin_categories_fin_categories_parent_id FOREIGN KEY (parent_id) REFERENCES fin_categories (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_categories_space_modules_smodule_id FOREIGN KEY (smodule_id) REFERENCES space_modules (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_monthly_periods (
        id uuid NOT NULL,
        smodule_id uuid NOT NULL,
        year integer NOT NULL,
        month integer NOT NULL,
        period_start timestamp with time zone NOT NULL,
        period_end timestamp with time zone NOT NULL,
        status text NOT NULL,
        total_income numeric(18,2) NOT NULL,
        total_expense numeric(18,2) NOT NULL,
        net_amount numeric(18,2) NOT NULL,
        category_breakdown jsonb,
        source_breakdown jsonb,
        transaction_count integer NOT NULL,
        currency character varying(8) NOT NULL,
        closed_at timestamp with time zone,
        closed_by uuid,
        note character varying(2048),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        CONSTRAINT pk_fin_monthly_periods PRIMARY KEY (id),
        CONSTRAINT fk_fin_monthly_periods_space_modules_smodule_id FOREIGN KEY (smodule_id) REFERENCES space_modules (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_monthly_periods_users_closed_by FOREIGN KEY (closed_by) REFERENCES users (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_sources (
        id uuid NOT NULL,
        smodule_id uuid NOT NULL,
        name character varying(128) NOT NULL,
        type text NOT NULL,
        balance numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        credit_limit numeric(18,2),
        statement_day integer,
        payment_due_day integer,
        min_installment_amt numeric(18,2),
        initial_balance numeric(18,2),
        icon character varying(64),
        color character varying(16),
        description character varying(1024),
        sort_order integer NOT NULL,
        is_archived boolean NOT NULL,
        external_ref character varying(255),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        version integer NOT NULL,
        updated_by uuid,
        last_session_id uuid,
        CONSTRAINT pk_fin_sources PRIMARY KEY (id),
        CONSTRAINT fk_fin_sources_space_modules_smodule_id FOREIGN KEY (smodule_id) REFERENCES space_modules (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_billing_cycles (
        id uuid NOT NULL,
        smodule_id uuid NOT NULL,
        source_id uuid NOT NULL,
        period_start timestamp with time zone NOT NULL,
        period_end timestamp with time zone NOT NULL,
        statement_date timestamp with time zone NOT NULL,
        payment_due_date timestamp with time zone NOT NULL,
        status text NOT NULL,
        opening_balance numeric(18,2) NOT NULL,
        closing_balance numeric(18,2) NOT NULL,
        total_spend numeric(18,2) NOT NULL,
        total_payments numeric(18,2) NOT NULL,
        minimum_payment numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        closed_at timestamp with time zone,
        paid_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        CONSTRAINT pk_fin_billing_cycles PRIMARY KEY (id),
        CONSTRAINT fk_fin_billing_cycles_fin_sources_source_id FOREIGN KEY (source_id) REFERENCES fin_sources (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_billing_cycles_space_modules_smodule_id FOREIGN KEY (smodule_id) REFERENCES space_modules (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_investments (
        id uuid NOT NULL,
        smodule_id uuid NOT NULL,
        source_id uuid,
        name character varying(128) NOT NULL,
        type text NOT NULL,
        symbol character varying(32),
        broker_name character varying(128),
        quantity numeric(28,8) NOT NULL,
        avg_cost numeric(28,8) NOT NULL,
        current_price numeric(28,8) NOT NULL,
        total_invested numeric(18,2) NOT NULL,
        total_proceeds numeric(18,2) NOT NULL,
        total_dividends numeric(18,2) NOT NULL,
        total_fees numeric(18,2) NOT NULL,
        current_value numeric(18,2) NOT NULL,
        price_updated_at timestamp with time zone,
        currency character varying(8) NOT NULL,
        start_date timestamp with time zone NOT NULL,
        is_closed boolean NOT NULL,
        closed_at timestamp with time zone,
        notes character varying(2048),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        CONSTRAINT pk_fin_investments PRIMARY KEY (id),
        CONSTRAINT fk_fin_investments_fin_sources_source_id FOREIGN KEY (source_id) REFERENCES fin_sources (id) ON DELETE SET NULL,
        CONSTRAINT fk_fin_investments_space_modules_smodule_id FOREIGN KEY (smodule_id) REFERENCES space_modules (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_savings (
        id uuid NOT NULL,
        smodule_id uuid NOT NULL,
        source_id uuid,
        name character varying(128) NOT NULL,
        bank_name character varying(128),
        account_number character varying(64),
        principal numeric(18,2) NOT NULL,
        interest_rate_pct numeric(7,4) NOT NULL,
        term_months integer NOT NULL,
        start_date timestamp with time zone NOT NULL,
        maturity_date timestamp with time zone NOT NULL,
        expected_return_amount numeric(18,2) NOT NULL,
        accrued_interest numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        status text NOT NULL,
        auto_rollover boolean NOT NULL,
        withdrawn_at timestamp with time zone,
        withdrawn_amount numeric(18,2),
        notes character varying(2048),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        CONSTRAINT pk_fin_savings PRIMARY KEY (id),
        CONSTRAINT fk_fin_savings_fin_sources_source_id FOREIGN KEY (source_id) REFERENCES fin_sources (id) ON DELETE SET NULL,
        CONSTRAINT fk_fin_savings_space_modules_smodule_id FOREIGN KEY (smodule_id) REFERENCES space_modules (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_source_history (
        id uuid NOT NULL,
        entity_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        version integer NOT NULL,
        changed_by uuid,
        session_id uuid,
        change_type text NOT NULL,
        changed_fields jsonb,
        snapshot jsonb,
        change_reason character varying(1024),
        CONSTRAINT pk_fin_source_history PRIMARY KEY (id),
        CONSTRAINT fk_fin_source_history_fin_sources_entity_id FOREIGN KEY (entity_id) REFERENCES fin_sources (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_debt_record_history (
        id uuid NOT NULL,
        entity_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        version integer NOT NULL,
        changed_by uuid,
        session_id uuid,
        change_type text NOT NULL,
        changed_fields jsonb,
        snapshot jsonb,
        change_reason character varying(1024),
        CONSTRAINT pk_fin_debt_record_history PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_debt_records (
        id uuid NOT NULL,
        smodule_id uuid NOT NULL,
        direction text NOT NULL,
        counterparty_name character varying(255) NOT NULL,
        counterparty_user_id uuid,
        counterparty_contact character varying(255),
        principal_amount numeric(18,2) NOT NULL,
        remaining_amount numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        interest_rate_pct numeric(7,4) NOT NULL,
        start_date timestamp with time zone NOT NULL,
        due_date timestamp with time zone,
        status text NOT NULL,
        origin_transaction_id uuid NOT NULL,
        settled_at timestamp with time zone,
        notes character varying(2048),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        version integer NOT NULL,
        updated_by uuid,
        last_session_id uuid,
        CONSTRAINT pk_fin_debt_records PRIMARY KEY (id),
        CONSTRAINT fk_fin_debt_records_space_modules_smodule_id FOREIGN KEY (smodule_id) REFERENCES space_modules (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_debt_records_users_counterparty_user_id FOREIGN KEY (counterparty_user_id) REFERENCES users (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_debt_transactions (
        id uuid NOT NULL,
        debt_record_id uuid NOT NULL,
        transaction_id uuid,
        txn_type text NOT NULL,
        amount numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        txn_date timestamp with time zone NOT NULL,
        remaining_after numeric(18,2) NOT NULL,
        notes character varying(2048),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_fin_debt_transactions PRIMARY KEY (id),
        CONSTRAINT fk_fin_debt_transactions_fin_debt_records_debt_record_id FOREIGN KEY (debt_record_id) REFERENCES fin_debt_records (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_installment_pays (
        id uuid NOT NULL,
        installment_plan_id uuid NOT NULL,
        sequence_no integer NOT NULL,
        due_date timestamp with time zone NOT NULL,
        amount numeric(18,2) NOT NULL,
        principal_amount numeric(18,2) NOT NULL,
        interest_amount numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        status text NOT NULL,
        billing_cycle_id uuid,
        transaction_id uuid,
        paid_at timestamp with time zone,
        note character varying(1024),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        CONSTRAINT pk_fin_installment_pays PRIMARY KEY (id),
        CONSTRAINT fk_fin_installment_pays_fin_billing_cycles_billing_cycle_id FOREIGN KEY (billing_cycle_id) REFERENCES fin_billing_cycles (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_installment_plan_history (
        id uuid NOT NULL,
        entity_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        version integer NOT NULL,
        changed_by uuid,
        session_id uuid,
        change_type text NOT NULL,
        changed_fields jsonb,
        snapshot jsonb,
        change_reason character varying(1024),
        CONSTRAINT pk_fin_installment_plan_history PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_installment_plans (
        id uuid NOT NULL,
        smodule_id uuid NOT NULL,
        source_id uuid NOT NULL,
        origin_transaction_id uuid NOT NULL,
        principal numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        total_months integer NOT NULL,
        interest_rate_pct numeric(7,4) NOT NULL,
        conversion_fee_rate numeric(7,4) NOT NULL,
        conversion_fee_amount numeric(18,2) NOT NULL,
        conversion_fee_status text NOT NULL,
        monthly_amount numeric(18,2) NOT NULL,
        start_date timestamp with time zone NOT NULL,
        end_date timestamp with time zone NOT NULL,
        paid_months integer NOT NULL,
        status text NOT NULL,
        notes character varying(2048),
        completed_at timestamp with time zone,
        cancelled_at timestamp with time zone,
        cancellation_reason character varying(1024),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        version integer NOT NULL,
        updated_by uuid,
        last_session_id uuid,
        CONSTRAINT pk_fin_installment_plans PRIMARY KEY (id),
        CONSTRAINT fk_fin_installment_plans_fin_sources_source_id FOREIGN KEY (source_id) REFERENCES fin_sources (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_installment_plans_space_modules_smodule_id FOREIGN KEY (smodule_id) REFERENCES space_modules (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_transactions (
        id uuid NOT NULL,
        smodule_id uuid NOT NULL,
        type text NOT NULL,
        status text NOT NULL,
        amount numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        txn_date timestamp with time zone NOT NULL,
        source_id uuid NOT NULL,
        dest_source_id uuid,
        category_id uuid,
        billing_cycle_id uuid,
        ref_txn_id uuid,
        debt_record_id uuid,
        installment_plan_id uuid,
        description character varying(512) NOT NULL,
        note character varying(2048),
        exchange_rate numeric(18,6),
        counterparty_name character varying(255),
        external_ref character varying(255),
        tags character varying(1024),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        version integer NOT NULL,
        updated_by uuid,
        last_session_id uuid,
        CONSTRAINT pk_fin_transactions PRIMARY KEY (id),
        CONSTRAINT fk_fin_transactions_fin_billing_cycles_billing_cycle_id FOREIGN KEY (billing_cycle_id) REFERENCES fin_billing_cycles (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_transactions_fin_categories_category_id FOREIGN KEY (category_id) REFERENCES fin_categories (id) ON DELETE SET NULL,
        CONSTRAINT fk_fin_transactions_fin_debt_records_debt_record_id FOREIGN KEY (debt_record_id) REFERENCES fin_debt_records (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_transactions_fin_installment_plans_installment_plan_id FOREIGN KEY (installment_plan_id) REFERENCES fin_installment_plans (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_transactions_fin_sources_dest_source_id FOREIGN KEY (dest_source_id) REFERENCES fin_sources (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_transactions_fin_sources_source_id FOREIGN KEY (source_id) REFERENCES fin_sources (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_transactions_fin_transactions_ref_txn_id FOREIGN KEY (ref_txn_id) REFERENCES fin_transactions (id) ON DELETE RESTRICT,
        CONSTRAINT fk_fin_transactions_space_modules_smodule_id FOREIGN KEY (smodule_id) REFERENCES space_modules (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_investment_txns (
        id uuid NOT NULL,
        investment_id uuid NOT NULL,
        transaction_id uuid,
        txn_type text NOT NULL,
        quantity numeric(28,8) NOT NULL,
        price_per_unit numeric(28,8) NOT NULL,
        total_amount numeric(18,2) NOT NULL,
        fee numeric(18,2) NOT NULL,
        tax numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        txn_date timestamp with time zone NOT NULL,
        realised_pn_l numeric(18,2),
        notes character varying(2048),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_fin_investment_txns PRIMARY KEY (id),
        CONSTRAINT fk_fin_investment_txns_fin_investments_investment_id FOREIGN KEY (investment_id) REFERENCES fin_investments (id) ON DELETE CASCADE,
        CONSTRAINT fk_fin_investment_txns_fin_transactions_transaction_id FOREIGN KEY (transaction_id) REFERENCES fin_transactions (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_transaction_history (
        id uuid NOT NULL,
        entity_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        version integer NOT NULL,
        changed_by uuid,
        session_id uuid,
        change_type text NOT NULL,
        changed_fields jsonb,
        snapshot jsonb,
        change_reason character varying(1024),
        CONSTRAINT pk_fin_transaction_history PRIMARY KEY (id),
        CONSTRAINT fk_fin_transaction_history_fin_transactions_entity_id FOREIGN KEY (entity_id) REFERENCES fin_transactions (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE TABLE fin_txn_splits (
        id uuid NOT NULL,
        transaction_id uuid NOT NULL,
        participant_name character varying(255) NOT NULL,
        participant_user_id uuid,
        participant_contact character varying(255),
        share_amount numeric(18,2) NOT NULL,
        reimbursed_amount numeric(18,2) NOT NULL,
        currency character varying(8) NOT NULL,
        status text NOT NULL,
        due_date timestamp with time zone,
        settled_at timestamp with time zone,
        note character varying(2048),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        CONSTRAINT pk_fin_txn_splits PRIMARY KEY (id),
        CONSTRAINT fk_fin_txn_splits_fin_transactions_transaction_id FOREIGN KEY (transaction_id) REFERENCES fin_transactions (id) ON DELETE CASCADE,
        CONSTRAINT fk_fin_txn_splits_users_participant_user_id FOREIGN KEY (participant_user_id) REFERENCES users (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    INSERT INTO locales (id, code, created_at, direction, english_name, is_active, is_default, name, updated_at)
    VALUES ('11111111-0000-0000-0000-000000000001', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'ltr', 'Vietnamese', TRUE, TRUE, 'Tiếng Việt', TIMESTAMPTZ '2024-01-01T00:00:00Z');
    INSERT INTO locales (id, code, created_at, direction, english_name, is_active, is_default, name, updated_at)
    VALUES ('11111111-0000-0000-0000-000000000002', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'ltr', 'English', TRUE, FALSE, 'English', TIMESTAMPTZ '2024-01-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('0356fc71-8e38-3c95-168a-2ffc7869db10', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Email input label.', 'auth.email', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Email');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('0ba9ad50-4b06-bd7e-7e28-b92a68b90769', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Password input label.', 'auth.password', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Mật khẩu');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('10ad085a-b06c-8c9f-0e4e-ce0cb85e4ffe', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Password input label.', 'auth.password', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Password');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('1503aa36-eebe-5976-9800-a311a99bb7ee', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic cancel button label.', 'common.cancel', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Cancel');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('1574fa20-4069-667e-d17e-bc719f0c54ab', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic delete button label.', 'common.delete', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Delete');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('20bb28e9-62f8-ad88-11d7-2df9a792fa9a', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic create button label.', 'common.create', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Tạo mới');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('2a43e2cb-cafb-4ff0-d8da-22cf1a40a50c', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Login button / page title.', 'auth.login', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Sign in');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('2a545327-5de8-4615-fe8e-ec59cb042637', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Inline validation message.', 'auth.email_invalid', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Email không hợp lệ.');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('317a7446-b672-cc88-e12a-66fa6ce5b441', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Loading indicator label.', 'common.loading', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Loading...');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('33bca43f-5706-3489-eca5-01eb2be265b7', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Main nav: settings.', 'nav.settings', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Settings');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('4754a4c1-c1d0-dd18-ce1f-2496561ab228', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Login button / page title.', 'auth.login', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Đăng nhập');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('4e32d742-d475-c995-2ffa-4e55a2eeca8e', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Main nav: dashboard.', 'nav.dashboard', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Dashboard');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('55ea0bda-5027-d5b0-b1cb-fc157ec4758d', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic auth-failure error message.', 'auth.login_failed', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Email hoặc mật khẩu không đúng.');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('5aeeb0c6-96ad-91d5-0224-75a52e4651e7', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic edit button label.', 'common.edit', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Edit');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('5cba09fe-eb35-86fb-e545-f4cb59120397', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic create button label.', 'common.create', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Create');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('608571af-b8d6-510c-4158-ca7802deb0f8', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic confirmation button label.', 'common.confirm', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Xác nhận');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('673a8d2f-2878-10f3-c063-2375741e320c', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Email input label.', 'auth.email', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Email');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('6798e3b9-4acb-bf52-86f5-396679a62644', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Main nav: finance transactions.', 'nav.transactions', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Giao dịch');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('687ad484-0b9f-de4c-8ed6-5ff1d33ec0e1', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Main nav: finance transactions.', 'nav.transactions', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Transactions');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('6b7aaf3b-a420-598e-9750-3eb3f1291838', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic save button label.', 'common.save', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Save');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('733eb310-ae67-490b-7518-ac5a0584444d', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Register button / page title.', 'auth.register', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Đăng ký');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('87de4fed-8fbb-bd83-9149-0965a329f72d', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic save button label.', 'common.save', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Lưu');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('8ba46305-adeb-395e-13b5-0afc6e6b2bf1', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Logout menu item.', 'auth.logout', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Sign out');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('94339832-a6c3-ff5b-d160-603c27bf02df', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Main nav: finance sources.', 'nav.sources', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nguồn tài chính');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('952149b5-989f-d353-9d51-5139b4343469', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Logout menu item.', 'auth.logout', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Đăng xuất');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('ae8e5f59-08c5-a5ba-1981-ddef6e38f924', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic cancel button label.', 'common.cancel', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Hủy');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('b294e269-67e3-f5c9-bf6a-6680588190c6', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic edit button label.', 'common.edit', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Sửa');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('ba248463-e3e9-bbba-8f60-7be997d6b287', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic delete button label.', 'common.delete', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Xóa');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('bc3f4c9c-36c5-1325-ee2b-13e58f749d5b', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic auth-failure error message.', 'auth.login_failed', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Invalid email or password.');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('c3ae1b21-b4cb-3551-7817-f91d2b226997', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Loading indicator label.', 'common.loading', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Đang tải...');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('c80f0f81-9ba1-74f2-0b0f-f19920f1a85c', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Main nav: dashboard.', 'nav.dashboard', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Tổng quan');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('d92223fb-0d41-247c-3a2f-7aea6f2d5f90', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Forgot-password link on login screen.', 'auth.forgot_password', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Quên mật khẩu?');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('da70554a-6edb-99c2-a26c-2bde4139f81c', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Main nav: settings.', 'nav.settings', 'vi', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Cài đặt');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('f0527c9b-319a-da46-e872-40de6de3dbf7', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Generic confirmation button label.', 'common.confirm', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Confirm');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('f17989ef-194b-dec8-a7d4-9d202bcf3a1a', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Forgot-password link on login screen.', 'auth.forgot_password', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Forgot password?');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('f1b02c77-f857-e64b-d8f0-92d2af4dde23', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Inline validation message.', 'auth.email_invalid', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Invalid email address.');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('f6a2e0bb-0895-fb98-20c4-d8cc41644fb4', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Register button / page title.', 'auth.register', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Sign up');
    INSERT INTO ui_strings (id, created_at, description, key, locale_code, updated_at, value)
    VALUES ('f84bd7b8-183b-167c-d9f7-1532f86d4140', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Main nav: finance sources.', 'nav.sources', 'en', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Accounts');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_audit_log_retentions_entity_type ON audit_log_retentions (entity_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_audit_logs_entity_type_entity_id_created_at ON audit_logs (entity_type, entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_audit_logs_user_id_created_at ON audit_logs (user_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_billing_cycles_payment_due_date ON fin_billing_cycles (payment_due_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_billing_cycles_smodule_id_status ON fin_billing_cycles (smodule_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_billing_cycles_source_id_period_start ON fin_billing_cycles (source_id, period_start);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_fin_billing_cycles_source_id_period_start_period_end ON fin_billing_cycles (source_id, period_start, period_end);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_categories_parent_id ON fin_categories (parent_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_fin_categories_smodule_id_code ON fin_categories (smodule_id, code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_categories_smodule_id_kind ON fin_categories (smodule_id, kind);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_debt_record_history_created_at ON fin_debt_record_history (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_debt_record_history_entity_id_created_at ON fin_debt_record_history (entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_debt_record_history_entity_id_version ON fin_debt_record_history (entity_id, version);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_debt_records_counterparty_user_id ON fin_debt_records (counterparty_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_debt_records_due_date ON fin_debt_records (due_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_debt_records_origin_transaction_id ON fin_debt_records (origin_transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_debt_records_smodule_id_direction_status ON fin_debt_records (smodule_id, direction, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_debt_transactions_debt_record_id_txn_date ON fin_debt_transactions (debt_record_id, txn_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_debt_transactions_transaction_id ON fin_debt_transactions (transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_installment_pays_billing_cycle_id ON fin_installment_pays (billing_cycle_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_fin_installment_pays_installment_plan_id_sequence_no ON fin_installment_pays (installment_plan_id, sequence_no);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_installment_pays_status_due_date ON fin_installment_pays (status, due_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_installment_pays_transaction_id ON fin_installment_pays (transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_installment_plan_history_created_at ON fin_installment_plan_history (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_installment_plan_history_entity_id_created_at ON fin_installment_plan_history (entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_installment_plan_history_entity_id_version ON fin_installment_plan_history (entity_id, version);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_fin_installment_plans_origin_transaction_id ON fin_installment_plans (origin_transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_installment_plans_smodule_id_status ON fin_installment_plans (smodule_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_installment_plans_source_id_status ON fin_installment_plans (source_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_investment_txns_investment_id_txn_date ON fin_investment_txns (investment_id, txn_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_investment_txns_transaction_id ON fin_investment_txns (transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_investments_smodule_id_is_closed ON fin_investments (smodule_id, is_closed);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_investments_smodule_id_type ON fin_investments (smodule_id, type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_investments_source_id ON fin_investments (source_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_monthly_periods_closed_by ON fin_monthly_periods (closed_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_monthly_periods_smodule_id_status ON fin_monthly_periods (smodule_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_fin_monthly_periods_smodule_id_year_month ON fin_monthly_periods (smodule_id, year, month);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_savings_maturity_date ON fin_savings (maturity_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_savings_smodule_id_status ON fin_savings (smodule_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_savings_source_id ON fin_savings (source_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_source_history_created_at ON fin_source_history (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_source_history_entity_id_created_at ON fin_source_history (entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_source_history_entity_id_version ON fin_source_history (entity_id, version);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_sources_smodule_id_is_archived ON fin_sources (smodule_id, is_archived);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_sources_smodule_id_type ON fin_sources (smodule_id, type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transaction_history_created_at ON fin_transaction_history (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transaction_history_entity_id_created_at ON fin_transaction_history (entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transaction_history_entity_id_version ON fin_transaction_history (entity_id, version);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transactions_billing_cycle_id ON fin_transactions (billing_cycle_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transactions_category_id ON fin_transactions (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transactions_debt_record_id ON fin_transactions (debt_record_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transactions_dest_source_id ON fin_transactions (dest_source_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transactions_installment_plan_id ON fin_transactions (installment_plan_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transactions_ref_txn_id ON fin_transactions (ref_txn_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transactions_smodule_id_txn_date ON fin_transactions (smodule_id, txn_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transactions_smodule_id_type_txn_date ON fin_transactions (smodule_id, type, txn_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_transactions_source_id_txn_date ON fin_transactions (source_id, txn_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_txn_splits_participant_user_id ON fin_txn_splits (participant_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_txn_splits_transaction_id ON fin_txn_splits (transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_fin_txn_splits_transaction_id_status ON fin_txn_splits (transaction_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_locales_code ON locales (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_locales_is_default_singleton ON locales (is_default) WHERE is_default = true;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_org_member_history_created_at ON org_member_history (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_org_member_history_entity_id_created_at ON org_member_history (entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_org_member_history_entity_id_version ON org_member_history (entity_id, version);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_org_members_org_id_user_id ON org_members (org_id, user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_org_members_user_id_is_active ON org_members (user_id, is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_organization_history_created_at ON organization_history (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_organization_history_entity_id_created_at ON organization_history (entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_organization_history_entity_id_version ON organization_history (entity_id, version);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_organizations_owner_id ON organizations (owner_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_organizations_slug ON organizations (slug);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_space_history_created_at ON space_history (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_space_history_entity_id_created_at ON space_history (entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_space_history_entity_id_version ON space_history (entity_id, version);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_space_member_history_created_at ON space_member_history (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_space_member_history_entity_id_created_at ON space_member_history (entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_space_member_history_entity_id_version ON space_member_history (entity_id, version);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_space_members_inherited_from_space_id ON space_members (inherited_from_space_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_space_members_space_id_user_id ON space_members (space_id, user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_space_members_user_id ON space_members (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_space_modules_space_id_module_code ON space_modules (space_id, module_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_spaces_org_id_parent_id ON spaces (org_id, parent_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_spaces_parent_id ON spaces (parent_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_spaces_path ON spaces (path);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_system_event_logs_entity_type_entity_id_created_at ON system_event_logs (entity_type, entity_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_system_event_logs_event_type_created_at ON system_event_logs (event_type, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_system_event_logs_job_name_created_at ON system_event_logs (job_name, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_translation_fallbacks_fallback_locale_code ON translation_fallbacks (fallback_locale_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_translation_fallbacks_locale_code_priority ON translation_fallbacks (locale_code, priority);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_translations_entity_type_entity_id_field_locale_code ON translations (entity_type, entity_id, field, locale_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_translations_entity_type_entity_id_locale_code ON translations (entity_type, entity_id, locale_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_translations_locale_code ON translations (locale_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_ui_strings_key_locale_code ON ui_strings (key, locale_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_ui_strings_locale_code ON ui_strings (locale_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_auth_providers_provider_provider_user_id ON user_auth_providers (provider, provider_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_auth_providers_user_id ON user_auth_providers (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_avatar_uploads_user_id_active ON user_avatar_uploads (user_id) WHERE is_active = true;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_avatar_uploads_user_id_created_at ON user_avatar_uploads (user_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_data_exports_status_created_at ON user_data_exports (status, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_data_exports_user_id_status ON user_data_exports (user_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_deletion_requests_status_scheduled_execution_at ON user_deletion_requests (status, scheduled_execution_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_deletion_requests_user_id_status ON user_deletion_requests (user_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_email_verifications_expires_at ON user_email_verifications (expires_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_email_verifications_token_hash ON user_email_verifications (token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_email_verifications_user_id_type ON user_email_verifications (user_id, type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_login_attempts_attempted_email_created_at ON user_login_attempts (attempted_email, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_login_attempts_ip_address_created_at ON user_login_attempts (ip_address, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_login_attempts_user_id_created_at ON user_login_attempts (user_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX "ix_user_notification_prefs_user_id_module_code_channel_event_t~" ON user_notification_prefs (user_id, module_code, channel, event_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_password_resets_expires_at ON user_password_resets (expires_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_password_resets_token_hash ON user_password_resets (token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_password_resets_user_id ON user_password_resets (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_profiles_language_code ON user_profiles (language_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_profiles_user_id ON user_profiles (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_sessions_expires_at ON user_sessions (expires_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_sessions_token_hash ON user_sessions (token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE INDEX ix_user_sessions_user_id_expires_at ON user_sessions (user_id, expires_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_users_email ON users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    ALTER TABLE fin_debt_record_history ADD CONSTRAINT fk_fin_debt_record_history_fin_debt_records_entity_id FOREIGN KEY (entity_id) REFERENCES fin_debt_records (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    ALTER TABLE fin_debt_records ADD CONSTRAINT fk_fin_debt_records_fin_transactions_origin_transaction_id FOREIGN KEY (origin_transaction_id) REFERENCES fin_transactions (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    ALTER TABLE fin_debt_transactions ADD CONSTRAINT fk_fin_debt_transactions_fin_transactions_transaction_id FOREIGN KEY (transaction_id) REFERENCES fin_transactions (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    ALTER TABLE fin_installment_pays ADD CONSTRAINT fk_fin_installment_pays_fin_installment_plans_installment_plan_id FOREIGN KEY (installment_plan_id) REFERENCES fin_installment_plans (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    ALTER TABLE fin_installment_pays ADD CONSTRAINT fk_fin_installment_pays_fin_transactions_transaction_id FOREIGN KEY (transaction_id) REFERENCES fin_transactions (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    ALTER TABLE fin_installment_plan_history ADD CONSTRAINT fk_fin_installment_plan_history_fin_installment_plans_entity_id FOREIGN KEY (entity_id) REFERENCES fin_installment_plans (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    ALTER TABLE fin_installment_plans ADD CONSTRAINT fk_fin_installment_plans_fin_transactions_origin_transaction_id FOREIGN KEY (origin_transaction_id) REFERENCES fin_transactions (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260515030530_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260515030530_InitialCreate', '8.0.11');
    END IF;
END $EF$;
COMMIT;

