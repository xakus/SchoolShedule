--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4 (Debian 17.4-1.pgdg120+2)
-- Dumped by pg_dump version 17.4 (Debian 17.4-1.pgdg120+2)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: Users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public."Users" (
    "Id" integer NOT NULL,
    "Email" character varying(255) NOT NULL,
    "PasswordHash" character varying(255) NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."Users" OWNER TO postgres;



--
-- Name: class_lessons; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.class_lessons (
    id integer NOT NULL,
    class_t_id integer NOT NULL,
    subject_t_id integer NOT NULL,
    hours_per_week integer NOT NULL
);


ALTER TABLE public.class_lessons OWNER TO postgres;


--
-- Name: class_t; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.class_t (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    school_t_id integer NOT NULL,
    active_day_week integer,
    active boolean DEFAULT true NOT NULL,
    create_date_time timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.class_t OWNER TO postgres;


--
-- Name: schedule; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.schedule (
    id integer NOT NULL,
    calss_id integer NOT NULL,
    teacher_id integer NOT NULL,
    subject_id integer NOT NULL,
    day_of_week integer NOT NULL,
    lessons_number integer NOT NULL,
    start_time interval NOT NULL,
    end_time interval NOT NULL,
    shedule_version_id integer NOT NULL
);


ALTER TABLE public.schedule OWNER TO postgres;


--
-- Name: schedule_version; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.schedule_version (
    id integer NOT NULL,
    generate_at timestamp without time zone NOT NULL
);


ALTER TABLE public.schedule_version OWNER TO postgres;



--
-- Name: school_t; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.school_t (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    max_lessons_day integer NOT NULL,
    user_id integer NOT NULL,
    create_date_time timestamp without time zone DEFAULT now() NOT NULL,
    active boolean DEFAULT true NOT NULL
);


ALTER TABLE public.school_t OWNER TO postgres;


--
-- Name: subject_t; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.subject_t (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    school_t_id integer NOT NULL,
    active boolean DEFAULT true NOT NULL,
    create_date_time timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.subject_t OWNER TO postgres;



--
-- Name: teacher_subject; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.teacher_subject (
    id integer NOT NULL,
    teacher_t_id integer NOT NULL,
    subject_t_id integer NOT NULL
);


ALTER TABLE public.teacher_subject OWNER TO postgres;



--
-- Name: teacher_t; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE IF NOT EXISTS public.teacher_t (
    id integer NOT NULL,
    full_name character varying(255) NOT NULL,
    school_t_id integer NOT NULL,
    active boolean DEFAULT true NOT NULL,
    create_date_time timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.teacher_t OWNER TO postgres;



--
-- Name: Users Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Users" ALTER COLUMN "Id" SET DEFAULT nextval('public."Users_Id_seq"'::regclass);


--
-- Name: class_lessons id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_lessons ALTER COLUMN id SET DEFAULT nextval('public.class_lessonse_id_seq'::regclass);


--
-- Name: class_t id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_t ALTER COLUMN id SET DEFAULT nextval('public.class_t_id_seq'::regclass);


--
-- Name: schedule id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule ALTER COLUMN id SET DEFAULT nextval('public.schedule_id_seq'::regclass);


--
-- Name: schedule_version id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule_version ALTER COLUMN id SET DEFAULT nextval('public.schedule_version_id_seq'::regclass);


--
-- Name: school_t id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.school_t ALTER COLUMN id SET DEFAULT nextval('public.school_t_id_seq'::regclass);


--
-- Name: subject_t id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.subject_t ALTER COLUMN id SET DEFAULT nextval('public.subject_t_id_seq'::regclass);


--
-- Name: teacher_subject id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.teacher_subject ALTER COLUMN id SET DEFAULT nextval('public.teacher_subject_id_seq'::regclass);


--
-- Name: teacher_t id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.teacher_t ALTER COLUMN id SET DEFAULT nextval('public.teacher_t_id_seq'::regclass);


--
-- Name: Users Users_Email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "Users_Email_key" UNIQUE ("Email");


--
-- Name: Users Users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "Users_pkey" PRIMARY KEY ("Id");


--
-- Name: class_lessons class_lessonse_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_lessons
    ADD CONSTRAINT class_lessonse_pkey PRIMARY KEY (id);


--
-- Name: class_t class_t_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_t
    ADD CONSTRAINT class_t_pkey PRIMARY KEY (id);


--
-- Name: schedule schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule
    ADD CONSTRAINT schedule_pkey PRIMARY KEY (id);


--
-- Name: schedule_version schedule_version_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule_version
    ADD CONSTRAINT schedule_version_pkey PRIMARY KEY (id);


--
-- Name: school_t school_t_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.school_t
    ADD CONSTRAINT school_t_pkey PRIMARY KEY (id);


--
-- Name: subject_t subject_t_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.subject_t
    ADD CONSTRAINT subject_t_pkey PRIMARY KEY (id);


--
-- Name: teacher_subject teacher_subject_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.teacher_subject
    ADD CONSTRAINT teacher_subject_pkey PRIMARY KEY (id);


--
-- Name: teacher_t teacher_t_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.teacher_t
    ADD CONSTRAINT teacher_t_pkey PRIMARY KEY (id);


--
-- Name: idx_schedule_class; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX IF NOT EXISTS idx_schedule_class ON public.schedule USING btree (calss_id);


--
-- Name: idx_schedule_teacher; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX IF NOT EXISTS idx_schedule_teacher ON public.schedule USING btree (teacher_id);


--
-- Name: idx_schedule_version; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX IF NOT EXISTS idx_schedule_version ON public.schedule USING btree (shedule_version_id);


--
-- Name: class_lessons class_lessonse_class_t_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_lessons
    ADD CONSTRAINT class_lessonse_class_t_id_fkey FOREIGN KEY (class_t_id) REFERENCES public.class_t(id) ON DELETE CASCADE;


--
-- Name: class_lessons class_lessonse_subject_t_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_lessons
    ADD CONSTRAINT class_lessonse_subject_t_id_fkey FOREIGN KEY (subject_t_id) REFERENCES public.subject_t(id) ON DELETE CASCADE;


--
-- Name: class_t class_t_school_t_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_t
    ADD CONSTRAINT class_t_school_t_id_fkey FOREIGN KEY (school_t_id) REFERENCES public.school_t(id) ON DELETE CASCADE;


--
-- Name: schedule schedule_shedule_version_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule
    ADD CONSTRAINT schedule_shedule_version_id_fkey FOREIGN KEY (shedule_version_id) REFERENCES public.schedule_version(id);


--
-- Name: subject_t subject_t_school_t_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.subject_t
    ADD CONSTRAINT subject_t_school_t_id_fkey FOREIGN KEY (school_t_id) REFERENCES public.school_t(id) ON DELETE CASCADE;


--
-- Name: teacher_subject teacher_subject_subject_t_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.teacher_subject
    ADD CONSTRAINT teacher_subject_subject_t_id_fkey FOREIGN KEY (subject_t_id) REFERENCES public.subject_t(id) ON DELETE CASCADE;


--
-- Name: teacher_subject teacher_subject_teacher_t_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.teacher_subject
    ADD CONSTRAINT teacher_subject_teacher_t_id_fkey FOREIGN KEY (teacher_t_id) REFERENCES public.teacher_t(id) ON DELETE CASCADE;


--
-- Name: teacher_t teacher_t_school_t_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.teacher_t
    ADD CONSTRAINT teacher_t_school_t_id_fkey FOREIGN KEY (school_t_id) REFERENCES public.school_t(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

